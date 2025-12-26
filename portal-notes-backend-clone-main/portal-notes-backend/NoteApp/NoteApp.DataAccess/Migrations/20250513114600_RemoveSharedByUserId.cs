using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSharedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteShares_Users_SharedByUserId",
                table: "NoteShares");

            migrationBuilder.DropIndex(
                name: "IX_NoteShares_SharedByUserId",
                table: "NoteShares");

            migrationBuilder.DropColumn(
                name: "SharedByUserId",
                table: "NoteShares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SharedByUserId",
                table: "NoteShares",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NoteShares_SharedByUserId",
                table: "NoteShares",
                column: "SharedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteShares_Users_SharedByUserId",
                table: "NoteShares",
                column: "SharedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
