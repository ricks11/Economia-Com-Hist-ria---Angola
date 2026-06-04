using ECHA.Mobile.Data;
namespace ECHA.Mobile.Pages {     public partial class ManageMetaPage : ContentPage     {         public ManageMetaPage(ManageMetaPageModel model)         {             InitializeComponent();             BindingContext = model;         }     } }
