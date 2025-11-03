using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedCouponModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Coupon",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Coupon",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "DiscountPercent",
                table: "Coupon",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Coupon",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCumulative",
                table: "Coupon",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MaxDiscountAmount",
                table: "Coupon",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinOrderAmount",
                table: "Coupon",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Coupon",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Code", "Description", "DiscountPercent", "DiscountType", "EndDate", "IsActive", "IsCumulative", "MaxDiscountAmount", "MinOrderAmount", "StartDate" },
                values: new object[] { "Electronics", "IPHONE150", "IPhone X Discount", null, "Fixed", null, true, false, null, null, null });

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Code", "Description", "DiscountPercent", "DiscountType", "EndDate", "IsActive", "IsCumulative", "MaxDiscountAmount", "MinOrderAmount", "StartDate" },
                values: new object[] { "Electronics", "SAMSUNG100", "Samsung 10 Discount", null, "Fixed", null, true, false, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "IsCumulative",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "MinOrderAmount",
                table: "Coupon");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Coupon");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Coupon",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "IPhone X New");

            migrationBuilder.UpdateData(
                table: "Coupon",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Samsung 10 New");
        }
    }
}
