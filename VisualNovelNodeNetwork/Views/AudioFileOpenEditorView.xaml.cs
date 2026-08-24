using Microsoft.Win32;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using VisualNovelNodeNetwork.ViewModels;

namespace VisualNovelNodeNetwork
{
    /// <summary>
    /// Interaction logic for AudioFileOpenEditorView.xaml
    /// </summary>
    public partial class AudioFileOpenEditorView : IViewFor<AudioFileOpenEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AudioFileOpenEditorViewModel), typeof(AudioFileOpenEditorView), new PropertyMetadata(null));

        public AudioFileOpenEditorViewModel? ViewModel
        {
            get => (AudioFileOpenEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (AudioFileOpenEditorViewModel?)value;
        }
        #endregion
        public AudioFileOpenEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel!.ShowOpenFileDialog.RegisterHandler(interaction =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = interaction.Input,
                        Filter = "Audio files|*.wav;*.mp3;*.ogg;*.aiff;*.aif;*.flac|All Files|*.*\""
                    };

                    Window ownerW = Window.GetWindow(this);
                    bool? result = ownerW != null ? dialog.ShowDialog(ownerW) : dialog.ShowDialog();

                    interaction.SetOutput(result == true ? dialog.SafeFileName : null);
                }).DisposeWith(d);

                Observable.FromEventPattern<MouseButtonEventHandler, RoutedEventArgs>(
                    h => TextBox.PreviewMouseDown += h,
                    h => TextBox.PreviewMouseDown -= h)
                    .Select(_ => System.Reactive.Unit.Default)
                    .InvokeCommand(ViewModel, vm => vm.BrowseCommand)
                    .DisposeWith(d);

                this.Bind(ViewModel, vm => vm.AdvanceOnAudioEnd, v => v.CheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Value, v => v.TextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsFileNameEmpty, v => v.CheckBox.Visibility,
                    boolValue => boolValue ? Visibility.Collapsed : Visibility.Visible).DisposeWith(d);
            });
        }
    }
}
