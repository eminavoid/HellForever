public class IsEnemyDetected : Node
{
    private AllyBehaviorTree _allyContext;

    public IsEnemyDetected(AllyBehaviorTree context)
    {
        _allyContext = context;
    }

    public override NodeState Evaluate()
    {
        if (_allyContext.enemyDetector.TargetDetected)
        {
            _allyContext.currentTarget = _allyContext.enemyDetector.DetectedObject;

            return NodeState.Success;
        }

        _allyContext.currentTarget = null;
        return NodeState.Failure;
    }
}