using NodeNetwork.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace NodeNetworkExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Static node list view model that provides access to available node types for the application.
        /// </summary>
        public static NodeListViewModel NodeList { get; set; } = new NodeListViewModel();

        private MainViewModel _viewModel;
        private NodeViewModel? _previewNode = null; // A visual represenation of the node that is activley being dragged but not added to the network yet

        public MainWindow()
        {
            InitializeComponent();

            // Create and set the main view model as the data context
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // Store the node list in the static property for backward compatibility
            NodeList = _viewModel.NodeList;
        }

        /// <summary>
        /// Handles the mouse left button down event on a node item to start dragging.
        /// </summary>
        private void NodeItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // Get the index by finding which node type matches this data context
                var index = -1;
                for (int i = 0; i < _viewModel.NodeList.NodeTypes.Count; i++)
                {
                    // Compare by name since tuples require exact comparison
                    if (element.DataContext is (string contextName, System.Type _) && 
                        _viewModel.NodeList.NodeTypes[i].Item1 == contextName)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    _viewModel.StartDragNode(index);
                }
            }
        }

        /// <summary>
        /// Handles the mouse move event to initiate the drag operation.
        /// </summary>
        private void NodeItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _viewModel.DraggedNodeType.HasValue)
            {
                var draggedData = _viewModel.DraggedNodeType.Value;
                DragDrop.DoDragDrop((DependencyObject)sender, draggedData, DragDropEffects.Copy);
            }
        }

        /// <summary>
        /// Handles the drag over event to allow drop on the network view.
        /// </summary>
        private void NetworkView_DragOver(object sender, DragEventArgs e)
        {
            if(_viewModel.DraggedNodeType.HasValue)
            {
                // Update preview node position during drag
                if (_previewNode != null)
                {
                    var mousePosition = e.GetPosition(networkView);
                    _previewNode.Position = mousePosition;
                }
                else if (_viewModel.GetDraggedNodeTypeIndex().HasValue)
                {
                    CreatePreviewNode(_viewModel.GetDraggedNodeTypeIndex());
                }

                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Handles the drop event to add a new node to the network.
        /// </summary>
        private void NetworkView_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetData(typeof((string, int))) is (string name, int index))
            {
                DropPreviewNode(name);
                e.Handled = true;
            }

            _viewModel.EndDragNode();
        }

        /// <summary>
        /// Creates a preview node that follows the mouse during dragging.
        /// </summary>
        private void CreatePreviewNode(int? nodeTypeIndex)
        {
            if (nodeTypeIndex >= 0 && nodeTypeIndex < _viewModel.NodeList.NodeTypes.Count)
            {
                _previewNode = _viewModel.NodeList.CreateNode(nodeTypeIndex.Value);
                if (_previewNode != null)
                {
                    _previewNode.Name = "Preview";
                    _viewModel.Network.Nodes.Edit(updater => updater.Add(_previewNode));
                }
            }
        }

        /// <summary>
        /// Removes the preview node from the network.
        /// </summary>
        private void DropPreviewNode(string name)
        {
            if (_previewNode != null)
            {
                var previewNode = _viewModel.Network.Nodes.Items.Last();
                previewNode.Name = $"{name} {_viewModel.Network.Nodes.Count}";
                previewNode.Position = _previewNode.Position;
                _previewNode = null;
            }
        }
    }
}