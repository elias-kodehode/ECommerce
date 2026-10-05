namespace ECommerce.Api.Features.Products.Common;

public static class ProductCacheKeys
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>product-id:{<paramref name="id"/>}</returns>
    public static string ById(int id) => $"product-id:{id}";
}