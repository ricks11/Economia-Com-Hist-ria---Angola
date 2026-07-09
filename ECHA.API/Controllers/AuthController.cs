using EconomiaComHistoria.Core.Enums;
using EconomiaComHistoria.Core.DTOs;
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
    private readonly IEmailService _emailService;

    // TODO Sprint 8: substituir por RefreshToken persistido na base de dados
    private static readonly Dictionary<string, string> RefreshTokenStore = new(); // Simple in-memory store

    public AuthController(AppDbContext dbContext, IAuthService authService, IEmailService emailService)
    {
        _dbContext = dbContext;
        _authService = authService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest(new { message = "Email, Password, and Nome are required." });
        }

        var existingUser = await _dbContext.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var passwordHash = _authService.HashPassword(request.Password);
        var newUser = new EconomiaComHistoria.Core.Entities.Utilizador
        {
            Nome = request.Nome,
            Email = request.Email,
            Telemovel = request.Telemovel,
            PasswordHash = passwordHash,
            Tipo = TipoUtilizador.Registado,
            DataRegisto = DateTime.UtcNow,
            PontosTotais = 0,
            StreakAtual = 0
        };

        _dbContext.Utilizadores.Add(newUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = _authService.GenerateAccessToken(newUser.Id, newUser.Email, newUser.Tipo.ToString());
        var refreshToken = _authService.GenerateRefreshToken();
        RefreshTokenStore[refreshToken] = newUser.Id.ToString();

        var response = new AuthResponseDto(
            newUser.Id,
            newUser.Email,
            newUser.Nome,
            accessToken,
            refreshToken,
            _authService.GetAccessTokenExpiration(),
            newUser.Tipo.ToString());

        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and Password are required." });
        }

        var user = await _dbContext.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email ou password inválidos." });

        // ====================================================================
        // VERIFICAÇÃO DE BLOQUEIO (BANIMENTO / SUSPENSÃO)
        // ====================================================================
        if (user.Suspenso)
        {
            if (user.SuspensaoPermanente)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Esta conta foi banida permanentemente da plataforma." });
            }

            if (user.SuspensoAte.HasValue && user.SuspensoAte.Value > DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = $"Esta conta encontra-se suspensa até {user.SuspensoAte.Value.ToString("dd/MM/yyyy HH:mm")}."
                });
            }
        }

        user.UltimoAcesso = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = _authService.GenerateAccessToken(user.Id, user.Email, user.Tipo.ToString());
        var refreshToken = _authService.GenerateRefreshToken();
        RefreshTokenStore[refreshToken] = user.Id.ToString();

        var response = new AuthResponseDto(
            user.Id,
            user.Email,
            user.Nome,
            accessToken,
            refreshToken,
            _authService.GetAccessTokenExpiration(),
            user.Tipo.ToString());

        return Ok(response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
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

        // ====================================================================
        // EXPULSAR UTILIZADOR NO REFRESH SE ELE FOI BANIDO ENQUANTO NAVEGAVA
        // ====================================================================
        if (user.Suspenso)
        {
            RefreshTokenStore.Remove(request.RefreshToken); // Limpa o token inválido
            if (user.SuspensaoPermanente || (user.SuspensoAte.HasValue && user.SuspensoAte.Value > DateTime.UtcNow))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Acesso negado. A sua conta foi suspensa ou banida." });
            }
        }

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
            _authService.GetAccessTokenExpiration(),
            user.Tipo.ToString());

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "O email é obrigatório." });
        }

        var user = await _dbContext.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is not null && !user.Suspenso) // Impede envio de reset para banidos
        {
            var resetToken = Guid.NewGuid().ToString();
            await _emailService.SendResetPasswordLinkAsync(user.Email, resetToken);
        }

        return Ok(new { message = "Se o email introduzido estiver registado, receberá um link de recuperação em breve." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG RESET] Email recebido: '{request.Email}', Password vazia?: {string.IsNullOrWhiteSpace(request.NewPassword)}");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "O email e a nova palavra-passe são obrigatórios." });
        }

        var user = await _dbContext.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return BadRequest(new { message = "Utilizador não encontrado no sistema." });
        }

        try
        {
            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Palavra-passe alterada com sucesso." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO GRAVAÇÃO] Falha ao atualizar password: {ex.Message}");
            return StatusCode(500, new { message = "Erro interno ao atualizar a base de dados." });
        }
    }
}