using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teste.ScoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Phone_Ddd = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Phone_Number = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address_Complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_ZipCode = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Cpf",
                table: "Customers",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
