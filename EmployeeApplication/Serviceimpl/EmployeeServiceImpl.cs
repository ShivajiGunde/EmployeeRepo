using AutoMapper;
using Azure.Identity;
using Dapper;
using EmployeeApplication.Pagination;
using EmployeeApplication.Service;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EmployeeApplication.Serviceimpl
{
    public class EmployeeServiceImpl : IEmployeeService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;

        private readonly IMapper _mapper;

        private readonly IConfiguration _configuration;

        public CommandType CommandType { get; private set; }

        public EmployeeServiceImpl(IConfiguration configuration, IMapper mapper,IMemoryCache cache
            )
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
            _mapper = mapper;
            _cache = cache;        }


        public async Task<Employee> InsertEmployee(Employee employee)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@Firstname", employee.Firstname);
            parameters.Add("@Lastname", employee.Lastname);
            parameters.Add("@Gender", employee.Gender);
            parameters.Add("@DateofBirth", employee.DateofBirth);
            parameters.Add("@PhoneNumber", employee.PhoneNumber);
            parameters.Add("@Email", employee.Email);

            employee.Status = "Active";
            parameters.Add("@Status", employee.Status);

            employee.Modified = DateTime.Now;
            parameters.Add("@Modified", employee.Modified);
            parameters.Add("@Username", employee.Username);
            parameters.Add("Password",employee.Password);
            parameters.Add("@Street", employee.Address.Street);
            parameters.Add("@Lane", employee.Address.Lane);
            parameters.Add("@City", employee.Address.City);
            parameters.Add("@ZipCode", employee.Address.ZipCode);
            parameters.Add("@State", employee.Address.State);
            parameters.Add("@Country", employee.Address.Country);
            parameters.Add("@DepartmentName", employee.Department.DepartmentName);
            parameters.Add("@EmployeeId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@AddressId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@DepartmentId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            string json = JsonSerializer.Serialize(employee);

            var result = await connection.ExecuteAsync("InsertEmployee", parameters, commandType: CommandType.StoredProcedure);


            employee.EmployeeId = parameters.Get<int>("@EmployeeId");
            employee.Department.DepartmentId = parameters.Get<int>("@DepartmentId");
            employee.Address.AddressId = parameters.Get<int>("@AddressId");

            return employee;
        }


       
        public async Task<Employee> GetEmployeeByEmail(string email)
        {
            try { 
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();

            parameters.Add("@Email", email);

            var sql = "GetEmployeeByEmail";

            string cacheKey = $"employee_{email}";

            if (_cache.TryGetValue(cacheKey, out Employee cachedEmployee))
            {
                Console.WriteLine("Employee retrieved from cache by email.");
                return cachedEmployee;
            }
            else
            {
                Console.WriteLine("✖ Employee not found in cache.");
            }

            var employees = await connection.QueryAsync<Employee, Address, Department, Employee>(
               sql, (employee, address, department) =>
               {
                   employee.Address = address;
                   employee.Department = department;
                   return employee;
               },
                param: parameters,
                splitOn: "AddressId,DepartmentId",
                commandType: CommandType.StoredProcedure
            );
            var employee = employees.FirstOrDefault();

            if (employee != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, employee, cacheEntryOptions);
                Console.WriteLine("Employee added to cache.");
            }

            return employee;
        }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employee: {ex.Message}", ex);
    }
}

        public async Task<PageDetails<EmployeeDto>> GetPaginatedEmployeeAsync(int page, int pageSize, string search)
        {

            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            var parameters = new DynamicParameters();

            parameters.Add("@Page", page);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@Search", search);

            using var multi = await connection.QueryMultipleAsync(
    "GetPaginatedEmployee",
    new { Page = page, PageSize = pageSize, Search = search },
    commandType: CommandType.StoredProcedure
);

            var employees = multi.Read<Employee, Address, Department, Employee>(
                (employee, address, department) =>
                {
                    employee.Address = address;
                    employee.Department = department;
                    return employee;
                },
                splitOn: "AddressId,DepartmentId"
            ).ToList();

            var totalCount = multi.ReadFirst<int>();

            var employeeDto = _mapper.Map<List<EmployeeDto>>(employees);

            return new PageDetails<EmployeeDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = employeeDto
            };
        }

        public async Task<object?> AddDepartment(Department department)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();

            parameters.Add("@DepartmentName", department.DepartmentName);

            var sql = "INSERT INTO Departments (DepartmentName) VALUES (@DepartmentName)";

            return await connection.ExecuteAsync(sql, new { DepartmentName = department.DepartmentName });

        }


        public async Task<string> AddEmployees(List<Employee> employees)
        {
            try
            {
                var table = new DataTable();

                table.Columns.Add("Firstname", typeof(string));
                table.Columns.Add("Lastname", typeof(string));
                table.Columns.Add("Gender", typeof(string));
                table.Columns.Add("DateofBirth", typeof(DateTime));
                table.Columns.Add("PhoneNumber", typeof(string));
                table.Columns.Add("Email", typeof(string));
                table.Columns.Add("Status", typeof(string));
                table.Columns.Add("Username", typeof(string));
                table.Columns.Add("Password", typeof(string));

                table.Columns.Add("Street", typeof(string));
                table.Columns.Add("Lane", typeof(string));
                table.Columns.Add("City", typeof(string));
                table.Columns.Add("ZipCode", typeof(string));
                table.Columns.Add("State", typeof(string));
                table.Columns.Add("Country", typeof(string));
                table.Columns.Add("DepartmentName", typeof(string));

                foreach (var emp in employees)
                {
                    table.Rows.Add(
                        emp.Firstname,
                        emp.Lastname,
                        emp.Gender,
                        emp.DateofBirth,
                        emp.PhoneNumber,
                        emp.Email,
                        emp.Status,
                        emp.Username,
                        emp.Password,
                        emp.Address.Street,
                        emp.Address.Lane,
                        emp.Address.City,
                        emp.Address.ZipCode,
                        emp.Address.State,
                        emp.Address.Country,
                        emp.Department.DepartmentName
                    );
                }

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();

                var sql = "InsertEmployeesBulk";


                parameters.Add("@Employees", table.AsTableValuedParameter("EmployeeTableType"));

                var result =  await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: CommandType.StoredProcedure);

                return "Employees Added successfully";
            }
            catch (Exception e) {

                throw new Exception("Employee not inserted in db" + e.Message);

            }
        }


        public async Task<bool> UpdateEmployee(int employeeId, Employee employee)
        {
            try
            {

                employee.EmployeeId = employeeId;

                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();

                parameters.Add("@EmployeeId", employee.EmployeeId);
                parameters.Add("@Firstname", employee.Firstname);
                parameters.Add("@Lastname", employee.Lastname);
                parameters.Add("@Gender", employee.Gender);
                parameters.Add("@DateofBirth", employee.DateofBirth);
                parameters.Add("@PhoneNumber", employee.PhoneNumber);
                parameters.Add("@Email", employee.Email);
                parameters.Add("@Status", employee.Status);
                parameters.Add("@Username", employee.Username);
                parameters.Add("@Password", employee.Password);

                parameters.Add("@Street", employee.Address.Street);
                parameters.Add("@City", employee.Address.City);
                parameters.Add("@State", employee.Address.State);
                parameters.Add("@ZipCode", employee.Address.ZipCode);
                parameters.Add("@Country", employee.Address.Country);

                parameters.Add("@DepartmentName", employee.Department.DepartmentName);

                var rowsUpdated = await connection.QuerySingleAsync<int>(
                                             "UpdateEmployeeDetails",
                                                          parameters,
                                                  commandType: CommandType.StoredProcedure
                                                                        );

                return rowsUpdated > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<Employee> DeleteEmployeeById(int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();

            try
            {
                parameters.Add("@EmployeeId", employeeId);

                var deletedEmployee = await connection.QueryFirstOrDefaultAsync<Employee>(
                                  "DeleteEmployeeById", parameters, commandType: CommandType.StoredProcedure);

                return deletedEmployee;
            }
            catch (Exception e) {
                throw new Exception("Employee not deleted");
            }
        }

        public async Task<Employee> DeleteByFirstname(string firstname)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@Firstname", firstname);

                var deletedEmployee = await connection.QueryFirstOrDefaultAsync<Employee>(
                                  "DeleteEmployeeByFirstname", parameters, commandType: CommandType.StoredProcedure);

                return deletedEmployee;
            }
            catch (Exception e) {
                throw new Exception("Employee not deleted"+e.Message);
            }
        }




        public async Task<Employee> DeleteAllemployees()
        {
            using var connection = new SqlConnection(_connectionString);
            try { 
                var deleted = await connection.ExecuteScalarAsync<Employee>("DeleteAllEmployees", 
                                                                        commandType: CommandType.StoredProcedure);

                return deleted ;

            }catch(Exception ex) {

                throw new Exception("Employee not deleted"+ex.Message);
            }

        }

        public async void UpdateEmployeesBulk(List<EmployeeUpdateDto> employees)
        {
            using var connection = new SqlConnection(_connectionString) ;
            using var command = new SqlCommand("UpdateEmployeesBulk", connection);

                command.CommandType = CommandType.StoredProcedure;
           try{
                DataTable table = new DataTable();
                table.Columns.Add("EmployeeId", typeof(int));
                table.Columns.Add("Firstname", typeof(string));
                table.Columns.Add("Lastname", typeof(string));
                table.Columns.Add("Gender", typeof(string));
                table.Columns.Add("DateofBirth", typeof(DateTime));
                table.Columns.Add("PhoneNumber", typeof(string));
                table.Columns.Add("Email", typeof(string));
                table.Columns.Add("Status", typeof(string));
                table.Columns.Add("Username", typeof (string));
                table.Columns.Add("Password", typeof(string));
                table.Columns.Add("Street", typeof(string));
                table.Columns.Add("Lane", typeof(string));
                table.Columns.Add("City", typeof(string));
                table.Columns.Add("State", typeof(string));
                table.Columns.Add("ZipCode", typeof(string));
                table.Columns.Add("Country", typeof(string));
                table.Columns.Add("DepartmentName", typeof(string));

                foreach (var emp in employees)
                {
                    table.Rows.Add(
                        emp.EmployeeId,
                        emp.Firstname,
                        emp.Lastname,
                        emp.Gender,
                        emp.DateofBirth,
                        emp.PhoneNumber,
                        emp.Email,
                        emp.Status,
                        emp.Username,
                        emp.Password,
                        emp.Street,
                        emp.Lane,
                        emp.City,
                        emp.ZipCode,
                        emp.State,
                        emp.Country,
                        emp.DepartmentName
                    );
                }
                var parameter = command.Parameters.AddWithValue("@EmployeeUpdates", table);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.EmployeeUpdateType";

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                 }
            catch (Exception e) {
                throw new Exception(" Employee not Updated" + e.Message);
            }
         }

       

        public async Task<string> Verify(string username, string password)
        {
            var connection = new SqlConnection(_connectionString);

            var sql = "SELECT  Username, Password FROM Employee WHERE Username = @Username";

            var employee = await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Username = username });

            if (employee == null)

                return "employee not found by Username or password";

            if (employee.Password == password)

                return GenerateToken(username);
            else
                return "Invalid employee  password";
        }

        public string GenerateToken(string username)
        {

            var jwtSettings = _configuration.GetSection("Jwt");

            if (jwtSettings == null || jwtSettings["Issuer"] == null || jwtSettings["Audience"] == null)
            {
                throw new Exception("JWT Configuration is missing in appsetting.json");
            }
            var claims = new[]
            {
             new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Employee?> GetEmployeeById(int employeeId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                string cacheKey = $"employee_{employeeId}";

                if (_cache.TryGetValue(cacheKey, out Employee cachedEmployee))
                {
                    Console.WriteLine("Employees retrieved from cache.");
                    return cachedEmployee;
                }
                else
                {
                    Console.WriteLine("✖ Employee not found in cache.");
                }

                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", employeeId);
                var sql = "GetEmployeeById";

                var employees = await connection.QueryAsync<Employee, Address, Department, Employee>(
                    sql,
                    static (employee, address, department) =>
                    {
                        employee.Address = address;
                        employee.Department = department;
                        return employee;
                    },
                    param: parameters,
                    splitOn: "AddressId,DepartmentId",
                    commandType: CommandType.StoredProcedure
                );

                var employee = employees.FirstOrDefault();

                if (employee != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _cache.Set(cacheKey, employee, cacheEntryOptions);
                    Console.WriteLine("Employee added to cache.");
                }

                return employee;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employee: {ex.Message}", ex);
            }
        }

            }
    }
    


    




        










