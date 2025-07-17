using AutoMapper;

namespace EmployeeApplication.Pagination
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Employee, EmployeeDto>();
            
        }
    }
}
    