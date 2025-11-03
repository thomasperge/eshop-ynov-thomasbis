using Discount.Grpc.Models;
using Microsoft.Extensions.Logging;

namespace Discount.Grpc.Services;

/// <summary>
/// Service for discount calculation and validation business logic.
/// Handles fixed amount, percentage, cumulative discounts, and validation rules.
/// </summary>
public class DiscountCalculationService : IDiscountCalculationService
{
    private readonly ILogger<DiscountCalculationService> _logger;

    public DiscountCalculationService(ILogger<DiscountCalculationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates the discounted price for a given original price and coupon.
    /// Supports both Fixed and Percentage discount types, with maximum discount capping.
    /// </summary>
    public (double DiscountedPrice, double DiscountAmount) CalculateDiscountedPrice(double originalPrice, Coupon coupon)
    {
        _logger.LogDebug("Calculating discounted price for original price {OriginalPrice} with coupon {CouponCode}", 
            originalPrice, coupon.Code);

        if (coupon == null)
        {
            _logger.LogWarning("Coupon is null, returning original price");
            return (originalPrice, 0);
        }

        double discountAmount = 0;

        try
        {
            // Calculate discount based on type
            if (coupon.DiscountType == "Percentage" && coupon.DiscountPercent.HasValue && coupon.DiscountPercent > 0)
            {
                // Calculate percentage discount
                discountAmount = originalPrice * (coupon.DiscountPercent.Value / 100.0);
                _logger.LogDebug("Calculated percentage discount: {DiscountAmount} ({Percent}% of {OriginalPrice})", 
                    discountAmount, coupon.DiscountPercent, originalPrice);

                // Apply maximum discount cap if defined
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                    _logger.LogDebug("Applied maximum discount cap: {MaxDiscountAmount}", coupon.MaxDiscountAmount.Value);
                }
            }
            else if (coupon.DiscountType == "Fixed" && coupon.Amount > 0)
            {
                // Fixed amount discount
                discountAmount = coupon.Amount;
                _logger.LogDebug("Applied fixed discount: {DiscountAmount}", discountAmount);
            }
            else
            {
                _logger.LogWarning("Invalid discount type or amount. Type: {DiscountType}, Amount: {Amount}, Percent: {Percent}", 
                    coupon.DiscountType, coupon.Amount, coupon.DiscountPercent);
                return (originalPrice, 0);
            }

            // Ensure discount doesn't exceed original price
            if (discountAmount > originalPrice)
            {
                discountAmount = originalPrice;
                _logger.LogDebug("Discount capped to original price: {DiscountAmount}", discountAmount);
            }

            double discountedPrice = originalPrice - discountAmount;
            
            _logger.LogInformation("Discount calculation complete. Original: {OriginalPrice}, Discount: {DiscountAmount}, Final: {DiscountedPrice}", 
                originalPrice, discountAmount, discountedPrice);

            return (discountedPrice, discountAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating discount for coupon {CouponCode}", coupon.Code);
            return (originalPrice, 0);
        }
    }

    /// <summary>
    /// Validates if a discount coupon can be applied to an order.
    /// Checks active status, dates, and minimum order amount.
    /// </summary>
    public (bool IsValid, string Message) ValidateDiscount(Coupon coupon, double orderAmount)
    {
        _logger.LogDebug("Validating discount coupon {CouponCode} for order amount {OrderAmount}", 
            coupon.Code, orderAmount);

        if (coupon == null)
        {
            return (false, "Coupon is required");
        }

        // Check if coupon is active and dates are valid
        if (!coupon.IsValid(DateTime.UtcNow))
        {
            return (false, "Discount code is expired or inactive");
        }

        // Check minimum order amount
        if (coupon.MinOrderAmount.HasValue && orderAmount < coupon.MinOrderAmount.Value)
        {
            return (false, $"Minimum order amount of {coupon.MinOrderAmount.Value:C} required");
        }

        _logger.LogInformation("Discount coupon {CouponCode} validated successfully", coupon.Code);
        return (true, "Discount code is valid");
    }

    /// <summary>
    /// Checks if multiple coupons can be combined (cumulative discounts).
    /// A coupon can only be cumulative if its IsCumulative property is true.
    /// </summary>
    public bool CanApplyCumulativeDiscounts(IEnumerable<Coupon> coupons)
    {
        if (coupons == null || !coupons.Any())
        {
            return false;
        }

        // All coupons must be cumulative to be combined
        var allCumulative = coupons.All(c => c.IsCumulative);
        
        _logger.LogDebug("Checking cumulative discount capability. All cumulative: {AllCumulative}, Coupon count: {Count}", 
            allCumulative, coupons.Count());

        return allCumulative;
    }
}

