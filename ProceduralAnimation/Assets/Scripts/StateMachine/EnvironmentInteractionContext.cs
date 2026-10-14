using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionContext
{
    public enum EbodySide
    {
        RIGHT, 
        LEFT 
    }

    private TwoBoneIKConstraint _leftIkConstraint;
    private TwoBoneIKConstraint _rightIkConstraint;
    private MultiRotationConstraint _leftMultiRotationConstraint;
    private MultiRotationConstraint _rightMultiRotationConstraint;
    private CapsuleCollider _rootCollider;
    private Rigidbody _rigidBody;
    private Transform _rootTransform;
    private Vector3 _leftOriginalTargetPosition;
    private Vector3 _rightOriginalTargetPosition;


    // Constructor
    public EnvironmentInteractionContext(TwoBoneIKConstraint leftIkConstraint, TwoBoneIKConstraint rightIkConstraint, 
            MultiRotationConstraint leftMultiRotationConstraint, MultiRotationConstraint rightMultiRotationConstraint, 
            Rigidbody rigidBody, CapsuleCollider rootCollider, Transform transform)
    {
        _leftIkConstraint = leftIkConstraint;
        _rightIkConstraint = rightIkConstraint;
        _leftMultiRotationConstraint = leftMultiRotationConstraint;
        _rightMultiRotationConstraint = rightMultiRotationConstraint;
        _rootCollider = rootCollider;
        _rigidBody = rigidBody;
        _rootTransform = transform;
        _leftOriginalTargetPosition = _leftIkConstraint.data.target.transform.position;
        _rightOriginalTargetPosition = _rightIkConstraint.data.target.transform.position;
        OriginalTargetRotation = _leftIkConstraint.data.target.rotation;

        OriginalCharacterShoulderHeight = leftIkConstraint.data.root.position.y;
        SetCurrentSideArm(Vector3.negativeInfinity);
    }

    // Read-only properties
    public TwoBoneIKConstraint LeftIkConstraint =>_leftIkConstraint;
    public TwoBoneIKConstraint RightIkConstraint =>_rightIkConstraint;
    public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
    public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
    public Rigidbody Rigidbody => _rigidBody;
    public CapsuleCollider RootCollider => _rootCollider;
    public Transform RootTransform => _rootTransform;
    

    public float OriginalCharacterShoulderHeight { get; private set; }
    public float CharacterShoulderHeight { get; private set; }


    public Collider CurrentIntersectingCollider { get; set; }
    public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
    public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
    public Transform CurrentIkTargetTransform { get; private set; }
    public Transform CurrentShoulderTransform { get; private set; }
    public EbodySide CurrentBodySide { get; private set; }
    public Vector3 ClosestPointOnColliderFromShoulder { get; set; } = Vector3.positiveInfinity;
    public float InteractionPointYOffset { get; set; } = 0.0f;
    public float ColliderCenterY { get; set; }
    public Vector3 CurrentOriginalTargetPosition { get; private set; }
    public Quaternion OriginalTargetRotation { get; private set; }
    public float LowestDistence { get; set; } = float.PositiveInfinity;

    public void SetCurrentSideArm(Vector3 positionToCheck) 
    {
        Vector3 leftShoulder = _leftIkConstraint.data.root.transform.position;
        Vector3 rightShoulder = _rightIkConstraint.data.root.transform.position;

        if (Vector3.Distance(positionToCheck, leftShoulder) < Vector3.Distance(positionToCheck, rightShoulder))
        {
            // Left Shoulder is closer
            CurrentBodySide = EbodySide.LEFT;
            CurrentIkConstraint = _leftIkConstraint;
            CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            CurrentOriginalTargetPosition = _leftOriginalTargetPosition;
        }
        else
        {
            // Right Shoulder is closer
            CurrentBodySide = EbodySide.RIGHT;
            CurrentIkConstraint = _rightIkConstraint;
            CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
            CurrentOriginalTargetPosition = _rightOriginalTargetPosition;
        }

        CurrentShoulderTransform = CurrentIkConstraint.data.root.transform;
        CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
    }
    
    public void UpdateClosestShoulderHeight()
    {
        if (CharacterShoulderHeight != OriginalCharacterShoulderHeight + _rootTransform.position.y)
        {
            CharacterShoulderHeight = OriginalCharacterShoulderHeight + _rootTransform.position.y;
            //Debug.LogWarning("New shoulder height: " + CharacterShoulderHeight);
        }
    }
}
