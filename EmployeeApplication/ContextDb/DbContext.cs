
using System.Data.Entity;



namespace EmployeeApplication.ContextDb { 
    public class DbContext 
    {


        public DbSet<Employee> Address { get; set; }

        public DbSet<Employee> Department { get; set; }


    }
}

