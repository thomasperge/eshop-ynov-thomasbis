namespace Discount.Grpc.DTOs;

/// <summary>
/// Data Transfer Object for price calculation response.
/// </summary>
public class CalculatePriceResponseDto
{
    /// <summary>
    /// Gets or sets the discounted price after applying the coupon.
    /// </summary>
    public double DiscountedPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the discount amount applied.
    /// </summary>
    public double DiscountAmount { get; set; }
}

