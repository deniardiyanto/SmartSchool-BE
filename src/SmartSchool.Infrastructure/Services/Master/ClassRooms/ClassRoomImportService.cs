// using ClosedXML.Excel;
// using Microsoft.EntityFrameworkCore;
// using SmartSchool.Application.Common.Interfaces;
// using SmartSchool.Application.Common.Models;
// using SmartSchool.Application.Features.Master.ClassRooms.Import.Contracts;
// using SmartSchool.Application.Features.Master.ClassRooms.Import.Interfaces;
// using SmartSchool.Domain.Entities;
// using SmartSchool.Infrastructure.Persistence.Context;

// namespace SmartSchool.Infrastructure.Services.Master.ClassRooms;

// public class ClassRoomImportService : IClassRoomImportService
// {
//     private readonly SmartSchoolDbContext _context;
//     private readonly IDateTimeProvider _dateTimeProvider;
//     private readonly ICurrentUserService _currentUser;

//     public ClassRoomImportService(
//         SmartSchoolDbContext context,
//         IDateTimeProvider dateTimeProvider,
//         ICurrentUserService currentUser)
//     {
//         _context = context;
//         _dateTimeProvider = dateTimeProvider;
//         _currentUser = currentUser;
//     }

//     public async Task<ImportClassRoomResponse> ImportAsync(
//         FileUpload file,
//         CancellationToken cancellationToken = default)
//     {
//         //---------------------------------------------------------
//         // Validate file
//         //---------------------------------------------------------

//         ValidateFile(file);

//         var response = new ImportClassRoomResponse();

//         //---------------------------------------------------------
//         // Open Excel
//         //---------------------------------------------------------

//         using var workbook =
//             new XLWorkbook(file.Content);

//         var worksheet =
//             workbook.Worksheets.FirstOrDefault();

//         if (worksheet == null)
//         {
//             throw new InvalidOperationException(
//                 "Excel worksheet tidak ditemukan.");
//         }

//         //---------------------------------------------------------
//         // Get rows
//         //---------------------------------------------------------

//         var rows = worksheet
//             .RowsUsed()
//             .Skip(1)
//             .ToList();

//         response.TotalRows = rows.Count;

//         if (rows.Count == 0)
//         {
//             return response;
//         }

//         //---------------------------------------------------------
//         // Load existing classroom
//         //---------------------------------------------------------

//         var existingNames =
//             await _context.ClassRooms
//                 .Where(x => !x.IsDeleted)
//                 .Select(x => x.Name)
//                 .ToListAsync(cancellationToken);

//         var existingNameLookup =
//             new HashSet<string>(
//                 existingNames
//                     .Where(x =>
//                         !string.IsNullOrWhiteSpace(x))
//                     .Select(NormalizeName),
//                 StringComparer.OrdinalIgnoreCase);

//         var existingCodes =
//     await _context.ClassRooms
//         .Where(x => !x.IsDeleted)
//         .Select(x => x.Code)
//         .ToListAsync(cancellationToken);

// var existingCodeLookup =
//     new HashSet<string>(
//         existingCodes
//             .Where(x =>
//                 !string.IsNullOrWhiteSpace(x))
//             .Select(x => x.Trim()),
//         StringComparer.OrdinalIgnoreCase);

//         //---------------------------------------------------------
//         // Track duplicate Excel
//         //---------------------------------------------------------

//         var excelNameLookup =
//             new HashSet<string>(
//                 StringComparer.OrdinalIgnoreCase);

//         var excelCodeLookup =
//     new HashSet<string>(
//         StringComparer.OrdinalIgnoreCase);

//         var entities =
//             new List<ClassRoom>();

//         //---------------------------------------------------------
//         // Validate rows
//         //---------------------------------------------------------

//         foreach (var row in rows)
//         {
//             var rowNumber = row.RowNumber();

//             var result =
//                 new ImportClassRoomRowResult
//                 {
//                     RowNumber = rowNumber
//                 };

//             try
//             {
//                 //-------------------------------------------------
//                 // Read Excel
//                 //-------------------------------------------------

//                 var code =
//     row.Cell(1)
//         .GetString()
//         .Trim();

// var name =
//     row.Cell(2)
//         .GetString()
//         .Trim();

// var gradeText =
//     row.Cell(3)
//         .GetString()
//         .Trim();

// var academicYear =
//     row.Cell(4)
//         .GetString()
//         .Trim();

// var description =
//     row.Cell(5)
//         .GetString()
//         .Trim();

// result.Name = name;

//                 if (string.IsNullOrWhiteSpace(code))
// {
//     result.Success = false;
//     result.Message = "Code classroom wajib diisi.";
//     response.Results.Add(result);
//     continue;
// }

// if (string.IsNullOrWhiteSpace(name))
// {
//     result.Success = false;
//     result.Message = "Name classroom wajib diisi.";
//     response.Results.Add(result);
//     continue;
// }

