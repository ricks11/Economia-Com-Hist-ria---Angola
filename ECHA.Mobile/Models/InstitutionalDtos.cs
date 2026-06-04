namespace ECHA.Mobile.Models;

public record AssociacaoDto(string CodigoConvite);

public record TurmaRankingDto(string NomeEstudante, int Pontuacao);

public record RelatorioProgressoDto(string NomeEstudante, double ProgressoGeral, string DestaqueTema);
