namespace EmployeeApplication.Pagination
{
    public class PageDetails<T>
    {

        //public int page { get; set; }
        public int Page { get; internal set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public IEnumerable<T> Items { get; set; }

    }
}
