using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftsLoggerV2.RyanW84.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Town",
                table: "Locations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PostCode",
                table: "Locations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.CreateIndex(name: "IX_Workers_Name", table: "Workers", column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EndTime",
                table: "Shifts",
                column: "EndTime"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_LocationId_StartTime",
                table: "Shifts",
                columns: new[] { "LocationId", "StartTime" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StartTime",
                table: "Shifts",
                column: "StartTime"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_WorkerId_StartTime",
                table: "Shifts",
                columns: new[] { "WorkerId", "StartTime" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Locations_PostCode",
                table: "Locations",
                column: "PostCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Town",
                table: "Locations",
                column: "Town"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Workers_Name", table: "Workers");

            migrationBuilder.DropIndex(name: "IX_Shifts_EndTime", table: "Shifts");

            migrationBuilder.DropIndex(name: "IX_Shifts_LocationId_StartTime", table: "Shifts");

            migrationBuilder.DropIndex(name: "IX_Shifts_StartTime", table: "Shifts");

            migrationBuilder.DropIndex(name: "IX_Shifts_WorkerId_StartTime", table: "Shifts");

            migrationBuilder.DropIndex(name: "IX_Locations_Name", table: "Locations");

            migrationBuilder.DropIndex(name: "IX_Locations_PostCode", table: "Locations");

            migrationBuilder.DropIndex(name: "IX_Locations_Town", table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Town",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "PostCode",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );
        }
    }
}
