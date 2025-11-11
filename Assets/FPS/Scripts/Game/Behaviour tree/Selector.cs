using System.Collections.Generic;

public class Selector : Node
{
    protected List<Node> m_nodes = new List<Node>();

    public Selector(List<Node> nodes)
    {
        m_nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        foreach (Node node in m_nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    continue;
                case NodeState.Success:
                    return NodeState.Success;
                case NodeState.Running:
                    return NodeState.Running;
                default:
                    continue;
            }
        }
        return NodeState.Failure;
    }
}