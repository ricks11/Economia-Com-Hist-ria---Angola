using ECHA.Mobile.Data;
using ECHA.Mobile.Models;  namespace ECHA.Mobile.Pages {     public partial class ProjectDetailPage : ContentPage     {         public ProjectDetailPage(ProjectDetailPageModel model)         {             InitializeComponent();              BindingContext = model;         }     } }
