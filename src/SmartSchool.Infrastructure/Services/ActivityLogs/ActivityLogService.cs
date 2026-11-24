using SmartSchool.Application.Common.Interfaces;
using SmartSchool.Application.Features.ActivityLogs.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Infrastructure.Persistence.Context;

namespace SmartSchool.Infrastructure.Services.ActivityLogs;

public class ActivityLogService : IActivityLogService
{
    private readonly SmartSchoolDbContext _context;

    private readonly ICurrentUserService _currentUser;

    public ActivityLogService(
        SmartSchoolDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;

        _currentUser = currentUser;
    }

    public void Record(
        string type,
        string title,
        string? description = null,
        Guid? entityId = null)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            Type = type,

            Title = title,

            Description = description,

            EntityId = entityId,

            ActorUserId = _currentUser.UserId,

            OccurredAt = DateTime.UtcNow
        });
    }
}
