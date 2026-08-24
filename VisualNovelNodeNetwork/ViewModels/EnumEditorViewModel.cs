using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System.Reactive.Linq;
using VisualNovelNodeNetwork.Views;

namespace VisualNovelNodeNetwork.ViewModels
{
    public class EnumEditorViewModel : ValueEditorViewModel<object>
    {
        static EnumEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new EnumEditorView(), typeof(IViewFor<EnumEditorViewModel>));
        }

        public object[] Options { get; }
        public string?[] OptionLabels { get; }

        private int _selectedOptionIndex;
        public int SelectedOptionIndex
        {
            get => _selectedOptionIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedOptionIndex, value);
        }

        public EnumEditorViewModel(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(enumType.Name + " is not an enum type");
            }
            Options = Enum.GetValues(enumType).Cast<object>().ToArray();
            OptionLabels = Options.Select(c => Enum.GetName(enumType, c)).ToArray();

            this.WhenAnyValue(vm => vm.SelectedOptionIndex)
                .Select(i => i == -1 ? null : Options[i])
                .BindTo(this, vm => vm.Value);
        }
    }
}
