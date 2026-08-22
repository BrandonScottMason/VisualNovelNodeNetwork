using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;

namespace VisualNovelNodeNetwork
{
    /// <summary>
    /// Interaction logic for BooleanValueEditorView.xaml
    /// </summary>
    public partial class BooleanValueEditorView : IViewFor<BooleanValueEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(BooleanValueEditorViewModel), typeof(BooleanValueEditorView), new PropertyMetadata(null));

        public BooleanValueEditorViewModel? ViewModel
        {
            get => (BooleanValueEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (BooleanValueEditorViewModel?)value;
        }
        #endregion
        public BooleanValueEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Value, v => v.CheckBox.IsChecked).DisposeWith(d);
            });
        }
    }
}
