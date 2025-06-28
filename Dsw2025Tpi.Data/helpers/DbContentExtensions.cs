using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data.helpers
{
    public static class DbContentExtensions
    {
        public static void Seedwork<T>(this Dsw2025TpiContext context, string dataSource) where T : class
        {
            if (context.Set<T>().Any()) return;

            var jsonPath = Path.Combine(AppContext.BaseDirectory, dataSource);
            if (!File.Exists(jsonPath)) return;

            var json = File.ReadAllText(jsonPath);
            var entities = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            if (entities == null || entities.Count == 0) return;

            context.Set<T>().AddRange(entities);
            context.SaveChanges();
        }
    }   
}
