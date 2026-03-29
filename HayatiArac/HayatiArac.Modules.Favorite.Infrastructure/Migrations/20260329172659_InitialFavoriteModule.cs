using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HayatiArac.Modules.Favorite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFavoriteModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "favorites");

            migrationBuilder.CreateTable(
                name: "AdvertSnapshots",
                schema: "favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvertId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedAdverts",
                schema: "favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvertId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedAdverts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertSnapshots_AdvertId",
                schema: "favorites",
                table: "AdvertSnapshots",
                column: "AdvertId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedAdverts_UserId",
                schema: "favorites",
                table: "SavedAdverts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedAdverts_UserId_AdvertId",
                schema: "favorites",
                table: "SavedAdverts",
                columns: new[] { "UserId", "AdvertId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvertSnapshots",
                schema: "favorites");

            migrationBuilder.DropTable(
                name: "SavedAdverts",
                schema: "favorites");
        }
    }
}
