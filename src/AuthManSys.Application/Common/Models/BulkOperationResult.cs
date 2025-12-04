namespace AuthManSys.Application.Common.Models;

public class BulkOperationResult
{
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public int SkippedOperations { get; set; }
    public int FailedOperations { get; set; }
    public List<string> ErrorMessages { get; set; } = new List<string>();
    public List<string> SuccessDetails { get; set; } = new List<string>();
    public List<string> SkippedDetails { get; set; } = new List<string>();

    public bool IsFullySuccessful => FailedOperations == 0;
    public bool HasPartialSuccess => SuccessfulOperations > 0 && FailedOperations > 0;
}