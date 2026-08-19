using HastaTakip.UI.Dtos;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

namespace HastaTakip.UI.Services;

public class HastaApi
{
    private const string ZIYARET_POST = "Ziyaretler";        
    private const string ANTRO_POST = "Antropometriler";     
    private const string OZET_BASE = "OzetHesaplar";         
    private readonly HttpClient _http;

    public HastaApi(IHttpClientFactory factory)
        => _http = factory.CreateClient("HastaApi");

   
    public async Task<List<AntropometriListDto>> GetAnthroByVisitAsync(int ziyaretId, CancellationToken ct = default)
    {
        var rel = $"Antropometriler/by-visit/{ziyaretId}";
        var list = await _http.GetFromJsonAsync<List<AntropometriListDto>>(rel, ct);
        return list ?? new List<AntropometriListDto>();
    }
    
    public async Task<int> CreateAntropometriForVisitAsync(int ziyaretId, AntropometriCreateDto dto)
    {
        var resp = await _http.PostAsJsonAsync($"Antropometriler/by-visit/{ziyaretId}", dto);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<int>();
    }
    
    public Task<List<PatientListDto>?> GetPatientsAsync()
        => _http.GetFromJsonAsync<List<PatientListDto>>("Hastalar");

    public async Task<PagedResult<PatientListDto>> GetPatientsPagedAsync(
    int page, int pageSize, string q = "", string? sort = null, bool asc = true)
    {
        
        string url = $"Hastalar/paged?page={page}&pageSize={pageSize}&q={Uri.EscapeDataString(q ?? "")}";
        if (!string.IsNullOrEmpty(sort))
            url += $"&sort={sort}&dir={(asc ? "asc" : "desc")}";

        return await _http.GetFromJsonAsync<PagedResult<PatientListDto>>(url)
               ?? new PagedResult<PatientListDto>();
    }
    public Task<PatientDetailDto?> GetPatientAsync(int id)
        => _http.GetFromJsonAsync<PatientDetailDto>($"Hastalar/{id}");

    public Task<PatientSummaryDto?> GetPatientSummaryAsync(int id)
        => _http.GetFromJsonAsync<PatientSummaryDto>($"Hastalar/{id}/ozet");

    public async Task<PagedResult<ZiyaretDetayDto>> GetPatientVisitsDetailAsync(
        int hastaId, int page, int pageSize, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var qs = $"page={page}&pageSize={pageSize}";
        if (dateFrom.HasValue) qs += $"&dateFrom={Uri.EscapeDataString(dateFrom.Value.ToString("yyyy-MM-dd"))}";
        if (dateTo.HasValue) qs += $"&dateTo={Uri.EscapeDataString(dateTo.Value.ToString("yyyy-MM-dd"))}";

        var url = $"Hastalar/{hastaId}/ziyaretler-detay?{qs}";
        var res = await _http.GetFromJsonAsync<PagedResult<ZiyaretDetayDto>>(url);
        return res ?? new PagedResult<ZiyaretDetayDto> { Page = page, PageSize = pageSize, Items = new() };
    }

    // GET /api/Hastalar/by-tc/{tc}
    public Task<PatientDetailDto?> GetPatientByTcAsync(string tc)
        => _http.GetFromJsonAsync<PatientDetailDto>($"Hastalar/by-tc/{Uri.EscapeDataString(tc)}");

    // GET /api/Health/db
    public Task<Dictionary<string, object>?> HealthAsync()
        => _http.GetFromJsonAsync<Dictionary<string, object>>("Health/db");

    //  GET/api/Hastalar/{id}/paket  → PatientBundleDto
    public async Task<PatientBundleDto?> GetPatientBundleAsync(int id, CancellationToken ct = default)
    {
        var rel = $"Hastalar/{id}/paket";
        var resp = await _http.GetAsync(rel, ct);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;

        resp.EnsureSuccessStatusCode();

        var jsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return await resp.Content.ReadFromJsonAsync<PatientBundleDto>(jsonOpts, ct);
    }

