namespace Shop.Shared.Constants;

public static class ProductsServiceRoutes
{
    public const string HideUserProducts = "products/users/{0}/deactivate";
    public const string ShowUserProducts = "products/users/{0}/reactivate";
    public const string DeleteUserProducts = "products/users/{0}/delete";
}