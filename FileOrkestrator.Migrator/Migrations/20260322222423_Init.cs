using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileOrkestrator.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "indexing_sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indexing_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "indexing_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalJobId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indexing_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_indexing_jobs_indexing_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "indexing_sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_indexing_jobs_ExternalJobId",
                table: "indexing_jobs",
                column: "ExternalJobId");

            migrationBuilder.CreateIndex(
                name: "IX_indexing_jobs_SourceId_CreatedAtUtc",
                table: "indexing_jobs",
                columns: new[] { "SourceId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_indexing_jobs_SourceId_IdempotencyKey",
                table: "indexing_jobs",
                columns: new[] { "SourceId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_indexing_sources_Name",
                table: "indexing_sources",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indexing_jobs");

            migrationBuilder.DropTable(
                name: "indexing_sources");
        }
    }
}
