using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MatterAccessGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matter_access_grants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MatterRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActorId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrantType = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrantedByActorId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    RevokedByActorId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    ActiveMarker = table.Column<int>(type: "int", nullable: true, computedColumnSql: "(CASE WHEN `RevokedAtUtc` IS NULL THEN 1 ELSE NULL END)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matter_access_grants", x => x.Id);
                    table.CheckConstraint("CK_matter_access_grants_GrantType", "GrantType IN ('Client', 'Staff')");
                    table.ForeignKey(
                        name: "FK_matter_access_grants_matters_MatterRecordId",
                        column: x => x.MatterRecordId,
                        principalTable: "matters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_matter_access_grants_ActorId_RevokedAtUtc",
                table: "matter_access_grants",
                columns: new[] { "ActorId", "RevokedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_matter_access_grants_MatterRecordId_ActorId_ActiveMarker",
                table: "matter_access_grants",
                columns: new[] { "MatterRecordId", "ActorId", "ActiveMarker" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matter_access_grants");
        }
    }
}
