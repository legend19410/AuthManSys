namespace AuthManSys.Application.Common.Models;

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; } = string.Empty;
    public string SortBy { get; set; } = "Id";
    public bool SortDescending { get; set; } = false;
}