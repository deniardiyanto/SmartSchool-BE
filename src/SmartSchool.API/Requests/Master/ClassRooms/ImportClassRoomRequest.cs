using Microsoft.AspNetCore.Http;

namespace SmartSchool.API.Requests.Master.ClassRooms;

public class ImportClassRoomRequest
{
    public IFormFile File { get; set; } = null!;
}