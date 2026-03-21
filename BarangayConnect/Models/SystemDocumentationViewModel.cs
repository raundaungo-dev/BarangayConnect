namespace BarangayConnect.Models;

public class SystemDocumentationViewModel
{
    public List<FeatureHighlight> Features { get; set; } = [];
    public List<ScreenPreview> Previews { get; set; } = [];
    public List<DatabaseTableReference> Tables { get; set; } = [];
    public List<SqlStatementReference> SqlStatements { get; set; } = [];
    public string ErdImagePath { get; set; } = string.Empty;
    public string ErdPdfPath { get; set; } = string.Empty;
}
