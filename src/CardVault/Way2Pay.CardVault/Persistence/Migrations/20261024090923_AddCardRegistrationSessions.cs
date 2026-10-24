using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Way2Pay.CardVault.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCardRegistrationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardRegistrationSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalCustomerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TokenHash = table.Column<string>(type: "char(64)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StoredCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardRegistrationSessions", x => x.Id);
                    table.CheckConstraint("CK_CardRegistrationSessions_Expiration", "[ExpiresAt] > [CreatedAt]");
                    table.CheckConstraint("CK_CardRegistrationSessions_Result", "([Status] = 1 AND [StoredCardId] IS NOT NULL) OR ([Status] IN (0, 2) AND [StoredCardId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_CardRegistrationSessions_StoredCards_StoredCardId",
                        column: x => x.StoredCardId,
                        principalTable: "StoredCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardRegistrationSessions_ExpiresAt",
                table: "CardRegistrationSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CardRegistrationSessions_StoredCardId",
                table: "CardRegistrationSessions",
                column: "StoredCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardRegistrationSessions_TokenHash",
                table: "CardRegistrationSessions",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardRegistrationSessions");
        }
    }
}
