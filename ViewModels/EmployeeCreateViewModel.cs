using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MyApp.Models;

namespace MyApp.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required]
        [MaxLength(50,ErrorMessage="Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Office Email")]
        [EmailAddress(ErrorMessage="Invalid Email Format")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }
        
        public IFormFile Photo { get; set; }
    }
}