using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountController : ControllerBase
{
    private readonly DiscountContext _context;

    public DiscountController(DiscountContext context)
    {
        _context = context;
    }

    // ✅ CREATE
    [HttpPost]
    public async Task<IActionResult> CreateCoupon([FromBody] Coupon coupon)
    {
        UpdateCouponStatus(coupon);
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCouponById), new { id = coupon.Id }, coupon);
    }

    // ✅ READ - Get all
    [HttpGet]
    public async Task<IActionResult> GetAllCoupons()
    {
        var coupons = await _context.Coupons.ToListAsync();
        foreach (var coupon in coupons)
        {
            UpdateCouponStatus(coupon);
        }
        await _context.SaveChangesAsync();
        return Ok(coupons);
    }

    // ✅ READ - Get by id
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCouponById(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
            return NotFound();

        UpdateCouponStatus(coupon);
        await _context.SaveChangesAsync();
        return Ok(coupon);
    }

    // ✅ UPDATE
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCoupon(int id, [FromBody] Coupon updatedCoupon)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
            return NotFound();

        coupon.ProductName = updatedCoupon.ProductName;
        coupon.Description = updatedCoupon.Description;
        coupon.Amount = updatedCoupon.Amount;
        coupon.Type = updatedCoupon.Type;
        coupon.StartDate = updatedCoupon.StartDate;
        coupon.EndDate = updatedCoupon.EndDate;
        coupon.MinimumPurchaseAmount = updatedCoupon.MinimumPurchaseAmount;
        coupon.IsCumulative = updatedCoupon.IsCumulative;

        UpdateCouponStatus(coupon);

        await _context.SaveChangesAsync();
        return Ok(coupon);
    }

    // ✅ DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null)
            return NotFound();

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ⚙️ Méthode privée pour calculer le statut automatiquement
    private static void UpdateCouponStatus(Coupon coupon)
    {
        var now = DateTime.UtcNow;

        if (coupon.EndDate < now)
            coupon.Status = "Expiré";
        else if (coupon.StartDate > now)
            coupon.Status = "ProchainementActif";
        else
            coupon.Status = "Actif";
    }
}