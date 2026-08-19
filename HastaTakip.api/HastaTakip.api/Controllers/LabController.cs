using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/lab")]
public class LabController : ControllerBase
{
    private readonly HastaDbContext _db;

    public LabController(HastaDbContext db)
    {
        _db = db;
    }

    // GET /api/lab/by-ziyaret?ziyaretId=36
    [HttpGet("by-ziyaret")]
    public async Task<IActionResult> GetByZiyaret([FromQuery] int ziyaretId)
    {
        var raw = await (
            from s in _db.LabSonuclari.AsNoTracking()
            join p in _db.LabParametreler.AsNoTracking()
                on s.LabParametreID equals p.LabParametreID
            where s.ZiyaretID == ziyaretId
            select new
            {
                Tarih = s.Tarih,
                p.LabParametreID,
                Param = p.Ad + (p.Birim != null ? $" ({p.Birim})" : ""),
                GosterimDegeri =
                    s.DegerSayisal != null
                        ? s.DegerSayisal.Value.ToString("0.###")
                        : s.Deger
            }
        ).ToListAsync();

        var dates = raw
            .Select(x => x.Tarih)
            .Distinct()
            .OrderBy(d => d)
            .Select(d => d.ToString("dd.MM.yyyy"))
            .ToList();

        var rows = raw
            .GroupBy(x => new { x.LabParametreID, x.Param })
            .OrderBy(g => g.Key.Param)
            .Select(g => new
            {
                param = g.Key.Param,
                values = dates.Select(d =>
                {
                    var hit = g.FirstOrDefault(x => x.Tarih.ToString("dd.MM.yyyy") == d);
                    return hit?.GosterimDegeri;
                }).ToList()
            })
            .ToList();

        return Ok(new { dates, rows });
    }
    [HttpGet("parametreler")]
    public async Task<IActionResult> GetParametreler()
    {
        var list = await _db.LabParametreler
            .AsNoTracking()
            .OrderBy(x => x.Ad)
            .Select(x => new
            {
                x.LabParametreID,
                x.Ad,
                x.Birim,
                x.Kategori
            })
            .ToListAsync();

        return Ok(list);
    }
    [HttpPost("by-ziyaret/{ziyaretId:int}")]
    public async Task<IActionResult> CreateForZiyaret(int ziyaretId, [FromBody] LabSonucCreateDto dto)
    {
        var entity = new LabSonuc
        {
            ZiyaretID = ziyaretId,
            LabParametreID = dto.LabParametreID,
            Deger = dto.Deger,
            DegerSayisal = dto.DegerSayisal,
            RefAlt = dto.RefAlt,
            RefUst = dto.RefUst,
            Tarih = DateTime.Now
        };

        _db.LabSonuclari.Add(entity);
        await _db.SaveChangesAsync();

        return Ok();
    }
    // GET /api/lab/by-ziyaret/{ziyaretId}/list
    [HttpGet("by-ziyaret/{ziyaretId:int}/list")]
    public async Task<IActionResult> GetFlatByZiyaret(int ziyaretId)
    {
        var rows = await _db.LabSonuclari
            .AsNoTracking()
            .Where(s => s.ZiyaretID == ziyaretId)
            .Select(s => new LabSonucDetailDto
            {
                LabSonucID = s.LabSonucID,
                LabParametreID = s.LabParametreID,
                Deger = s.Deger,
                RefAlt = s.RefAlt,
                RefUst = s.RefUst,
                Tarih = s.Tarih
            })
            .ToListAsync();

        return Ok(rows);
    }

    // POST /api/lab/by-ziyaret/{ziyaretId}/bulk
    [HttpPost("by-ziyaret/{ziyaretId:int}/bulk")]
    public async Task<IActionResult> UpsertForZiyaret(int ziyaretId, [FromBody] LabForVisitBulkDto dto)
    {
        
        var existing = await _db.LabSonuclari.Where(x => x.ZiyaretID == ziyaretId).ToListAsync();
        if (existing.Any())
        {
            _db.LabSonuclari.RemoveRange(existing);
            await _db.SaveChangesAsync();
        }

       
        foreach (var it in dto.Items)
        {
            var ent = new LabSonuc
            {
                ZiyaretID = ziyaretId,
                LabParametreID = it.LabParametreID,
                Deger = it.Deger,
                DegerSayisal = decimal.TryParse(it.Deger?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ds) ? ds : (decimal?)null,
                RefAlt = it.RefAlt,
                RefUst = it.RefUst,
                Tarih = dto.Tarih
            };
            _db.LabSonuclari.Add(ent);
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
