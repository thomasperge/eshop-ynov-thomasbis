namespace Discount.Grpc.DTOs;

/// <summary>
/// Data Transfer Object for price calculation request.
/// </summary>
public class CalculatePriceRequestDto
{
    /// <summary>
    /// Gets or sets the original price of the product.
    /// </summary>
    public double OriginalPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the coupon to apply.
    /// </summary>
    public CouponDto? Coupon { get; set; }
}

