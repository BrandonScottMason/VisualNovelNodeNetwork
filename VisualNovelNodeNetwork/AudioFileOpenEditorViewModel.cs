using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace VisualNovelNodeNetwork
{
    public class AudioFileOpenEditorViewModel : StringValueEditorViewModel
    {
        public bool AdvanceOnAudioEnd { get; set; }
        readonly ObservableAsPropertyHelper<bool> _isFileNameEmpty;
        public bool IsFileNameEmpty => _isFileNameEmpty.Value;
        public Interaction<string, string?> ShowOpenFileDialog { get; } = new();
        public ReactiveCommand<Unit, Unit> BrowseCommand { get; }
        static AudioFileOpenEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new AudioFileOpenEditorView(), typeof(IViewFor<AudioFileOpenEditorViewModel>));
        }
        public AudioFileOpenEditorViewModel()
        {
            Value = "";
            LabelText = "";
            AdvanceOnAudioEnd = false;

            _isFileNameEmpty = this.WhenAnyValue(x => x.Value)
                .Select(name => string.IsNullOrEmpty(name))
                .StartWith(true)
                .ToProperty(this, x => x.IsFileNameEmpty);

            BrowseCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = await ShowOpenFileDialog.Handle("Select audio file");
                if(!string.IsNullOrEmpty(dialog))
                {
                    Value = dialog;
                }
                else
                {
                    Value = string.Empty;
                }
            });
        }
    }
}
