using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ECHA.Mobile.PageModels;

public partial class LundaHeritagePatrimonioPageModel : ObservableObject
{
    public record Modulo(string Titulo, string Descricao, string DataConclusao);

    [ObservableProperty]
    private double _progressoPercent = 0.65;
    
    [ObservableProperty]
    private string _progressoTexto = "65% Concluído";

    [ObservableProperty]
    private ObservableCollection<Modulo> _modulosCompletados = new();

    public LundaHeritagePatrimonioPageModel()
    {
        ModulosCompletados = new ObservableCollection<Modulo>
        {
            new("Introdução Histórica", "Fundamentos da economia no período pré-colonial", "Concluído a 12/05/2026"),
            new("Rotas Comerciais", "Principais rotas de comércio na região", "Concluído a 18/05/2026"),
            new("Impacto do Comércio", "O papel do comércio no desenvolvimento", "Concluído a 24/05/2026")
        };
    }
}
