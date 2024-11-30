namespace Social_Media_2._0.Models
{
    public class Broadcast
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime Published { get; set; } = DateTime.Now; //Automatiskt satt till datumet meddelandet sparades. 
        public string? ImageUrl { get; set; }
        public ICollection<ApplicationUser> Likes { get; set; } = new List<ApplicationUser>();

    }
}
