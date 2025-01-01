﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Social_Media_2._0.Data;
using Social_Media_2._0.Models;
using Social_Media_2._0.ViewModels;

namespace Social_Media_2._0.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index(UserIndexViewModel viewModel)
        {
            if (viewModel.Search != null)
            {
                var users = await _dbContext.Users.Where(u => u.Name.Contains(viewModel.Search))
                    .ToListAsync();

                viewModel.Result = users;
            }

            return View(viewModel);
        }

        [Route("/Users/{id}")]
        public async Task<IActionResult> ShowUser(string id)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            var currentUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == loggedInUser.Id);

            var broadcasts = await _dbContext.Broadcasts.Where(b => b.User.Id == id)
                .OrderByDescending(b => b.Published).ToListAsync();

            var user = await _dbContext.Users.Where(u => u.Id == id)
                .FirstOrDefaultAsync();

            var viewModel = new UsersShowUserViewModel()
            {
                Broadcasts = broadcasts,
                User = user,
            };

            return View(viewModel);
        }

        [HttpPost, Route("/Users/Listen")]
        public async Task<IActionResult> ListenToUser(UsersListenToUserViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            var currentUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == loggedInUser.Id);

            var userToListenTo = await _dbContext.Users.FindAsync(viewModel.UserId);


            // Se till att båda användarna finns
            if (currentUser == null || userToListenTo == null)
            {
                return NotFound();
            }

            // Kontrollera om följrelationen redan finns
            var alreadyFollowing = currentUser.ListeningTo.Any(u => u.Id == userToListenTo.Id);
            if (alreadyFollowing)
            {
                return BadRequest("Du följer redan denna användare.");
            }

            // Lägg till följrelationen
            currentUser.ListeningTo.Add(userToListenTo);

            // Spara ändringarna
            await _dbContext.SaveChangesAsync();

            return Redirect("/");
        }

        [HttpPost, Route("/Users/Unlisten")]
        public async Task<IActionResult> Unlisten(UsersListenToUserViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            var currentUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == loggedInUser.Id);

            var userToUnfollow = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);

            if (userToUnfollow == null)
            {
                return NotFound();
            }

            // Dubbelkolla om användaren finns i listan över följda
            if (currentUser.ListeningTo.Contains(userToUnfollow))
            {
                currentUser.ListeningTo.Remove(userToUnfollow);

                var result = await _userManager.UpdateAsync(currentUser);
                if (!result.Succeeded)
                {
                    // Hantera fel på lämpligt sätt, t.ex. genom att visa ett felmeddelande
                    return BadRequest();
                }

                await _dbContext.SaveChangesAsync();
            }
            return Redirect("/");
        }

    }
}
