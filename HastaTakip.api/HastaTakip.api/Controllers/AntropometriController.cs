using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using HastaTakip.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AntropometrilerController : ControllerBase
{
    private readonly HastaDbContext _db;
    private readonly CeddService _cedd;

    public AntropometrilerController(HastaDbContext db, CeddService cedd)
    {
        _db = db;
        _cedd = cedd;
    }

    // GET /api/Antropometriler/by-hasta/2
    [HttpGet("by-hasta/{hastaId:int}")]
    public async Task<ActionResult<List<AntropometriListDto>>> ByHasta(int hastaId)
    {
        var list = await _db.Antropometriler
            .AsNoTracking()
            .Where(a => a.Ziyaret.HastaID == hastaId)
            .OrderByDescending(a => a.AntropometriID)
            .Select(a => new AntropometriListDto
            {
                AntropometriID = a.AntropometriID,
                ZiyaretID = a.ZiyaretID,
                YasAy = a.YasAy,
                BoyCm = a.BoyCm,
                KiloKg = a.KiloKg,
                BasCevresiCm = a.BasCevresiCm,
                BKI = a.BKI,
                OturmaBoyuCm = a.OturmaBoyuCm,
                ObTb = a.ObTb,
                GogusCevresiCm = a.GogusCevresiCm,
                BasPubisCm = a.BasPubisCm,
                PubisTopukCm = a.PubisTopukCm,
                BoySDS = a.BoySDS,
                KiloSDS = a.KiloSDS,
                BKISDS = a.BKISDS,
                BasCevresiSDS = a.BasCevresiSDS,
                YBHSDS = a.YBHSDS
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/Antropometriler/by-visit/{ziyaretId}
    [HttpGet("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<List<AntropometriListDto>>> ByZiyaret(int ziyaretId)
    {
        var list = await _db.Antropometriler
            .AsNoTracking()
            .Where(a => a.ZiyaretID == ziyaretId)
            .OrderByDescending(a => a.AntropometriID)
            .Select(a => new AntropometriListDto
            {
                AntropometriID = a.AntropometriID,
                ZiyaretID = a.ZiyaretID,
                YasAy = a.YasAy,
                BoyCm = a.BoyCm,
                KiloKg = a.KiloKg,
                BasCevresiCm = a.BasCevresiCm,
                BKI = a.BKI,
                OturmaBoyuCm = a.OturmaBoyuCm,
                ObTb = a.ObTb,
                GogusCevresiCm = a.GogusCevresiCm,
                BasPubisCm = a.BasPubisCm,
                PubisTopukCm = a.PubisTopukCm,
                BoySDS = a.BoySDS,
                KiloSDS = a.KiloSDS,
                BKISDS = a.BKISDS,
                BasCevresiSDS = a.BasCevresiSDS,
                YBHSDS = a.YBHSDS
            })
            .ToListAsync();

        return Ok(list);
    }


    // GET /api/antropometriler/10
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AntropometriDetailDto>> GetById(int id)
    {
        var a = await _db.Antropometriler
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AntropometriID == id);

        if (a is null) return NotFound();

        var dto = new AntropometriDetailDto
        {
            AntropometriID = a.AntropometriID,
            ZiyaretID = a.ZiyaretID,
            YasAy = a.YasAy,
            BoyCm = a.BoyCm,
            KiloKg = a.KiloKg,
            BasCevresiCm = a.BasCevresiCm,
            BKI = a.BKI,
            OturmaBoyuCm = a.OturmaBoyuCm,
            ObTb = a.ObTb,
            GogusCevresiCm = a.GogusCevresiCm,
            BasPubisCm = a.BasPubisCm,
            PubisTopukCm = a.PubisTopukCm,
            BoySDS = a.BoySDS,
            KiloSDS = a.KiloSDS,
            BKISDS = a.BKISDS,
            BasCevresiSDS = a.BasCevresiSDS,
            YBHSDS = a.YBHSDS
        };
        return Ok(dto);
    }

    // POST /api/antropometriler
    [HttpPost]
    public async Task<ActionResult<AntropometriDetailDto>> Create([FromBody] AntropometriCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (input.ZiyaretID <= 0) return BadRequest("ZiyaretID zorunludur.");

        var info = await _db.Ziyaretler
            .AsNoTracking()
            .Where(z => z.ZiyaretID == input.ZiyaretID)
            .Select(z => new { z.ZiyaretID, z.Tarih, z.HastaID, z.Hasta.BirthDate })
            .FirstOrDefaultAsync();

        if (info is null) return NotFound($"Ziyaret bulunamadı: {input.ZiyaretID}");

        if (input.BoyCm is < 30 or > 220) return BadRequest("BoyCm 30–220 aralığında olmalıdır.");
        if (input.KiloKg is < 1 or > 200) return BadRequest("KiloKg 1–200 aralığında olmalıdır.");
        if (input.BasCevresiCm is < 25 or > 65) return BadRequest("BasCevresiCm 25–65 aralığında olmalıdır.");

        int? yasAy = null;
        if (info.BirthDate != default && info.Tarih != default)
        {
            var days = (info.Tarih.Date - info.BirthDate.Date).TotalDays;
            yasAy = (int)Math.Round(days / 30.4375, MidpointRounding.AwayFromZero);
        }

        var entity = new Antropometri
        {
            ZiyaretID = input.ZiyaretID,
            YasAy = yasAy,
            BoyCm = input.BoyCm,
            KiloKg = input.KiloKg,
            BasCevresiCm = input.BasCevresiCm,
            OturmaBoyuCm = input.OturmaBoyuCm,
            ObTb = input.ObTb,
            GogusCevresiCm = input.GogusCevresiCm,
            BasPubisCm = input.BasPubisCm,
            PubisTopukCm = input.PubisTopukCm
            // BKI set ETME — SQL (SP/trigger) otomatik hesaplayacak
        };

        _db.Antropometriler.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
            await _db.Entry(entity).ReloadAsync(); // computed alanları (BKI/SDS) geri yükle
            await FillSdsAsync(entity);            // 🔥 SDS'yi C# (Neyzi birebir) hesapla
            await _db.Entry(entity).ReloadAsync(); // 🔹 SDS'ler DTO’ya yüklensin
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                           (sql.Number == 2601 || sql.Number == 2627))
        {
            return Conflict("Bu ziyarete ait antropometri zaten var.");
        }

        var dto = new AntropometriDetailDto
        {
            AntropometriID = entity.AntropometriID,
            ZiyaretID = entity.ZiyaretID,
            YasAy = entity.YasAy,
            BoyCm = entity.BoyCm,
            KiloKg = entity.KiloKg,
            BasCevresiCm = entity.BasCevresiCm,
            BKI = entity.BKI,
            OturmaBoyuCm = entity.OturmaBoyuCm,
            ObTb = entity.ObTb,
            GogusCevresiCm = entity.GogusCevresiCm,
            BasPubisCm = entity.BasPubisCm,
            PubisTopukCm = entity.PubisTopukCm,
            BoySDS = entity.BoySDS,
            KiloSDS = entity.KiloSDS,
            BKISDS = entity.BKISDS,
            BasCevresiSDS = entity.BasCevresiSDS,
            YBHSDS = entity.YBHSDS
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.AntropometriID }, dto);
    }

    // PUT /api/antropometriler/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AntropometriDetailDto>> Update(int id, [FromBody] AntropometriUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var entity = await _db.Antropometriler.FirstOrDefaultAsync(a => a.AntropometriID == id);
        if (entity is null) return NotFound();

        if (input.BoyCm.HasValue && (input.BoyCm < 30 || input.BoyCm > 220))
            return BadRequest("BoyCm 30–220 aralığında olmalıdır.");
        if (input.KiloKg.HasValue && (input.KiloKg < 1 || input.KiloKg > 200))
            return BadRequest("KiloKg 1–200 aralığında olmalıdır.");
        if (input.BasCevresiCm.HasValue && (input.BasCevresiCm < 25 || input.BasCevresiCm > 65))
            return BadRequest("BasCevresiCm 25–65 aralığında olmalıdır.");

        if (input.BoyCm.HasValue) entity.BoyCm = input.BoyCm;
        if (input.KiloKg.HasValue) entity.KiloKg = input.KiloKg;
        if (input.BasCevresiCm.HasValue) entity.BasCevresiCm = input.BasCevresiCm;
        if (input.OturmaBoyuCm.HasValue) entity.OturmaBoyuCm = input.OturmaBoyuCm;
        if (input.ObTb.HasValue) entity.ObTb = input.ObTb;
        if (input.GogusCevresiCm.HasValue) entity.GogusCevresiCm = input.GogusCevresiCm;
        if (input.BasPubisCm.HasValue) entity.BasPubisCm = input.BasPubisCm;
        if (input.PubisTopukCm.HasValue) entity.PubisTopukCm = input.PubisTopukCm;

        try
        {
            await _db.SaveChangesAsync();
            await FillSdsAsync(entity);
            await _db.Entry(entity).ReloadAsync(); // computed alanları (BKI/SDS) geri yükle
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                           (sql.Number == 2601 || sql.Number == 2627))
        {
            return Conflict("Bu ziyarete ait antropometri zaten var.");
        }

        var dto = new AntropometriDetailDto
        {
            AntropometriID = entity.AntropometriID,
            ZiyaretID = entity.ZiyaretID,
            YasAy = entity.YasAy,
            BoyCm = entity.BoyCm,
            KiloKg = entity.KiloKg,
            BasCevresiCm = entity.BasCevresiCm,
            BKI = entity.BKI,
            OturmaBoyuCm = entity.OturmaBoyuCm,
            ObTb = entity.ObTb,
            GogusCevresiCm = entity.GogusCevresiCm,
            BasPubisCm = entity.BasPubisCm,
            PubisTopukCm = entity.PubisTopukCm,
            BoySDS = entity.BoySDS,
            KiloSDS = entity.KiloSDS,
            BKISDS = entity.BKISDS,
            BasCevresiSDS = entity.BasCevresiSDS,
            YBHSDS = entity.YBHSDS
        };
        return Ok(dto);
    }
    // POST /api/Antropometriler/by-visit/{ziyaretId}
    [HttpPost("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<int>> CreateForVisit(int ziyaretId, [FromBody] AntropometriCreateDto dto)
    {
        var zInfo = await _db.Ziyaretler
            .AsNoTracking()
            .Where(x => x.ZiyaretID == ziyaretId)
            .Select(x => new { x.ZiyaretID, x.Tarih, x.Hasta.BirthDate })
            .FirstOrDefaultAsync();

        if (zInfo is null) return NotFound("Ziyaret bulunamadı.");

    
        var exists = await _db.Antropometriler.AnyAsync(a => a.ZiyaretID == ziyaretId);
        if (exists) return Conflict("Bu ziyaret için zaten antropometri kaydı var.");

        int? yasAy = null;
        if (zInfo.BirthDate != default && zInfo.Tarih != default)
        {
            var days = (zInfo.Tarih.Date - zInfo.BirthDate.Date).TotalDays;
            yasAy = (int)Math.Round(days / 30.4375, MidpointRounding.AwayFromZero);
        }

        var ent = new Antropometri
        {
            ZiyaretID = ziyaretId,
            YasAy = yasAy,                
            BoyCm = dto.BoyCm,
            KiloKg = dto.KiloKg,
            BasCevresiCm = dto.BasCevresiCm,
            OturmaBoyuCm = dto.OturmaBoyuCm
        };

        _db.Antropometriler.Add(ent);
        await _db.SaveChangesAsync();   

        await FillSdsAsync(ent);

        await _db.Entry(ent).ReloadAsync();

        return Ok(ent.AntropometriID);


    }

    // DELETE /api/antropometriler/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Antropometriler.FindAsync(id);
        if (entity is null) return NotFound();

        _db.Antropometriler.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static int CalcAgeMonths(DateTime birth, DateTime visit)
    {
        var months = (visit.Year - birth.Year) * 12 + (visit.Month - birth.Month);
        if (visit.Day < birth.Day) months -= 1;
        return Math.Max(0, months);
    }

    private async Task FillSdsAsync(Antropometri ent)
    {
        await _db.Entry(ent).Reference(a => a.Ziyaret).LoadAsync();
        await _db.Entry(ent.Ziyaret).Reference(z => z.Hasta).LoadAsync();

        var hasta = ent.Ziyaret.Hasta;

        ent.YasAy ??= CalcAgeMonths(
            hasta.BirthDate,
            ent.Ziyaret.Tarih);

        if (!ent.BoyCm.HasValue || !ent.KiloKg.HasValue)
            return;

        var sex = hasta.Cinsiyet == "E"
            ? "male"
            : "female";

        var ageYears = ent.YasAy.Value / 12.0;

        var results = await _cedd.CalculateResultsAsync(
            sex,
            ageYears,
            Convert.ToDouble(ent.BoyCm.Value),
            Convert.ToDouble(ent.KiloKg.Value),
            ent.BasCevresiCm.HasValue
            ? Convert.ToDouble(ent.BasCevresiCm.Value)
            : null
        );

        ent.BoySDS = (decimal?)results.FirstOrDefault(x => x.Label == "Height")?.Sds;

        ent.KiloSDS = (decimal?)results.FirstOrDefault(x => x.Label == "Weight")?.Sds;

        ent.BKISDS = (decimal?)results.FirstOrDefault(x => x.Label == "BMI")?.Sds;

        ent.BasCevresiSDS = (decimal?)results.FirstOrDefault(x => x.Label == "Head Circumference")?.Sds;
        await _db.SaveChangesAsync();
    }

    

}
