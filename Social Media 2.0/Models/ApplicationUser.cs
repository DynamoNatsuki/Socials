using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Social_Media_2._0.Models
{
    public class ApplicationUser : IdentityUser
    {  
        public string? Name { get; set; }
        public ICollection<Broadcast> Broadcasts { get; set; }
        public string? ProfileImagePath { get; set; }
        public ICollection<ApplicationUser> ListeningTo { get; set; } = new List<ApplicationUser>(); //I och med = new List så finns det alltid en lista, även om den är tom, och det hjälper till att göra applikation mer hanterbar genom att applikationen inte behöver kolla varje gång vad som finns och inte. 
        public ICollection<ApplicationUser> Followers { get; set; } = new List<ApplicationUser>();
    }
}
