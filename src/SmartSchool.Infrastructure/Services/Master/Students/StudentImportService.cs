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
        CancellationToken cancellationToken = default)
    {
        //---------------------------------------------------------
        // Validate file
        //---------------------------------------------------------

        ValidateFile(file);

        var response = new ImportStudentResponse();

        //---------------------------------------------------------
        // Open Excel
        //---------------------------------------------------------

        using var workbook =
            new XLWorkbook(file.Content);

        var worksheet =
            workbook.Worksheets.FirstOrDefault();

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
        // Load existing students from database
        //---------------------------------------------------------

        var existingFullNames =
            await _context.Students
                .Where(x => !x.IsDeleted)
                .Select(x => x.FullName)
                .ToListAsync(cancellationToken);

        var existingFullNameLookup =
            new HashSet<string>(
                existingFullNames
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Select(NormalizeFullName),
                StringComparer.OrdinalIgnoreCase);

        var existingNISNumbers =
      await _context.Students
          .Where(x => !x.IsDeleted)
          .Select(x => x.NIS)
          .ToListAsync(cancellationToken);

        var existingNISLookup =
            new HashSet<string>(
                existingNISNumbers
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Track duplicate Excel
        //---------------------------------------------------------

        var excelFullNameLookup =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var excelNISLookup =
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

            var result =
                new ImportStudentRowResult
                {
                    RowNumber = rowNumber
                };

            try
            {
                //-------------------------------------------------
                // Read Excel
                //-------------------------------------------------

                var nis =
    row.Cell(1)
        .GetString()
        .Trim();

                var nisn =
                    row.Cell(2)
                        .GetString()
                        .Trim();

                var fullName =
                    row.Cell(3)
                        .GetString()
                        .Trim();

                var gender =
                    row.Cell(4)
                        .GetString()
                        .Trim();

                var birthPlace =
                    row.Cell(5)
                        .GetString()
                        .Trim();
                var birthDate =
                    row.Cell(6)
                        .GetString()
                        .Trim();
                var address =
                    row.Cell(7)
                        .GetString()
                        .Trim();
                var photoUrl =
                    row.Cell(8)
                        .GetString()
                        .Trim();
                var classRoomCode =
                    row.Cell(9)
                        .GetString()
                        .Trim();

                var guardianPhone =
                    row.Cell(10)
                        .GetString()
                        .Trim();
                var status =
                    row.Cell(11)
                        .GetString()
                        .Trim();
                var enrollmentDate =
                    row.Cell(12)
                        .GetString()
                        .Trim();

                result.FullName = fullName;

                if (string.IsNullOrWhiteSpace(nis))
                {
                    result.Success = false;
                    result.Message = "NIS wajib diisi.";
                    response.Results.Add(result);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    result.Success = false;
                    result.Message = "Name Siswa wajib diisi.";
                    response.Results.Add(result);
                    continue;
                }


                if (string.IsNullOrWhiteSpace(gender))
                {
                    result.Success = false;
                    result.Message = "Gender wajib diisi.";
                    response.Results.Add(result);
                    continue;
                }

                if (existingNISLookup.Contains(nis))
                {
                    result.Success = false;
                    result.Message = $"NIS '{nis}' sudah ada.";
                    response.Results.Add(result);
                    continue;
                }

                if (!excelNISLookup.Add(nis))
                {
                    result.Success = false;
                    result.Message = $"NIS '{nis}' duplicate di Excel.";
                    response.Results.Add(result);
                    continue;
                }

                Gender genderEnum;

                switch (gender.Trim().ToLowerInvariant())
                {
                    case "laki-laki":
                    case "laki":
                    case "pria":
                        genderEnum = Gender.Male;
                        break;

                    case "perempuan":
                    case "wanita":
                        genderEnum = Gender.Female;
                        break;

                    default:
                        result.Success = false;
                        result.Message = $"Gender '{gender}' tidak valid.";
                        response.Results.Add(result);
                        continue;
                }

                StudentStatus statusEnum;

                switch (status.Trim().ToLowerInvariant())
                {
                    case "aktif":
                    case "active":
                        statusEnum = StudentStatus.Active;
                        break;

                    case "tidak aktif":
                    case "inactive":
                        statusEnum = StudentStatus.Inactive;
                        break;

                    default:
                        result.Success = false;
                        result.Message = $"Status '{status}' tidak valid.";
                        response.Results.Add(result);
                        continue;
                }

                //-------------------------------------------------
                // Normalize fullname
                //-------------------------------------------------

                var normalizedFullName =
                    NormalizeFullName(fullName);

                //-------------------------------------------------
                // Check database duplicate
                //-------------------------------------------------

                if (existingFullNameLookup.Contains(
                        normalizedFullName))
                {
                    result.Success = false;

                    result.Message =
                        $"Student '{normalizedFullName}' sudah ada.";

                    response.Results.Add(result);

                    continue;
                }

                //-------------------------------------------------
                // Check Excel duplicate
                //-------------------------------------------------

                if (!excelFullNameLookup.Add(
                        normalizedFullName))
                {
                    result.Success = false;

                    result.Message =
                        $"Student '{normalizedFullName}' duplicate di Excel.";

                    response.Results.Add(result);

                    continue;
                }


                var classrooms = await _context.ClassRooms
                    .Where(x => !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.Code
                    })
                    .ToListAsync(cancellationToken);

                var classroomLookup = classrooms
                    .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                    .ToDictionary(
                        x => x.Code.Trim(),
                        x => x.Id,
                        StringComparer.OrdinalIgnoreCase);

                var guardians = await _context.Guardians
                    .Where(x => !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.PhoneNumber
                    })
                    .ToListAsync(cancellationToken);

                var guardianLookup = guardians
                    .Where(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                    .ToDictionary(
                        x => x.PhoneNumber.Trim(),
                        x => x.Id,
                        StringComparer.OrdinalIgnoreCase);

                if (!classroomLookup.TryGetValue(
                        classRoomCode,
                        out var classRoomId))
                {
                    result.Success = false;
                    result.Message =
                        $"Classroom dengan code '{classRoomCode}' tidak ditemukan.";

                    response.Results.Add(result);
                    continue;
                }

                if (!guardianLookup.TryGetValue(
                        guardianPhone,
                        out var guardianId))
                {
                    result.Success = false;
                    result.Message =
                        $"Guardian dengan nomor '{guardianPhone}' tidak ditemukan.";

                    response.Results.Add(result);
                    continue;
                }

                //-------------------------------------------------
                // Create entity
                //-------------------------------------------------

                var now =
                    _dateTimeProvider.UtcNow;

                var entity = new Student
                {
                    Id = Guid.NewGuid(),

                    NIS = nis,

                    NISN = nisn,

                    FullName = fullName,

                    Gender = genderEnum,

                    BirthPlace = birthPlace,

                    BirthDate = DateTime.SpecifyKind(
     DateTime.Parse(birthDate),
     DateTimeKind.Utc
 ),
                    Address = address,

                    PhotoUrl = photoUrl,

                    ClassRoomId = classRoomId,

                    GuardianId = guardianId,

                    Status = statusEnum,

                    EnrollmentDate = DateTime.SpecifyKind(
     DateTime.Parse(enrollmentDate),
     DateTimeKind.Utc
 ),
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

                result.StudentId =
                    entity.Id;

                result.Message =
                    "Student siap diimport.";
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
                response.Results.Count(
                    x => !x.Success);

            return response;
        }

        //---------------------------------------------------------
        // Insert all
        //---------------------------------------------------------

        if (entities.Count > 0)
        {
            _context.Students.AddRange(
                entities);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        //---------------------------------------------------------
        // Summary
        //---------------------------------------------------------

        response.SuccessRows =
            response.Results.Count(
                x => x.Success);

        response.FailedRows =
            response.Results.Count(
                x => !x.Success);

        return response;
    }

    //=============================================================
    // Validate File
    //=============================================================

    private static void ValidateFile(
        FileUpload file)
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
            Path.GetExtension(
                file.FileName);

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
    // Normalize Student Full Name
    //=============================================================

    private static string NormalizeFullName(
        string value)
    {
        return value
            .Trim()
            .ToLowerInvariant();
    }
}