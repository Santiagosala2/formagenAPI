namespace Models;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string FormCollectionName { get; set; } = null!;

    public string AdminSessionCollectionName { get; set; } = null!;

    public string AdminUserCollectionName { get; set; } = null!;

    public string UserSessionCollectionName { get; set; } = null!;

    public string UserCollectionName { get; set; } = null!;
}