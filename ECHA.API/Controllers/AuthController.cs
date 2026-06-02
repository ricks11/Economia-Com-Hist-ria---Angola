using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.API.DTOs;
using EconomiaComHistoria.Core.Interfaces;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EconomiaComHistoria.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAuthService _authService;
    private static readonly Dictionary<string, string> RefreshTokenStore = new(); // Simple in-memory store

    public AuthController(AppDbContext dbContext, IAuthService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest(new { message = "Email, Password, and Nome are required." });
        }

        // Check if email already exists
        var existingUser = await _dbContext.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        // Hash password and create user
        var passwordHash = _authService.HashPassword(request.Password);
        var newUser = new EconomiaComHistoria.Core.Entities.Utilizador
        {
            Nome = request.Nome,
            Email = request.Email,
            Telemovel = request.Telemovel,
            PasswordHash = passwordHash,
            Tipo = TipoUtilizador.Estudante,
            DataRegisto = DateTime.UtcNow,
            PontosTotais = 0,
            StreakAtual = 0
        };

        _dbContext.Utilizadores.Add(newUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _authService.GenerateAccessToken(newUser.Id, newUser.Email, newUser.Tipo.ToString());
        var refreshToken = _authService.GenerateRefreshToken();
        RefreshTokenStore[refreshToken] = newUser.Id.ToString();

        var response = new AuthResponseDto(
            newUser.Id,
            newUser.Email,
            newUser.Nome,
            accessToken,
            refreshToken,
            _authService.GetAccessTokenExpiration());

        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        var user = await _dbContext.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Verify user exists and password is correct
        if (user is null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Generate tokens
        var accessToken = _authService.GenerateAccessToken(user.Id, user.Email, user.Tipo.ToString());
        var refreshToken = _authService.GenerateRefreshToken();
        RefreshTokenStore[refreshToken] = user.Id.ToString();

        var response = new AuthResponseDto(
            user.Id,
            user.Email,
            user.Nome,
            accessToken,
            refreshToken,
            _authService.GetAccessTokenExpiration());

        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        // Validate refresh token
        if (!RefreshTokenStore.TryGetValue(request.RefreshToken, out var userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        var user = await _dbContext.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "User not found." });
        }

        // Remove old refresh token and generate new ones
        RefreshTokenStore.Remove(request.RefreshToken);
        var newAccessToken = _authService.GenerateAccessToken(user.Id, user.Email, user.Tipo.ToString());
        var newRefreshToken = _authService.GenerateRefreshToken();
        RefreshTokenStore[newRefreshToken] = user.Id.ToString();

        var response = new AuthResponseDto(
            user.Id,
            user.Email,
            user.Nome,
            newAccessToken,
            newRefreshToken,
            _authService.GetAccessTokenExpiration());

        return Ok(response);
    }
}
