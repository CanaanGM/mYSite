namespace Domain.Shared;

public class PagedList<T>
{
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<T> Data { get; set; }

    public PagedList(List<T> data, int totalCount, int totalPages, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        TotalPages = totalPages;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}
