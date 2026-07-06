namespace EconomiaComHistoria.Core.Interfaces;

public record ResultadoValidacaoTemporal(bool DentroDaTolerancia, TimeSpan Diferenca);

public interface IValidadorSincronizacao
{
    ResultadoValidacaoTemporal ValidarTimestamp(DateTime dataRealizacaoCliente, DateTime dataServidorUtc);
    bool RespeitaIntervaloRanking(DateTime dataUltimaTentativaElegivel, DateTime dataNovaTentativa);
}