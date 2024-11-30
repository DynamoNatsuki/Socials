using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Social_Media_2._0.Data;
using Social_Media_2._0.Models;
using Social_Media_2._0.ViewModels;

namespace Social_Media_2._0.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return View();
            }

            var dbUser = await _dbContext.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync();

            var listeningTo = await _dbContext.Users.Where(u => u.Id == user.Id)
                .SelectMany(u => u.ListeningTo)
                .ToListAsync();

            var viewModel = new ProfileIndexViewModel()
            {
                Name = user.Name ?? "",
                ProfileImageUrl = user.ProfileImagePath ?? "",
                ListeningTo = user.ListeningTo,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            var isNameTaken = await _dbContext.Users
                .AnyAsync(u => u.Name == viewModel.Name && u.Id != user.Id);

            if (isNameTaken)
            {
                // Use TempData to store the error message
                TempData["ErrorMessage"] = "This username is already taken. Please choose another one.";

                // Redirect to the profile editing page (or wherever appropriate)
                return RedirectToAction("Index");
            }

            user.Name = viewModel.Name;

            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(ImageUploadViewModel model)
        {
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Validate content type
                if (model.ImageFile.ContentType != "image/jpeg" && model.ImageFile.ContentType != "image/png")
                {
                    ModelState.AddModelError("ImageFile", "Only JPG and PNG images are allowed.");
                    return View(model);
                }

                // Validate file size (5 MB limit)
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size cannot exceed 5 MB.");
                    return View(model);
                }

                // Make a unique filename to avoid conflicts
                string fileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
                string path = Path.Combine(_environment.WebRootPath, "images", "profileImages", fileName);

                // Save the image to wwwroot/images/profileImages
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                var user = await _userManager.GetUserAsync(User);

                // Generate the path to the saved file
                string imagePath = $"/images/profileImages/{fileName}";

                user.ProfileImagePath = imagePath ;
                // You could also save this imagePath in the database for the user

                await _userManager.UpdateAsync(user);
                await _dbContext.SaveChangesAsync();

                return Redirect("/");
            }

            return BadRequest("Invalid file upload.");
        }
    }
}
