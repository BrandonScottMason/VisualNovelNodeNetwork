using DynamicData;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System.IO;
using System.Reactive.Disposables;
using System.Text.Json;

namespace NodeNetworkExample
{
    /// <summary>
    /// Main view model for the application that orchestrates the network and node list.
    /// </summary>
    public class MainViewModel : ReactiveObject
    {
        private (string Name, int Index)? _draggedNodeType;
        private readonly CompositeDisposable _disposables = new();

        /// <summary>
        /// Gets the node list view model that manages available node types.
        /// </summary>
        public NodeListViewModel NodeList { get; } = new();

        /// <summary>
        /// Gets the network view model that manages the visual node network.
        /// </summary>
        public NetworkViewModel Network { get; }

        /// <summary>
        /// Gets the currently dragged node type during drag operations.
        /// </summary>
        public (string Name, int Index)? DraggedNodeType
        {
            get => _draggedNodeType;
            set => this.RaiseAndSetIfChanged(ref _draggedNodeType, value);
        }

        public MainViewModel()
        {
            // Create and initialize the network view model
            Network = new NetworkViewModel();
        }

        /// <summary>
        /// Starts dragging a node type from the available nodes list.
        /// </summary>
        public void StartDragNode(int nodeTypeIndex)
        {
            if (nodeTypeIndex >= 0 && nodeTypeIndex < NodeList.NodeTypes.Count)
            {
                var nodeType = NodeList.NodeTypes[nodeTypeIndex];
                DraggedNodeType = (nodeType.Name, nodeTypeIndex);
            }
        }

        /// <summary>
        /// Ends the current drag operation.
        /// </summary>
        public void EndDragNode()
        {
            DraggedNodeType = null;
        }

        /// <summary>
        /// Gets the index of the dragged node type, if any.
        /// </summary>
        public int? GetDraggedNodeTypeIndex()
        {
            return DraggedNodeType?.Index;
        }

        public async Task SaveNetworkAsync(string filePath)
        {
            var nodes = Network.Nodes.Items.OfType<BaseNarrativeNode>().ToList();

            var networkData = new
            {
                Nodes = Network.Nodes.Items
                    .OfType<BaseNarrativeNode>()
                    .Select(node => new
                    {
                        Type = "BaseNarrativeNode",
                        Data = node.Serialize()
                    })
                    .ToList(),
                Connections = Network.Connections.Items
                    .Select(conn => new
                    {
                        OutputNodeIndex = nodes.IndexOf((BaseNarrativeNode)conn.Output.Parent),
                        OutputPortName = conn.Output.Name,
                        InputNodeIndex = nodes.IndexOf((BaseNarrativeNode)conn.Input.Parent),
                        InputPortName = conn.Input.Name
                    })
                    .ToList()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(networkData, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task LoadNetworkAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Clear existing network
            Network.Nodes.Clear();
            Network.Connections.Clear();

            var loadedNodes = new List<NodeViewModel>();

            // Restore nodes
            if (root.TryGetProperty("Nodes", out var nodesArray))
            {
                foreach (var nodeElement in nodesArray.EnumerateArray())
                {
                    if (nodeElement.TryGetProperty("Type", out var typeProperty) && typeProperty.GetString() == "BaseNarrativeNode")
                    {
                        if (nodeElement.TryGetProperty("Data", out var dataProperty))
                        {
                            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(dataProperty.GetRawText());
                            if (data != null)
                            {
                                var node = new BaseNarrativeNode();
                                node.Deserialize(data);
                                Network.Nodes.Add(node);
                                loadedNodes.Add(node);
                            }
                        }
                    }
                }
            }

            // Restore connections
            if (root.TryGetProperty("Connections", out var connectionsArray))
            {
                foreach (var connElement in connectionsArray.EnumerateArray())
                {
                    var outputIndex = connElement.GetProperty("OutputNodeIndex").GetInt32();
                    var outputPort = connElement.GetProperty("OutputPortName").GetString();
                    var inputIndex = connElement.GetProperty("InputNodeIndex").GetInt32();
                    var inputPort = connElement.GetProperty("InputPortName").GetString();

                    if (outputIndex >= 0 && outputIndex < loadedNodes.Count &&
                        inputIndex >= 0 && inputIndex < loadedNodes.Count)
                    {
                        var outputNode = loadedNodes[outputIndex];
                        var inputNode = loadedNodes[inputIndex];

                        NodeOutputViewModel? outPort = null;
                        foreach (var port in outputNode.Outputs.Items)
                        {
                            if (port.Name == outputPort)
                            {
                                outPort = port;
                                break;
                            }
                        }

                        NodeInputViewModel? inPort = null;
                        foreach (var port in inputNode.Inputs.Items)
                        {
                            if (port.Name == inputPort)
                            {
                                inPort = port;
                                break;
                            }
                        }

                        if (outPort != null && inPort != null)
                        {
                            Network.Connections.Add(new ConnectionViewModel(this.Network, inPort, outPort));
                        }
                    }
                }
            }
        }
    }
}
