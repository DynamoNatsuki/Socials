using Social_Media_2._0.Models;

namespace Social_Media_2._0.ViewModels
{
    public class ProfileIndexViewModel
    {
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public ICollection<UsersListenToUserViewModel> ListeningTo { get; set; } = new List<UsersListenToUserViewModel>();
    }
}

