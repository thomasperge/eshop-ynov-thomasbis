using Discount.Grpc.Data;
using Discount.Grpc.DTOs;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

/// <summary>
/// REST API controller for managing discount coupons.
/// Provides HTTP endpoints for CRUD operations and discount calculations.
/// </summary>
[ApiController]
[Route("api/discounts")]
[Produces("application/json")]
public class DiscountController : ControllerBase
{
    private readonly DiscountContext _dbContext;
    private readonly IDiscountCalculationService _discountCalculationService;
    private readonly ILogger<DiscountController> _logger;

    public DiscountController(DiscountContext dbContext, IDiscountCalculationService discountCalculationService, ILogger<DiscountController> logger)
    {
        _dbContext = dbContext;
        _discountCalculationService = discountCalculationService;
        _logger = logger;
    }
    
    /// <summary>
    /// Retrieves all discount coupons.
    /// </summary>
    /// <returns>A list of all coupons.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CouponDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CouponDto>>> GetAllDiscounts()
    {
        _logger.LogInformation("Retrieving all discounts");
        
        var coupons = await _dbContext.Coupons.ToListAsync();
        var result = coupons.Select(MapToDto).ToList();
        
        return Ok(result);
    }
    
    /// <summary>
    /// Retrieves a specific discount coupon by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the coupon.</param>
    /// <returns>The coupon details if found, otherwise returns NotFound.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponDto>> GetDiscountById(int id)
    {
        _logger.LogInformation("Retrieving discount with ID {Id}", id);
        
        var coupon = await _dbContext.Coupons.FindAsync(id);
        
        if (coupon == null)
        {
            _logger.LogWarning("Discount with ID {Id} not found", id);
            return NotFound($"Discount with ID {id} not found");
        }
        
        return Ok(MapToDto(coupon));
    }
    
