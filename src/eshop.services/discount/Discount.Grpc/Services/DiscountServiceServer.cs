using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

/// <summary>
/// The DiscountServiceServer class implements the gRPC service for managing discount data.
/// It provides CRUD operations for discounts and communicates with the underlying database using a DbContext.
/// This class inherits from DiscountProtoServiceBase, which defines the service methods in the gRPC contract,
/// and implements the necessary logic for handling those methods.
/// </summary>
/// <remarks>
/// This class uses the DiscountContext for database interactions, IDiscountCalculationService for business logic,
/// and ILogger for logging purposes. It is registered with the gRPC pipeline in the application startup configuration.
/// </remarks>
public class DiscountServiceServer(DiscountContext dbContext, IDiscountCalculationService discountCalculationService, ILogger<DiscountServiceServer> logger) : DiscountProtoService.DiscountProtoServiceBase
{
    /// <summary>
    /// Retrieves discount details for a given product from the database.
    /// </summary>
    /// <param name="request">The request containing the product name to fetch the discount for.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="CouponModel"/> containing the discount details for the specified product.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if no discount is found for the specified product name.
    /// </exception>
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving discount for {ProductName}", request.ProductName);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName);
        
        if (coupon == null)
        {
            // Retourner un coupon avec amount=0 au lieu de lever une exception (comme demandé)
            logger.LogWarning("No discount found for {ProductName}, returning zero discount", request.ProductName);
            return MapToCouponModel(new Coupon 
            { 
                ProductName = request.ProductName, 
                Description = "", 
                Amount = 0,
                DiscountType = "Fixed"
            });
        }
        
        logger.LogInformation("Discount retrieved for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);
        
        return MapToCouponModel(coupon);
    }

