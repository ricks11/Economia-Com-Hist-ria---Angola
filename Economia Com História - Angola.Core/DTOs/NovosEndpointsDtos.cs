namespace EconomiaComHistoria.Core.DTOs;

// ─── Dashboard do Professor ──────────────────────────────────────────────────

public record ProfessorDashboardDto(
    int TotalAlunos,
    int TotalTurmas,
    int QuizzesAtivos,
    double MediaPontosTurmas,
    List<TurmaResumoDto> Turmas,
    List<AlunoAtividadeRecenteDto> AlunosRecentes
);

public record TurmaResumoDto(
    int Id,
    string Nome,
    int TotalAlunos,
    double MediaPontos,
    string? Ano
);

public record AlunoAtividadeRecenteDto(
    int Id,
    string Nome,
    int PontosTotais,
    DateTime? UltimaAtividade
);

// ─── Progresso no Mapa por Província ────────────────────────────────────────

public record MapaProgressoDto(
    List<ProvinciaProgressoDto> Provincias
);

public record ProvinciaProgressoDto(
    string ProvinciaId,
    string NomeProvincia,
    double PercentualExplorado,
    int ConteudosVistos,
    int TotalConteudos
);

// ─── Ranking de uma Turma específica ────────────────────────────────────────

public record TurmaRankingResponseDto(
    int TurmaId,
    string TurmaNome,
    List<TurmaRankingEntradaDto> Entradas,
    int PosicaoUtilizador
);

public record TurmaRankingEntradaDto(
    int Posicao,
    int AlunoId,
    string NomeAluno,
    int Pontos,
    int QuizzesCompletados,
    bool IsCurrentUser
);

// ─── Notificações ────────────────────────────────────────────────────────────

public record NotificacaoDto(
    int Id,
    string Titulo,
    string Mensagem,
    bool Lida,
    DateTime DataCriacao,
    string? Tipo // "quiz", "badge", "ranking", "sistema"
);

// ─── Plano de Estudo ─────────────────────────────────────────────────────────

public record PlanoEstudoDto(
    int UtilizadorId,
    int MetaSemanalMinutos,
    int MinutosEstudadosSemana,
    double PercentualMetaSemana,
    int StreakDias,
    List<SessaoEstudoDto> SessoesRecentes,
    List<TopicoPlanoDto> Topicos
);

public record SessaoEstudoDto(
    DateTime Data,
    int DuracaoMinutos,
    string Descricao
);

public record TopicoPlanoDto(
    string Nome,
    double Progresso,
    int ConteudosPendentes
);
