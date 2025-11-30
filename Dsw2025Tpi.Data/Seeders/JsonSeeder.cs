using System.Text.Json;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public static class JsonSeeder
{
    public static async Task SeedAsync(Dsw2025TpiContext context)
    {

        if (!await context.Products.AnyAsync())
            await SeedProductsAsync(context);

        if (!await context.Customers.AnyAsync())
            await SeedCustomersAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(Dsw2025TpiContext context)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Sources", "products.json");
        if (!File.Exists(path)) return;

        var json = await File.ReadAllTextAsync(path);
        var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Product>();

        context.Products.AddRange(products);
    }

    private static async Task SeedCustomersAsync(Dsw2025TpiContext context)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Sources", "customers.json");
        if (!File.Exists(path)) return;

        var json = await File.ReadAllTextAsync(path);
        var customers = JsonSerializer.Deserialize<List<Customer>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Customer>();

        context.Customers.AddRange(customers);
    }
}