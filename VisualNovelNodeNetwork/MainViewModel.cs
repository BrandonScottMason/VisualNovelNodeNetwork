using NodeNetwork.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

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
    }
}
