namespace SmartSchool.Application.Features.Master.ClassRooms.Import.Contracts;

public class ImportClassRoomRowResult
{
    public int RowNumber { get; set; }

    public bool Success { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? ClassRoomId { get; set; }
}