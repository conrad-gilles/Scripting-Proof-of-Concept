public class MonacoMarker
{
    public string Message { get; set; }
    public int Severity { get; set; } // 8 = Error, 4 = Warning, 2 = Info
    public int StartLineNumber { get; set; }
    public int StartColumn { get; set; }
    public int EndLineNumber { get; set; }
    public int EndColumn { get; set; }
}
