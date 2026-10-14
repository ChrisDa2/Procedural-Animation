using NUnit.Framework;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionLegStateMachine : StateManager<EnvironmentInteractionLegStateMachine.EEnvironmentInteractionLegState>
{
    public enum EEnvironmentInteractionLegState
    {
        Search_Leg,
        Rise_Leg,
        Approach_Leg,
        Touch_Leg,
        Reset_Leg,
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
        States.Add(EEnvironmentInteractionLegState.Reset_Leg, new ResetLegState(_context, EEnvironmentInteractionLegState.Reset_Leg));
        States.Add(EEnvironmentInteractionLegState.Search_Leg, new SearchLegState(_context, EEnvironmentInteractionLegState.Search_Leg));

        CurrentState = States[EEnvironmentInteractionLegState.Reset_Leg];
    }

    private void ConstructEnvironmentDetectionCollider()
    {
        float wingspan = _rootCollider.height * 1.2f;

        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(wingspan, wingspan * 0.3f, wingspan);
        boxCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y - (.25f * wingspan ), _rootCollider.center.z);
        boxCollider.isTrigger = true;

        _context.ColliderCenterY = _rootCollider.center.y;
    }
}
