using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using VisualNovelNodeNetwork.ViewModels;

namespace VisualNovelNodeNetwork
{
    public partial class IntegerValueEditorView : UserControl, IViewFor<IntegerValueEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(IntegerValueEditorViewModel), typeof(IntegerValueEditorView), new PropertyMetadata(null));

        public IntegerValueEditorViewModel? ViewModel
        {
            get => (IntegerValueEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IntegerValueEditorViewModel?)value;
        }
        #endregion
        
        public IntegerValueEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.Value, v => v.valueUpDown.Value)
            ));
        }
    }
}
