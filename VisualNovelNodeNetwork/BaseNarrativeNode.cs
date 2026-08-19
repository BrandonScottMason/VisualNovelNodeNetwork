using DynamicData;
using NodeNetwork.ViewModels;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace VisualNovelNodeNetwork
{
    public class BaseNarrativeNode : NodeViewModel
    {
        private CompositeDisposable _disposables = new();

        public NodeInputViewModel Input { get; }
        public StringValueEditorViewModel SpeakerName { get; } = new StringValueEditorViewModel() { Value = "", LabelText = "Speaker Name" };
        public StringValueEditorViewModel SpeakerDialogue { get; } = new StringValueEditorViewModel() { Value = "", LabelText = "Speaker Dialog", BoxWidth = 300 };
        public IntegerValueEditorViewModel ConnectionCount { get; } = new IntegerValueEditorViewModel() { Value = 0 };
        public ValueNodeOutputViewModel<int?> RCountOutput { get; }
        public ObservableCollection<ValueNodeOutputViewModel<string>> Connections { get; } = new();
        public ReactiveCommand<int, Unit> UpdateConnectionsCommand { get; }

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
                Editor = ConnectionCount,
                Value = this.WhenAnyValue(vm => vm.ConnectionCount.Value)
            };
            this.Outputs.Add(RCountOutput);

            UpdateConnectionsCommand = ReactiveCommand.Create<int>(UpdateConnections);

            this.WhenAnyValue(x => x.ConnectionCount.Value)
                .InvokeCommand(UpdateConnectionsCommand)
                .DisposeWith(_disposables);
        }

        private void UpdateConnections(int newValue)
        {
            while (newValue > Connections.Count)
            {
                AddConnectionOutput();
            }

            while (newValue < Connections.Count)
            {
                RemoveLastConnectionOutput();
            }
        }

        private void AddConnectionOutput()
        {
            var newConnection = new StringValueEditorViewModel { Value = string.Empty };

            var newOutput = new ValueNodeOutputViewModel<string>()
            {
                Name = $"Connection {Connections.Count + 1}",
                Editor = newConnection,
                Value = newConnection.WhenAnyValue(x => x.Value)
            };

            Connections.Add(newOutput);
            this.Outputs.Add(newOutput);
        }

        private void RemoveLastConnectionOutput()
        {
            if (Connections.Count > 0)
            {
                var lastConnection = Connections[Connections.Count - 1];
                Connections.Remove(lastConnection);
                this.Outputs.Remove(lastConnection);
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
                { "ConnectionCount", ConnectionCount.Value ?? 0},
                { "Connections", Connections
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

            if (data.TryGetValue("ConnectionCount", out var connectionCount) && int.TryParse(connectionCount.ToString(), out var count))
            {
                ConnectionCount.Value = count;
            }

            // Restore connection texts
            if (data.TryGetValue("Connections", out var connections))
            {
                List<string> connectionsList = new();

                if (connections is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    connectionsList = jsonElement.EnumerateArray().Select(r => r.GetString() ?? "").ToList();
                }
                else if (connections is List<object> list)
                {
                    connectionsList = list.Cast<object>().Select(r => r?.ToString() ?? "").ToList();
                }
                else if (connections is IEnumerable<object> enumerable)
                {
                    connectionsList = enumerable.Select(r => r?.ToString() ?? "").ToList();
                }

                for (int i = 0; i < connectionsList.Count && i < Connections.Count; i++)
                {
                    if (Connections[i].Editor is StringValueEditorViewModel editor)
                    {
                        editor.Value = connectionsList[i].ToString() ?? string.Empty;
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
