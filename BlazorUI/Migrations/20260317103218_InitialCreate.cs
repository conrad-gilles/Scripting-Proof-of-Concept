using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorUI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_scripts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    script_name = table.Column<string>(type: "text", nullable: true),
                    script_type = table.Column<string>(type: "text", nullable: true),
                    source_code = table.Column<string>(type: "text", nullable: false),
                    min_api_version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_scripts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ember_instances",
                columns: table => new
                {
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_name = table.Column<string>(type: "text", nullable: true),
                    ember_version = table.Column<string>(type: "text", nullable: true),
                    sdk_version = table.Column<int>(type: "integer", nullable: false),
                    last_heartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hostname = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ember_instances", x => x.instance_id);
                });

            migrationBuilder.CreateTable(
                name: "script_compiled_cache",
                columns: table => new
                {
                    script_id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_version = table.Column<int>(type: "integer", nullable: false),
                    assembly_bytes = table.Column<byte[]>(type: "bytea", nullable: true),
                    compilation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    compilation_success = table.Column<bool>(type: "boolean", nullable: false),
                    copilation_errors = table.Column<string>(type: "text", nullable: true),
                    old_source_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_script_compiled_cache", x => new { x.script_id, x.api_version });
                    table.ForeignKey(
                        name: "FK_script_compiled_cache_customer_scripts_script_id",
                        column: x => x.script_id,
                        principalTable: "customer_scripts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ember_instances");

            migrationBuilder.DropTable(
                name: "script_compiled_cache");

            migrationBuilder.DropTable(
                name: "customer_scripts");
        }
    }
}
