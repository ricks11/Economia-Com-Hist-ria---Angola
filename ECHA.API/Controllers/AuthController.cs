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
    // campo RefreshToken e RefreshTokenExpiry na entidade Utilizador
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
            Tipo = TipoUtilizador.Registado,
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
            _authService.GetAccessTokenExpiration(),
            newUser.Tipo.ToString());

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
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email ou password inválidos." });

        user.UltimoAcesso = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

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
            _authService.GetAccessTokenExpiration(),
            user.Tipo.ToString());

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
            _authService.GetAccessTokenExpiration(),
            user.Tipo.ToString());

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        // 1. Validar se o input não é nulo ou vazio
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "O email é obrigatório." });
        }

        // 2. Procurar o utilizador na base de dados
        var user = await _dbContext.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // 3. Se o utilizador existir, geramos o token e disparamos o email real
        if (user is not null)
        {
            var resetToken = Guid.NewGuid().ToString();

            // Opcional Sprint 8/9: Persistir o token se tiveres campos dedicados na entidade Utilizador
            // user.ResetToken = resetToken;
            // user.ResetTokenExpiry = DateTime.UtcNow.AddHours(2);
            // await _dbContext.SaveChangesAsync(cancellationToken);

            await _emailService.SendResetPasswordLinkAsync(user.Email, resetToken);
        }

        // 4. Retorna sempre Ok por questões de segurança (Impede atacantes de descobrirem emails válidos)
        return Ok(new { message = "Se o email introduzido estiver registado, receberá um link de recuperação em breve." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        // Log de diagnóstico para ver o que o Front-end está a enviar
        Console.WriteLine($"[DEBUG RESET] Email recebido: '{request.Email}', Password vazia?: {string.IsNullOrWhiteSpace(request.NewPassword)}");

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "O email e a nova palavra-passe são obrigatórios." });
        }

        // 1. Procura o utilizador de forma explícita pelo Email
        var user = await _dbContext.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            // Se entrar aqui, dá 400 mas NÃO apaga nada porque o utilizador nem foi encontrado
            return BadRequest(new { message = "Utilizador não encontrado no sistema." });
        }

        try
        {
            // 2. Faz APENAS a atualização da propriedade PasswordHash
            user.PasswordHash = _authService.HashPassword(request.NewPassword);

            // Garante que o Entity Framework sabe que isto é uma MODIFICAÇÃO e não uma remoção/inserção
            _dbContext.Entry(user).State = EntityState.Modified;

            // 3. Grava apenas a alteração
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
