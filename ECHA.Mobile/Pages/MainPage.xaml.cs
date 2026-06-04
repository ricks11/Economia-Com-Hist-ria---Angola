using ECHA.Mobile.Data;
using ECHA.Mobile.Models; using ECHA.Mobile.PageModels;  namespace ECHA.Mobile.Pages {     public partial class MainPage : ContentPage     {         public MainPage(MainPageModel model)         {             InitializeComponent();             BindingContext = model;         }     } }
