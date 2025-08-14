using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleRecordFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_saleRecords",
                table: "saleRecords");

            migrationBuilder.RenameTable(
                name: "saleRecords",
                newName: "SaleRecords");

            migrationBuilder.RenameColumn(
                name: "QuantityId",
                table: "SaleRecords",
                newName: "QuantitySold");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaleRecords",
                table: "SaleRecords",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaleRecords",
                table: "SaleRecords");

            migrationBuilder.RenameTable(
                name: "SaleRecords",
                newName: "saleRecords");

            migrationBuilder.RenameColumn(
                name: "QuantitySold",
                table: "saleRecords",
                newName: "QuantityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_saleRecords",
                table: "saleRecords",
                column: "Id");
        }
    }
}
