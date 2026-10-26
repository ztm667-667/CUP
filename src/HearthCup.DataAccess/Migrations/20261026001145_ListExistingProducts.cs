using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HearthCup.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ListExistingProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The column was introduced with no delisted products in existence, so every existing row
            // is a listed one. Fresh databases have no rows here and this statement is a no-op.
            migrationBuilder.Sql("UPDATE [Products] SET [IsListed] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: dropping the column removes the value entirely.
        }
    }
}
