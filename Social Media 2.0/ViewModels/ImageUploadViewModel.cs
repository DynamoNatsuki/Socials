namespace Social_Media_2._0.ViewModels
{
    public class ImageUploadViewModel
    {
        public IFormFile ImageFile { get; set; } // This will hold the uploaded image
        public int UserId { get; set; } // For profile pictures
        public int PostId { get; set; } // For post images
    }
}
