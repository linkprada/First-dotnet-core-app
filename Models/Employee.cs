using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [NotMapped]
        public string EncryptedId { get; set; }

        [Required]
        [MaxLength(50,ErrorMessage="Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Office Email")]
        [EmailAddress(ErrorMessage="Invalid Email Format")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }
        
        public string PhotoPath { get; set; }

    }
}