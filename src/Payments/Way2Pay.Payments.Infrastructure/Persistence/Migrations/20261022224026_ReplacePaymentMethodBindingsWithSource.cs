using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Way2Pay.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePaymentMethodBindingsWithSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethodReferences");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodSourceId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodSourceKind",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethodSourceId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethodSourceKind",
                table: "Payments");

            migrationBuilder.CreateTable(
                name: "PaymentMethodReferences",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodReferences", x => new { x.PaymentId, x.ProviderAccountId });
                    table.ForeignKey(
                        name: "FK_PaymentMethodReferences_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
