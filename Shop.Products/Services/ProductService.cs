using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Products.Data;
using Shop.Products.DTOs;
using Shop.Products.Models;
using Shop.Products.Services.Interfaces;
using Shop.Shared.Exceptions;

namespace Shop.Products.Services;

public partial class ProductService(ProductsDbContext dbContext) : IProductService;