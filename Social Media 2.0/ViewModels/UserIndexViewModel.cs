using Social_Media_2._0.Models;

namespace Social_Media_2._0.ViewModels
{
    public class UserIndexViewModel
    {
        public string Search { get; set; }
        public List<ApplicationUser> Result { get; set; } = new List<ApplicationUser>();
        //A list must be set to the property in order for us to be able to display the data at all. Otherwise, there will be problems if the property is empty.

    }
}
