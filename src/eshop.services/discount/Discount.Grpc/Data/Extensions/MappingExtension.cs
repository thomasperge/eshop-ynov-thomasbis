using Discount.Grpc.Models;

namespace Discount.Grpc.Data.Extensions;

/// <summary>
/// Provides extension methods for mapping between domain models and gRPC models.
/// </summary>
public static class MappingExtension
{
    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    public static long ToUnixTimeSeconds(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return 0;
            
        return ((DateTimeOffset)dateTime.Value).ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Converts a Unix timestamp to DateTime.
    /// </summary>
    public static DateTime? FromUnixTimeSeconds(long unixTimestamp)
    {
        if (unixTimestamp == 0)
            return null;
            
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
    }
}

