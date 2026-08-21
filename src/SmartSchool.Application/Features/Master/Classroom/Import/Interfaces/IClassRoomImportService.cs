using SmartSchool.Application.Common.Models;
using SmartSchool.Application.Features.Master.ClassRooms.Import.Contracts;

namespace SmartSchool.Application.Features.Master.ClassRooms.Import.Interfaces;

public interface IClassRoomImportService
{
    Task<ImportClassRoomResponse> ImportAsync(
        FileUpload file,
        CancellationToken cancellationToken = default);
}