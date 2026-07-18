using EconomiaComHistoria.Core.DTOs;
using EconomiaComHistoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using EconomiaComHistoria.Core.Interfaces;

namespace EconomiaComHistoria.Infrastructure.Services;

public class EscolaService : IEscolaService
{
    private readonly AppDbContext _context;

    public EscolaService(AppDbContext context) => _context = context;

    public async Task<InviteCodeResponseDto> GerarCodigoConviteAsync(int escolaId, int ttlDias = 7, CancellationToken ct = default)
    {
        var escola = await _context.Escolas.FindAsync(new object[] { escolaId }, ct);
        if (escola == null) throw new Exception("Escola não encontrada");

        var codigo = GenerateRandomCode(8);
        escola.CodigoConvite = codigo;
        escola.CodigoConviteExpiracao = DateTime.UtcNow.AddDays(ttlDias);

        await _context.SaveChangesAsync(ct);

        return new InviteCodeResponseDto(codigo, escola.CodigoConviteExpiracao.Value);
    }

    public async Task<bool> AssociarAlunoAsync(int utilizadorId, string codigo, CancellationToken ct = default)
    {
        var escola = await _context.Escolas
            .FirstOrDefaultAsync(e => e.CodigoConvite == codigo && e.CodigoConviteExpiracao > DateTime.UtcNow, ct);
        if (escola == null) return false;

        var user = await _context.Utilizadores.FindAsync(new object[] { utilizadorId }, ct);
        if (user == null) return false;

        user.EscolaId = escola.Id;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<EscolaResponseDto>> ListarEscolasAsync(CancellationToken ct = default)
    {
        return await _context.Escolas
            .Include(e => e.Turmas)
            .Include(e => e.Alunos)
            .Select(e => new EscolaResponseDto(
                e.Id,
                e.Nome,
                e.CodigoMEC,
                e.Provincia,
                e.Municipio,          // Mapeia para Localizacao
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