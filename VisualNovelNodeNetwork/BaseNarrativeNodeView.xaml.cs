using NodeNetwork.ViewModels;
using ReactiveUI;
using System.Drawing;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NodeNetworkExample
{
    /// <summary>
    /// Interaction logic for BaseNarrativeNodeView.xaml
    /// </summary>
    public partial class BaseNarrativeNodeView : IViewFor<BaseNarrativeNode>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(BaseNarrativeNode), typeof(BaseNarrativeNodeView), new PropertyMetadata(null));

        public BaseNarrativeNode? ViewModel
        {
            get => (BaseNarrativeNode)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (BaseNarrativeNode?)value;
        }
        #endregion
        public BaseNarrativeNodeView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                NodeView.ViewModel = this.ViewModel;
                Disposable.Create(() => NodeView.ViewModel = null).DisposeWith(d);
            });
        }

        public static System.Windows.Media.Brush ConvertNodeToBrush()
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x60, 0x0f));
        }
    }
}
