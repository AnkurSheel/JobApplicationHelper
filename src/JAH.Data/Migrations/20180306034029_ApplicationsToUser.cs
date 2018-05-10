using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace JAH.Data.Migrations
{
    public partial class ApplicationsToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "JobApplications",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_OwnerId",
                table: "JobApplications",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_AspNetUsers_OwnerId",
                table: "JobApplications",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_AspNetUsers_OwnerId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_OwnerId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "JobApplications");
        }
    }
}
