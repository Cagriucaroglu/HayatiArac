using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HayatiArac.Modules.User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneVerificationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAt",
                schema: "users",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneVerified",
                schema: "users",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneVerifiedAt",
                schema: "users",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                schema: "users",
                table: "Users",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAt",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPhoneVerified",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneVerifiedAt",
                schema: "users",
                table: "Users");
        }
    }
}
