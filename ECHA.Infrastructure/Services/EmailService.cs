using System.Net.Mail;
using System.Net;
using EconomiaComHistoria.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EconomiaComHistoria.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendResetPasswordLinkAsync(string email, string token)
    {
        // Vai buscar as configurações ao appsettings.json
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var smtpUser = _configuration["EmailSettings:SmtpUser"];
        var smtpPass = _configuration["EmailSettings:SmtpPass"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];

        // URL do teu Front-end Web onde o utilizador irá introduzir a nova senha
        var webUrl = _configuration["ApiSettings:WebUrl"] ?? "https://localhost:5001";
        var resetLink = $"{webUrl}/Auth/ResetPassword?token={token}&email={Uri.EscapeDataString(email)}";

        var body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 16px;'>
                <h2 style='color: #1c1b1f;'>Recuperação de Palavra-passe</h2>
                <p>Olá,</p>
                <p>Recebemos um pedido para redefinir a palavra-passe da sua conta no <strong>Economia Com História</strong>.</p>
                <p>Clique no botão abaixo para escolher uma nova palavra-passe. Este link é válido por 2 horas.</p>
                <div style='margin: 30px 0; text-align: center;'>
                    <a href='{resetLink}' style='background-color: #0061a4; color: white; padding: 14px 28px; text-decoration: none; font-weight: bold; border-radius: 24px; display: inline-block;'>Redefinir Palavra-passe</a>
                </div>
                <p style='color: #75777a; font-size: 12px;'>Se não solicitou esta alteração, por favor ignore este email.</p>
            </div>";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail ?? smtpUser!, "Economia Com História"),
            Subject = "Recuperação de Palavra-passe - ECHA",
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}