// if (!int.TryParse(gradeText, out var grade))
// {
//     result.Success = false;
//     result.Message = "Grade harus berupa angka.";
//     response.Results.Add(result);
//     continue;
// }

// if (grade <= 0)
// {
//     result.Success = false;
//     result.Message = "Grade harus lebih besar dari 0.";
//     response.Results.Add(result);
//     continue;
// }

// if (string.IsNullOrWhiteSpace(academicYear))
// {
//     result.Success = false;
//     result.Message = "Academic year wajib diisi.";
//     response.Results.Add(result);
//     continue;
// }

// if (existingCodeLookup.Contains(code))
// {
//     result.Success = false;
//     result.Message = $"Code '{code}' sudah ada.";
//     response.Results.Add(result);
//     continue;
// }

// if (!excelCodeLookup.Add(code))
// {
//     result.Success = false;
//     result.Message = $"Code '{code}' duplicate di Excel.";
//     response.Results.Add(result);
//     continue;
// }

//                 //-------------------------------------------------
//                 // Normalize name
//                 //-------------------------------------------------

//                 var normalizedName =
//                     NormalizeName(name);

//                 //-------------------------------------------------
//                 // Check database duplicate
//                 //-------------------------------------------------

//                 if (existingNameLookup.Contains(
//                         normalizedName))
//                 {
//                     result.Success = false;

//                     result.Message =
//                         $"Classroom '{name}' sudah ada.";

//                     response.Results.Add(result);

//                     continue;
//                 }

//                 //-------------------------------------------------
//                 // Check Excel duplicate
//                 //-------------------------------------------------

//                 if (!excelNameLookup.Add(
//                         normalizedName))
//                 {
//                     result.Success = false;

//                     result.Message =
//                         $"Classroom '{name}' duplicate di Excel.";

//                     response.Results.Add(result);

//                     continue;
//                 }

//                 //-------------------------------------------------
//                 // Create entity
//                 //-------------------------------------------------

//                 var now =
//                     _dateTimeProvider.UtcNow;

//                 var entity = new ClassRoom
// {
//     Id = Guid.NewGuid(),
//     Code = code,
//     Name = name,
//     Grade = grade,
//     AcademicYear = academicYear,
//     Description = string.IsNullOrWhiteSpace(description)
//         ? null
//         : description,
//     IsActive = true,
//     CreatedAt = _dateTimeProvider.UtcNow,
//     CreatedBy = _currentUser.UserId
// };
//                 entities.Add(entity);

//                 //-------------------------------------------------
//                 // Result
//                 //-------------------------------------------------

//                 result.Success = true;

//                 result.ClassRoomId =
//                     entity.Id;

//                 result.Message =
//                     "Classroom siap diimport.";

//                 response.Results.Add(result);
//             }
//             catch (Exception ex)
//             {
//                 result.Success = false;

//                 result.Message =
//                     $"Gagal membaca row: {ex.Message}";

//                 response.Results.Add(result);
//             }
//         }

//         //---------------------------------------------------------
//         // Stop if validation failed
//         //---------------------------------------------------------

//         if (response.Results.Any(x => !x.Success))
//         {
//             response.SuccessRows = 0;

//             response.FailedRows =
//                 response.Results.Count(
//                     x => !x.Success);

//             return response;
//         }

//         //---------------------------------------------------------
//         // Insert all
//         //---------------------------------------------------------

//         if (entities.Count > 0)
//         {
//             _context.ClassRooms.AddRange(
//                 entities);

//             await _context.SaveChangesAsync(
//                 cancellationToken);
//         }

//         //---------------------------------------------------------
//         // Summary
//         //---------------------------------------------------------

//         response.SuccessRows =
//             response.Results.Count(
//                 x => x.Success);

//         response.FailedRows =
//             response.Results.Count(
//                 x => !x.Success);

//         return response;
//     }

//     //=============================================================
//     // Validate File
//     //=============================================================

//     private static void ValidateFile(
//         FileUpload file)
//     {
//         if (file == null)
//         {
//             throw new ArgumentNullException(
//                 nameof(file),
//                 "File Excel wajib diupload.");
//         }

//         if (file.Content == null)
//         {
//             throw new InvalidOperationException(
//                 "Content file tidak tersedia.");
//         }

//         if (file.Content == Stream.Null)
//         {
//             throw new InvalidOperationException(
//                 "Content file tidak tersedia.");
//         }

//         if (file.Content.Length == 0)
//         {
//             throw new InvalidOperationException(
//                 "File Excel kosong.");
//         }

//         var extension =
//             Path.GetExtension(
//                 file.FileName);

//         if (!string.Equals(
//                 extension,
//                 ".xlsx",
//                 StringComparison.OrdinalIgnoreCase))
//         {
//             throw new InvalidOperationException(
//                 "File harus berformat .xlsx.");
//         }
//     }

//     //=============================================================
//     // Normalize Classroom Name
//     //=============================================================

