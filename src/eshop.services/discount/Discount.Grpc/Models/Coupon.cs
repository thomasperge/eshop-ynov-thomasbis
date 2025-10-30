namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    
    public string Status { get; set; } = "ProchainementActif";

    public double Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    // Ne pas mettre de valeur dynamique ici
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public double MinimumPurchaseAmount { get; set; }

    public bool IsCumulative { get; set; } = false;
}