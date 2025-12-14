using System.Text.Json;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public static class JsonSeeder
{
    public static async Task SeedAsync(Dsw2025TpiContext context)
    {
        Console.WriteLine(">>> Entrando a JsonSeeder");

        await SeedProductsAsync(context);
        await SeedCustomersAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(Dsw2025TpiContext context)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Source", "Products.json");
        Console.WriteLine($">>> Products.json path: {path}");
        Console.WriteLine($">>> Products.json existe? {File.Exists(path)}");

        var json = await File.ReadAllTextAsync(path);

        var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Product>();

        Console.WriteLine($">>> Productos leídos: {products.Count}");

        foreach (var product in products)
        {
            // 👉 Aseguramos que InternalCode nunca vaya null a la BD
            if (string.IsNullOrWhiteSpace(product.InternalCode))
            {
                // Usa el SKU como código interno por defecto (o genera uno)
                product.InternalCode = product.Sku ?? Guid.NewGuid().ToString("N");
            }

            // Evitar duplicar si ya se seedeó antes
            var exists = await context.Products.AnyAsync(p => p.Id == product.Id);
            if (!exists)
            {
                context.Products.Add(product);
            }
        }
    }

    private static async Task SeedCustomersAsync(Dsw2025TpiContext context)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Source", "Customers.json");
        Console.WriteLine($">>> Customers.json path: {path}");
        Console.WriteLine($">>> Customers.json existe? {File.Exists(path)}");

        var json = await File.ReadAllTextAsync(path);

        var customers = JsonSerializer.Deserialize<List<Customer>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Customer>();

        Console.WriteLine($">>> Customers leídos: {customers.Count}");

        foreach (var customer in customers)
        {
            var exists = await context.Customers.AnyAsync(c => c.Id == customer.Id);
            if (!exists)
            {
                context.Customers.Add(customer);
            }
        }
    }
}
