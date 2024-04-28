using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojectBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateorderDetail15042024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "od_product_price",
                table: "Order_Details",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "od_product_price",
                table: "Order_Details");
        }
    }
}
