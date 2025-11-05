using Catalog.API.Models;
using Marten;
using Marten.Schema;

namespace Catalog.API.Data;

/// <summary>
/// Responsible for populating initial catalog data into the database using Marten's document store.
/// </summary>
public class CatalogInitialData : IInitialData
{
    /// <summary>
    /// Populates the initial catalog data into the database if not already present.
    /// </summary>
    /// <param name="store">The document store used to manage the database sessions.</param>
    /// <param name="cancellation">The cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        
        // Check if products already exist
        var existingProducts = await session.Query<Product>().ToListAsync(cancellation);
        
        if (existingProducts.Any())
        {
            // Update existing products that have Stock = 0 (products created before Stock field was added)
            // We set default Stock = 50 for products that were likely created without the Stock field
            bool hasUpdates = false;
            foreach (var product in existingProducts)
            {
                // If Stock is 0, update it to 50 (default value)
                // This handles products that were created before the Stock field was added to the model
                if (product.Stock == 0)
                {
                    product.Stock = 50; // Default stock value for existing products
                    session.Update(product);
                    hasUpdates = true;
                }
            }
            
            if (hasUpdates)
            {
                await session.SaveChangesAsync(cancellation);
            }
            
            return;
        }
        
        // No products exist, create initial products with their predefined stock values
        session.Store(GetPreconfiguredProducts());
        await session.SaveChangesAsync(cancellation);
    }


    /// <summary>
    /// Retrieves a collection of pre-configured product data to be used as the initial catalog dataset.
    /// </summary>
    /// <returns>A collection of <see cref="Product"/> objects containing pre-configured product details.</returns>
    private static IEnumerable<Product> GetPreconfiguredProducts() => new List<Product>()
    {
                new Product()
                {
                    Id = new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61"),
                    Name = "IPhone X",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-1.png",
                    Price = 950.00M,
                    Categories = ["Smart Phone"],
                    Stock = 50
                },
                new Product()
                {
                    Id = new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914"),
                    Name = "Samsung 10",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-2.png",
                    Price = 840.00M,
                    Categories = ["Smart Phone"],
                    Stock = 30
                },
                new Product()
                {
                    Id = new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8"),
                    Name = "Huawei Plus",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-3.png",
                    Price = 650.00M,
                    Categories = ["White Appliances"],
                    Stock = 25
                },
                new Product()
                {
                    Id = new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27"),
                    Name = "Xiaomi Mi 9",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-4.png",
                    Price = 470.00M,
                    Categories = ["White Appliances"],
                    Stock = 40
                },
                new Product()
                {
                    Id = new Guid("b786103d-c621-4f5a-b498-23452610f88c"),
                    Name = "HTC U11+ Plus",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-5.png",
                    Price = 380.00M,
                    Categories = ["Smart Phone"],
                    Stock = 15
                },
                new Product()
                {
                    Id = new Guid("c4bbc4a2-4555-45d8-97cc-2a99b2167bff"),
                    Name = "LG G7 ThinQ",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-6.png",
                    Price = 240.00M,
                    Categories = ["Home Kitchen"],
                    Stock = 20
                },
                new Product()
                {
                    Id = new Guid("93170c85-7795-489c-8e8f-7dcf3b4f4188"),
                    Name = "Panasonic Lumix",
                    Description = "This phone is the company's biggest change to its flagship smartphone in years. It includes a borderless.",
                    ImageFile = "product-6.png",
                    Price = 240.00M,
                    Categories = ["Camera"],
                    Stock = 10
                }
    };
}