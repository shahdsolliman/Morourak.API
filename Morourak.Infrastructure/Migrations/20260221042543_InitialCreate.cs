using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Morourak.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CitizenRegistries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalId = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FatherFirstNameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenRegistries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress_Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress_City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress_Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailOtps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    RequestNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CitizenNationalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.RequestNumber);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleTypeAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrivingLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenseNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CitizenRegistryId = table.Column<int>(type: "int", nullable: false),
                    LicensingUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress_Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryAddress_Details = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsReplaced = table.Column<bool>(type: "bit", nullable: false),
                    IsPendingRenewal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrivingLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrivingLicenses_CitizenRegistries_CitizenRegistryId",
                        column: x => x.CitizenRegistryId,
                        principalTable: "CitizenRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrafficViolations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenRegistryId = table.Column<int>(type: "int", nullable: false),
                    RelatedLicenseId = table.Column<int>(type: "int", nullable: false),
                    LicenseType = table.Column<int>(type: "int", nullable: false),
                    ViolationType = table.Column<int>(type: "int", nullable: false),
                    ViolationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LegalReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ViolationDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FineAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsPayable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficViolations_CitizenRegistries_CitizenRegistryId",
                        column: x => x.CitizenRegistryId,
                        principalTable: "CitizenRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrivingLicenseApplication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicensingUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonalPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EducationalCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdCardPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResidenceProofPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DrivingLicenseId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CitizenRegistryId = table.Column<int>(type: "int", nullable: false),
                    DrivingTestPassed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrivingLicenseApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrivingLicenseApplication_CitizenRegistries_CitizenRegistryId",
                        column: x => x.CitizenRegistryId,
                        principalTable: "CitizenRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrivingLicenseApplication_DrivingLicenses_DrivingLicenseId",
                        column: x => x.DrivingLicenseId,
                        principalTable: "DrivingLicenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RenewalApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenRegistryId = table.Column<int>(type: "int", nullable: false),
                    DrivingLicenseId = table.Column<int>(type: "int", nullable: false),
                    CurrentCategory = table.Column<int>(type: "int", nullable: false),
                    RequestedCategory = table.Column<int>(type: "int", nullable: false),
                    MedicalCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewalApplications_CitizenRegistries_CitizenRegistryId",
                        column: x => x.CitizenRegistryId,
                        principalTable: "CitizenRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenewalApplications_DrivingLicenses_DrivingLicenseId",
                        column: x => x.DrivingLicenseId,
                        principalTable: "DrivingLicenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminationAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    CitizenNationalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewalApplicationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminationAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminationAppointments_RenewalApplications_RenewalApplicationId",
                        column: x => x.RenewalApplicationId,
                        principalTable: "RenewalApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleLicenseNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CitizenRegistryId = table.Column<int>(type: "int", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufactureYear = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReplaced = table.Column<bool>(type: "bit", nullable: false),
                    IsPendingRenewal = table.Column<bool>(type: "bit", nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChassisNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EngineNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExaminationId = table.Column<int>(type: "int", nullable: true),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress_Governorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress_City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress_Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleLicenses_CitizenRegistries_CitizenRegistryId",
                        column: x => x.CitizenRegistryId,
                        principalTable: "CitizenRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleLicenses_ExaminationAppointments_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "ExaminationAppointments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleLicenseApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenRegistryId = table.Column<int>(type: "int", nullable: false),
                    VehicleLicenseId = table.Column<int>(type: "int", nullable: true),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManufactureYear = table.Column<int>(type: "int", nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnershipProofPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleDataCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdCardPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InsuranceCertificatePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomClearancePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicalInspectionPassed = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleLicenseApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleLicenseApplications_CitizenRegistries_CitizenRegistryId",
                        column: x => x.CitizenRegistryId,
                        principalTable: "CitizenRegistries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleLicenseApplications_VehicleLicenses_VehicleLicenseId",
                        column: x => x.VehicleLicenseId,
                        principalTable: "VehicleLicenses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleViolations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleLicenseId = table.Column<int>(type: "int", nullable: false),
                    ViolationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViolationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleViolations_VehicleLicenses_VehicleLicenseId",
                        column: x => x.VehicleLicenseId,
                        principalTable: "VehicleLicenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitizenRegistries_NationalId",
                table: "CitizenRegistries",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrivingLicenseApplication_CitizenRegistryId",
                table: "DrivingLicenseApplication",
                column: "CitizenRegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_DrivingLicenseApplication_DrivingLicenseId",
                table: "DrivingLicenseApplication",
                column: "DrivingLicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_DrivingLicenses_CitizenRegistryId",
                table: "DrivingLicenses",
                column: "CitizenRegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_DrivingLicenses_LicenseNumber",
                table: "DrivingLicenses",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationAppointments_RenewalApplicationId",
                table: "ExaminationAppointments",
                column: "RenewalApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalApplications_CitizenRegistryId",
                table: "RenewalApplications",
                column: "CitizenRegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalApplications_DrivingLicenseId",
                table: "RenewalApplications",
                column: "DrivingLicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficViolations_CitizenRegistryId",
                table: "TrafficViolations",
                column: "CitizenRegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficViolations_RelatedLicenseId_LicenseType",
                table: "TrafficViolations",
                columns: new[] { "RelatedLicenseId", "LicenseType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficViolations_ViolationNumber",
                table: "TrafficViolations",
                column: "ViolationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLicenseApplications_CitizenRegistryId",
                table: "VehicleLicenseApplications",
                column: "CitizenRegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLicenseApplications_VehicleLicenseId",
                table: "VehicleLicenseApplications",
                column: "VehicleLicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLicenses_CitizenRegistryId",
                table: "VehicleLicenses",
                column: "CitizenRegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLicenses_ExaminationId",
                table: "VehicleLicenses",
                column: "ExaminationId",
                unique: true,
                filter: "[ExaminationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLicenses_VehicleLicenseNumber",
                table: "VehicleLicenses",
                column: "VehicleLicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleViolations_VehicleLicenseId",
                table: "VehicleViolations",
                column: "VehicleLicenseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryRequest");

            migrationBuilder.DropTable(
                name: "DrivingLicenseApplication");

            migrationBuilder.DropTable(
                name: "EmailOtps");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "TrafficViolations");

            migrationBuilder.DropTable(
                name: "VehicleLicenseApplications");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropTable(
                name: "VehicleViolations");

            migrationBuilder.DropTable(
                name: "VehicleLicenses");

            migrationBuilder.DropTable(
                name: "ExaminationAppointments");

            migrationBuilder.DropTable(
                name: "RenewalApplications");

            migrationBuilder.DropTable(
                name: "DrivingLicenses");

            migrationBuilder.DropTable(
                name: "CitizenRegistries");
        }
    }
}
