namespace EmployeeApplication
{
    public class EmployeeDto
    {
   

        public int EmployeeId { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Gender { get; set; }

        
        public required DateTime DateofBirth { get; set; }

        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }

        public required string   Status  { get; set; }

        public required DateTime Modified { get; set; }

        public required string Username     { get; set; }
        public required string Password { get; set; }


            public Address Address { get; set; }

            public Department Department { get; set; }

       }

    }
