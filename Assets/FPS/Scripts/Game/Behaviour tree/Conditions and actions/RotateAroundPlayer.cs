using UnityEngine;
using System.Collections.Generic;

public class RotateAroundPlayer : Node
{
    private AllyBehaviorTree _allyContext;
    private Transform _allyTransform;

    private AllyRuntimeConfig _config;

    private float _worldOrbitAngle = 0f;

    private float _timeCounter = 0f;

    public RotateAroundPlayer(AllyBehaviorTree context, Transform allyTransform)
    {
        _allyContext = context;
        _allyTransform = allyTransform;

        _config = context.Config;

        Vector3 worldOffset = _allyTransform.position - _allyContext.playerTransform.position;
        _worldOrbitAngle = Mathf.Atan2(worldOffset.x, worldOffset.z) * Mathf.Rad2Deg;
    }

    public override NodeState Evaluate()
    {
        if (_allyContext.playerTransform == null) return NodeState.Failure;

        _worldOrbitAngle += _config.orbitSpeed * Time.deltaTime;
        if (_worldOrbitAngle > 360f) { _worldOrbitAngle -= 360f; }

        _timeCounter += _config.vibrationSpeed * Time.deltaTime;

        float variedRadius = _config.orbitRadius + Mathf.Cos(_timeCounter) * _config.rAmplitude;

        Quaternion orbitRotation = Quaternion.Euler(0, _worldOrbitAngle, 0);
        Vector3 worldOffset = orbitRotation * (Vector3.forward * variedRadius);

        Vector3 targetPosition = _allyContext.playerTransform.position + worldOffset;

        float orbitHeight = _allyContext.playerTransform.position.y + _allyContext._initialOrbitHeight;
        float yVibration = Mathf.Sin(_timeCounter) * _config.yAmplitude;
        float targetY = orbitHeight + yVibration;

        float newX = Mathf.Lerp(_allyTransform.position.x, targetPosition.x, Time.deltaTime * _config.returnSpeed);
        float newZ = Mathf.Lerp(_allyTransform.position.z, targetPosition.z, Time.deltaTime * _config.returnSpeed);
        float newY = Mathf.Lerp(_allyTransform.position.y, targetY, Time.deltaTime * _config.verticalFollowSpeed);

        Vector3 nextPosition = new Vector3(newX, newY, newZ);

        Vector3 moveDirection = nextPosition - _allyTransform.position;

        if (moveDirection.magnitude > 0.01f)
        {
            Vector3 moveDirectionXZ = new Vector3(moveDirection.x, 0, moveDirection.z);
            Quaternion yawRotation = _allyTransform.rotation;

            if (moveDirectionXZ.magnitude > 0.01f)
            {
                yawRotation = Quaternion.LookRotation(moveDirectionXZ);
            }

            float pitchAngle = Mathf.Atan2(moveDirection.y, moveDirectionXZ.magnitude) * Mathf.Rad2Deg;

            Quaternion finalRotation = yawRotation * Quaternion.Euler(_config.fixedPitchAngle - pitchAngle, 0f, 0f);

            _allyTransform.rotation = Quaternion.Slerp(_allyTransform.rotation, finalRotation, Time.deltaTime * _config.rotationSlerpSpeed);
        }

        _allyTransform.position = nextPosition;

        return NodeState.Running;
    }
}