using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace VisualNovelNodeNetwork.ViewModels
{
    public class BooleanValueEditorViewModel : ValueEditorViewModel<bool>
    {
        public string LabelText { get; set; }

        static BooleanValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new BooleanValueEditorView(), typeof(IViewFor<BooleanValueEditorViewModel>));
        }
        public BooleanValueEditorViewModel() { Value = false; LabelText = ""; }
    }
}
