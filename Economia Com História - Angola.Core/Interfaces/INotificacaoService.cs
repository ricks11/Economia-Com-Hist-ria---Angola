namespace EconomiaComHistoria.Core.Interfaces;

public interface INotificacaoService
{
    Task EnviarPushAsync(int utilizadorId, string titulo, string corpo, CancellationToken cancellationToken = default);
}
