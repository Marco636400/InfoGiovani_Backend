namespace InfoGiovani_Back.DTOs
{
    public class PagedResponseDTO<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public IEnumerable<T> Schede { get; set; }
    }
}