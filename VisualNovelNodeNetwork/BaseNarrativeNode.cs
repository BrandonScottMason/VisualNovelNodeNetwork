using DynamicData;
using NodeNetwork.ViewModels;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace NodeNetworkExample
{
    public class BaseNarrativeNode : NodeViewModel
    {
        private CompositeDisposable _disposables = new();

        public NodeInputViewModel Input { get; }
        public StringValueEditorViewModel SpeakerName { get; } = new StringValueEditorViewModel() { Value = "Speaker Name" };
        public StringValueEditorViewModel SpeakerDialogue { get; } = new StringValueEditorViewModel() { Value = "Speaker Dialog" };
        public IntegerValueEditorViewModel ResponseCount { get; } = new IntegerValueEditorViewModel() { Value = 0 };
        public ValueNodeOutputViewModel<int?> RCountOutput { get; }
        public ObservableCollection<ValueNodeOutputViewModel<string>> Responses { get; } = new();
        public ReactiveCommand<int, Unit> UpdateResponsesCommand { get; }

        public BaseNarrativeNode()
        {
            this.Name = "Base Narrative Node";
            Input = new ValueNodeInputViewModel<string>
            {
                Name = "Input"
            };
            this.Inputs.Add(Input);

            var input1 = new ValueNodeInputViewModel<string>()
            {
                Port = null,
                Editor = SpeakerName
            };

            var input2 = new ValueNodeInputViewModel<string>()
            {
                Port = null,
                Editor = SpeakerDialogue
            };

            this.Inputs.Add(input1);
            this.Inputs.Add(input2);

            RCountOutput = new ValueNodeOutputViewModel<int?>
            {
                Name = "Value",
                Port = null,
                Editor = ResponseCount,
                Value = this.WhenAnyValue(vm => vm.ResponseCount.Value)
            };
            this.Outputs.Add(RCountOutput);

            UpdateResponsesCommand = ReactiveCommand.Create<int>(UpdateResponses);

            this.WhenAnyValue(x => x.ResponseCount.Value)
                .InvokeCommand(UpdateResponsesCommand)
                .DisposeWith(_disposables);
        }

        private void UpdateResponses(int newValue)
        {
            if (newValue > Responses.Count)
            {
                AddResponseOutput();
            }
            else if (newValue < Responses.Count)
            {
                RemoveLastResponseOutput();
            }
        }

        private void AddResponseOutput()
        {
            var newResponse = new StringValueEditorViewModel { Value = $"Response {ResponseCount.Value}" };

            var newOutput = new ValueNodeOutputViewModel<string>()
            {
                Name = newResponse.Value,
                Editor = newResponse,
                Value = newResponse.WhenAnyValue(x => x.Value)
            };

            Responses.Add(newOutput);
            this.Outputs.Add(newOutput);
        }

        private void RemoveLastResponseOutput()
        {
            if (Responses.Count > 0)
            {
                var lastResponse = Responses[Responses.Count - 1];
                Responses.Remove(lastResponse);
                this.Outputs.Remove(lastResponse);
            }
        }

        static BaseNarrativeNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new BaseNarrativeNodeView(), typeof(IViewFor<BaseNarrativeNode>));
        }
    }
}
