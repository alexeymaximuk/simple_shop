using Shop.Shared.Constants;
using Shop.Users.Services.Interfaces;

namespace Shop.Users.Services;

public class ProductServiceClient(HttpClient httpClient) : IProductServiceClient
{
    public async Task DeactivateUserProducts(Guid userId)
    {
        var response = await httpClient.PostAsync(string.Format(ProductsServiceRoutes.HideUserProducts, userId), null);
        
        response.EnsureSuccessStatusCode();
    }
    
    public async Task ReactivateUserProducts(Guid userId)
    {
        var response = await httpClient.PostAsync(string.Format(ProductsServiceRoutes.ShowUserProducts, userId), null);
        
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUserProducts(Guid userId)
    {
        var response = await httpClient.PostAsync(
            string.Format(ProductsServiceRoutes.DeleteUserProducts, userId), null
        );
        
        response.EnsureSuccessStatusCode();
    }
}