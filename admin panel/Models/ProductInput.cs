namespace AdminPanel.Models;

public record ProductInput(int ProductId, string ProductName, decimal Price, int StockQuantity, string Description, string Sex);
public record PerfumeAndScent(int ProductId, string ScentName);