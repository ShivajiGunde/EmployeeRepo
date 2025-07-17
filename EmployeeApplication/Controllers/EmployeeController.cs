using Azure.Core;
using Dapper;
using EmployeeApplication.Service;
using EmployeeApplication.Serviceimpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]

    public class EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService) : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger = logger;
        private readonly IEmployeeService _employeeService = employeeService;




        [HttpPost("add")]
        public async Task<IActionResult> InsertEmployee([FromBody] Employee employee)
        {
            var createdEmployee = await _employeeService.InsertEmployee(employee);

            _logger.LogInformation("Employee Inserted successfully");
            return CreatedAtAction(nameof(GetEmployeeById), new { employeeId = createdEmployee.EmployeeId }, createdEmployee);
        }



        [HttpGet("{employeeId}")]

        public async Task<IActionResult> GetEmployeeById(int employeeId)
        {
            var employee = await _employeeService.GetEmployeeById(employeeId);

            if (employee == null)

                _logger.LogInformation("Employee not found by Id:{employeeId}", employeeId);
            NotFound();

            _logger.LogInformation("Employee Fetched sucessfully");

            return Ok(employee);
        }


        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetEmployeeByEmail(string email)
        {

            var employee = await _employeeService.GetEmployeeByEmail(email);

            if (employee == null)

                _logger.LogInformation("Employee Not Found with email");
            NotFound($"No employee found with email: {email}");

            _logger.LogInformation("Employee fetched By email");
            return Ok(employee);
        }



        [HttpGet("paged")]

        public async Task<IActionResult> GetPaginatedEmployeeAsync([FromQuery] int page = 1, int pageSize = 10, string? search = null)
        {

            if (page <= 0 || pageSize <= 0)
            {

                return BadRequest("Page size and page number are greater than zero");

            }

            var result = await _employeeService.GetPaginatedEmployeeAsync(page, pageSize, search);

            if (result.TotalCount == 0)

                _logger.LogInformation("User not Available");
            NotFound($"No users found for the search term '{search}'.");

            _logger.LogInformation("Pagination implemented successfully");
            return Ok(result);

        }



        [HttpPost("add/department")]

        public async Task<IActionResult> AddDepartment(Department department)
        {

            var createdDepartment = await _employeeService.AddDepartment(department);

            _logger.LogInformation("Department added successfully");
            return Ok(createdDepartment);

        }



        [HttpPut("{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] Employee employee)
        {
            if (employee == null)

                return BadRequest("Invalid employee data");

            var success = await _employeeService.UpdateEmployee(employeeId, employee);

            if (success)

                return Ok("Employee updated successfully");

            else
                return NotFound("Employee not found or no changes applied.");

        }



        [HttpPost("add/employees")]
        public async Task<IActionResult> AddEmployees([FromBody] List<Employee> employees)
        {

            var result = await _employeeService.AddEmployees(employees);

            _logger.LogInformation("Employees inserted successfully.");
            return Ok("Employees Inserted successfully");

        }

        [HttpDelete("delete-by-Id/{employeeId}")]
        public async void DeleteEmployeeById(int employeeId)
        {


            _logger.LogInformation("Employee Deleted By employeeId: {employeeId}", employeeId);
            await _employeeService.DeleteEmployeeById(employeeId);
        }

        [HttpDelete("delete-by-firstname/{firstname}")]
        public async void DeleteByFirstname(string firstname)
        {
            _logger.LogInformation("Employee Deleted by firstname:{Firstname}", firstname);
            await _employeeService.DeleteByFirstname(firstname);
        }


        [HttpDelete("delete/Employees")]
        public async void DeleteAllemployees()
        {
            _logger.LogInformation("All Employees Deleted successfully:");
            await _employeeService.DeleteAllemployees();
        }



        [HttpPut("updateAll-employees")]
        public async void UpdateEmployeesBulk([FromBody] List<EmployeeUpdateDto> employees)
        {

            _logger.LogInformation("All Employees Updated successfully");

            _employeeService.UpdateEmployeesBulk(employees);

        }


        [HttpPost("login/Username/{username}/password/{password}")]
        [AllowAnonymous]

        public async Task<IActionResult> Login(string username, string password)
        {

            var token =  await _employeeService.Verify(username, password);

            if (token == null)
            {

               return  Unauthorized(new { message = "Username and Password are invalid" });
            }
            else

               return  Ok(new { token });

        }



    }
}








