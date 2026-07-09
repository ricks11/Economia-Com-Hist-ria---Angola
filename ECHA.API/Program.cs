using AspNetCoreRateLimit;
using EconomiaComHistoria.API.Services;
using EconomiaComHistoria.API.Swagger;
using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using EconomiaComHistoria.Infrastructure.Repositories;
using EconomiaComHistoria.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/economia-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ─────────────────────────────────────────
// CONTROLLERS & API EXPLORER
// ─────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// ─────────────────────────────────────────
// SWAGGER
// ─────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Economia com História Angola API",
        Version = "v1",
        Description = "API da plataforma de educação — Tavares Royale, Inc."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insere o token JWT no formato: Bearer {token}"
    });
});

// ─────────────────────────────────────────
// BASE DE DADOS
// ─────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: Array.Empty<int>())));

// ─────────────────────────────────────────
// AUTENTICAÇÃO JWT (Corrigido para mapear Roles)
// ─────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,

            // Força o ASP.NET Core a usar a URI correta para ler os Roles das claims
            RoleClaimType = ClaimTypes.Role
        };
    });

// ─────────────────────────────────────────
// AUTORIZAÇÃO (Políticas)
// ─────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Moderador", "Admin", "SuperAdmin"));
});

// ─────────────────────────────────────────
// CORS
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());

    options.AddPolicy("Producao", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ─────────────────────────────────────────
// CONFIGURAÇÕES ADICIONAIS & REPOSITÓRIOS
// ─────────────────────────────────────────
builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; });
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddMemoryCache();

// Repositórios Scoped
builder.Services.AddScoped<IUtilizadorRepository, UtilizadorRepository>();
builder.Services.AddScoped<IConteudoRepository, ConteudoRepository>();
builder.Services.AddScoped<IVisualizacaoRepository, VisualizacaoRepository>();
builder.Services.AddScoped<IConteudoFavoritoRepository, ConteudoFavoritoRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<ITentativaQuizRepository, TentativaQuizRepository>();
builder.Services.AddScoped<ITopicoForumRepository, TopicoForumRepository>();
builder.Services.AddScoped<IRespostaForumRepository, RespostaForumRepository>();
builder.Services.AddScoped<IDenunciaRepository, DenunciaRepository>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<IEventoGamificacaoRepository, EventoGamificacaoRepository>();
builder.Services.AddScoped<IPlanoEstudoRepository, PlanoEstudoRepository>();
builder.Services.AddScoped<IEscolaRepository, EscolaRepository>();
builder.Services.AddScoped<ITurmaRepository, TurmaRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();
builder.Services.AddScoped<ISolicitacaoAcessoRepository, SolicitacaoAcessoRepository>();
builder.Services.AddScoped<IPropostaQuizRepository, PropostaQuizRepository>();
builder.Services.AddScoped<IAuditoriaLogRepository, AuditoriaLogRepository>();

// Serviços Scoped
builder.Services.AddScoped<IAuthService, BCryptAuthService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IQuizScoringService, QuizScoringService>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<IModeracaoService, ModeracaoService>();
builder.Services.AddScoped<INotificacaoService, FirebaseNotificacaoService>();
builder.Services.AddScoped<ISincronizacaoService, SincronizacaoService>();
builder.Services.AddScoped<IValidadorSincronizacao, ValidadorSincronizacao>();
builder.Services.AddScoped<IConteudoCacheExportService, ConteudoCacheExportService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGamificacaoService, GamificacaoService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IPlanoEstudoService, PlanoEstudoService>();
builder.Services.AddScoped<IEscolaService, EscolaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddHostedService<WeeklyRankingJob>();

var app = builder.Build();

// Diagnóstico de tipos de compilação
try { var types = typeof(Program).Assembly.GetTypes(); }
catch (ReflectionTypeLoadException ex)
{
    foreach (var loaderEx in ex.LoaderExceptions) { Console.WriteLine("LOADER EXCEPTION: " + loaderEx?.Message); }
    throw;
}

// ─────────────────────────────────────────
// MIDDLEWARE PIPELINE (Ordem Corrigida)
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Economia com História Angola API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Headers encaminhados de proxies
var forwardedHeadersOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto };
if (app.Environment.IsDevelopment())
{
    forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("127.0.0.1"), 8));
    forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("::1"), 128));
}
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseRouting();

// CORS e Rate Limiting devem vir antes de autenticar e autorizar
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Producao");
app.UseIpRateLimiting();
app.UseResponseCaching();

// Headers de Segurança customizados
app.Use(async (context, next) =>
{
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https:; frame-src 'none'; object-src 'none'; upgrade-insecure-requests";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
    await next();
});

// Autenticação primeiro, Autorização a seguir
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────
// INICIALIZAÇÃO DO BANCO DE DADOS & SEED
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // SEED ADMIN
    var adminEmail = config["SeedAdmin:Email"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !await db.Utilizadores.AnyAsync(u => u.Email == adminEmail))
    {
        db.Utilizadores.Add(new EconomiaComHistoria.Core.Entities.Utilizador
        {
            Nome = config["SeedAdmin:Nome"] ?? "Administrador",
            Email = adminEmail,
            PasswordHash = authService.HashPassword(config["SeedAdmin:Password"]!),
            Tipo = TipoUtilizador.Admin,
            DataRegisto = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    // SEED SUPER ADMIN
    var superAdminEmail = config["SeedSuperAdmin:Email"];
    if (!string.IsNullOrWhiteSpace(superAdminEmail) && !await db.Utilizadores.AnyAsync(u => u.Email == superAdminEmail))
    {
        db.Utilizadores.Add(new EconomiaComHistoria.Core.Entities.Utilizador
        {
            Nome = config["SeedSuperAdmin:Nome"] ?? "Super Administrador",
            Email = superAdminEmail,
            PasswordHash = authService.HashPassword(config["SeedSuperAdmin:Password"]!),
            Tipo = TipoUtilizador.SuperAdmin,
            DataRegisto = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

app.Run();