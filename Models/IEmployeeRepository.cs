using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Models
{
    public interface IEmployeeRepository
    {
        Employee GetEmployee (int Id);
        IEnumerable <Employee> GetAllEmployees();
        Employee add (Employee employee);
        Employee delete (int Id);
        Employee update (Employee employeeChanges);
    } 
    
}