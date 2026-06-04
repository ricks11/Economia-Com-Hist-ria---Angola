using ECHA.Mobile.Data;
using ECHA.Mobile.PageModels;  namespace ECHA.Mobile.Pages;  public partial class ProfilePage : ContentPage {     public ProfilePage(ProfilePageModel viewModel)     {         InitializeComponent();         BindingContext = viewModel;     } }
