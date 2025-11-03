namespace Discount.Grpc.DTOs;

/// <summary>
/// Data Transfer Object for Coupon used in REST API.
/// </summary>
public class CouponDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the coupon.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product this discount applies to. Optional for general coupons.
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the discount coupon.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the discount amount. Can be a fixed amount or a percentage value.
    /// </summary>
    public double Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the discount amount as a percentage (0-100).
    /// </summary>
    public double? DiscountPercent { get; set; }
    
    /// <summary>
    /// Gets or sets the type of discount: "Fixed" or "Percentage".
    /// </summary>
    public string DiscountType { get; set; } = "Fixed";
    
    /// <summary>
    /// Gets or sets the unique code for this discount (e.g., "SUMMER2024").
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether this discount is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the start date when this discount becomes valid.
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date when this discount expires.
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum order amount required to apply this discount.
    /// </summary>
    public double? MinOrderAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum discount amount cap (for percentage discounts).
    /// </summary>
    public double? MaxDiscountAmount { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this discount can be combined with other discounts.
    /// </summary>
    public bool IsCumulative { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the product category this discount applies to (e.g., "Electronics", "Clothing").
    /// </summary>
    public string? Category { get; set; }
}

