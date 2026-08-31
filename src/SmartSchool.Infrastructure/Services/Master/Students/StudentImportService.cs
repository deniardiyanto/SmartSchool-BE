using ClosedXML.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Common.Models;
using SmartSchool.Application.Features.Master.Students.Import.Contracts;
using SmartSchool.Application.Features.Master.Students.Import.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;
using SmartSchool.Domain.Enums;

namespace SmartSchool.Infrastructure.Services.Master.Students;
public class StudentImportService : IStudentImportService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUser;

    public StudentImportService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public async Task<ImportStudentResponse> ImportAsync(
        FileUpload file,
        string academicYear,
        CancellationToken cancellationToken = default)
    {
        //---------------------------------------------------------
        // Validate file
        //---------------------------------------------------------

        ValidateFile(file);

        if (string.IsNullOrWhiteSpace(academicYear))
        {
            throw new ArgumentException(
                "Academic Year wajib diisi.",
                nameof(academicYear));
        }

        academicYear = academicYear.Trim();

        var response = new ImportStudentResponse();

        //---------------------------------------------------------
        // Open Excel
        //---------------------------------------------------------

        using var workbook = new XLWorkbook(file.Content);

        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
            throw new InvalidOperationException(
                "Excel worksheet tidak ditemukan.");
        }

        //---------------------------------------------------------
        // Get rows
        //---------------------------------------------------------

        var rows = worksheet
            .RowsUsed()
            .Skip(1)
            .ToList();

        response.TotalRows = rows.Count;

        if (rows.Count == 0)
        {
            return response;
        }

        //---------------------------------------------------------
        // Load existing NIS
        //---------------------------------------------------------

        var existingNISNumbers =
            await _context.Students
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => x.NIS)
                .ToListAsync(cancellationToken);

        var existingNISLookup =
            new HashSet<string>(
                existingNISNumbers
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Load existing NISN
        //---------------------------------------------------------

        var existingNISNs =
    await _context.Students
        .AsNoTracking()
        .Where(x =>
            !x.IsDeleted &&
            !string.IsNullOrWhiteSpace(x.NISN))
        .Select(x => x.NISN!)
        .ToListAsync(cancellationToken);

var existingNISNLookup =
    existingNISNs
        .Select(x => x.Trim())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Load classrooms for selected academic year
        //---------------------------------------------------------

        var classrooms =
            await _context.ClassRooms
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.AcademicYear == academicYear)
                .Select(x => new
                {
                    x.Id,
                    x.Code
                })
                .ToListAsync(cancellationToken);

        var classroomLookup =
            classrooms
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .ToDictionary(
                    x => x.Code.Trim(),
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Load guardians
        // Temporary: lookup using PhoneNumber
        //---------------------------------------------------------

        var guardians = await _context.Guardians
    .AsNoTracking()
    .Where(x =>
        !x.IsDeleted &&
        x.IsActive)
    .Select(x => new
    {
        x.Id,
        x.GuardianCode
    })
    .ToListAsync(cancellationToken);

      var guardianLookup = guardians
    .Where(x =>
        !string.IsNullOrWhiteSpace(x.GuardianCode))
    .ToDictionary(
        x => x.GuardianCode.Trim(),
        x => x.Id,
        StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Track duplicate Excel
        //---------------------------------------------------------

        var excelNISLookup =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
        var excelNISNLookup =
    new HashSet<string>(
        StringComparer.OrdinalIgnoreCase);

        var entities =
            new List<Student>();

        //---------------------------------------------------------
        // Validate rows
        //---------------------------------------------------------

        foreach (var row in rows)
        {
            var rowNumber = row.RowNumber();

            var result = new ImportStudentRowResult
            {
                RowNumber = rowNumber
            };

            try
            {
                // DEBUG: cek isi setiap kolom Excel
        for (int i = 1; i <= 15; i++)
        {
            Console.WriteLine(
                $"Row {rowNumber} - Cell {i}: '{row.Cell(i).GetString()}'");
        }
                //-------------------------------------------------
                // Read Excel
                //-------------------------------------------------

                var nis =
    GetString(row.Cell(1));

var nisn =
    GetString(row.Cell(2));

var fullName =
    GetString(row.Cell(3));

var gender =
    GetString(row.Cell(4));

var birthPlace =
    GetString(row.Cell(5));

var birthdate =
    GetString(row.Cell(6));

var address =
    GetString(row.Cell(7));

var photoUrl =
    GetString(row.Cell(8));

var classRoomCode =
    GetString(row.Cell(9));

var guardianCode =
    GetString(row.Cell(10));

var status =
    GetString(row.Cell(11));

var enrollmentdate =
    GetString(row.Cell(12));
              

                result.FullName = fullName;

                //-------------------------------------------------
                // Required validation
                //-------------------------------------------------

                if (string.IsNullOrWhiteSpace(nis))
                {
                    AddFailedResult(
                        response,
                        result,
                        "NIS wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Nama siswa wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(gender))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Gender wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(classRoomCode))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Classroom Code wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(status))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Status wajib diisi.");

                    continue;
                }

                //-------------------------------------------------
                // NIS validation
                //-------------------------------------------------

                nis = nis.Trim();

                if (existingNISLookup.Contains(nis))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"NIS '{nis}' sudah ada.");

                    continue;
                }

                if (!excelNISLookup.Add(nis))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"NIS '{nis}' duplicate di Excel.");

                    continue;
                }

                 //-------------------------------------------------
                // NISN validation
                //-------------------------------------------------

                nisn = nisn.Trim();

                if (existingNISNLookup.Contains(nisn))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"NISN '{nisn}' sudah ada.");

                    continue;
                }

                if (!excelNISNLookup.Add(nisn))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"NISN '{nisn}' duplicate di Excel.");

                    continue;
                }

                //-------------------------------------------------
                // Gender
                //-------------------------------------------------

                if (!TryParseGender(
                        gender,
                        out var genderEnum))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Gender '{gender}' tidak valid.");

                    continue;
                }

                //-------------------------------------------------
                // Status
                //-------------------------------------------------

                if (!TryParseStudentStatus(
                        status,
                        out var statusEnum))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Status '{status}' tidak valid.");

                    continue;
                }

                //-------------------------------------------------
                // Birth Date
                //-------------------------------------------------

                if (!TryGetDateTime(
                        row.Cell(6),
                        out var birthDate))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Birth Date tidak valid.");

                    continue;
                }

                //-------------------------------------------------
                // Enrollment Date
                //-------------------------------------------------

                if (!TryGetDateTime(
                        row.Cell(12),
                        out var enrollmentDate))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Enrollment Date tidak valid.");

                    continue;
                }

                //-------------------------------------------------
                // Classroom lookup
                //-------------------------------------------------

                classRoomCode = classRoomCode.Trim();

                if (!classroomLookup.TryGetValue(
                        classRoomCode,
                        out var classRoomId))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Classroom dengan code '{classRoomCode}' " +
                        $"tidak ditemukan untuk Academic Year '{academicYear}'.");

                    continue;
                }

                //-------------------------------------------------
                // Guardian lookup
                // Temporary using guardianCode
                //-------------------------------------------------

                guardianCode = guardianCode.Trim();

               if (string.IsNullOrWhiteSpace(guardianCode))
{
    result.Success = false;
    result.Message =
        "Guardian Code wajib diisi.";

    response.Results.Add(result);
    continue;
}

