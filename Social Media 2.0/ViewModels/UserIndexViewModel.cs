using Social_Media_2._0.Models;

namespace Social_Media_2._0.ViewModels
{
    public class UserIndexViewModel
    {
        public string Search { get; set; }
        public List<ApplicationUser> Result { get; set; } = new List<ApplicationUser>();
        //En lista måste sättas till propertyn för att vi ska kunna visa datan överhuvudtaget. Det bråkar annars om propertyn är tom. 

    }
}
