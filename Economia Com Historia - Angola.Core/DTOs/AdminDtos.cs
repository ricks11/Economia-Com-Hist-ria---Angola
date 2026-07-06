using System.ComponentModel.DataAnnotations;

namespace EconomiaComHistoria.Core.DTOs;

public class RoleChangeDto
{
    [Required(ErrorMessage = "Nova role é obrigatória")]
    public string NovaRole { get; set; } = string.Empty;
}