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
        public StringValueEditorViewModel SpeakerName { get; } = new StringValueEditorViewModel() { Value = "", LabelText = "Speaker Name" };
        public StringValueEditorViewModel SpeakerDialogue { get; } = new StringValueEditorViewModel() { Value = "", LabelText = "Speaker Dialog" };
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
                Name = SpeakerName.LabelText,
                Port = null,
                Editor = SpeakerName
            };

            var input2 = new ValueNodeInputViewModel<string>()
            {
                Name = SpeakerDialogue.LabelText,
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
            while (newValue > Responses.Count)
            {
                AddResponseOutput();
            }

            while (newValue < Responses.Count)
            {
                RemoveLastResponseOutput();
            }
        }

        private void AddResponseOutput()
        {
            var newResponse = new StringValueEditorViewModel { Value = $"Response {ResponseCount.Value}" };

            var newOutput = new ValueNodeOutputViewModel<string>()
            {
                Name = $"Response {Responses.Count + 1}",
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

        /// <summary>
        /// Serializes the node data to a dictionary.
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "Name", this.Name },
                { "PositionX", this.Position.X },
                { "PositionY", this.Position.Y },
                { "SpeakerName", SpeakerName.Value },
                { "SpeakerDialogue", SpeakerDialogue.Value },
                { "ResponseCount", ResponseCount.Value ?? 0},
                { "Responses", Responses
                    .Select(r => (r.Editor as StringValueEditorViewModel)?.Value ?? "")
                    .ToList() }
            };
        }

        /// <summary>
        /// Deserializes node data from a dictionary.
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            if (data.TryGetValue("Name", out var name))
                this.Name = name.ToString();

            if (data.TryGetValue("PositionX", out var posX) && data.TryGetValue("PositionY", out var posY))
            {
                if (double.TryParse(posX.ToString(), out var x) && double.TryParse(posY.ToString(), out var y))
                    this.Position = new System.Windows.Point(x, y);
            }

            if (data.TryGetValue("SpeakerName", out var speakerName))
                SpeakerName.Value = speakerName.ToString() ?? string.Empty;

            if (data.TryGetValue("SpeakerDialogue", out var dialogue))
                SpeakerDialogue.Value = dialogue.ToString() ?? string.Empty;

            if (data.TryGetValue("ResponseCount", out var responseCount) && int.TryParse(responseCount.ToString(), out var count))
            {
                ResponseCount.Value = count;
            }

            // Restore response texts
            if (data.TryGetValue("Responses", out var responses))
            {
                List<string> responsesList = new();

                if (responses is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    responsesList = jsonElement.EnumerateArray().Select(r => r.GetString() ?? "").ToList();
                }
                else if (responses is List<object> list)
                {
                    responsesList = list.Cast<object>().Select(r => r?.ToString() ?? "").ToList();
                }
                else if (responses is IEnumerable<object> enumerable)
                {
                    responsesList = enumerable.Select(r => r?.ToString() ?? "").ToList();
                }

                for (int i = 0; i < responsesList.Count && i < Responses.Count; i++)
                {
                    if (Responses[i].Editor is StringValueEditorViewModel editor)
                    {
                        editor.Value = responsesList[i].ToString() ?? string.Empty;
                    }
                }
            }
        }

        static BaseNarrativeNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new BaseNarrativeNodeView(), typeof(IViewFor<BaseNarrativeNode>));
        }
    }
}
