// using ClosedXML.Excel;
// using DocumentFormat.OpenXml.Vml.Office;
// using Microsoft.EntityFrameworkCore;
// using SmartSchool.Application.Common.Interfaces;
// using SmartSchool.Application.Common.Models;
// using SmartSchool.Application.Features.Master.Guardians.Import.Contracts;
// using SmartSchool.Application.Features.Master.Guardians.Import.Interfaces;
// using SmartSchool.Domain.Entities;
// using SmartSchool.Infrastructure.Persistence.Context;
// using SmartSchool.Domain.Enums;

// namespace SmartSchool.Infrastructure.Services.Master.Guardians;

// public class GuardianImportService : IGuardianImportService
// {
//     private readonly SmartSchoolDbContext _context;
//     private readonly IDateTimeProvider _dateTimeProvider;
//     private readonly ICurrentUserService _currentUser;

//     public GuardianImportService(
//         SmartSchoolDbContext context,
//         IDateTimeProvider dateTimeProvider,
//         ICurrentUserService currentUser)
//     {
//         _context = context;
//         _dateTimeProvider = dateTimeProvider;
//         _currentUser = currentUser;
//     }

//     public async Task<ImportGuardianResponse> ImportAsync(
//         FileUpload file,
//         CancellationToken cancellationToken = default)
//     {
//         //---------------------------------------------------------
//         // Validate file
//         //---------------------------------------------------------

//         ValidateFile(file);

//         var response = new ImportGuardianResponse();

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
//         // Load existing guardians
//         //---------------------------------------------------------

//         var existingFullNames =
//             await _context.Guardians
//                 .Where(x => !x.IsDeleted)
//                 .Select(x => x.FullName)
//                 .ToListAsync(cancellationToken);

//         var existingFullNameLookup =
//             new HashSet<string>(
//                 existingFullNames
//                     .Where(x =>
//                         !string.IsNullOrWhiteSpace(x))
//                     .Select(NormalizeFullName),
//                 StringComparer.OrdinalIgnoreCase);

//       var existingPhoneNumbers =
//     await _context.Guardians
//         .Where(x => !x.IsDeleted)
//         .Select(x => x.PhoneNumber)
//         .ToListAsync(cancellationToken);

// var existingPhoneLookup =
//     new HashSet<string>(
//         existingPhoneNumbers
//             .Where(x =>
//                 !string.IsNullOrWhiteSpace(x))
//             .Select(x => x.Trim()),
//         StringComparer.OrdinalIgnoreCase);

//         //---------------------------------------------------------
//         // Track duplicate Excel
//         //---------------------------------------------------------

//         var excelFullNameLookup =
//             new HashSet<string>(
//                 StringComparer.OrdinalIgnoreCase);

//       var excelPhoneNumberLookup =
//     new HashSet<string>(
//         StringComparer.OrdinalIgnoreCase);
//         var entities =
//             new List<Guardian>();

//         //---------------------------------------------------------
//         // Validate rows
//         //---------------------------------------------------------

//         foreach (var row in rows)
//         {
//             var rowNumber = row.RowNumber();

//             var result =
//                 new ImportGuardianRowResult
//                 {
//                     RowNumber = rowNumber
//                 };

//             try
//             {
//                 //-------------------------------------------------
//                 // Read Excel
//                 //-------------------------------------------------

//                 var fullname =
//     row.Cell(1)
//         .GetString()
//         .Trim();

// var phonenumber =
//     row.Cell(2)
//         .GetString()
//         .Trim();

// var email =
//     row.Cell(3)
//         .GetString()
//         .Trim();

// var address =
//     row.Cell(4)
//         .GetString()
//         .Trim();

// var relationship =
//     row.Cell(5)
//         .GetString()
//         .Trim();
// var occupation =
//     row.Cell(6)
//         .GetString()
//         .Trim();

// result.FullName = fullname;

//                 if (string.IsNullOrWhiteSpace(phonenumber))
// {
//     result.Success = false;
//     result.Message = "Phone number wajib diisi.";
//     response.Results.Add(result);
//     continue;
// }

// if (string.IsNullOrWhiteSpace(fullname))
// {
//     result.Success = false;
//     result.Message = "Name guardian wajib diisi.";
//     response.Results.Add(result);
//     continue;
// }

// // if (!int.TryParse(phonenumber, out var phone))
// // {
// //     result.Success = false;
// //     result.Message = "Phone number harus berupa angka.";
// //     response.Results.Add(result);
// //     continue;
// // }

// // if (phone <= 0)
// // {
// //     result.Success = false;
// //     result.Message = "Phone number harus lebih besar dari 0.";
// //     response.Results.Add(result);
// //     continue;
// // }

// if (string.IsNullOrWhiteSpace(email))
// {
//     result.Success = false;
//     result.Message = "Email wajib diisi.";
//     response.Results.Add(result);
//     continue;
// }

// if (existingPhoneLookup.Contains(phonenumber))
// {
//     result.Success = false;
//     result.Message = $"Phone number '{phonenumber}' sudah ada.";
//     response.Results.Add(result);
//     continue;
// }

