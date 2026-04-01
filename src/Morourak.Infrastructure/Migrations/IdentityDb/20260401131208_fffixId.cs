using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Morourak.Infrastructure.Migrations.IdentityDb
{
    /// <inheritdoc />
    public partial class fffixId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationCodeExpiry",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OtpAttempts",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }
    }
}
