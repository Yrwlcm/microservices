using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class good_row_version_optional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Goods",
                newName: "ReservedQuantity");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Goods",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "Goods",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "Goods");

            migrationBuilder.RenameColumn(
                name: "ReservedQuantity",
                table: "Goods",
                newName: "Quantity");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Goods",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true);
        }
    }
}
