using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojectBE.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateaccount25042024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "account_rejected_times",
                table: "Accounts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_rejected_times",
                table: "Accounts");
        }
    }
}
