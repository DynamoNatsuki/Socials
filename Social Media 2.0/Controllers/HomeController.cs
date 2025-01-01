using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

            // Validate the uploaded file, if any
            if (viewModel.ImageUrl != null && viewModel.ImageUrl.Length > 0)
            {
                // Validate content type
                if (viewModel.ImageUrl.ContentType != "image/jpeg" && viewModel.ImageUrl.ContentType != "image/png")
                {
                    TempData["ErrorMessage"] = "Only JPG and PNG images are allowed.";
                    return RedirectToAction("Index");
                }

                // Validate file size (500 KB limit)
                if (viewModel.ImageUrl.Length > 500 * 1024) // 500 KB
                {
                    TempData["ErrorMessage"] = "File size cannot exceed 500 KB.";
                    return RedirectToAction("Index");
                }

                try
                {
                    // Generate a unique filename to avoid conflicts
                    string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(viewModel.ImageUrl.FileName)}";
                    string path = Path.Combine(_environment.WebRootPath, "images", "broadcastImages");

                    // Ensure the directory exists
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

                    // Generate the relative path to the saved file
                    imagePath = $"/images/broadcastImages/{fileName}";
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while uploading your image. Please try again.";
                    return RedirectToAction("Index");
                }
            }

            // Create a new broadcast post
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

            // Find the post (broadcast) being liked, including its associated Likes
            var post = await _dbContext.Broadcasts
                .Where(b => b.Id == like.PostId)
                .Include(b => b.Likes)
                .FirstOrDefaultAsync();

            bool isLiked;

            // Check if the current user has already liked the post
            if (post.Likes.Contains(currentUser))
            {
                post.Likes.Remove(currentUser);
                isLiked = false;
            }
            else
            {
                post.Likes.Add(currentUser);
                isLiked = true;
            }

            await _dbContext.SaveChangesAsync();

            return Json(new { likeCount = post.Likes.Count, isLiked });
        }


        public async Task<IActionResult> Featured()
        {
            // Retrieve the top 10 most popular posts based on the number of likes
            var popularPosts = await _dbContext.Broadcasts
                .Include(u => u.User) // Include the user who created the post
                .Include(b => b.Likes) // Include the list of likes for each post
                .OrderByDescending(b => b.Likes.Count) // Order posts by descending like count
                .Take(10) // Limit the results to the top 10 posts
                .ToListAsync();

            // Create a view model containing the list of popular posts
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
