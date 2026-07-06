using EconomiaComHistoria.Core.Interfaces;

namespace EconomiaComHistoriaAngola.Infrastructure.Services
{
    public class ValidadorSincronizacao : IValidadorSincronizacao
    {
        private static readonly TimeSpan ToleranciaMaxima = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan IntervaloMinimoRanking = TimeSpan.FromHours(24);

        public ResultadoValidacaoTemporal ValidarTimestamp(DateTime dataRealizacaoCliente, DateTime dataServidorUtc)
        {
            var diferenca = (dataServidorUtc - dataRealizacaoCliente.ToUniversalTime()).Duration();
            var dentroDaTolerancia = diferenca <= ToleranciaMaxima;
            return new ResultadoValidacaoTemporal(dentroDaTolerancia, diferenca);
        }

        public bool RespeitaIntervaloRanking(DateTime dataUltimaTentativaElegivel, DateTime dataNovaTentativa)
        {
            return (dataNovaTentativa - dataUltimaTentativaElegivel) >= IntervaloMinimoRanking;
        }
    }
}