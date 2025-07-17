namespace EmployeeApplication.OpenApiHelpers
{
    internal class Models
    {
        internal class OpenApiSecurityScheme
        {
            public string Description { get; set; }
            public string Name { get; set; }
            public object In { get; set; }
            public object Type { get; set; }
            public string Scheme { get; set; }
            public string BearerFormat { get; set; }
        }
    }
}