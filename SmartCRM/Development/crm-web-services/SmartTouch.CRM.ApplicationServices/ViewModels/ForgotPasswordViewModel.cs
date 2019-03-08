using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please select account")]
        string AccountName { get; set; }
        int AccountId { get; set; }
        string AccountPrimaryEmail { get; set; }

        [Required(ErrorMessage = "Please enter a valid email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        string Email { get; set; }
    }
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please select account")]
        public string AccountName { get; set; }
        public int AccountId { get; set; }
        public string AccountPrimaryEmail { get; set; }

        [Required(ErrorMessage = "Please enter a valid email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

    }
}
