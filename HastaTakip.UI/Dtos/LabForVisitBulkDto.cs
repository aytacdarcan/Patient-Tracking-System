namespace HastaTakip.UI.Dtos;

public class LabForVisitBulkDto
{
    public DateTime Tarih { get; set; }
    public List<LabSonucForVisitDto> Items { get; set; } = new();
}