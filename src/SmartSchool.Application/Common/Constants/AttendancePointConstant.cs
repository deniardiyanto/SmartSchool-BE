namespace SmartSchool.Application.Common.Constants;

/// <summary>
/// Poin yang dipakai sistem adalah poin pelanggaran:
/// semakin tinggi total poin siswa, semakin buruk.
/// Nilai positif menambah poin pelanggaran, nilai negatif
/// mengurangi (koreksi/pembinaan).
/// </summary>
public static class AttendancePointConstant
{
    public const int OnTime = 0;

    public const int Late = 5;

    public const int CheckOut = 0;

    public const int ManualAdjustment = 0;
}
