using AspNetCoreRateLimit;
using EconomiaComHistoria.API.Services;
using EconomiaComHistoria.API.Swagger;
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
using System.Net;
using System.Reflection;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/economia-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ─────────────────────────────────────────
// CONTROLLERS & API EXPLORER
// ─────────────────────────────────────────
builder.Services.AddControllers();
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

    // Adicionar suporte para exemplos de resposta
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
// AUTENTICAÇÃO JWT
// ─────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
            ClockSkew = TimeSpan.Zero  // sem tolerância extra na expiração
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────
// CORS
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    // Desenvolvimento — permissivo
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());

    // TODO Sprint 10: activar esta policy em produção
    options.AddPolicy("Producao", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ─────────────────────────────────────────
// UPLOAD DE FICHEIROS
// ─────────────────────────────────────────
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

builder.Services.AddInMemoryRateLimiting();

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ─────────────────────────────────────────
// CACHE
// ─────────────────────────────────────────
builder.Services.AddMemoryCache();

// ─────────────────────────────────────────
// REPOSITÓRIOS
// ─────────────────────────────────────────
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

// ─────────────────────────────────────────
// SERVICES — INFRASTRUCTURE
// ─────────────────────────────────────────
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Moderador", "Admin", "SuperAdmin"));
    // Adiciona outras políticas se necessário
});

// ─────────────────────────────────────────
// BACKGROUND JOBS
// ─────────────────────────────────────────
builder.Services.AddScoped<IGamificacaoService, GamificacaoService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IPlanoEstudoService, PlanoEstudoService>();
builder.Services.AddScoped<IEscolaService, EscolaService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddHostedService<WeeklyRankingJob>();

// ─────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────
var app = builder.Build();

// --- DIAGNÓSTICO TEMPORÁRIO — remove depois de resolver ---
try
{
    var assembly = typeof(Program).Assembly;
    var types = assembly.GetTypes();
}
catch (ReflectionTypeLoadException ex)
{
    foreach (var loaderEx in ex.LoaderExceptions)
    {
        Console.WriteLine("LOADER EXCEPTION: " + loaderEx?.Message);
    }
    throw;
}
// --- FIM DIAGNÓSTICO ---

// ─────────────────────────────────────────
// MIDDLEWARE PIPELINE
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Economia com História Angola API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz: https://localhost:PORT/
    });
}

app.UseHttpsRedirection();

// ─────────────────────────────────────────
// FORWARDED HEADERS (para proxies reversos confiáveis)
// ─────────────────────────────────────────
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};

// Configurar proxies conhecidos (adicionar IPs do seu proxy reverso)
// Exemplos: Azure App Service, Nginx local, etc.
if (app.Environment.IsDevelopment())
{
    // In development, trust localhost and loopback.
    forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("127.0.0.1"), 8));
    forwardedHeadersOptions.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("::1"), 128));
}
else
{
    // In production, only trust known proxies (e.g., Azure App Service, or a specific reverse proxy).
    // Clear the known networks list to avoid trusting any network.
    forwardedHeadersOptions.KnownNetworks.Clear();

    // Add known proxies from configuration.
    var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
    foreach (var proxy in knownProxies)
    {
        if (IPAddress.TryParse(proxy, out var ip))
        {
            forwardedHeadersOptions.KnownProxies.Add(ip);
        }
    }

    // Optionally, also add known networks if you have trusted subnets.
    var knownNetworks = builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>();
    foreach (var network in knownNetworks)
    {
        if (Microsoft.AspNetCore.HttpOverrides.IPNetwork.TryParse(network, out var ipNetwork))
        {
            forwardedHeadersOptions.KnownNetworks.Add(ipNetwork);
        }
    }
}

forwardedHeadersOptions.RequireHeaderSymmetry = false;
forwardedHeadersOptions.ForwardLimit = null;

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseRouting();
app.UseResponseCaching();

// ─────────────────────────────────────────
// HEADERS DE SEGURANÇA
// ─────────────────────────────────────────
app.Use(async (context, next) =>
{
    // HSTS (HTTP Strict-Transport-Security)
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

    // X-Frame-Options (contra clickjacking)
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

    // X-Content-Type-Options (contra MIME sniffing)
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Content-Security-Policy (CSP) — melhorado para remover 'unsafe-inline'
    // Para melhor segurança, considere usar nonces para scripts e styles inline
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' https:; " +
        "frame-src 'none'; " +
        "object-src 'none'; " +
        "upgrade-insecure-requests";

    // Referrer-Policy
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Permissions-Policy (Feature-Policy)
    context.Response.Headers["Permissions-Policy"] =
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

    await next();
});

app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Producao");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────
// INICIALIZAÇÃO DO BANCO DE DADOS (apenas desenvolvimento)
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
