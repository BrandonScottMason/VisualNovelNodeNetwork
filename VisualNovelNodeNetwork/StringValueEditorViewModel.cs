using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace NodeNetworkExample
{
    public class StringValueEditorViewModel : ValueEditorViewModel<string>
    {
        static StringValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new StringValueEditorView(), typeof(IViewFor<StringValueEditorViewModel>));
        }
        public StringValueEditorViewModel() { Value = ""; }
    }
}
