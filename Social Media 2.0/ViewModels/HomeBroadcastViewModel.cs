using System.ComponentModel.DataAnnotations;

namespace Social_Media_2._0.ViewModels
{
    public class HomeBroadcastViewModel
    {
        [Required]
        public string Message { get; set; }
        public IFormFile ImageUrl { get; set; }
        public int PostId { get; set; } // For post images
    }
}
