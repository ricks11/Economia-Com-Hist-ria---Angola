using ECHA.Mobile.Data;
using ECHA.Mobile.PageModels;  namespace ECHA.Mobile.Pages;  public partial class CreateTopicPage : ContentPage {     public CreateTopicPage(CreateTopicPageModel viewModel)     {         InitializeComponent();         BindingContext = viewModel;     } }
