using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Way2Pay.Payments.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261113155852_AddPaymentCreations : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PaymentCreations",
            columns: table => new
            {
                MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IdempotencyKey = table.Column<byte[]>(type: "varbinary(512)", nullable: false),
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                Currency = table.Column<string>(type: "char(3)", unicode: false, nullable: false),
                SourceKind = table.Column<int>(type: "int", nullable: false),
                SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentCreations", x => new { x.MerchantId, x.IdempotencyKey });
                table.ForeignKey(
                    name: "FK_PaymentCreations_Payments_PaymentId",
                    column: x => x.PaymentId,
                    principalTable: "Payments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PaymentCreations_PaymentId",
            table: "PaymentCreations",
            column: "PaymentId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PaymentCreations");
    }
}
