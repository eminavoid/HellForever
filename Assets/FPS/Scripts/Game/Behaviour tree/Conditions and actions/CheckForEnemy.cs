using UnityEngine;

public class CheckForEnemy : Node
{
    private AllyBehaviorTree _allyContext;

    public CheckForEnemy(AllyBehaviorTree context)
    {
        _allyContext = context;
    }

    public override NodeState Evaluate()
    {
        AllyDetector detector = _allyContext.enemyDetector;

        if (detector != null && detector.DetectedObject != null)
        {
            _allyContext.CurrentTarget = detector.DetectedObject;

            return NodeState.Success;
        }

        _allyContext.CurrentTarget = null;
        return NodeState.Failure;
    }
}