// if (!excelPhoneNumberLookup.Add(phonenumber))
// {
//     result.Success = false;
//     result.Message = $"Phone number '{phonenumber}' duplicate di Excel.";
//     response.Results.Add(result);
//     continue;
// }

// // if (!Enum.TryParse<GuardianRelationship>(
// //         relationship,
// //         true,
// //         out var RelationshipEnum))
// // {
// //     result.Success = false;

// //     result.Message =
// //         $"Relationship '{relationship}' tidak valid.";

// //     response.Results.Add(result);

// //     continue;
// // }
// GuardianRelationship relationshipEnum;

// switch (relationship.Trim().ToLowerInvariant())
// {
//     case "ayah":
//     case "bapak":
//         relationshipEnum = GuardianRelationship.Father;
//         break;

//     case "ibu":
//     case "mama":
//         relationshipEnum = GuardianRelationship.Mother;
//         break;

//     case "wali":
//         relationshipEnum = GuardianRelationship.Guardian;
//         break;

//     default:
//         result.Success = false;

//         result.Message =
//             $"Relationship '{relationship}' tidak valid. " +
//             "Gunakan Ayah, Ibu, atau Wali.";

//         response.Results.Add(result);

//         continue;
// }

//                 //-------------------------------------------------
//                 // Normalize fullname
//                 //-------------------------------------------------

//                 var normalizedFullName =
//                     NormalizeFullName(fullname);

//                 //-------------------------------------------------
//                 // Check database duplicate
//                 //-------------------------------------------------

//                 if (existingFullNameLookup.Contains(
//                         normalizedFullName))
//                 {
//                     result.Success = false;

//                     result.Message =
//                         $"Guardian '{normalizedFullName}' sudah ada.";

//                     response.Results.Add(result);

//                     continue;
//                 }

//                 //-------------------------------------------------
//                 // Check Excel duplicate
//                 //-------------------------------------------------

//                 if (!excelFullNameLookup.Add(
//                         normalizedFullName))
//                 {
//                     result.Success = false;

//                     result.Message =
//                         $"Guardian '{normalizedFullName}' duplicate di Excel.";

//                     response.Results.Add(result);

//                     continue;
//                 }

//                 //-------------------------------------------------
//                 // Create entity
//                 //-------------------------------------------------

//                 var now =
//                     _dateTimeProvider.UtcNow;

//                 var entity = new Guardian
// {
//     Id = Guid.NewGuid(),
//     PhoneNumber = phonenumber,
//     FullName = fullname,
//     Email = email,
//     Address = address,
//     Relationship = relationshipEnum,
//     Occupation = occupation,
//     IsActive = true,
//     CreatedAt = _dateTimeProvider.UtcNow,
//     CreatedBy = _currentUser.UserId
// };
//                 entities.Add(entity);

//                 //-------------------------------------------------
//                 // Result
//                 //-------------------------------------------------

//                 result.Success = true;

//                 result.GuardianId =
//                     entity.Id;

//                 result.Message =
//                     "Guardian siap diimport.";
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
//             _context.Guardians.AddRange(
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
//     // Normalize Guardian Full Name
//     //=============================================================

//     private static string NormalizeFullName(
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
using SmartSchool.Application.Features.Master.Guardians.Import.Contracts;
using SmartSchool.Application.Features.Master.Guardians.Import.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Domain.Enums;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Master.Guardians;

public class GuardianImportService : IGuardianImportService
{
    private readonly SmartSchoolDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUser;

    public GuardianImportService(
        SmartSchoolDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currentUser = currentUser;
    }

    public async Task<ImportGuardianResponse> ImportAsync(
        FileUpload file,
        CancellationToken cancellationToken = default)
    {
        //---------------------------------------------------------
        // Validate file
        //---------------------------------------------------------

        ValidateFile(file);

        var response = new ImportGuardianResponse();

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
        // Load existing GuardianCode
        //---------------------------------------------------------

        var existingGuardianCodes =
            await _context.Guardians
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => x.GuardianCode)
                .ToListAsync(cancellationToken);

