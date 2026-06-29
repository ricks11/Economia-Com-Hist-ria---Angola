using CommunityToolkit.Mvvm.ComponentModel;

namespace ECHA.Mobile.PageModels;

public partial class LundaHeritagePatrimonioPageModel : ObservableObject
{
    [ObservableProperty]
    private double _progress = 0.65;

    public LundaHeritagePatrimonioPageModel()
    {
    }
}