    /// <summary>
    /// Creates a new discount for a specified product and stores it in the database.
    /// </summary>
    /// <param name="request">The request containing the details of the new discount to create, including the coupon information.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="CouponModel"/> representing the newly created discount.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the request's coupon information is null.
    /// </exception>
    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));
        
        var coupon = MapFromCouponModel(request.Coupon);
        logger.LogInformation("Creating new discount for {ProductName}", coupon.ProductName);
        
        // Vérifier si un coupon avec le même code existe déjà
        var existingCoupon = await dbContext.Coupons
            .FirstOrDefaultAsync(x => x.Code == coupon.Code);
        if (existingCoupon != null)
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"A coupon with code {coupon.Code} already exists"));
        
        await dbContext.Coupons.AddAsync(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount created for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);
        return MapToCouponModel(coupon);
    }

    /// <summary>
    /// Updates the discount details for a specific product based on the provided request.
    /// </summary>
    /// <param name="request">An object containing the updated discount information for a specific product.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns an updated <see cref="CouponModel"/> containing the modified discount details.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the provided coupon is null, or if the specified product or coupon identifier is not found in the database.
    /// </exception>
    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));
        
        logger.LogInformation("Updating discount for {ProductName}", request.Coupon.ProductName);

        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName 
                                                                      || x.Id == request.Coupon.Id);
        if(coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        
        // Mapper les nouveaux champs depuis le gRPC model
        coupon.ProductName = request.Coupon.ProductName;
        coupon.Description = request.Coupon.Description;
        coupon.Amount = request.Coupon.Amount;
        coupon.DiscountPercent = request.Coupon.DiscountPercent;
        coupon.DiscountType = request.Coupon.DiscountType;
        coupon.Code = request.Coupon.Code;
        coupon.IsActive = request.Coupon.IsActive;
        coupon.StartDate = MappingExtension.FromUnixTimeSeconds(request.Coupon.StartDateUnix);
        coupon.EndDate = MappingExtension.FromUnixTimeSeconds(request.Coupon.EndDateUnix);
        coupon.MinOrderAmount = request.Coupon.MinOrderAmount;
        coupon.MaxDiscountAmount = request.Coupon.MaxDiscountAmount;
        coupon.IsCumulative = request.Coupon.IsCumulative;
        coupon.Category = request.Coupon.Category;
        
        dbContext.Coupons.Update(coupon);
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Discount updated for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);
        return MapToCouponModel(coupon);
    }

    /// <summary>
    /// Deletes a discount for a specified product based on the provided coupon details.
    /// </summary>
    /// <param name="request">The request containing the details of the coupon to be deleted, including the product name or ID.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="DeleteDiscountResponse"/> indicating whether the discount was successfully deleted.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the provided coupon is null, or if no matching discount is found for the specified product name or ID.
    /// </exception>
    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request,
        ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));

        logger.LogInformation("Deleting discount for {ProductName}", request.Coupon.ProductName);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName 
                                                                      || x.Id == request.Coupon.Id);
        if(coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount deleted for {ProductName}", coupon.ProductName);
        
        return new DeleteDiscountResponse(){Success = true};
    }
    
    // ============ NOUVELLES MÉTHODES ============
    
    /// <summary>
    /// Retrieves a discount by its code.
    /// </summary>
    public override async Task<CouponModel> GetDiscountByCode(GetDiscountByCodeRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving discount for code {Code}", request.Code);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.Code == request.Code);
        
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with code {request.Code} not found"));
        
        logger.LogInformation("Discount retrieved for code {Code}: {Amount}", request.Code, coupon.Amount);
        return MapToCouponModel(coupon);
    }
    
    /// <summary>
    /// Retrieves all active discounts, optionally filtered by category.
    /// </summary>
    public override async Task<GetActiveDiscountsResponse> GetActiveDiscounts(GetActiveDiscountsRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving active discounts with category filter: {Category}", request.Category);
        
        var now = DateTime.UtcNow;
        var query = dbContext.Coupons.Where(c => c.IsActive 
            && (!c.StartDate.HasValue || c.StartDate.Value <= now)
            && (!c.EndDate.HasValue || c.EndDate.Value >= now));
        
        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(c => c.Category == request.Category);
        }
        
        var coupons = await query.ToListAsync();
        logger.LogInformation("Found {Count} active discounts", coupons.Count);
        
        var response = new GetActiveDiscountsResponse();
        response.Coupons.AddRange(coupons.Select(MapToCouponModel));
        return response;
    }
    
    /// <summary>
    /// Validates a discount code and checks if it can be applied to an order.
    /// </summary>
    public override async Task<ValidateDiscountResponse> ValidateDiscount(ValidateDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Validating discount code {Code} for order amount {OrderAmount}", request.Code, request.OrderAmount);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.Code == request.Code);
        
        if (coupon == null)
        {
            return new ValidateDiscountResponse
            {
                IsValid = false,
                Message = "Discount code not found"
            };
        }
        
        // Utiliser le service de calcul pour la validation
        var (isValid, message) = discountCalculationService.ValidateDiscount(coupon, request.OrderAmount);
        
        return new ValidateDiscountResponse
        {
            IsValid = isValid,
            Message = message,
            Coupon = MapToCouponModel(coupon)
        };
    }
    
    /// <summary>
    /// Calculates the discounted price for a given product and coupon.
    /// </summary>
    public override Task<CalculatePriceResponse> CalculateDiscountedPrice(CalculatePriceRequest request, ServerCallContext context)
    {
        logger.LogInformation("Calculating discounted price for original price {OriginalPrice}", request.OriginalPrice);
        
        if (request.Coupon == null)
        {
            return Task.FromResult(new CalculatePriceResponse
            {
                DiscountedPrice = request.OriginalPrice,
                DiscountAmount = 0
            });
        }
        
        // Convertir le CouponModel en Coupon pour utiliser le service de calcul
        var coupon = MapFromCouponModel(request.Coupon);
        
        // Utiliser le service de calcul pour calculer le prix
        var (discountedPrice, discountAmount) = discountCalculationService.CalculateDiscountedPrice(request.OriginalPrice, coupon);
        
        logger.LogInformation("Calculated discount: {DiscountAmount}, Final price: {DiscountedPrice}", discountAmount, discountedPrice);
        
        return Task.FromResult(new CalculatePriceResponse
        {
            DiscountedPrice = discountedPrice,
            DiscountAmount = discountAmount
        });
    }
    
    // ============ MÉTHODES DE MAPPING ============
    
    /// <summary>
    /// Maps a Coupon domain model to CouponModel gRPC message.
    /// </summary>
    private CouponModel MapToCouponModel(Coupon coupon)
    {
        return new CouponModel
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName ?? string.Empty,
            Description = coupon.Description,
            Amount = coupon.Amount,
            DiscountPercent = coupon.DiscountPercent ?? 0,
            DiscountType = coupon.DiscountType,
            Code = coupon.Code,
            IsActive = coupon.IsActive,
            StartDateUnix = MappingExtension.ToUnixTimeSeconds(coupon.StartDate),
            EndDateUnix = MappingExtension.ToUnixTimeSeconds(coupon.EndDate),
            MinOrderAmount = coupon.MinOrderAmount ?? 0,
            MaxDiscountAmount = coupon.MaxDiscountAmount ?? 0,
            IsCumulative = coupon.IsCumulative,
            Category = coupon.Category ?? string.Empty
        };
    }
    
    /// <summary>
    /// Maps a CouponModel gRPC message to Coupon domain model.
    /// </summary>
    private Coupon MapFromCouponModel(CouponModel model)
    {
        return new Coupon
        {
            Id = model.Id,
            ProductName = string.IsNullOrEmpty(model.ProductName) ? null : model.ProductName,
            Description = model.Description,
            Amount = model.Amount,
            DiscountPercent = model.DiscountPercent > 0 ? model.DiscountPercent : null,
            DiscountType = model.DiscountType,
            Code = model.Code,
            IsActive = model.IsActive,
            StartDate = MappingExtension.FromUnixTimeSeconds(model.StartDateUnix),
            EndDate = MappingExtension.FromUnixTimeSeconds(model.EndDateUnix),
            MinOrderAmount = model.MinOrderAmount > 0 ? model.MinOrderAmount : null,
            MaxDiscountAmount = model.MaxDiscountAmount > 0 ? model.MaxDiscountAmount : null,
            IsCumulative = model.IsCumulative,
            Category = string.IsNullOrEmpty(model.Category) ? null : model.Category
        };
    }
}