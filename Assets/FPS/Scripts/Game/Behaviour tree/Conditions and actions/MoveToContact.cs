using UnityEngine;
using System.Collections.Generic;

public class MoveToContact : Node
{
    private Transform _allyTransform;
    private AllyBehaviorTree _allyContext;
    private AllyRuntimeConfig _config;    

    private bool _isBouncing = false;

    public MoveToContact(AllyBehaviorTree context, Transform allyTransform)
    {
        _allyContext = context;
        _allyTransform = allyTransform;
        _config = context.Config;      
        _isBouncing = false;
    }

    public override NodeState Evaluate()
    {
        Transform target = _allyContext.CurrentTarget;

        if (target == null)
        {
            return NodeState.Failure;
        }

        Vector3 targetPos = target.position;
        Vector3 allyPos = _allyTransform.position;

        Vector3 direction3D = (targetPos - allyPos);
        Vector3 directionFlat = direction3D;
        directionFlat.y = 0;

        float flatDistance = directionFlat.magnitude;
        direction3D.Normalize();
        directionFlat.Normalize();

        if (directionFlat != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionFlat);
            Quaternion fixedZRotation = Quaternion.Euler(_config.fixedPitchAngle, 0f, 0f);
            Quaternion finalRotation = lookRotation * fixedZRotation;

            _allyTransform.rotation = Quaternion.Slerp(_allyTransform.rotation, finalRotation, Time.deltaTime * _config.rotationSlerpSpeed);
        }

        if (!_isBouncing && flatDistance <= _config.minContactDistance)
        {
            _isBouncing = true;
        }

        Vector3 moveVector = Vector3.zero;

        float currentSpeed = _config.attackSpeed;

        if (_isBouncing)
        {
            moveVector = -direction3D * currentSpeed * Time.deltaTime;

            if (flatDistance >= _config.bounceDistance)
            {
                _isBouncing = false;
            }
        }

        if (!_isBouncing && flatDistance > _config.minContactDistance)
        {
            moveVector = direction3D * currentSpeed * Time.deltaTime;
        }

        _allyTransform.position += moveVector;

        return NodeState.Running;
    }
}