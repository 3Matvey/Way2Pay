using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Way2Pay.CardVault.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoredCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalCustomerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PanCiphertext = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastFour = table.Column<string>(type: "char(4)", nullable: false),
                    ExpirationMonth = table.Column<int>(type: "int", nullable: false),
                    ExpirationYear = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredCards", x => x.Id);
                    table.CheckConstraint("CK_StoredCards_Expiration", "[ExpirationMonth] BETWEEN 1 AND 12 AND [ExpirationYear] BETWEEN 1 AND 9999");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoredCards_MerchantId_ExternalCustomerId",
                table: "StoredCards",
                columns: new[] { "MerchantId", "ExternalCustomerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredCards");
        }
    }
}