        var existingGuardianCodeLookup =
            existingGuardianCodes
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeCode)
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Track duplicate Excel
        //---------------------------------------------------------

        var excelGuardianCodeLookup =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Track duplicate FullName
        //---------------------------------------------------------

        var existingFullNames =
            await _context.Guardians
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => x.FullName)
                .ToListAsync(cancellationToken);

        var existingFullNameLookup =
            existingFullNames
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeFullName)
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var excelFullNameLookup =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // Entities
        //---------------------------------------------------------

        var entities =
            new List<Guardian>();

        //---------------------------------------------------------
        // Validate rows
        //---------------------------------------------------------

        foreach (var row in rows)
        {
            var rowNumber = row.RowNumber();

            var result =
                new ImportGuardianRowResult
                {
                    RowNumber = rowNumber
                };

            try
            {
                //-------------------------------------------------
                // Read Excel
                //-------------------------------------------------

                var guardianCode =
                    GetString(row.Cell(1));

                var fullName =
                    GetString(row.Cell(2));

                var phoneNumber =
                    GetString(row.Cell(3));

                var email =
                    GetString(row.Cell(4));

                var address =
                    GetString(row.Cell(5));

                var relationship =
                    GetString(row.Cell(6));

                var occupation =
                    GetString(row.Cell(7));

                result.FullName = fullName;

                //-------------------------------------------------
                // Required validation
                //-------------------------------------------------

                if (string.IsNullOrWhiteSpace(guardianCode))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Guardian Code wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Nama guardian wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Phone number wajib diisi.");

                    continue;
                }

                if (string.IsNullOrWhiteSpace(relationship))
                {
                    AddFailedResult(
                        response,
                        result,
                        "Relationship wajib diisi.");

                    continue;
                }

                //-------------------------------------------------
                // Normalize
                //-------------------------------------------------

                guardianCode =
                    NormalizeCode(guardianCode);

                fullName =
                    fullName.Trim();

                phoneNumber =
                    phoneNumber.Trim();

                //-------------------------------------------------
                // Validate GuardianCode length
                //-------------------------------------------------

                if (guardianCode.Length > 30)
                {
                    AddFailedResult(
                        response,
                        result,
                        "Guardian Code maksimal 30 karakter.");

                    continue;
                }

                //-------------------------------------------------
                // Check database GuardianCode
                //-------------------------------------------------

                if (existingGuardianCodeLookup.Contains(
                        guardianCode))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Guardian Code '{guardianCode}' sudah ada.");

                    continue;
                }

                //-------------------------------------------------
                // Check duplicate GuardianCode in Excel
                //-------------------------------------------------

                if (!excelGuardianCodeLookup.Add(
                        guardianCode))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Guardian Code '{guardianCode}' duplicate di Excel.");

                    continue;
                }

                //-------------------------------------------------
                // Normalize FullName
                //-------------------------------------------------

                var normalizedFullName =
                    NormalizeFullName(fullName);

                //-------------------------------------------------
                // Check database duplicate FullName
                //-------------------------------------------------

                if (existingFullNameLookup.Contains(
                        normalizedFullName))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Guardian '{fullName}' sudah ada.");

                    continue;
                }

                //-------------------------------------------------
                // Check Excel duplicate FullName
                //-------------------------------------------------

                if (!excelFullNameLookup.Add(
                        normalizedFullName))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Guardian '{fullName}' duplicate di Excel.");

                    continue;
                }

                //-------------------------------------------------
                // Relationship
                //-------------------------------------------------

                if (!TryParseRelationship(
                        relationship,
                        out var relationshipEnum))
                {
                    AddFailedResult(
                        response,
                        result,
                        $"Relationship '{relationship}' tidak valid. " +
                        "Gunakan Ayah, Ibu, atau Wali.");

                    continue;
                }

                //-------------------------------------------------
                // Create entity
                //-------------------------------------------------

                var now =
                    _dateTimeProvider.UtcNow;

                var entity = new Guardian
                {
                    Id = Guid.NewGuid(),

                    GuardianCode =
                        guardianCode,

                    FullName =
                        fullName,

                    PhoneNumber =
                        phoneNumber,

                    Email =
                        string.IsNullOrWhiteSpace(email)
                            ? null
                            : email,

                    Address =
                        string.IsNullOrWhiteSpace(address)
                            ? null
                            : address,

                    Relationship =
                        relationshipEnum,

                    Occupation =
                        string.IsNullOrWhiteSpace(occupation)
                            ? null
                            : occupation,

                    IsActive = true,

                    IsDeleted = false,

                    CreatedAt = now,

                    CreatedBy =
                        _currentUser.UserId
                };

                entities.Add(entity);

                //-------------------------------------------------
                // Result
                //-------------------------------------------------

                result.Success = true;

                result.GuardianId =
                    entity.Id;

                result.Message =
                    "Guardian siap diimport.";

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
            _context.Guardians.AddRange(entities);

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
    // Parse Relationship
    //=============================================================

    private static bool TryParseRelationship(
        string value,
        out GuardianRelationship relationship)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "ayah":
            case "bapak":
                relationship =
                    GuardianRelationship.Father;
                return true;

            case "ibu":
            case "mama":
                relationship =
                    GuardianRelationship.Mother;
                return true;

            case "wali":
                relationship =
                    GuardianRelationship.Guardian;
                return true;

            default:
                relationship =
                    default;
                return false;
        }
    }

    //=============================================================
    // Normalize Guardian Code
    //=============================================================

    private static string NormalizeCode(
        string value)
    {
        return value
            .Trim()
            .ToUpperInvariant();
    }

    //=============================================================
    // Normalize FullName
    //=============================================================

    private static string NormalizeFullName(
        string value)
    {
        return value
            .Trim()
            .ToLowerInvariant();
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
        ImportGuardianResponse response,
        ImportGuardianRowResult result,
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
}