using PortalDeCurriculos.Api.Data;
using PortalDeCurriculos.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();

var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();   // erros inesperados viram ProblemDetails (JSON) em vez de stack trace
app.UseCors();
app.MapControllers();

app.Run();

public partial class Program { }
