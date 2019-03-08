using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ILoginViewModel
    {
        [Required(ErrorMessage = "Please select valid account")]
        string AccountName { get; set; }
        [Required(ErrorMessage = "Please select valid account")]
        int? AccountId { get; set; }

        [Required(ErrorMessage = "Please enter email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        string Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        string Password { get; set; }

        [Display(Name = "Remember me?")]
        bool RememberMe { get; set; }
        
    }

    public class LoginViewModel : ILoginViewModel
    {
        [Required(ErrorMessage = "Please select valid account")]
        public string AccountName { get; set; }
         [Required(ErrorMessage = "Please select valid account")]
        public int? AccountId { get; set; }

        [Required(ErrorMessage = "Please enter email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        
    }

    public class LoginAPIViewModel : ILoginViewModel
    {
        public string AccountName { get; set; }
        public int? AccountId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string AccessToken { get; set; }
        public string ErrorMessage { get; set; }
        public UserViewModel UserModel { get; set; }
    }
}
