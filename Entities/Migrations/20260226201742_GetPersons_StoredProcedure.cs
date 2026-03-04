using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetPersons_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            string sp_GetAllPersons = @"
            CREATE PROCEDURE [dbo].[GetAllPersons]
            As
            Begin
                Select Id, Name, Email, DateOfBirth, Gender, CountryId, Address , ReceiveNewsLetters 
                from [dbo].Persons
            End
            ";

            migrationBuilder.Sql(sp_GetAllPersons);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Countries_Name",
                table: "Countries");

            string sp_GetAllPersons = @"
                DROP PROCEDURE [dbo].[GetAllPersons]
            ";

            migrationBuilder.Sql(sp_GetAllPersons);
        }
    }
}
