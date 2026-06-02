using EconomiaComHistoria.Core.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace EconomiaComHistoria.Infrastructure.Services;

public class FirebaseNotificacaoService : INotificacaoService
{
    private readonly ILogger<FirebaseNotificacaoService> _logger;

    public FirebaseNotificacaoService(ILogger<FirebaseNotificacaoService> logger)
    {
        _logger = logger;
    }

    public async Task EnviarPushAsync(int utilizadorId, string titulo, string corpo, CancellationToken cancellationToken = default)
    {
        if (FirebaseApp.DefaultInstance is null)
        {
            _logger.LogInformation("Firebase nao configurado. Push pendente para utilizador {UtilizadorId}: {Titulo} - {Corpo}", utilizadorId, titulo, corpo);
            return;
        }

        var message = new Message
        {
            Topic = $"utilizador-{utilizadorId}",
            Notification = new Notification
            {
                Title = titulo,
                Body = corpo
            }
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }
}
