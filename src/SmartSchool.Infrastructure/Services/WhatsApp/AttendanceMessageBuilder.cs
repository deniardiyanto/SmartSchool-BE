using Microsoft.Extensions.Options;
using SmartSchool.Application.Features.WhatsApp.Interfaces;
using SmartSchool.Domain.Entities;
using SmartSchool.Domain.Enums;
using SmartSchool.Infrastructure.Configuration;

namespace SmartSchool.Infrastructure.Services.WhatsApp;

public class AttendanceMessageBuilder
    : IAttendanceMessageBuilder
{
    private readonly SchoolOptions _options;

    public AttendanceMessageBuilder(
        IOptions<SchoolOptions> options)
    {
        _options = options.Value;
    }

    public string BuildCheckInMessage(
        Student student,
        Attendance attendance,
        int point)
    {
        var checkIn =
            attendance.CheckInTime.HasValue
                ? ToWib(attendance.CheckInTime.Value)
                : "-";

        return
$"""
🏫 {_options.SchoolName}

Halo Bapak/Ibu,

Kami informasikan bahwa siswa berikut telah melakukan absensi.

━━━━━━━━━━━━━━━━━━

👨‍🎓 Nama
{student.FullName}

🏫 Kelas
{student.ClassRoom.Name}

📅 Tanggal
{attendance.AttendanceDate:dd MMM yyyy}

🕒 Jam Masuk
{checkIn}

📌 Status
{attendance.Status}

⚠️ Poin Pelanggaran
{point}

━━━━━━━━━━━━━━━━━━

Terima kasih.

Pesan ini dikirim otomatis oleh {_options.SchoolName}.
""";
    }

    public string BuildCheckOutMessage(
        Student student,
        Attendance attendance)
    {
        var checkOut =
            attendance.CheckOutTime.HasValue
                ? ToWib(attendance.CheckOutTime.Value)
                : "-";

        return
$"""
🏫 {_options.SchoolName}

Halo Bapak/Ibu,

Siswa telah menyelesaikan kegiatan sekolah.

━━━━━━━━━━━━━━━━━━

👨‍🎓 Nama
{student.FullName}

🏫 Kelas
{student.ClassRoom.Name}

🕒 Jam Pulang
{checkOut}

━━━━━━━━━━━━━━━━━━

Terima kasih.

Pesan ini dikirim otomatis oleh {_options.SchoolName}.
""";
    }

    private static string ToWib(DateTime utc)
    {
        return utc
            .AddHours(7)
            .ToString("dd MMM yyyy HH:mm 'WIB'");
    }
}