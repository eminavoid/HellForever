using System.Collections.Generic;

// Hereda de la clase base abstracta 'Node'
public class Sequence : Node
{
    protected List<Node> m_nodes = new List<Node>();

    public Sequence(List<Node> nodes)
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
                    return NodeState.Failure;
                case NodeState.Success:
                    continue;
                case NodeState.Running:
                    return NodeState.Running;
            }
        }
        return NodeState.Success;
    }
}