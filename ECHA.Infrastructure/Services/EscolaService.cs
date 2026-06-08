using EconomiaComHistoria.Core.Entities;
using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EconomiaComHistoria.Infrastructure.Services;

public interface IEscolaService
{
    Task<InviteCodeResponseDto> GerarCodigoConviteAsync(int escolaId, int ttlDias = 7, CancellationToken ct = default);
    Task<bool> AssociarAlunoAsync(int utilizadorId, string codigo, CancellationToken ct = default);
    Task<List<EscolaResponseDto>> ListarEscolasAsync(CancellationToken ct = default);
}

public class EscolaService : IEscolaService
{
    private readonly AppDbContext _context;

    public EscolaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InviteCodeResponseDto> GerarCodigoConviteAsync(int escolaId, int ttlDias = 7, CancellationToken ct = default)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { escolaId }, ct);
        if (escola == null) throw new Exception("Escola não encontrada");

        var codigo = GenerateRandomCode(8);
        escola.CodigoConvite = codigo;
        escola.CodigoConviteExpiracao = DateTime.UtcNow.AddDays(ttlDias);

        await _context.SaveChangesAsync(ct);

        return new InviteCodeResponseDto(codigo, escola.CodigoConviteExpiracao);
    }

    public async Task<bool> AssociarAlunoAsync(int utilizadorId, string codigo, CancellationToken ct = default)
    {
        var escola = await _context.Escolas.FirstOrDefaultAsync(e => e.CodigoConvite == codigo, ct);
        if (escola == null || escola.CodigoConviteExpiracao < DateTime.UtcNow) return false;

        var user = await _context.Utilizadores.FindAsync(new object[] { utilizadorId }, ct);
        if (user == null) return false;

        user.EscolaId = escola.Id;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<EscolaResponseDto>> ListarEscolasAsync(CancellationToken ct = default)
    {
        return await _context.Escolas
            .Select(e => new EscolaResponseDto(
                e.Id,
                e.Nome,
                null, // CodigoMEC not in entity
                e.Provincia,
                e.Municipio, // Using Municipio instead of Localizacao
                e.CodigoConvite,
                e.CodigoConviteExpiracao,
                e.Alunos.Count,
                e.Turmas.Count))
            .ToListAsync(ct);
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
    }
}