//     private static string NormalizeName(
//         string value)
//     {
//         return value
//             .Trim()
//             .ToLowerInvariant();
//     }
// }
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Common.Models;
using SmartSchool.Application.Features.Master.ClassRooms.Import.Contracts;
using SmartSchool.Application.Features.Master.ClassRooms.Import.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Master.ClassRooms;

public class ClassRoomImportService : IClassRoomImportService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUser;

    public ClassRoomImportService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public async Task<ImportClassRoomResponse> ImportAsync(
        FileUpload file,
        CancellationToken cancellationToken = default)
    {
        //---------------------------------------------------------
        // Validate file
        //---------------------------------------------------------

        ValidateFile(file);

        var response = new ImportClassRoomResponse();

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
        // Load existing classrooms
        //
        // Unique business key:
        // Code + AcademicYear
        //---------------------------------------------------------

        var existingClassrooms =
            await _context.ClassRooms
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Code,
                    x.AcademicYear
                })
                .ToListAsync(cancellationToken);

        var existingClassRoomLookup =
            existingClassrooms
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Code) &&
                    !string.IsNullOrWhiteSpace(x.AcademicYear))
                .Select(x => BuildClassRoomKey(
                    x.Code,
                    x.AcademicYear))
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Track duplicate Excel
        //---------------------------------------------------------

        var excelClassRoomLookup =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Entities
        //---------------------------------------------------------

        var entities =
            new List<ClassRoom>();

        //---------------------------------------------------------
        // Validate rows
        //---------------------------------------------------------

        foreach (var row in rows)
        {
            var rowNumber = row.RowNumber();

            var result =
                new ImportClassRoomRowResult
                {
                    RowNumber = rowNumber
                };

            try
            {
                //-------------------------------------------------
                // Read Excel
                //-------------------------------------------------

                var code =
                    GetString(row.Cell(1));

                var name =
                    GetString(row.Cell(2));

                var gradeText =
                    GetString(row.Cell(3));

                var academicYear =
                    GetString(row.Cell(4));

                var description =
                    GetString(row.Cell(5));

                result.Name = name;

                //-------------------------------------------------
                // Required validation
                //-------------------------------------------------

                if (string.IsNullOrWhiteSpace(code))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Code classroom wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Name classroom wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(gradeText))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Grade wajib diisi.");

                    continue;
                }

                if (!int.TryParse(
                        gradeText,
                        out var grade))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Grade harus berupa angka.");

                    continue;
                }

                if (grade <= 0)
                {
                    AddFailedResult(
                        response,
                        result,
                        "Grade harus lebih besar dari 0.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(academicYear))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Academic year wajib diisi.");

                    continue;
                }

                //-------------------------------------------------
                // Normalize
                //-------------------------------------------------

                code = code.Trim();
                name = name.Trim();
                academicYear = academicYear.Trim();

                //-------------------------------------------------
                // Build business key
                //
                // Code + AcademicYear
                //-------------------------------------------------

                var classRoomKey =
                    BuildClassRoomKey(
                        code,
                        academicYear);

                //-------------------------------------------------
                // Check database duplicate
                //-------------------------------------------------

                if (existingClassRoomLookup.Contains(
                        classRoomKey))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Classroom dengan code '{code}' " +
                        $"untuk academic year '{academicYear}' " +
                        "sudah ada.");

                    continue;
                }

                //-------------------------------------------------
                // Check Excel duplicate
                //-------------------------------------------------

                if (!excelClassRoomLookup.Add(
                        classRoomKey))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Classroom dengan code '{code}' " +
                        $"untuk academic year '{academicYear}' " +
                        "duplicate di Excel.");

                    continue;
                }

                //-------------------------------------------------
                // Create entity
                //-------------------------------------------------

                var now =
                    _dateTimeProvider.UtcNow;

                var entity = new ClassRoom
                {
                    Id = Guid.NewGuid(),

                    Code = code,

                    Name = name,

                    Grade = grade,

                    AcademicYear = academicYear,

                    Description =
                        string.IsNullOrWhiteSpace(description)
                            ? null
                            : description,

                    IsActive = true,

                    IsDeleted = false,

                    CreatedAt = now,

                    CreatedBy = _currentUser.UserId
                };

                entities.Add(entity);

                //-------------------------------------------------
                // Result
                //-------------------------------------------------

                result.Success = true;

                result.ClassRoomId =
                    entity.Id;

                result.Message =
                    "Classroom siap diimport.";

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
            _context.ClassRooms.AddRange(entities);

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
    // Build Classroom Key
    //=============================================================

    private static string BuildClassRoomKey(
        string code,
        string academicYear)
    {
        return $"{code.Trim().ToUpperInvariant()}|{academicYear.Trim().ToUpperInvariant()}";
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
    // Add Failed Result
    //=============================================================

    private static void AddFailedResult(
        ImportClassRoomResponse response,
        ImportClassRoomRowResult result,
        string message)
    {
        result.Success = false;
        result.Message = message;

        response.Results.Add(result);
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
}