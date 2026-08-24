using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using VisualNovelNodeNetwork.Views;

namespace VisualNovelNodeNetwork.ViewModels
{
    public class StringValueEditorViewModel : ValueEditorViewModel<string>
    {
        public string LabelText { get; set; }
        public int BoxWidth { get; set; }
        static StringValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new StringValueEditorView(), typeof(IViewFor<StringValueEditorViewModel>));
        }
        public StringValueEditorViewModel() { Value = ""; LabelText = ""; BoxWidth = 200; }
    }
}
