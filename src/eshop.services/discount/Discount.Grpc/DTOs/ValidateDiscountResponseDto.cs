namespace Discount.Grpc.DTOs;

/// <summary>
/// Data Transfer Object for discount validation response.
/// </summary>
public class ValidateDiscountResponseDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the discount is valid.
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets a message describing the validation result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the coupon details if valid.
    /// </summary>
    public CouponDto? Coupon { get; set; }
}

