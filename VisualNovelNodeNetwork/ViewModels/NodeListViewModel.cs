using ReactiveUI;
using System.Text.RegularExpressions;
using VisualNovelNodeNetwork.Views;

namespace VisualNovelNodeNetwork.ViewModels
{
    /// <summary>
    /// Represents a node type that can be added to the network.
    /// </summary>
    public class NodeTypeInfo(string name, System.Type type)
    {
        public string Name { get; } = name;
        public System.Type Type { get; } = type;
    }

    /// <summary>
    /// ViewModel for managing a list of available node types that can be added to the network.
    /// </summary>
    public class NodeListViewModel : ReactiveObject
    {
        private readonly List<NodeTypeInfo> _nodeTypes;

        public IReadOnlyList<NodeTypeInfo> NodeTypes => _nodeTypes.AsReadOnly();

        public ReactiveCommand<int, BaseNarrativeNode> CreateNodeCommand { get; }

        public NodeListViewModel()
        {
            _nodeTypes =
            [
                new NodeTypeInfo(GetReadableNodeName(typeof(BaseNarrativeNode)), typeof(BaseNarrativeNode))
            ];

            CreateNodeCommand = ReactiveCommand.Create<int, BaseNarrativeNode>(
                index => (BaseNarrativeNode)System.Activator.CreateInstance(_nodeTypes[index].Type)!);
        }

        /// <summary>
        /// Registers a new node type that can be created from the node list.
        /// </summary>
        public void RegisterNodeType(string name, System.Type nodeType)
        {
            if (!typeof(BaseNarrativeNode).IsAssignableFrom(nodeType))
            {
                throw new System.ArgumentException($"Node type must derive from BaseNarrativeNode", nameof(nodeType));
            }

            _nodeTypes.Add(new NodeTypeInfo(name, nodeType));
        }

        /// <summary>
        /// Creates a new instance of the node type at the specified index.
        /// </summary>
        public BaseNarrativeNode CreateNode(int index)
        {
            return (BaseNarrativeNode)System.Activator.CreateInstance(_nodeTypes[index].Type)!;
        }

        private static readonly Regex CamelCaseRegex = new(@"(?<!^)(\B[A-Z])");
        private string GetReadableNodeName(Type classType)
        {
            string classNmae = classType.Name;
            return CamelCaseRegex.Replace(classNmae, " $1");
        }
    }
}
