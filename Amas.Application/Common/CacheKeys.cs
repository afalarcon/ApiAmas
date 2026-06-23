namespace Amas.Application.Common;

public static class CacheKeys
{
    public const string Products = "amas:products:list";
    public const string Categories = "amas:categories:list";
    public const string Configurations = "amas:configurations:list";
    public const string Catalogs = "amas:catalogs:list";
    public const string CatalogImages = "amas:catalogs:images";
    public const string CatalogProductsAll = "amas:catalogs:products:all";
    public const string InventoryItems = "amas:inventory:items";

    public static string CategoryImages(Guid categoryId) => $"amas:categories:{categoryId:N}:images";
    public static string CategoryProducts(Guid categoryId) => $"amas:categories:{categoryId:N}:products";
    public static string CatalogProductsByCategory(Guid categoryId) => $"amas:catalogs:products:category:{categoryId:N}";
    public static string ProductImages(Guid productId) => $"amas:products:{productId:N}:images";
}
