using EmployeeApplication.Pagination;
using EmployeeApplication.Serviceimpl;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApplication.Service
{
    public interface IEmployeeService
    {
        Task<Employee> InsertEmployee(Employee employee);
        Task<Employee> GetEmployeeById(int id);
        Task<Employee> GetEmployeeByEmail(string email);

         Task<PageDetails<EmployeeDto>> GetPaginatedEmployeeAsync(int page, int pageSize, string search);
        Task<object?> AddDepartment(Department department);
        Task<string> AddEmployees(List<Employee> employees);

        Task<bool> UpdateEmployee(int employeeId,Employee employee);
        Task<Employee> DeleteEmployeeById(int employeeId);
        Task<Employee> DeleteByFirstname(string firstname);
        Task<Employee> DeleteAllemployees();
       void UpdateEmployeesBulk(List<EmployeeUpdateDto> employees);
       
        
        string GenerateToken(string username);
        Task<string> Verify(string username, string password);
    }
}
