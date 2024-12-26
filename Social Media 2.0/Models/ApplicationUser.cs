using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Social_Media_2._0.Models
{
    public class ApplicationUser : IdentityUser
    {  
        public string? Name { get; set; }
        public ICollection<Broadcast> Broadcasts { get; set; }
        public string? ProfileImagePath { get; set; }
        public ICollection<ApplicationUser> ListeningTo { get; set; } = new List<ApplicationUser>(); //Because "= new List" there is always a list, even if the list is empty, and it helps make the application more manageable by not having to check every time what is and isn't there.
        public ICollection<ApplicationUser> Followers { get; set; } = new List<ApplicationUser>();
    }
}
