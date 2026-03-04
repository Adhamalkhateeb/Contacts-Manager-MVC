using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class InsertPerson_StoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"
            CREATE Procedure InsertPerson
            @Id uniqueidentifier,
            @Name nvarchar(40),
            @Email nvarchar(200),
            @Address nvarchar(250),
            @Gender nvarchar(10),
            @ReceiveNewsLetters bit,
            @CountryId uniqueidentifier,
            @DateOfBirth datetime2(7)
            as 
            begin
                Insert into [dbo].[Persons] (Id,Name,Email,Gender,Address,ReceiveNewsLetters,CountryId,DateOfBirth)
                values
                (@Id,@Name,@Email,@Gender,@Address,@ReceiveNewsLetters,@CountryId,@DateOfBirth)

            end
            ";

            migrationBuilder.Sql(sp_InsertPerson);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"
            DROP Procedure InsertPerson
            ";

            migrationBuilder.Sql(sp_InsertPerson);
        }
    }
}
