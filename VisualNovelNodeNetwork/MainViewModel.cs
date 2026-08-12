using DynamicData;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;

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

        /// <summary>
        /// Reactive command to handle when a node is dropped onto the network.
        /// </summary>
        public ReactiveCommand<(string Name, int Index), (string Name, int Index)> DropNodeCommand { get; }

        public MainViewModel()
        {
            // Create and initialize the network view model
            Network = new NetworkViewModel();

            // Create the drop command with explicit canExecute that always returns true
            DropNodeCommand = ReactiveCommand.Create<(string Name, int Index), (string Name, int Index)>(
                nodeData =>
                {
                    // Handle the node addition directly in the execute function
                    AddNodeToNetwork(nodeData);
                    return nodeData;
                },
                canExecute: null);  // Explicitly allow the command to always execute
        }

        /// <summary>
        /// Synchronous method to handle node drops from the UI.
        /// </summary>
        public void DropNode(string name, int index)
        {
            AddNodeToNetwork((name, index));
        }

        /// <summary>
        /// Adds a new node to the network based on the dropped node type.
        /// </summary>
        private void AddNodeToNetwork((string Name, int Index) nodeData)
        {
            try
            {
                if (nodeData.Index >= 0 && nodeData.Index < NodeList.NodeTypes.Count)
                {
                    var newNode = NodeList.CreateNode(nodeData.Index);
                    if (newNode != null)
                    {
                        newNode.Name = $"{nodeData.Name} {Network.Nodes.Count + 1}";
                        Network.Nodes.Edit(updater => updater.Add(newNode));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding node: {ex.Message}");
            }
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
    }
}
