namespace HastaTakip.UI.Dtos;

public class LabSonucDetailDto
{
    public int LabSonucID { get; set; }
    public int LabParametreID { get; set; }
    public string? Deger { get; set; }
    public decimal? RefAlt { get; set; }
    public decimal? RefUst { get; set; }
    public DateTime Tarih { get; set; }
}