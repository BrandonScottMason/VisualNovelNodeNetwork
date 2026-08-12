using System.Collections.Generic;
using System.Reactive;
using ReactiveUI;

namespace NodeNetworkExample
{
    /// <summary>
    /// ViewModel for managing a list of available node types that can be added to the network.
    /// </summary>
    public class NodeListViewModel : ReactiveObject
    {
        private readonly List<(string Name, System.Type Type)> _nodeTypes;

        public IReadOnlyList<(string Name, System.Type Type)> NodeTypes => _nodeTypes.AsReadOnly();

        public ReactiveCommand<int, BaseNarrativeNode> CreateNodeCommand { get; }

        public NodeListViewModel()
        {
            _nodeTypes = new List<(string, System.Type)>
            {
                ("Base Narrative Node", typeof(BaseNarrativeNode))
            };

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

            _nodeTypes.Add((name, nodeType));
        }

        /// <summary>
        /// Creates a new instance of the node type at the specified index.
        /// </summary>
        public BaseNarrativeNode CreateNode(int index)
        {
            return (BaseNarrativeNode)System.Activator.CreateInstance(_nodeTypes[index].Type)!;
        }
    }
}
