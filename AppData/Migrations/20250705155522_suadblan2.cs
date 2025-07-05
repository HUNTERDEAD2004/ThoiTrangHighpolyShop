using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppData.Migrations
{
    public partial class suadblan2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhDaiDien",
                table: "SanPham",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserVoucher",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IDKhachHang = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IDVoucher = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DaSuDung = table.Column<bool>(type: "bit", nullable: false),
                    NgaySuDung = table.Column<DateTime>(type: "Date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVoucher", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserVoucher_KhachHang_IDKhachHang",
                        column: x => x.IDKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "IDKhachHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVoucher_Voucher_IDVoucher",
                        column: x => x.IDVoucher,
                        principalTable: "Voucher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_IDKhachHang",
                table: "UserVoucher",
                column: "IDKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_IDVoucher",
                table: "UserVoucher",
                column: "IDVoucher");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVoucher");

            migrationBuilder.DropColumn(
                name: "AnhDaiDien",
                table: "SanPham");
        }
    }
}
