namespace Discount.Grpc.Services;

/// <summary>
/// Service interface for discount calculation business logic.
/// Handles discount calculation, validation, and cumulative discount support.
/// </summary>
public interface IDiscountCalculationService
{
    /// <summary>
    /// Calculates the discounted price for a given original price and coupon.
    /// </summary>
    /// <param name="originalPrice">The original price before discount.</param>
    /// <param name="coupon">The discount coupon to apply.</param>
    /// <returns>A tuple containing the discounted price and the discount amount.</returns>
    (double DiscountedPrice, double DiscountAmount) CalculateDiscountedPrice(double originalPrice, Models.Coupon coupon);
    
    /// <summary>
    /// Validates if a discount coupon can be applied to an order.
    /// </summary>
    /// <param name="coupon">The discount coupon to validate.</param>
    /// <param name="orderAmount">The total amount of the order.</param>
    /// <returns>A tuple indicating if the coupon is valid and a validation message.</returns>
    (bool IsValid, string Message) ValidateDiscount(Models.Coupon coupon, double orderAmount);
    
    /// <summary>
    /// Checks if multiple coupons can be combined (cumulative discounts).
    /// </summary>
    /// <param name="coupons">The list of coupons to check.</param>
    /// <returns>True if all coupons can be combined, false otherwise.</returns>
    bool CanApplyCumulativeDiscounts(IEnumerable<Models.Coupon> coupons);
}

