using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApp.Models;
using MyApp.Security;
using MyApp.ViewModels;

namespace MyApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<HomeController> logger;

        private readonly IDataProtector protector; 

        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment webHostEnvironment,
                              ILogger<HomeController> logger, IDataProtectionProvider dataProtectionProvider,
                              DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
            this.protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }
        
        [AllowAnonymous]
        public ViewResult Index ()
        {
            var model = _employeeRepository.GetAllEmployees().Select( e =>
            {
                e.EncryptedId = protector.Protect(e.Id.ToString());
                return e;                              
            });
            return View(model) ;
        } 

        [AllowAnonymous]
        public ViewResult Details (string id)
        {
            logger.LogTrace("LogTrace");
            logger.LogInformation("LogInformation");
            logger.LogDebug("LogDebug");
            logger.LogWarning("LogWarning");
            logger.LogError("LogError");
            logger.LogCritical("LogCritical");

            string decryptedId = protector.Unprotect(id);
            int employeeId = Convert.ToInt32(decryptedId);
            
            Employee employee = _employeeRepository.GetEmployee(employeeId);
            if (employee == null) 
            {
                Response.StatusCode = 404 ;
                return View("EmployeeNotFound",employeeId);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee ,
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
        } 

        [HttpGet]
        public ViewResult Create ()
        {
            return View() ;
        }

        [HttpGet]
        public ViewResult Edit (int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditView = new EmployeeEditViewModel 
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath,
            };
            return View(employeeEditView) ;
        }  

        [HttpPost]
        public IActionResult Create (EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid) 
            {
                string uniqueFileName = ProcessUploadedFile(model);

                Employee newEmployee = new Employee {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName,
                };
                _employeeRepository.add(newEmployee);
                return RedirectToAction("details", new {id = newEmployee.Id});
            }
            
            return View();
        } 

        [HttpPost]
        public IActionResult Edit (EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if (model.Photo != null) 
                {
                    if (model.ExistingPhotoPath != null) 
                    {
                        string filePath = Path.Combine(webHostEnvironment.WebRootPath,"images",model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }
                _employeeRepository.update(employee);
                return RedirectToAction("Index");
            }

            return View();
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
                
            }

            return uniqueFileName;
        }
    }
}