using DynamicData;
using NodeNetwork.ViewModels;
using System.Windows;
using System.Windows.Input;

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
        /// Handles the drag over event to allow drop on the network view.
        /// </summary>
        private void NetworkView_DragOver(object sender, DragEventArgs e)
        {
            if(_viewModel.DraggedNodeType.HasValue)
            {
                // Update preview node position during drag
                if (_previewNode != null)
                {
                    Size nodeSize = _previewNode.Size;
                    var canvasMousePos = e.GetPosition(networkView.CanvasOriginElement);
                    double x = canvasMousePos.X - (nodeSize.Width * 0.5);
                    double y = canvasMousePos.Y - (nodeSize.Height * 0.5);
                    _previewNode.Position = new Point(x, y);
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
                previewNode.Name = $"Node {_viewModel.Network.Nodes.Count} ({name})";
                previewNode.Position = _previewNode.Position;
                _previewNode = null;
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