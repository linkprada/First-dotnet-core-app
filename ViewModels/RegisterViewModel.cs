using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Utilities;

namespace MyApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage="Invalid Email Format")]
        [Remote(controller:"Account",action:"IsEmailInUse")]
        [ValidEmailDomain(allowedDomain:"email.com",ErrorMessage="Email Domain must be email.com")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Display(Name="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage="Password and Confirmation do not match")]
        public string ConfirmPassword { get; set; }

        public string City { get; set; }
    }
}