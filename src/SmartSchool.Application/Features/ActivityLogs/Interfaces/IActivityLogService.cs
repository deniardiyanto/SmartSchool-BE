namespace SmartSchool.Application.Features.ActivityLogs.Interfaces;

public interface IActivityLogService
{
    /// <summary>
    /// Menambahkan log ke DbContext tanpa memanggil SaveChanges.
    /// Log tersimpan bersama perubahan data pemanggil pada
    /// SaveChangesAsync berikutnya (satu transaksi).
    /// </summary>
    void Record(
        string type,
        string title,
        string? description = null,
        Guid? entityId = null);
}
