using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Features.Students.Contracts;
using SmartSchool.Application.Features.Students.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Master;

public class StudentService : IStudentService
{
    private readonly SmartSchoolDbContext _context;

    public StudentService(SmartSchoolDbContext context)
    {
        _context = context;
    }

    public async Task<PagedStudentResponse> GetPagedAsync(StudentFilterRequest request)
    {
        var query = _context.Students
            .AsNoTracking()
            .Include(x => x.ClassRoom)
            .Include(x => x.Guardian)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.NIS.ToLower().Contains(keyword) ||
                (x.NISN != null && x.NISN.ToLower().Contains(keyword)) ||
                x.FullName.ToLower().Contains(keyword));
        }

        if (request.ClassRoomId.HasValue)
        {
            query = query.Where(x =>
                x.ClassRoomId == request.ClassRoomId.Value);
        }

        if (request.GuardianId.HasValue)
        {
            query = query.Where(x =>
                x.GuardianId == request.GuardianId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == request.IsActive.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new StudentDto
            {
                Id = x.Id,
                NIS = x.NIS,
                NISN = x.NISN,
                FullName = x.FullName,
                Gender = x.Gender,
                BirthPlace = x.BirthPlace,
                BirthDate = x.BirthDate,
                Address = x.Address,
                PhotoUrl = x.PhotoUrl,
                ClassRoomId = x.ClassRoomId,
                ClassRoomName = x.ClassRoom.Name,
                GuardianId = x.GuardianId,
                GuardianName = x.Guardian.FullName,
                Status = x.Status,
                EnrollmentDate = x.EnrollmentDate,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return new PagedStudentResponse
        {
            Items = items,
            TotalRecords = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<StudentDto> GetByIdAsync(Guid id)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Include(x => x.ClassRoom)
            .Include(x => x.Guardian)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (student == null)
            throw new NotFoundException("Student not found.");

        return new StudentDto
        {
            Id = student.Id,
            NIS = student.NIS,
            NISN = student.NISN,
            FullName = student.FullName,
            Gender = student.Gender,
            BirthPlace = student.BirthPlace,
            BirthDate = student.BirthDate,
            Address = student.Address,
            PhotoUrl = student.PhotoUrl,
            ClassRoomId = student.ClassRoomId,
            ClassRoomName = student.ClassRoom.Name,
            GuardianId = student.GuardianId,
            GuardianName = student.Guardian.FullName,
            Status = student.Status,
            EnrollmentDate = student.EnrollmentDate,
            IsActive = student.IsActive
        };
    }

    public async Task<Guid> CreateAsync(CreateStudentRequest request,
    CancellationToken cancellationToken = default)
{
    //---------------------------------------------------------
    // Validate NIS
    //---------------------------------------------------------

    if (await _context.Students.AnyAsync(x =>
            x.NIS == request.NIS &&
            !x.IsDeleted))
    {
        throw new ConflictException("NIS already exists.");
    }

    //---------------------------------------------------------
    // Validate NISN
    //---------------------------------------------------------

    if (!string.IsNullOrWhiteSpace(request.NISN))
    {
        if (await _context.Students.AnyAsync(x =>
                x.NISN == request.NISN &&
                !x.IsDeleted))
        {
            throw new ConflictException("NISN already exists.");
        }
    }

    //---------------------------------------------------------
    // Validate Classroom Code
    //---------------------------------------------------------

    if (string.IsNullOrWhiteSpace(request.ClassRoomCode))
    {
        throw new NotFoundException(
            "Class room code is required.");
    }

    if (string.IsNullOrWhiteSpace(request.AcademicYear))
    {
        throw new NotFoundException(
            "Academic year is required.");
    }

    var classroom = await _context.ClassRooms
    .AsNoTracking()
    .FirstOrDefaultAsync(
        x =>
            !x.IsDeleted &&
            x.Code == request.ClassRoomCode.Trim() &&
            x.AcademicYear == request.AcademicYear.Trim(),
        cancellationToken);

    if (classroom == null)
    {
        throw new NotFoundException(
            $"Class room with code '{request.ClassRoomCode}' " +
            $"for academic year '{request.AcademicYear}' not found.");
    }

    //---------------------------------------------------------
    // Validate Guardian Code
    //---------------------------------------------------------

    if (string.IsNullOrWhiteSpace(request.GuardianCode))
    {
        throw new NotFoundException(
            "Guardian code is required.");
    }

    var guardian = await _context.Guardians
        .AsNoTracking()
        .FirstOrDefaultAsync(x =>
            !x.IsDeleted &&
            x.GuardianCode == request.GuardianCode.Trim(),
            cancellationToken);

    if (guardian == null)
    {
        throw new NotFoundException(
            $"Guardian with code '{request.GuardianCode}' not found.");
    }

    //---------------------------------------------------------
    // Create Student
    //---------------------------------------------------------

    var student = new Student
    {
        Id = Guid.NewGuid(),

        NIS = request.NIS.Trim(),

        NISN = string.IsNullOrWhiteSpace(request.NISN)
            ? null
            : request.NISN.Trim(),

        FullName = request.FullName.Trim(),

        Gender = request.Gender,

        BirthPlace = request.BirthPlace,

        BirthDate = DateTime.SpecifyKind(
            request.BirthDate,
            DateTimeKind.Utc),

        Address = request.Address,

        PhotoUrl = request.PhotoUrl,

        //-----------------------------------------------------
        // UUID diambil dari hasil lookup
        //-----------------------------------------------------

        ClassRoomId = classroom.Id,

        GuardianId = guardian.Id,

        Status = request.Status,

        EnrollmentDate = DateTime.SpecifyKind(
            request.EnrollmentDate,
            DateTimeKind.Utc),

        IsActive = true,

        IsDeleted = false
    };

    //---------------------------------------------------------
    // Save
    //---------------------------------------------------------

    _context.Students.Add(student);

    await _context.SaveChangesAsync(cancellationToken);

    return student.Id;
}

    public async Task UpdateAsync(Guid id, UpdateStudentRequest request)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (student == null)
            throw new NotFoundException("Student not found.");

        if (await _context.Students.AnyAsync(x =>
                x.Id != id &&
                x.NIS == request.NIS &&
                !x.IsDeleted))
        {
            throw new ConflictException("NIS already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.NISN))
        {
            if (await _context.Students.AnyAsync(x =>
                    x.Id != id &&
                    x.NISN == request.NISN &&
                    !x.IsDeleted))
            {
                throw new ConflictException("NISN already exists.");
            }
        }

        student.NIS = request.NIS;
        student.NISN = request.NISN;
        student.FullName = request.FullName;
        student.Gender = request.Gender;
        student.BirthPlace = request.BirthPlace;
        student.BirthDate = DateTime.SpecifyKind(
        request.BirthDate,
        DateTimeKind.Utc);
        student.Address = request.Address;
        student.PhotoUrl = request.PhotoUrl;
        student.ClassRoomId = request.ClassRoomId;
        student.GuardianId = request.GuardianId;
        student.Status = request.Status;
        student.EnrollmentDate = DateTime.SpecifyKind(
        request.EnrollmentDate,
        DateTimeKind.Utc);
        student.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (student == null)
            throw new NotFoundException("Student not found.");

        student.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}