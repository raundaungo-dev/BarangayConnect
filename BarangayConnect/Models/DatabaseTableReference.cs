namespace BarangayConnect.Models;

public class DatabaseTableReference
{
    public string TableName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<ColumnReference> Columns { get; set; } = [];
}
