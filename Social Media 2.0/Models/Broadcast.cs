namespace Social_Media_2._0.Models
{
    public class Broadcast
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime Published { get; set; } = DateTime.Now; //Automatically set to the date the message was saved/posted.
        public string? ImageUrl { get; set; }
        public ICollection<ApplicationUser> Likes { get; set; } = new List<ApplicationUser>();

    }
}
