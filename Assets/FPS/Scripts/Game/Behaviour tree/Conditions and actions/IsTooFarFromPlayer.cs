using UnityEngine;

public class IsTooFarFromPlayer : Node
{
    private AllyBehaviorTree _allyContext;
    private Transform _allyTransform;
    private AllyRuntimeConfig _config;    

    public IsTooFarFromPlayer(AllyBehaviorTree context, Transform allyTransform)
    {
        _allyContext = context;
        _allyTransform = allyTransform;
        _config = context.Config;      
    }

    public override NodeState Evaluate()
    {
        if (_allyContext.playerTransform == null)
        {
            return NodeState.Failure;
        }

        float distance = Vector3.Distance(_allyTransform.position, _allyContext.playerTransform.position);

        if (distance > _config.maxChaseDistance)
        {
            return NodeState.Failure;
        }

        return NodeState.Success;
    }
}