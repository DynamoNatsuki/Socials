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

            var listeningTo = await _dbContext.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.ListeningTo)
                .Select(l => new UsersListenToUserViewModel
                {
                    UserId = l.Id,
                    Name = l.Name
                })
                .ToListAsync();

            var viewModel = new ProfileIndexViewModel()
            {
                Name = user.Name ?? "",
                ProfileImageUrl = user.ProfileImagePath ?? "",
                ListeningTo = listeningTo
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
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid file to upload.";
                return RedirectToAction("Index");
            }

            // Validate file type and size
            if (!IsValidImageFile(model.ImageFile, out string validationError))
            {
                TempData["ErrorMessage"] = validationError;
                return RedirectToAction("Index");
            }

            try
            {
                // Create a sanitized and unique file name
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ImageFile.FileName)}";
                string sanitizedFileName = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_") + Path.GetExtension(fileName);
                string path = Path.Combine(_environment.WebRootPath, "images", "profileImages", sanitizedFileName);

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                // Save the file
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                var user = await _userManager.GetUserAsync(User);

                // Generate the path to the saved file for the user
                string imagePath = $"/images/profileImages/{sanitizedFileName}";
                user.ProfileImagePath = imagePath;

                // Update user profile and save changes
                await _userManager.UpdateAsync(user);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Profile");
            }
            catch
            {
                // Redirect to the profile page with an error message
                TempData["ErrorMessage"] = "An unexpected error occurred while uploading your profile picture. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // Helper method for validation
        private bool IsValidImageFile(IFormFile file, out string error)
        {
            error = string.Empty;

            // Validate MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedMimeTypes.Contains(file.ContentType))
            {
                error = "Only JPG and PNG images are allowed.";
                return false;
            }

            // Validate file size (500 KB limit)
            const long maxFileSize = 500 * 1024; // 500 KB
            if (file.Length > maxFileSize)
            {
                error = "File size cannot exceed 500 KB.";
                return false;
            }

            return true;
        }
    }
}
