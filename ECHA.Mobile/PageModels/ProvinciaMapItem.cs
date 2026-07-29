using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace ECHA.Mobile.PageModels;

public partial class ProvinciaMapItem : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string NomeProvincia { get; set; } = string.Empty;
    public string PathData { get; set; } = string.Empty;

    [ObservableProperty]
    private double _percentualExplorado;

    [ObservableProperty]
    private Color _corFill = Colors.Gray;

    [ObservableProperty]
    private bool _isSelected;

    public string StatusTexto => PercentualExplorado switch
    {
        <= 0 => "Não iniciado (0%)",
        < 0.5 => $"Em início ({(int)(PercentualExplorado * 100)}%)",
        < 0.8 => $"Em progresso ({(int)(PercentualExplorado * 100)}%)",
        _ => $"Concluído ({(int)(PercentualExplorado * 100)}%)"
    };

    public string PercentualTexto => $"{(int)(PercentualExplorado * 100)}%";

    public void UpdateCor()
    {
        CorFill = PercentualExplorado switch
        {
            <= 0.0 => Color.FromArgb("#EF4444"), // Vermelho (0%)
            < 0.40 => Color.FromArgb("#F97316"), // Laranja (1-39%)
            < 0.80 => Color.FromArgb("#F59E0B"), // Amarelo (40-79%)
            _ => Color.FromArgb("#10B981")       // Verde (80-100%)
        };
    }
}
