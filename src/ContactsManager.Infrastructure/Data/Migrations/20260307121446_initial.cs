using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactsManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(40)",
                        maxLength: 40,
                        nullable: false
                    ),
                    Gender = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(
                        type: "nvarchar(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    Email = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiveNewsLetters = table.Column<bool>(type: "bit", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.CheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");
                    table.ForeignKey(
                        name: "FK_Persons_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Persons_CountryId",
                table: "Persons",
                column: "CountryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Email",
                table: "Persons",
                column: "Email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Persons");

            migrationBuilder.DropTable(name: "Countries");
        }
    }
}
