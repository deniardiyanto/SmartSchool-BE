namespace SmartSchool.Application.Features.Master.ClassRooms.Import.Contracts;

public class ImportClassRoomResponse
{
    public int TotalRows { get; set; }

    public int SuccessRows { get; set; }

    public int FailedRows { get; set; }

    public List<ImportClassRoomRowResult> Results { get; set; } = new();
}