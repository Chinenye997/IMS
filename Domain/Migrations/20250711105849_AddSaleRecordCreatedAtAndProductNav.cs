using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleRecordCreatedAtAndProductNav : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "SaleRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SaleRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_SaleRecords_ProductId",
                table: "SaleRecords",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleRecords_Products_ProductId",
                table: "SaleRecords",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleRecords_Products_ProductId",
                table: "SaleRecords");

            migrationBuilder.DropIndex(
                name: "IX_SaleRecords_ProductId",
                table: "SaleRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SaleRecords");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "SaleRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
