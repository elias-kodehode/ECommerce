using ECommerce.Api.Common.Results;

namespace ECommerce.Api.Features.Products.Common;

public static class ProductErrors
{
    public static Error NotFound(int id) => Error.NotFound(
            "Product.NotFound",
            $"Product with id {id} was not found.");


    public static Error SkuConflict(string sku) =>
        Error.Conflict(
            "Product.Sku.Conflict",
            $"A product with SKU '{sku}' already exists.");
}
