using ECommerce.Api.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
	public DbSet<Product> Products { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		var product = modelBuilder.Entity<Product>();

		product.Property(x => x.Name)
			.HasMaxLength(Product.MaxNameLength)
			.IsRequired();

		product.Property(x => x.Sku)
			.HasMaxLength(Product.MaxSkuLength)
			.IsRequired();

		product.Property(x => x.Brand)
			.HasMaxLength(Product.MaxBrandLength)
			.IsRequired();

		product.Property(x => x.Price)
			.HasPrecision(18, 2);

		product.HasIndex(x => x.Sku)
			.IsUnique();

	}
}
