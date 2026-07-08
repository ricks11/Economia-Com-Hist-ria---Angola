namespace EconomiaComHistoria.Core.Interfaces;

public interface IEmailService
{
    Task SendResetPasswordLinkAsync(string email, string token);
}