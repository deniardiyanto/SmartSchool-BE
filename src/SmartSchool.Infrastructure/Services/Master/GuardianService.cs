using Microsoft.EntityFrameworkCore;
using SmartSchool.Application.Common.Exceptions;
using SmartSchool.Application.Features.Guardians.Contracts;
using SmartSchool.Application.Features.Guardians.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.Master;

public class GuardianService : IGuardianService
{
    private readonly SmartSchoolDbContext _context;

    public GuardianService(SmartSchoolDbContext context)
    {
        _context = context;
    }

    public async Task<PagedGuardianResponse> GetPagedAsync(GuardianFilterRequest request)
    {
        var query = _context.Guardians
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();

            query = query.Where(x =>
                x.FullName.ToLower().Contains(keyword) ||
                x.PhoneNumber.Contains(keyword));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new GuardianDto
            {
                Id = x.Id,
                FullName = x.FullName,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                Address = x.Address,
                Relationship = x.Relationship,
                Occupation = x.Occupation,
                IsActive = x.IsActive,
                GuardianCode = x.GuardianCode
            })
            .ToListAsync();

        return new PagedGuardianResponse
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<GuardianDto> GetByIdAsync(Guid id)
    {
        var guardian = await _context.Guardians
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (guardian == null)
            throw new NotFoundException("Guardian not found.");

        return new GuardianDto
        {
            Id = guardian.Id,
            FullName = guardian.FullName,
            PhoneNumber = guardian.PhoneNumber,
            Email = guardian.Email,
            Address = guardian.Address,
            Relationship = guardian.Relationship,
            Occupation = guardian.Occupation,
            IsActive = guardian.IsActive,
            GuardianCode = guardian.GuardianCode
        };
    }

    public async Task<Guid> CreateAsync(CreateGuardianRequest request)
    {
        var guardianCode =
    request.GuardianCode.Trim();

var guardianCodeExists =
    await _context.Guardians
        .AnyAsync(
            x =>
                !x.IsDeleted &&
                x.GuardianCode == guardianCode);
                if (guardianCodeExists)
{
    throw new InvalidOperationException(
        $"Guardian code '{guardianCode}' sudah digunakan.");
}
        var guardian = new Guardian
        {
            GuardianCode = request.GuardianCode.Trim(),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Address = request.Address,
            Relationship = request.Relationship,
            Occupation = request.Occupation,
            IsActive = true
        };

        _context.Guardians.Add(guardian);

        await _context.SaveChangesAsync();

        return guardian.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateGuardianRequest request)
    {
        var guardian = await _context.Guardians
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (guardian == null)
            throw new NotFoundException("Guardian not found.");
guardian.GuardianCode = request.GuardianCode.Trim();
        guardian.FullName = request.FullName;
        guardian.PhoneNumber = request.PhoneNumber;
        guardian.Email = request.Email;
        guardian.Address = request.Address;
        guardian.Relationship = request.Relationship;
        guardian.Occupation = request.Occupation;
        guardian.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var guardian = await _context.Guardians
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (guardian == null)
            throw new NotFoundException("Guardian not found.");

        guardian.IsDeleted = true;

        await _context.SaveChangesAsync();
    }
}