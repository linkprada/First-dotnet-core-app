using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Utilities;

namespace MyApp.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Roles = new List<string>();
            Claims = new List<string>();
        }
        public string Id { get; set; }
        
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress(ErrorMessage="Invalid Email Format")]
        [Remote(controller:"Account",action:"IsEmailInUse")]
        [ValidEmailDomain(allowedDomain:"email.com",ErrorMessage="Email Domain must be email.com")]
        public string Email { get; set; }
        public string City { get; set; }
        public IList<string> Roles { get; set; }

        public List<string> Claims { get; set; }
    }
}