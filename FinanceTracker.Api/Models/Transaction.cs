namespace FinanceTracker.Api.Models;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; } // income positive, expense negative
    public string Merchant { get; set; } = "";
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
