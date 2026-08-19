namespace HastaTakip.Api.Services;
using System.Diagnostics;
using System.Text.Json;
using HastaTakip.Api.Dtos;

public class CeddService
{
    private readonly string _ceddScriptPath;

    public CeddService(IWebHostEnvironment environment)
    {_ceddScriptPath = Path.Combine(
            environment.ContentRootPath,
            "node_modules",
            "ceddcozum",
            "dist",
            "index.js");
    }
    public async Task<string> CalculateAsync(
    string sex,
    double age,
    double height,
    double weight,
    double? headCircumference)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(_ceddScriptPath);
        startInfo.ArgumentList.Add("auxology");
        startInfo.ArgumentList.Add($"sex={sex}");
        startInfo.ArgumentList.Add($"age={age.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        startInfo.ArgumentList.Add($"height={height.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        startInfo.ArgumentList.Add($"weight={weight.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        if (headCircumference.HasValue)
        {
            startInfo.ArgumentList.Add($"headCircumference={headCircumference.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        startInfo.ArgumentList.Add("--format");
        startInfo.ArgumentList.Add("json");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("ÇEDD işlemi başlatılamadı.");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(error);

        return output;
    }
    public async Task<List<CeddResultDto>> CalculateResultsAsync(
    string sex,
    double age,
    double height,
    double weight,
    double? headCircumference)
    {
        var json = await CalculateAsync(sex, age, height, weight, headCircumference);

        return JsonSerializer.Deserialize<List<CeddResultDto>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CeddResultDto>();
    }
}