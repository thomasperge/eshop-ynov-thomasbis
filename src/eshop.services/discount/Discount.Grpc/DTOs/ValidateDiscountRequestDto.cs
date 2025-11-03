namespace Discount.Grpc.DTOs;

/// <summary>
/// Data Transfer Object for discount validation request.
/// </summary>
public class ValidateDiscountRequestDto
{
    /// <summary>
    /// Gets or sets the discount code to validate.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the total order amount.
    /// </summary>
    public double OrderAmount { get; set; }
}

