namespace Athena.Api.Database;

public class CustomerEntity
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}