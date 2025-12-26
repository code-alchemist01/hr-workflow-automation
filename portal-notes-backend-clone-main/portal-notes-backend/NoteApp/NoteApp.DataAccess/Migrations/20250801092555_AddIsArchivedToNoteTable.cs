using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToNoteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Notes");
        }
    }
}
