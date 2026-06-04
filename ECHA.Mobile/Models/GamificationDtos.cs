namespace ECHA.Mobile.Models;

public record BadgeDto(string Id, string Nome, string Descricao, string IconUrl, bool Desbloqueado);

public record ProgressoProvinciaDto(string NomeProvincia, double PercentualExplorado);

public record SugestaoEstudoDto(string Titulo, string ConteudoId, string Prioridade);

public record UserStatsDto(
    int Nivel, 
    int XPAtual, 
    int XPProximoNivel, 
    int StreakDias, 
    List<BadgeDto> Badges,
    List<ProgressoProvinciaDto> ProgressoProvincias
);
