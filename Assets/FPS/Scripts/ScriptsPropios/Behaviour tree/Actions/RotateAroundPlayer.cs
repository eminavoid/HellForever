using UnityEngine;

public class RotateAroundPlayer : Node
{
    private AllyBehaviorTree _allyContext;
    private Transform _allyTransform;

    // Parámetros de la Órbita
    private float _radius = 3f;
    private float _orbitSpeed = 60f;
    private float _currentAngle = 0f;

    public RotateAroundPlayer(AllyBehaviorTree context, Transform allyTransform, float radius = 3f, float speed = 60f)
    {
        _allyContext = context;
        _allyTransform = allyTransform;
        _radius = radius;
        _orbitSpeed = speed;

        Vector3 displacement = _allyTransform.position - _allyContext.playerTransform.position;
        _currentAngle = Mathf.Atan2(displacement.x, displacement.z) * Mathf.Rad2Deg;
    }

    public override NodeState Evaluate()
    {
        if (_allyContext.playerTransform == null)
        {
            return NodeState.Failure;
        }

        _currentAngle += _orbitSpeed * Time.deltaTime;

        if (_currentAngle > 360f)
        {
            _currentAngle -= 360f;
        }

        float angleRad = _currentAngle * Mathf.Deg2Rad;

        float newX = Mathf.Sin(angleRad) * _radius;
        float newZ = Mathf.Cos(angleRad) * _radius;

        Vector3 newPosition = _allyContext.playerTransform.position;

        newPosition.x += newX;
        newPosition.y = _allyTransform.position.y;
        newPosition.z += newZ;

        Vector3 moveDirection = newPosition - _allyTransform.position;

        _allyTransform.position = newPosition;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            _allyTransform.rotation = Quaternion.Slerp(_allyTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        return NodeState.Running;
    }
}