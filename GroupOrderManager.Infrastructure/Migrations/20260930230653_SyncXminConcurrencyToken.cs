using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroupOrderManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncXminConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: "xmin" is a PostgreSQL system column that already exists
            // on every table. This migration exists only to bring EF Core's migration
            // snapshot in sync with the model (which configures xmin as a shadow-property
            // concurrency token) — there is no real schema change to apply.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — see Up().
        }
    }
}
