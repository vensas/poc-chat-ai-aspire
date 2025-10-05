namespace Athena.Api.Database;

public class SalesOrderEntity
{
    public Guid Id { get; set; }
    public string SalesOrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}