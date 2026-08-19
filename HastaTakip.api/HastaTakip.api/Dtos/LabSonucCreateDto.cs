namespace HastaTakip.Api.Dtos;

public class LabSonucCreateDto
{
    public int LabParametreID { get; set; }

    public string? Deger { get; set; }

    public decimal? DegerSayisal { get; set; }

    public decimal? RefAlt { get; set; }

    public decimal? RefUst { get; set; }
}