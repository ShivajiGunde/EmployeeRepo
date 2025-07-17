using System.ComponentModel.DataAnnotations;

namespace EmployeeApplication
{



    public class Employee 
    {

        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Firstname is required.")]
        [MinLength(1), MaxLength(50)]
        public required string Firstname { get; set; }

        [Required(ErrorMessage = "Lastname is required.")]
        [MinLength(1),MaxLength(50)]
        public required string Lastname { get; set; }
       
        [Required(ErrorMessage = "Gender is required.")]
        [MinLength(1), MaxLength(50)]
        public required string Gender { get; set; }

        [Required(ErrorMessage = "DateofBirth is required.")]
        public required DateTime DateofBirth { get; set; }

        [Required(ErrorMessage = "MobNumber  is required.")]
        [MinLength(1), MaxLength(20)]
         public required string PhoneNumber { get; set; }
      
        [Required(ErrorMessage = "Email is required.")]
        [MinLength(1), MaxLength(50)]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        [MinLength(1), MaxLength(50)]
        public required string Status { get; set; }

        public required DateTime Modified;


        public required string Username { get; set; }

        public required string Password  { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public Address Address { get; set; }
       
        [Required(ErrorMessage = "Department  is required.")]
        public Department  Department { get; set; }

        internal Employee FirstOrDefault()
        {
            throw new NotImplementedException();
        }
    }

}


