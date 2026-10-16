using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Way2Pay.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, nullable: false),
                    AuthorizedAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CapturedAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_Payments_Balance", "[AuthorizedAmount] >= 0 AND [CapturedAmount] >= 0 AND [RefundedAmount] >= 0 AND [RefundedAmount] <= [CapturedAmount]");
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodReferences",
                columns: table => new
                {
                    ProviderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "PaymentOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OriginalProviderTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOperations", x => x.Id);
                    table.CheckConstraint("CK_PaymentOperations_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_PaymentOperations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOperationRoutes",
                columns: table => new
                {
                    PaymentOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigurationVersion = table.Column<long>(type: "bigint", nullable: false),
                    PolicyVersion = table.Column<long>(type: "bigint", nullable: true),
                    MatchedRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOperationRoutes", x => x.PaymentOperationId);
                    table.CheckConstraint("CK_PaymentOperationRoutes_Versions", "[ConfigurationVersion] > 0 AND ([PolicyVersion] IS NULL OR [PolicyVersion] > 0)");
                    table.ForeignKey(
                        name: "FK_PaymentOperationRoutes_PaymentOperations_PaymentOperationId",
                        column: x => x.PaymentOperationId,
                        principalTable: "PaymentOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOperationRouteSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ProviderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOperationRouteSteps", x => x.Id);
                    table.UniqueConstraint("AK_PaymentOperationRouteSteps_Id_PaymentOperationId", x => new { x.Id, x.PaymentOperationId });
                    table.CheckConstraint("CK_PaymentOperationRouteSteps_Position", "[Position] > 0");
                    table.ForeignKey(
                        name: "FK_PaymentOperationRouteSteps_PaymentOperationRoutes_PaymentOperationId",
                        column: x => x.PaymentOperationId,
                        principalTable: "PaymentOperationRoutes",
                        principalColumn: "PaymentOperationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAttempts", x => x.Id);
                    table.CheckConstraint("CK_PaymentAttempts_Number", "[Number] > 0");
                    table.ForeignKey(
                        name: "FK_PaymentAttempts_PaymentOperationRouteSteps_RouteStepId_PaymentOperationId",
                        columns: x => new { x.RouteStepId, x.PaymentOperationId },
                        principalTable: "PaymentOperationRouteSteps",
                        principalColumns: new[] { "Id", "PaymentOperationId" });
                    table.ForeignKey(
                        name: "FK_PaymentAttempts_PaymentOperations_PaymentOperationId",
                        column: x => x.PaymentOperationId,
                        principalTable: "PaymentOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_PaymentOperationId_Number",
                table: "PaymentAttempts",
                columns: new[] { "PaymentOperationId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_RouteStepId_PaymentOperationId",
                table: "PaymentAttempts",
                columns: new[] { "RouteStepId", "PaymentOperationId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOperationRouteSteps_PaymentOperationId_Position",
                table: "PaymentOperationRouteSteps",
                columns: new[] { "PaymentOperationId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOperationRouteSteps_PaymentOperationId_ProviderAccountId",
                table: "PaymentOperationRouteSteps",
                columns: new[] { "PaymentOperationId", "ProviderAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOperations_PaymentId_CreatedAt",
                table: "PaymentOperations",
                columns: new[] { "PaymentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_PaymentOperations_ActivePayment",
                table: "PaymentOperations",
                column: "PaymentId",
                unique: true,
                filter: "[Status] IN (0, 1, 4)");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MerchantId_CreatedAt",
                table: "Payments",
                columns: new[] { "MerchantId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentAttempts");

            migrationBuilder.DropTable(
                name: "PaymentMethodReferences");

            migrationBuilder.DropTable(
                name: "PaymentOperationRouteSteps");

            migrationBuilder.DropTable(
                name: "PaymentOperationRoutes");

            migrationBuilder.DropTable(
                name: "PaymentOperations");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
