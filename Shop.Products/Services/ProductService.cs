using Shop.Products.Data;
using Shop.Products.Services.Interfaces;

namespace Shop.Products.Services;

public partial class ProductService(ProductsDbContext dbContext) : IProductService;