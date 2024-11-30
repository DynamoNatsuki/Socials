using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Social_Media_2._0.Data;
using Social_Media_2._0.Models;
using Social_Media_2._0.ViewModels;
using System.Diagnostics;

namespace Social_Media_2._0.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _dbContext.Users
                .Include(u => u.ListeningTo) // Ensure ListeningTo is loaded
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null)
            {
                return View();
            }

            var broadcasts = await _dbContext.Users
                .Where(u => u.Id == user.Id || user.ListeningTo.Contains(u))
                .Include(f => f.ListeningTo)
                .SelectMany(u => u.Broadcasts)
                .Include(b => b.User)
                .Include(b => b.Likes)
                .OrderByDescending(b => b.Published)
                .ToListAsync();

            var viewModel = new HomeIndexViewModel()
            {
                Broadcasts = broadcasts,
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Broadcast(HomeBroadcastViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            string imagePath = null;

            if (viewModel.ImageUrl != null && viewModel.ImageUrl.Length > 0)
            {
                // Validate content type
                if (viewModel.ImageUrl.ContentType != "image/jpeg" && viewModel.ImageUrl.ContentType != "image/png")
                {
                    ModelState.AddModelError("ImageUrl", "Only JPG and PNG images are allowed.");
                    return View(viewModel);
                }

                // Validate file size (5 MB limit)
                if (viewModel.ImageUrl.Length > 5 * 1024 * 1024)

                {
                    ModelState.AddModelError("ImageUrl", "File size cannot exceed 5 MB.");
                    return View(viewModel);
                }

                // Make a unique filename to avoid conflicts
                string fileName = $"{Guid.NewGuid()}_{viewModel.ImageUrl.FileName}";
                string path = Path.Combine(_environment.WebRootPath, "images", "broadcastImages");

                //Ensure the directory exists
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Full file path
                string filePath = Path.Combine(path, fileName);

                // Save the image to wwwroot/images/broadcastImages
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageUrl.CopyToAsync(stream);
                }

                // Generate the path to the saved file
                imagePath = $"/images/broadcastImages/{fileName}";
            }

            var broadcast = new Broadcast()
            {
                Message = viewModel.Message,
                ImageUrl = imagePath,
                User = user,
            };

            _dbContext.Broadcasts.Add(broadcast);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Like([FromBody] LikeModel like)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var post = await _dbContext.Broadcasts.Where(l => l.Id == like.PostId)
                .Include(b => b.Likes)
                .FirstOrDefaultAsync();

            if (!post.Likes.Contains(currentUser))
            {
                post.Likes.Add(currentUser);
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }

        public async Task<IActionResult> Featured()
        {
            var popularPosts = await _dbContext.Broadcasts
                .Include(u => u.User)
                .Include(b => b.Likes)
                .OrderByDescending(b => b.Likes.Count)
                .Take(10)
                .ToListAsync();

            var viewModel = new HomeIndexViewModel()
            {
                Broadcasts = popularPosts,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> RecommendedUsers()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);

            // Fetch the IDs of users the current user is following
            var currentUser = await _dbContext.Users
                .Include(u => u.ListeningTo)
                .FirstOrDefaultAsync(u => u.Id == loggedInUser.Id);

            var followingIds = currentUser.ListeningTo.Select(u => u.Id).ToHashSet();

            // Step 1: Calculate total likes for each user (SQL equivalent works)
            var userLikes = await _dbContext.Users
                .Select(u => new
                {
                    UserId = u.Id,
                    TotalLikes = u.Broadcasts
                        .SelectMany(b => b.Likes)
                        .Count() // Count likes for each user's broadcasts
                })
                .ToListAsync();

            // Step 2: Filter out current user and followed users
            var recommendedUsers = userLikes
                .Where(u => u.UserId != loggedInUser.Id && !followingIds.Contains(u.UserId))
                .OrderByDescending(u => u.TotalLikes)
                .Take(5)
                .ToList();

            // Step 3: Retrieve full user objects for the recommended users
            var recommendedUserIds = recommendedUsers.Select(u => u.UserId).ToList();
            var recommendedUserObjects = await _dbContext.Users
                .Where(u => recommendedUserIds.Contains(u.Id))
                .ToListAsync();

            var viewModel = new HomeRecommendedUsersViewModel()
            {
                Users = recommendedUserObjects,
            };

            return View(viewModel);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
