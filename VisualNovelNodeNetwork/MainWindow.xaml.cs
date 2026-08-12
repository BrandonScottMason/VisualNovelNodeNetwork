using ReactiveUI;
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
                DragDrop.DoDragDrop((System.Windows.DependencyObject)sender, draggedData, DragDropEffects.Copy);
                _viewModel.EndDragNode();
            }
        }

        /// <summary>
        /// Handles the drag over event to allow drop on the network view.
        /// </summary>
        private void NetworkView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof((string, int))))
            {
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
            if (e.Data.GetData(typeof((string, int))) is (string name, int index))
            {
                if (index >= 0 && index < _viewModel.NodeList.NodeTypes.Count)
                {
                    _viewModel.DropNode(name, index);
                    e.Handled = true;
                }
            }
        }
    }
}