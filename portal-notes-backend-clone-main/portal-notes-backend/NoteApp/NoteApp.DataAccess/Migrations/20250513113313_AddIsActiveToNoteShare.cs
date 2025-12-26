using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToNoteShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "NoteShares",
                newName: "SharedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "NoteShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "NoteShares");

            migrationBuilder.RenameColumn(
                name: "SharedAt",
                table: "NoteShares",
                newName: "CreatedAt");
        }
    }
}