    // POST/api/Hastalar/register
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto body, CancellationToken ct = default)
    {
        var rel = "Hastalar/register";
        var full = new Uri(_http.BaseAddress!, rel);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var res = await _http.PostAsJsonAsync(rel, body, options, ct);
        var text = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} on {full} -> {text}");

        var dto = JsonSerializer.Deserialize<RegisterResponseDto>(text, options);
        if (dto is null)
            throw new Exception("Register yanıtı çözümlenemedi.");

        return dto;
    }
    // POST/api/Ziyaretler
    public async Task<int> CreateVisitAsync(ZiyaretCreateDto dto, CancellationToken ct = default)
    {
        var rel = ZIYARET_POST;
        var full = new Uri(_http.BaseAddress!, rel);
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} on {full} -> {await res.Content.ReadAsStringAsync(ct)}");

        var created = await res.Content.ReadFromJsonAsync<Dictionary<string, int>>(cancellationToken: ct);
        return created != null && created.TryGetValue("id", out var id) ? id : 0;
    }
    // POST/api/Antropometriler
    public async Task<int> CreateAnthroAsync(AntropometriCreateDto dto, CancellationToken ct = default)
    {
        var rel = ANTRO_POST;
        var full = new Uri(_http.BaseAddress!, rel);
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} on {full} -> {await res.Content.ReadAsStringAsync(ct)}");

        var created = await res.Content.ReadFromJsonAsync<Dictionary<string, int>>(cancellationToken: ct);
        return created != null && created.TryGetValue("id", out var id) ? id : 0;
    }
    // PUT/api/Hastalar/{id}
    public async Task UpdatePatientAsync(int id, PatientUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"Hastalar/{id}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} PUT {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
    // DELETE /api/Hastalar/{id}
    public async Task DeletePatientAsync(int id, CancellationToken ct = default)
    {
        var rel = $"Hastalar/{id}";
        var res = await _http.DeleteAsync(rel, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} DELETE {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
   
    // GET/api/OzetHesaplar/by-ziyaret/{ziyaretId}
    public async Task<List<OzetHesapDto>> GetOzetByZiyaretAsync(int ziyaretId, CancellationToken ct = default)
    {
        var rel = $"{OZET_BASE}/by-visit/{ziyaretId}";
        var list = await _http.GetFromJsonAsync<List<OzetHesapDto>>(rel, ct);
        return list ?? new List<OzetHesapDto>();
    }

    // POST /api/OzetHesaplar
    public async Task<int> CreateOzetAsync(OzetHesapCreateDto dto, CancellationToken ct = default)
    {
        var rel = OZET_BASE;
        var full = new Uri(_http.BaseAddress!, rel);
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} on {full} -> {await res.Content.ReadAsStringAsync(ct)}");

        var created = await res.Content.ReadFromJsonAsync<OzetHesapDto>(cancellationToken: ct);
        return created?.OzetID ?? 0;
    }
    // DELETE /api/Hastalar/{id}/tam-sil  → tüm ilişkilerle sil
    public async Task DeletePatientCascadeAsync(int id, CancellationToken ct = default)
    {
        var rel = $"Hastalar/{id}/tam-sil";
        var res = await _http.DeleteAsync(rel, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} DELETE {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
    // ÖYKÜ 
    public async Task<int> CreateOykuAsync(OykulerCreateDto dto, CancellationToken ct = default)
    {
        var rel = "Oykuler";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} POST {rel} -> {await res.Content.ReadAsStringAsync(ct)}");

        
        var created = await res.Content.ReadFromJsonAsync<OykulerDetailDto>(cancellationToken: ct);
        return created?.OykulerID ?? 0;
    }
    public async Task UpdateOykuAsync(int oykulerId, OykulerUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"Oykuler/{oykulerId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} PUT {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
    // Diyet -Ekle
    public async Task<int> CreateDiyetAsync(DiyetCreateDto dto, CancellationToken ct = default)
    {
        var rel = "Diyetler";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} POST {rel} -> {await res.Content.ReadAsStringAsync(ct)}");

        var created = await res.Content.ReadFromJsonAsync<DiyetDetailDto>(cancellationToken: ct);
        return created?.DiyetID ?? 0;
    }

    // DİYET — güncelle/sil
    public async Task UpdateDiyetAsync(int diyetId, DiyetUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"Diyetler/{diyetId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} PUT {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }

    public async Task DeleteDiyetAsync(int diyetId, CancellationToken ct = default)
    {
        var rel = $"Diyetler/{diyetId}";
        var res = await _http.DeleteAsync(rel, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} DELETE {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
    // Aile Üyesi — Ekle
    public async Task<int> CreateAileUyesiAsync(AileUyesiCreateDto dto, CancellationToken ct = default)
    {
        var rel = "AileUyeleri";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} POST {rel} -> {await res.Content.ReadAsStringAsync(ct)}");

        
        var created = await res.Content.ReadFromJsonAsync<AileUyesiListDto>(cancellationToken: ct);
        return created?.AileUyesiID ?? 0;
    }
    // Aile Üyesi — Güncelle
    public async Task UpdateAileUyesiAsync(int aileUyesiId, AileUyesiUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"AileUyeleri/{aileUyesiId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} PUT {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
    // Aile Üyesi — Sil
    public async Task DeleteAileUyesiAsync(int aileUyesiId, CancellationToken ct = default)
    {
        var rel = $"AileUyeleri/{aileUyesiId}";
        var res = await _http.DeleteAsync(rel, ct);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)res.StatusCode} DELETE {rel} -> {await res.Content.ReadAsStringAsync(ct)}");
    }
    public async Task<ZiyaretBundleDto> GetZiyaretBundleAsync(int ziyaretId, CancellationToken ct = default)
    {
        var rel = $"Ziyaretler/{ziyaretId}/bundle"; 
        var dto = await _http.GetFromJsonAsync<ZiyaretBundleDto>(rel, ct);
        if (dto is null) throw new Exception("Ziyaret bulunamadı.");
        return dto;
    }
    public async Task<int> CreateVisitBundleAsync(ZiyaretBundleCreateDto dto, CancellationToken ct = default)
    {
        var rel = "Ziyaretler/bundle";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
        var created = await res.Content.ReadFromJsonAsync<Dictionary<string, int>>(cancellationToken: ct);
        return created != null && created.TryGetValue("id", out var id) ? id : 0;
    }
    public async Task<List<AntropometriListDto>> GetAnthroByHastaAsync(int hastaId, CancellationToken ct = default)
    {
        var rel = $"Antropometriler/by-hasta/{hastaId}";
        var list = await _http.GetFromJsonAsync<List<AntropometriListDto>>(rel, ct);
        return list ?? new List<AntropometriListDto>();
    }
    
    public async Task<List<Dictionary<string, object?>>> GetGrowthRefsAsync(
     string measure,
     char sex,
     int? maxMonth = null,
     int step = 1,
     CancellationToken ct = default)
    {
        var url = $"Growth/refs?measure={measure}&sex={sex}&step={step}";
        if (maxMonth.HasValue)
            url += $"&maxMonth={maxMonth.Value}";

        return await _http.GetFromJsonAsync<List<Dictionary<string, object?>>>(url, ct)
               ?? new();
    }
    
    public async Task UpdateAnthroAsync(int antropometriId, AntropometriUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"Antropometriler/{antropometriId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
    }

    
    public async Task<int> CreatePuberteAsync(int ziyaretId, PuberteFizikCreateDto dto, CancellationToken ct = default)
    {
        var rel = $"PuberteFizikler/by-visit/{ziyaretId}";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();

        
        var id = await res.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        return id;
    }
    //  PUT /api/PuberteFizikler/{id}
    public async Task UpdatePuberteAsync(int puberteId, PuberteFizikUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"PuberteFizikler/{puberteId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
    }

    //  POST /api/YorumPlanlar/by-visit/{ziyaretId}
    public async Task<int> CreateYorumPlanAsync(int ziyaretId, YorumPlanCreateDto dto, CancellationToken ct = default)
    {
        var rel = $"YorumPlanlar/by-visit/{ziyaretId}";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();

        
        var id = await res.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        return id;
    }

    //  PUT /api/YorumPlanlar/{id}
    public async Task UpdateYorumPlanAsync(int yorumPlanId, YorumPlanUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"YorumPlanlar/{yorumPlanId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
    }
    //  PUT /api/OzetHesaplar/{id}
    public async Task UpdateOzetAsync(int ozetId, OzetHesapUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"OzetHesaplar/{ozetId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
    }

    // (opsiyonel) POST /api/OzetHesaplar  — by-visit kısayolu
    public async Task<int> CreateOzetForVisitAsync(int ziyaretId, OzetHesapCreateDto dto, CancellationToken ct = default)
    {
        dto.ZiyaretID = ziyaretId;                
        var rel = "OzetHesaplar";
        var res = await _http.PostAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
        var created = await res.Content.ReadFromJsonAsync<OzetHesapDto>(cancellationToken: ct);
        return created?.OzetID ?? 0;
    }
    public async Task UpdateVisitAsync(int ziyaretId, ZiyaretUpdateDto dto, CancellationToken ct = default)
    {
        var rel = $"Ziyaretler/{ziyaretId}";
        var res = await _http.PutAsJsonAsync(rel, dto, ct);
        res.EnsureSuccessStatusCode();
    }


    public async Task<LabTableDto> GetLabByZiyaretAsync(int ziyaretId)
    {
        return await _http.GetFromJsonAsync<LabTableDto>(
            $"lab/by-ziyaret?ziyaretId={ziyaretId}"
        ) ?? new LabTableDto();
    }

    public async Task<List<LabParametreDto>> GetLabParametrelerAsync()
    {
        return await _http.GetFromJsonAsync<List<LabParametreDto>>(
            "lab/parametreler"
        ) ?? new();
    }

    public async Task CreateLabForZiyaretAsync(
    int ziyaretId,
    LabSonucCreateDto dto)
    {
        var res = await _http.PostAsJsonAsync(
            $"lab/by-ziyaret/{ziyaretId}", dto);

        res.EnsureSuccessStatusCode();
    }
    public async Task<LoginResultDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("Auth/login", dto, ct);

        if (!res.IsSuccessStatusCode)
            return null;

        return await res.Content.ReadFromJsonAsync<LoginResultDto>(cancellationToken: ct);
    }

    
    public async Task<List<LabSonucDetailDto>> GetLabResultsForVisitAsync(int ziyaretId)
    {
        return await _http.GetFromJsonAsync<List<LabSonucDetailDto>>($"lab/by-ziyaret/{ziyaretId}/list")
               ?? new List<LabSonucDetailDto>();
    }

    
    public async Task UpsertLabResultsForVisitAsync(int ziyaretId, LabForVisitBulkDto dto)
    {
        var res = await _http.PostAsJsonAsync($"lab/by-ziyaret/{ziyaretId}/bulk", dto);
        res.EnsureSuccessStatusCode();
    }
    public async Task<List<CeddResultDto>> GetCeddCalculationAsync(
     string sex,
     double age,
     double height,
     double weight,
     CancellationToken ct = default)
    {
        var url =
            $"Cedd/calculate" +
            $"?sex={Uri.EscapeDataString(sex)}" +
            $"&age={age.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&height={height.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&weight={weight.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        return await _http.GetFromJsonAsync<List<CeddResultDto>>(url, ct)
               ?? new List<CeddResultDto>();
    }
}
