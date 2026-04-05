using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InternshipManager.Api.Migrations.Supervisor
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupervisorApplications",
                columns: table => new
                {
                    IdSupervisorApplication = table.Column<Guid>(type: "uuid", nullable: false),
                    IdEmployee = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IdSpecialization = table.Column<int>(type: "integer", nullable: false),
                    IdDepartment = table.Column<int>(type: "integer", nullable: false),
                    IdAddress = table.Column<int>(type: "integer", nullable: false),
                    IdScheduledPractice = table.Column<int>(type: "integer", nullable: true),
                    RequestedStudentsCount = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PracticeFormat = table.Column<int>(type: "integer", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorApplications", x => x.IdSupervisorApplication);
                });

            migrationBuilder.CreateTable(
                name: "SupervisorReviews",
                columns: table => new
                {
                    IdEmployee = table.Column<Guid>(type: "uuid", nullable: false),
                    IdStudentApplication = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendedForEmployment = table.Column<bool>(type: "boolean", nullable: false),
                    PvScore = table.Column<int>(type: "integer", nullable: false),
                    SkillsScore = table.Column<int>(type: "integer", nullable: false),
                    IndependenceScore = table.Column<int>(type: "integer", nullable: false),
                    TeamworkScore = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorReviews", x => new { x.IdEmployee, x.IdStudentApplication });
                });

            migrationBuilder.CreateTable(
                name: "TimeIntervals",
                columns: table => new
                {
                    IdInterval = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEmployee = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCreator = table.Column<Guid>(type: "uuid", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxCount = table.Column<int>(type: "integer", nullable: false),
                    BreakDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeIntervals", x => x.IdInterval);
                });

            migrationBuilder.CreateTable(
                name: "StudentSupervisorApplications",
                columns: table => new
                {
                    IdSupervisorApplication = table.Column<Guid>(type: "uuid", nullable: false),
                    IdStudentApplication = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSupervisorApplications", x => new { x.IdSupervisorApplication, x.IdStudentApplication });
                    table.ForeignKey(
                        name: "FK_StudentSupervisorApplications_SupervisorApplications_IdSupe~",
                        column: x => x.IdSupervisorApplication,
                        principalTable: "SupervisorApplications",
                        principalColumn: "IdSupervisorApplication",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterviewSlots",
                columns: table => new
                {
                    IdInterviewSlot = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEmployee = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCreator = table.Column<Guid>(type: "uuid", nullable: true),
                    IdInterval = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MeetingPlace = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSlots", x => x.IdInterviewSlot);
                    table.ForeignKey(
                        name: "FK_InterviewSlots_TimeIntervals_IdInterval",
                        column: x => x.IdInterval,
                        principalTable: "TimeIntervals",
                        principalColumn: "IdInterval",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    IdInterviewSlot = table.Column<int>(type: "integer", nullable: false),
                    InterviewType = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<bool>(type: "boolean", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.IdInterviewSlot);
                    table.ForeignKey(
                        name: "FK_Interviews_InterviewSlots_IdInterviewSlot",
                        column: x => x.IdInterviewSlot,
                        principalTable: "InterviewSlots",
                        principalColumn: "IdInterviewSlot",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_IdInterval",
                table: "InterviewSlots",
                column: "IdInterval");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_StartTime",
                table: "InterviewSlots",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSlots_Status",
                table: "InterviewSlots",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorApplications_IdEmployee",
                table: "SupervisorApplications",
                column: "IdEmployee");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorApplications_Status",
                table: "SupervisorApplications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "StudentSupervisorApplications");

            migrationBuilder.DropTable(
                name: "SupervisorReviews");

            migrationBuilder.DropTable(
                name: "InterviewSlots");

            migrationBuilder.DropTable(
                name: "SupervisorApplications");

            migrationBuilder.DropTable(
                name: "TimeIntervals");
        }
    }
}
