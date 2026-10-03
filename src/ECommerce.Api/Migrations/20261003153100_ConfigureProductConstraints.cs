using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Api.Migrations
{
	/// <inheritdoc />
	public partial class ConfigureProductConstraints : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Products_Sku",
				table: "Products");

			migrationBuilder.AlterColumn<string>(
				name: "Sku",
				table: "Products",
				type: "character varying(64)",
				maxLength: 64,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

			migrationBuilder.AlterColumn<decimal>(
				name: "Price",
				table: "Products",
				type: "numeric(18,2)",
				precision: 18,
				scale: 2,
				nullable: false,
				oldClrType: typeof(decimal),
				oldType: "numeric");

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "Products",
				type: "character varying(200)",
				maxLength: 200,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

			migrationBuilder.AlterColumn<string>(
				name: "Brand",
				table: "Products",
				type: "character varying(100)",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "text");

			migrationBuilder.CreateIndex(
				name: "IX_Products_Sku",
				table: "Products",
				column: "Sku",
				unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Products_Sku",
				table: "Products");

			migrationBuilder.AlterColumn<string>(
				name: "Sku",
				table: "Products",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(64)",
				oldMaxLength: 64);

			migrationBuilder.AlterColumn<decimal>(
				name: "Price",
				table: "Products",
				type: "numeric",
				nullable: false,
				oldClrType: typeof(decimal),
				oldType: "numeric(18,2)",
				oldPrecision: 18,
				oldScale: 2);

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "Products",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(200)",
				oldMaxLength: 200);

			migrationBuilder.AlterColumn<string>(
				name: "Brand",
				table: "Products",
				type: "text",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(100)",
				oldMaxLength: 100);

			migrationBuilder.CreateIndex(
				name: "IX_Products_Sku",
				table: "Products",
				column: "Sku");
		}
	}
}
