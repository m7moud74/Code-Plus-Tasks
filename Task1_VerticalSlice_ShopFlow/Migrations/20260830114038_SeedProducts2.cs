using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task1_VerticalSlice_ShopFlow.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "ProductID",
                keyValue: 1,
                columns: new[] { "Price", "Quantity" },
                values: new object[] { 10000m, 5 });

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "ProductID",
                keyValue: 2,
                columns: new[] { "Price", "Quantity" },
                values: new object[] { 200m, 3 });

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "ProductID",
                keyValue: 3,
                columns: new[] { "Price", "Quantity" },
                values: new object[] { 200m, 3 });

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "ProductID",
                keyValue: 4,
                columns: new[] { "Price", "Quantity" },
                values: new object[] { 1000m, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "products");
        }
    }
}
