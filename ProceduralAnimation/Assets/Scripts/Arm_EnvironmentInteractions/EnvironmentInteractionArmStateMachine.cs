using NUnit.Framework;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionArmStateMachine : StateManager<EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState>
{
    public enum EEnvironmentInteractionArmState
    {
        Search_Arm,
        Approach_Arm,
        Rise_Arm,
        Touch_Arm,
        Reset_Arm,
    }

    public EnvironmentInteractionContext _context;

    [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
    [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private CapsuleCollider _rootCollider;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (_context != null && _context.ClosestPointOnColliderFromShoulder != null)
        {
            Gizmos.DrawSphere(_context.ClosestPointOnColliderFromShoulder, 0.03f);
        }
    }

    private void Awake()
    {
        ValidateConstraints();

        _context = new EnvironmentInteractionContext(_leftIkConstraint, _rightIkConstraint, _leftMultiRotationConstraint, _rightMultiRotationConstraint, _rigidBody, _rootCollider, transform.root);

        InitializeStates();
        ConstructEnvironmentDetectionCollider();
    }

    // Assert that the attributes are assigned correctly
    private void ValidateConstraints()
    {
        Assert.IsNotNull(_leftIkConstraint, "Left IK constraint is not assigned");
        Assert.IsNotNull(_rightIkConstraint, "Right IK constraint is not assigned");
        Assert.IsNotNull(_leftMultiRotationConstraint, "Left MultiRotation constraint is not assigned");
        Assert.IsNotNull(_rightMultiRotationConstraint, "Right MultiRotation constraint is not assigned");
        Assert.IsNotNull(_rigidBody, "Rigidbody used to control the character is not assigned");
        Assert.IsNotNull(_rootCollider, "RootCollider attached to character is not assigned");
    }

    // Add States to the inherited dictionary States and Set the initial state
    private void InitializeStates()
    {
        States.Add(EEnvironmentInteractionArmState.Reset_Arm, new ResetArmState(_context, EEnvironmentInteractionArmState.Reset_Arm));
        States.Add(EEnvironmentInteractionArmState.Search_Arm, new SearchArmState(_context, EEnvironmentInteractionArmState.Search_Arm));
        States.Add(EEnvironmentInteractionArmState.Approach_Arm, new ApproachArmState(_context, EEnvironmentInteractionArmState.Approach_Arm));
        States.Add(EEnvironmentInteractionArmState.Rise_Arm, new RiseArmState(_context, EEnvironmentInteractionArmState.Rise_Arm));
        States.Add(EEnvironmentInteractionArmState.Touch_Arm, new TouchArmState(_context, EEnvironmentInteractionArmState.Touch_Arm));

        CurrentState = States[EEnvironmentInteractionArmState.Reset_Arm];
    }

    private void ConstructEnvironmentDetectionCollider()
    {
        float wingspan = _rootCollider.height;

        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(wingspan, wingspan * .6f, wingspan * .6f);
        boxCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y + (.25f * wingspan ), _rootCollider.center.z + (.4f * wingspan));
        boxCollider.isTrigger = true;

        _context.ColliderCenterY = _rootCollider.center.y;
    }
}
