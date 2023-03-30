using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace car_db.Migrations.ApplicationDb
{
    public partial class app01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Car",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameNormal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeAdd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Car", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CarId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeAdd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCar_Car_CarId",
                        column: x => x.CarId,
                        principalTable: "Car",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCar_CarId",
                table: "UserCar",
                column: "CarId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCar");

            migrationBuilder.DropTable(
                name: "Car");
        }
    }
}
