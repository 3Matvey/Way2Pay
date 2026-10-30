using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Way2Pay.Merchants.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                    table.CheckConstraint("CK_Merchants_Status", "[Status] IN (0, 1)");
                });

            migrationBuilder.CreateTable(
                name: "MerchantApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SecretHash = table.Column<string>(type: "char(64)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantApiKeys", x => x.Id);
                    table.CheckConstraint("CK_MerchantApiKeys_Expiration", "[ExpiresAt] IS NULL OR [ExpiresAt] > [CreatedAt]");
                    table.CheckConstraint("CK_MerchantApiKeys_Revocation", "[RevokedAt] IS NULL OR [RevokedAt] >= [CreatedAt]");
                    table.ForeignKey(
                        name: "FK_MerchantApiKeys_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantApiKeys_MerchantId",
                table: "MerchantApiKeys",
                column: "MerchantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantApiKeys");

            migrationBuilder.DropTable(
                name: "Merchants");
        }
    }
}
