using EconomiaComHistoria.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomiaComHistoria.Core.Interfaces;

public interface IEscolaService
{
    Task<InviteCodeResponseDto> GerarCodigoConviteAsync(int escolaId, int ttlDias = 7, CancellationToken ct = default);
    Task<bool> AssociarAlunoAsync(int utilizadorId, string codigo, CancellationToken ct = default);
    Task<List<EscolaResponseDto>> ListarEscolasAsync(CancellationToken ct = default);
}