if (!guardianLookup.TryGetValue(
        guardianCode,
        out var guardianId))
{
    result.Success = false;
    result.Message =
        $"Guardian dengan code '{guardianCode}' tidak ditemukan.";

    response.Results.Add(result);
    continue;
}
                //-------------------------------------------------
                // Create entity
                //-------------------------------------------------

                var now = _dateTimeProvider.UtcNow;

                var entity = new Student
                {
                    Id = Guid.NewGuid(),

                    NIS = nis,

                    NISN = string.IsNullOrWhiteSpace(nisn)
                        ? null
                        : nisn,

                    FullName = fullName,

                    Gender = genderEnum,

                    BirthPlace = string.IsNullOrWhiteSpace(birthPlace)
                        ? null
                        : birthPlace,

                    BirthDate = birthDate,

                    Address = string.IsNullOrWhiteSpace(address)
                        ? null
                        : address,

                    PhotoUrl = string.IsNullOrWhiteSpace(photoUrl)
                        ? null
                        : photoUrl,

                    ClassRoomId = classRoomId,

                    GuardianId = guardianId,

                    Status = statusEnum,

                    EnrollmentDate = enrollmentDate,

                    CreatedAt = now,

                    CreatedBy = _currentUser.UserId,

                    IsActive = true,

                    IsDeleted = false
                };

                entities.Add(entity);

                //-------------------------------------------------
                // Result
                //-------------------------------------------------

                result.Success = true;

                result.StudentId = entity.Id;

                result.Message = "Student siap diimport.";

                response.Results.Add(result);
            }
            catch (Exception ex)
            {
                result.Success = false;

                result.Message =
                    $"Gagal membaca row: {ex.Message}";

                response.Results.Add(result);
            }
        }

        //---------------------------------------------------------
        // Stop if validation failed
        //---------------------------------------------------------

        if (response.Results.Any(x => !x.Success))
        {
            response.SuccessRows = 0;

            response.FailedRows =
                response.Results.Count(x => !x.Success);

            return response;
        }

        //---------------------------------------------------------
        // Insert all
        //---------------------------------------------------------

        if (entities.Count > 0)
        {
            _context.Students.AddRange(entities);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        //---------------------------------------------------------
        // Summary
        //---------------------------------------------------------

        response.SuccessRows =
            response.Results.Count(x => x.Success);

        response.FailedRows =
            response.Results.Count(x => !x.Success);

        return response;
    }

    //=============================================================
    // Validate File
    //=============================================================

    private static void ValidateFile(FileUpload file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(
                nameof(file),
                "File Excel wajib diupload.");
        }

        if (file.Content == null)
        {
            throw new InvalidOperationException(
                "Content file tidak tersedia.");
        }

        if (file.Content == Stream.Null)
        {
            throw new InvalidOperationException(
                "Content file tidak tersedia.");
        }

        if (file.Content.Length == 0)
        {
            throw new InvalidOperationException(
                "File Excel kosong.");
        }

        var extension =
            Path.GetExtension(file.FileName);

        if (!string.Equals(
                extension,
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "File harus berformat .xlsx.");
        }
    }

    //=============================================================
    // Get String
    //=============================================================

    private static string GetString(
        IXLCell cell)
    {
        return cell
            .GetString()
            .Trim();
    }

    //=============================================================
    // Parse Gender
    //=============================================================

    private static bool TryParseGender(
        string value,
        out Gender gender)
    {
        gender = default;

        switch (value.Trim().ToLowerInvariant())
        {
            case "laki-laki":
            case "laki":
            case "pria":
            case "male":

                gender = Gender.Male;
                return true;

            case "perempuan":
            case "wanita":
            case "female":

                gender = Gender.Female;
                return true;

            default:
                return false;
        }
    }

    //=============================================================
    // Parse Student Status
    //=============================================================

    private static bool TryParseStudentStatus(
        string value,
        out StudentStatus status)
    {
        status = default;

        switch (value.Trim().ToLowerInvariant())
        {
            case "aktif":
            case "active":

                status = StudentStatus.Active;
                return true;

            case "tidak aktif":
            case "inactive":

                status = StudentStatus.Inactive;
                return true;

            default:
                return false;
        }
    }

    //=============================================================
    // Parse Date
    //=============================================================

    private static bool TryGetDateTime(
        IXLCell cell,
        out DateTime value)
    {
        value = default;

        if (cell.IsEmpty())
        {
            return false;
        }

        if (cell.TryGetValue<DateTime>(
                out var date))
        {
            value = DateTime.SpecifyKind(
                date,
                DateTimeKind.Utc);

            return true;
        }

        var text =
            cell.GetString().Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (DateTime.TryParse(
                text,
                out date))
        {
            value = DateTime.SpecifyKind(
                date,
                DateTimeKind.Utc);

            return true;
        }

        return false;
    }

    //=============================================================
    // Add Failed Result
    //=============================================================

    private static void AddFailedResult(
        ImportStudentResponse response,
        ImportStudentRowResult result,
        string message)
    {
        result.Success = false;
        result.Message = message;

        response.Results.Add(result);
    }
}