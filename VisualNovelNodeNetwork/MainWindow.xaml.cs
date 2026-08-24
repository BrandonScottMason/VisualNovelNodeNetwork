using DynamicData;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VisualNovelNodeNetwork.ViewModels;

namespace VisualNovelNodeNetwork
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
        private string _currentFileName = string.Empty;
        private MainViewModel _viewModel;
        private DragNodePreviewAdorner? _dragAdorner = null; // A visual represenation of the node that is activley being dragged but not added to the network yet

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
                    if (element.DataContext is NodeTypeInfo context && 
                        _viewModel.NodeList.NodeTypes[i].Name == context.Name)
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
        /// Handles the Drag Enter event to create an adorner to preview the dragged node.
        /// </summary>
        private void NetworkView_DragEnter(object sender, DragEventArgs e)
        {
            if(_viewModel.DraggedNodeType.HasValue && _dragAdorner == null)
            {
                Size adornerSize = new(300, 250);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(networkView);
                _dragAdorner = new DragNodePreviewAdorner(networkView, BaseNarrativeNode.DefaultSize);
                layer.Add(_dragAdorner);
            }
        }

        /// <summary>
        /// Handles the drag over event to allow drop on the network view.
        /// </summary>
        private void NetworkView_DragOver(object sender, DragEventArgs e)
        {
            if(_viewModel.DraggedNodeType.HasValue && _dragAdorner != null)
            {
                Point viewMousePos = e.GetPosition(networkView);
                if (networkView.CanvasOriginElement is FrameworkElement zoomedCanvasElement)
                {
                    double currentScale = 1.0;
                    if (zoomedCanvasElement.RenderTransform is ScaleTransform scale)
                    {
                        currentScale = scale.ScaleX;
                    }
                    else if (zoomedCanvasElement.RenderTransform is MatrixTransform matrixTransform)
                    {
                        currentScale = matrixTransform.Matrix.M11; // M11 represents horizontal scaling
                    }

                    _dragAdorner.UpdatePosition(viewMousePos, currentScale);
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
            ClearAdorner();

            if (e.Data.GetData(typeof((string, int))) is (string name, int index) && _viewModel.DraggedNodeType.HasValue)
            {
                CreateNode(name, index, e.GetPosition(networkView.CanvasOriginElement));
                e.Handled = true;
                e.Effects = DragDropEffects.None;
                _viewModel.EndDragNode();
            }
        }

        private void NetworkView_DragLeave(object sender, DragEventArgs e)
        {
            ClearAdorner();
        }

        /// <summary>
        /// Creates a node at the current position of the preview adorner.
        /// </summary>
        private void CreateNode(string name, int? nodeTypeIndex, Point position)
        {
            if (nodeTypeIndex >= 0 && nodeTypeIndex < _viewModel.NodeList.NodeTypes.Count)
            {
                var node = _viewModel.NodeList.CreateNode(nodeTypeIndex.Value);
                if (node != null)
                {
                    node.Name = $"Node {_viewModel.Network.Nodes.Count + 1} ({name})";
                    Point adjustedPos = new(position.X - (BaseNarrativeNode.DefaultSize.Width * 0.5), position.Y - (BaseNarrativeNode.DefaultSize.Height * 0.5));
                    node.Position = adjustedPos;
                    _viewModel.Network.Nodes.Edit(updater => updater.Add(node));
                }
            }
        }

        /// <summary>
        /// Removes the Adorner from the adorner layer.
        /// </summary>
        private void ClearAdorner()
        {
            if(_dragAdorner != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(networkView);
                layer?.Remove(_dragAdorner);
                _dragAdorner = null;
            }
        }

        #region MenuMethods
        private async Task SaveFile(object sender, string fileName, bool clearNetworkWhenDone = false)
        {
            var menuItem = (System.Windows.Controls.MenuItem)sender;
            menuItem.IsEnabled = false;

            try
            {
                await _viewModel.SaveNetworkAsync(fileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Save Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                menuItem.IsEnabled = true;
                if (clearNetworkWhenDone)
                {
                    _viewModel.Network.Nodes.Clear();
                    _currentFileName = string.Empty;
                }
                else
                {
                    _currentFileName = fileName;
                }
            }
        }

        private async Task SaveFileAs(object sender, bool clearNetworkWhenDone = false)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new() { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog.FileName != string.Empty)
            {
                await SaveFile(sender, saveFileDialog.FileName, clearNetworkWhenDone);
            }
        }
        private async void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFileName != string.Empty || _viewModel.Network.Nodes.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("You have potentially unsaved changes, do you want to save first?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentFileName == string.Empty)
                    {
                        await SaveFileAs(sender, true);
                    }
                    else
                    {
                        await SaveFile(sender, _currentFileName, true);
                    }
                }
                else
                {
                    _viewModel.Network.Nodes.Clear();
                    _currentFileName = string.Empty;
                }
            }
        }

        private async void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (System.Windows.Controls.MenuItem)sender;
            menuItem.IsEnabled = false;

            if (_currentFileName != string.Empty || _viewModel.Network.Nodes.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("You have potentially unsaved changes, do you want to save first?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (_currentFileName == string.Empty)
                    {
                        await SaveFileAs(sender, true);
                    }
                    else
                    {
                        await SaveFile(sender, _currentFileName, true);
                    }
                }
                else
                {
                    _viewModel.Network.Nodes.Clear();
                    _currentFileName = string.Empty;
                }
            }

            menuItem.IsEnabled = false;
            System.Windows.Forms.OpenFileDialog openFileDialog = new() { Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*" };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    await _viewModel.LoadNetworkAsync(openFileDialog.FileName);
                    _currentFileName = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Open Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    menuItem.IsEnabled = true;
                }
            }
        }

        private async void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFileName == string.Empty)
            {
                await SaveFileAs(sender);
            }
            else
            {
                await SaveFile(sender, _currentFileName);
            }
        }

        private async void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveFileAs(sender);
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Network.Nodes.Count > 0 || _currentFileName != string.Empty)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to exit? Any unsaved work will be lost.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
        #endregion
}