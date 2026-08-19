using HastaTakip.Api.Data;
using HastaTakip.Api.Services;   // 🔴 BUNU EKLE
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//  Growth LMS
builder.Services.AddScoped<IGrowthLmsService, GrowthLmsService>();

//  ANTROPOMETRİ SDS SERVİSİ 
builder.Services.AddScoped<IAntropometriSdsService, AntropometriSdsService>();
builder.Services.AddSingleton<CeddService>();

// 1) DbContext
builder.Services.AddDbContext<HastaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection eksik.")
    )
);

// 2) Controllers + JSON 
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    o.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});

// 3) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5) Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    if (app.Configuration.GetValue<bool>("Swagger:Enabled", false))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
