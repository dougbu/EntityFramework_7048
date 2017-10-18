using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework_7048
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (var context = new MyDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlCommand(
                    @"CREATE PROCEDURE [dbo].[GetSuppliersCount]
(
    @Value int OUTPUT
)
AS
BEGIN
    SELECT @Value = Count(*)
    FROM [dbo].[Suppliers];
    SELECT *
    FROM [dbo].[Suppliers]
END");
                context.SaveChanges();

                var match = new Supplier
                {
                    Name = "Joe",
                };

                context.Add(match);
                context.SaveChanges();

                // See aspnet/EntityFrameworkCore#7048
                var suppliers = context.Suppliers
                    .OrderBy(x => x.Name)
                    .Select(x => new
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                    })
                    .ToList();
                Console.WriteLine($"Aggregated supplier count: {suppliers.Count}.");

                // See aspnet/EntityFrameworkCore#9277 and aspnet/EntityFrameworkCore#9309
                var parameter = new SqlParameter
                {
                    ParameterName = "Value",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Int,
                    Value = -1,
                };
                context.Database.ExecuteSqlCommand("SELECT @Value = Count(*) FROM [dbo].[Suppliers]", parameter);
                Console.WriteLine($"Direct supplier count: {parameter.Value}.");
                parameter.Value = -1;

                context.Database.ExecuteSqlCommand("[dbo].[GetSuppliersCount] @Value out", parameter);
                Console.WriteLine($"Procedure supplier count: {parameter.Value}.");
                parameter.Value = -1;

                context.Suppliers.FromSql("[dbo].[GetSuppliersCount] @Value out", parameter).ToList();
                Console.WriteLine($"FromSql supplier count: {parameter.Value}.");
                parameter.Value = -1;

                context.Database.ExecuteSqlCommand("[dbo].[GetSuppliersCount] @Value out", parameter);
                Console.WriteLine($"Procedure supplier count: {parameter.Value}.");
                parameter.Value = -1;

                context.Suppliers.FromSql("[dbo].[GetSuppliersCount] @Value out", parameter).ToList();
                Console.WriteLine($"FromSql supplier count: {parameter.Value}.");
            }
        }

        private class Supplier
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            public string Name { get; set; }

            [EmailAddress]
            public string Email { get; set; }
        }

        private class MyDbContext : DbContext
        {
            public DbSet<Supplier> Suppliers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EF_7048;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
    }
}
