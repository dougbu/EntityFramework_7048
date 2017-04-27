using System;
using System.ComponentModel.DataAnnotations;
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
                context.Database.EnsureCreated();

                var suppliers = context.Suppliers
                    .OrderBy(x => x.Name)
                    .Select(x => new
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                    })
                    .ToList();

                Console.WriteLine(suppliers.Count);
            }
        }
    }

    public class Supplier
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-EntityFramework_7048;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}