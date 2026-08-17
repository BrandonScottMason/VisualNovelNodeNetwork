using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace VisualNovelNodeNetwork
{
    public class StringValueEditorViewModel : ValueEditorViewModel<string>
    {
        public string LabelText { get; set; }
        static StringValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new StringValueEditorView(), typeof(IViewFor<StringValueEditorViewModel>));
        }
        public StringValueEditorViewModel() { Value = ""; LabelText = ""; }
    }
}
