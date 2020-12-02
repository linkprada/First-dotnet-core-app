using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Models
{
    public static class ModelBuilderExtension
    {
        public static void Seed (this ModelBuilder modelBuilder){
                modelBuilder.Entity<Employee>().HasData(
                new Employee{
                    Id = 1,
                    Name = "Mary",
                    Email = "Mary@hotmail.com",
                    Department = Dept.IT,
                },
                new Employee{
                    Id = 2,
                    Name = "John",
                    Email = "John@hotmail.com",
                    Department = Dept.HR,
                }
            );
        }
    }
}