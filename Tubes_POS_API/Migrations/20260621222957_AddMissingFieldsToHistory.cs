using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tubes_POS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "TransactionHistories",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionHistories_TransactionDate",
                table: "TransactionHistories",
                newName: "IX_TransactionHistories_CreatedAt");

            migrationBuilder.AddColumn<decimal>(
                name: "Change",
                table: "TransactionHistories",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "TransactionHistories",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "TransactionHistories",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TableNumber",
                table: "TransactionHistories",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionCode",
                table: "TransactionHistories",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Change",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "TransactionHistories");

            migrationBuilder.DropColumn(
                name: "TransactionCode",
                table: "TransactionHistories");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TransactionHistories",
                newName: "TransactionDate");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionHistories_CreatedAt",
                table: "TransactionHistories",
                newName: "IX_TransactionHistories_TransactionDate");
        }
    }
}
