using System.Diagnostics;
using UnityEngine;

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
            _allyContext.CurrentTarget = _allyContext.enemyDetector.DetectedObject;
            UnityEngine.Debug.Log("IsEnemyDetected evaluated");
            return NodeState.Success;
        }

        _allyContext.CurrentTarget = null;
        return NodeState.Failure;
    }
}