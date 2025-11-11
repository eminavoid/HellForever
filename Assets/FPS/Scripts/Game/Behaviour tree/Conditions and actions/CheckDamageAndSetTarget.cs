using UnityEngine;

public class CheckDamageAndSetTarget : Node
{
    private AllyBehaviorTree _allyContext;

    public CheckDamageAndSetTarget(AllyBehaviorTree context)
    {
        _allyContext = context;
    }

    public override NodeState Evaluate()
    {
        if (_allyContext.HasTakenDamage)
        {
            _allyContext.HasTakenDamage = false;

            if (_allyContext.LastAttacker != null)
            {
                _allyContext.CurrentTarget = _allyContext.LastAttacker;
                _allyContext.LastAttacker = null;    

                return NodeState.Success;       
            }
        }
        return NodeState.Failure;
    }
}