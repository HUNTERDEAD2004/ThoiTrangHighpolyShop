using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppData.Migrations
{
    public partial class newdb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDon_ChiTietSanPham_IDCTSP",
                table: "ChiTietHoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDon_DanhGia_ID",
                table: "ChiTietHoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_PhuongThucThanhToan_ID",
                table: "HoaDon");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "LichSuHoaDon",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AddColumn<string>(
                name: "MaNhanvien",
                table: "LichSuHoaDon",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IDPhuongThucTT",
                table: "HoaDon",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_IDPhuongThucTT",
                table: "HoaDon",
                column: "IDPhuongThucTT");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDon_ChiTietSanPham_IDCTSP",
                table: "ChiTietHoaDon",
                column: "IDCTSP",
                principalTable: "ChiTietSanPham",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDon_DanhGia_ID",
                table: "ChiTietHoaDon",
                column: "ID",
                principalTable: "DanhGia",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_PhuongThucThanhToan_IDPhuongThucTT",
                table: "HoaDon",
                column: "IDPhuongThucTT",
                principalTable: "PhuongThucThanhToan",
                principalColumn: "IDPTTT",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDon_ChiTietSanPham_IDCTSP",
                table: "ChiTietHoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietHoaDon_DanhGia_ID",
                table: "ChiTietHoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_PhuongThucThanhToan_IDPhuongThucTT",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_IDPhuongThucTT",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "MaNhanvien",
                table: "LichSuHoaDon");

            migrationBuilder.DropColumn(
                name: "IDPhuongThucTT",
                table: "HoaDon");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "LichSuHoaDon",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDon_ChiTietSanPham_IDCTSP",
                table: "ChiTietHoaDon",
                column: "IDCTSP",
                principalTable: "ChiTietSanPham",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietHoaDon_DanhGia_ID",
                table: "ChiTietHoaDon",
                column: "ID",
                principalTable: "DanhGia",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_PhuongThucThanhToan_ID",
                table: "HoaDon",
                column: "ID",
                principalTable: "PhuongThucThanhToan",
                principalColumn: "IDPTTT",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
