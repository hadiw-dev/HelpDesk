using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaxFileUploadSizeMb = table.Column<int>(type: "int", nullable: false),
                    AllowedFileExtensions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DefaultPageSize = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "AllowedFileExtensions", "CreatedAt", "CreatedBy", "DefaultPageSize", "DeletedAt", "DeletedBy", "IsDeleted", "MaxFileUploadSizeMb", "SiteName", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("f0000000-0000-0000-0000-000000000001"), ".pdf,.png,.jpg,.jpeg,.docx,.xlsx,.txt,.zip", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 20, null, null, false, 10, "HelpDesk System", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_IsDeleted",
                table: "SystemSettings",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