    /// <summary>
    /// Retrieves discount information for a specific product.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    /// <returns>The discount details for the product.</returns>
    [HttpGet("product/{productName}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CouponDto>> GetDiscountByProduct(string productName)
    {
        _logger.LogInformation("Retrieving discount for product {ProductName}", productName);
        
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.ProductName == productName);
        
        if (coupon == null)
        {
            return Ok(new CouponDto { ProductName = productName, Amount = 0, DiscountType = "Fixed" });
        }
        
        return Ok(MapToDto(coupon));
    }
    
    /// <summary>
    /// Creates a new discount coupon.
    /// </summary>
    /// <param name="createDto">The coupon details to create.</param>
    /// <returns>The created coupon details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CouponDto>> CreateDiscount([FromBody] CreateCouponDto createDto)
    {
        _logger.LogInformation("Creating new discount with code {Code}", createDto.Code);
        
        try
        {
            var coupon = MapFromCreateDto(createDto);
            
            // Vérifier si un coupon avec le même code existe déjà
            var existingCoupon = await _dbContext.Coupons
                .FirstOrDefaultAsync(x => x.Code == coupon.Code);
            if (existingCoupon != null)
            {
                _logger.LogWarning("A coupon with code {Code} already exists", coupon.Code);
                return Conflict($"A coupon with code {coupon.Code} already exists");
            }
            
            await _dbContext.Coupons.AddAsync(coupon);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Discount created with ID {Id}", coupon.Id);
            return CreatedAtAction(nameof(GetDiscountById), new { id = coupon.Id }, MapToDto(coupon));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating discount");
            return BadRequest($"Error creating discount: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Updates an existing discount coupon.
    /// </summary>
    /// <param name="id">The unique identifier of the coupon to update.</param>
    /// <param name="updateDto">The updated coupon details.</param>
    /// <returns>The updated coupon details.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponDto>> UpdateDiscount(int id, [FromBody] CreateCouponDto updateDto)
    {
        _logger.LogInformation("Updating discount with ID {Id}", id);
        
        var coupon = await _dbContext.Coupons.FindAsync(id);
        
        if (coupon == null)
        {
            _logger.LogWarning("Discount with ID {Id} not found for update", id);
            return NotFound($"Discount with ID {id} not found");
        }
        
        // Mapper les champs mis à jour
        coupon.ProductName = updateDto.ProductName;
        coupon.Description = updateDto.Description;
        coupon.Amount = updateDto.Amount;
        coupon.DiscountPercent = updateDto.DiscountPercent;
        coupon.DiscountType = updateDto.DiscountType;
        coupon.Code = updateDto.Code;
        coupon.IsActive = updateDto.IsActive;
        coupon.StartDate = updateDto.StartDate;
        coupon.EndDate = updateDto.EndDate;
        coupon.MinOrderAmount = updateDto.MinOrderAmount;
        coupon.MaxDiscountAmount = updateDto.MaxDiscountAmount;
        coupon.IsCumulative = updateDto.IsCumulative;
        coupon.Category = updateDto.Category;
        
        _dbContext.Coupons.Update(coupon);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Discount with ID {Id} updated successfully", id);
        return Ok(MapToDto(coupon));
    }
    
    /// <summary>
    /// Deletes a discount coupon.
    /// </summary>
    /// <param name="id">The unique identifier of the coupon to delete.</param>
    /// <returns>No content if successful, NotFound if the coupon doesn't exist.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDiscount(int id)
    {
        _logger.LogInformation("Deleting discount with ID {Id}", id);
        
        var coupon = await _dbContext.Coupons.FindAsync(id);
        
        if (coupon == null)
        {
            _logger.LogWarning("Discount with ID {Id} not found for deletion", id);
            return NotFound($"Discount with ID {id} not found");
        }
        
        _dbContext.Coupons.Remove(coupon);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Discount with ID {Id} deleted successfully", id);
        return NoContent();
    }
    
    /// <summary>
    /// Validates a discount code.
    /// </summary>
    /// <param name="code">The discount code to validate.</param>
    /// <returns>The validation result.</returns>
    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(ValidateDiscountResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidateDiscountResponseDto>> ValidateDiscount(string code)
    {
        _logger.LogInformation("Validating discount code {Code}", code);
        
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        
        if (coupon == null)
        {
            return Ok(new ValidateDiscountResponseDto
            {
                IsValid = false,
                Message = "Discount code not found"
            });
        }
        
        // Utiliser le service de calcul pour la validation
        var (isValid, message) = _discountCalculationService.ValidateDiscount(coupon, 0); // 0 = pas de montant minimum dans cette validation
        
        return Ok(new ValidateDiscountResponseDto
        {
            IsValid = isValid,
            Message = message,
            Coupon = MapToDto(coupon)
        });
    }
    
    /// <summary>
    /// Applies a discount to calculate the final price.
    /// </summary>
    /// <param name="request">The calculation request containing the original price and coupon.</param>
    /// <returns>The calculated discounted price.</returns>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(CalculatePriceResponseDto), StatusCodes.Status200OK)]
    public Task<ActionResult<CalculatePriceResponseDto>> ApplyDiscount([FromBody] CalculatePriceRequestDto request)
    {
        _logger.LogInformation("Calculating discounted price for original price {OriginalPrice}", request.OriginalPrice);
        
        if (request.Coupon == null)
        {
            return Task.FromResult<ActionResult<CalculatePriceResponseDto>>(Ok(new CalculatePriceResponseDto
            {
                DiscountedPrice = request.OriginalPrice,
                DiscountAmount = 0
            }));
        }
        
        // Convertir le CouponDto en Coupon pour utiliser le service de calcul
        var coupon = MapFromDto(request.Coupon);
        
        // Utiliser le service de calcul pour calculer le prix
        var (discountedPrice, discountAmount) = _discountCalculationService.CalculateDiscountedPrice(request.OriginalPrice, coupon);
        
        _logger.LogInformation("Calculated discount: {DiscountAmount}, Final price: {DiscountedPrice}", discountAmount, discountedPrice);
        
        return Task.FromResult<ActionResult<CalculatePriceResponseDto>>(Ok(new CalculatePriceResponseDto
        {
            DiscountedPrice = discountedPrice,
            DiscountAmount = discountAmount
        }));
    }
    
    // ============ MÉTHODES DE MAPPING ============
    
    private static CouponDto MapToDto(Coupon coupon)
    {
        return new CouponDto
        {
            Id = coupon.Id,
            ProductName = coupon.ProductName,
            Description = coupon.Description,
            Amount = coupon.Amount,
            DiscountPercent = coupon.DiscountPercent,
            DiscountType = coupon.DiscountType,
            Code = coupon.Code,
            IsActive = coupon.IsActive,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            MinOrderAmount = coupon.MinOrderAmount,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            IsCumulative = coupon.IsCumulative,
            Category = coupon.Category
        };
    }
    
    private static Coupon MapFromCreateDto(CreateCouponDto dto)
    {
        return new Coupon
        {
            ProductName = dto.ProductName,
            Description = dto.Description,
            Amount = dto.Amount,
            DiscountPercent = dto.DiscountPercent,
            DiscountType = dto.DiscountType,
            Code = dto.Code,
            IsActive = dto.IsActive,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            IsCumulative = dto.IsCumulative,
            Category = dto.Category
        };
    }
    
    private static Coupon MapFromDto(CouponDto dto)
    {
        return new Coupon
        {
            Id = dto.Id,
            ProductName = dto.ProductName,
            Description = dto.Description,
            Amount = dto.Amount,
            DiscountPercent = dto.DiscountPercent,
            DiscountType = dto.DiscountType,
            Code = dto.Code,
            IsActive = dto.IsActive,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            IsCumulative = dto.IsCumulative,
            Category = dto.Category
        };
    }
}

