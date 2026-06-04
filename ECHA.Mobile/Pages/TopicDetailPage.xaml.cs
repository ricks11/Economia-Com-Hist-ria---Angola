using ECHA.Mobile.Data;
using ECHA.Mobile.PageModels;  namespace ECHA.Mobile.Pages;  public partial class TopicDetailPage : ContentPage {     public TopicDetailPage(TopicDetailPageModel viewModel)     {         InitializeComponent();         BindingContext = viewModel;     } }
