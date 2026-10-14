using UnityEngine;
using UnityEngine.Rendering;

public class ApproachArmState : EnvironmentInteractionArmState
{
    float _elapsedTime = 0.0f;
    float _lerpDuration = 5.5f;
    float _approachDuration = 4.0f;
    float _approachWeight = 2.0f;
    float _approachRotationWeight = 0.25f;
    float _rotationSpeed = 500f;
    float _riseDistanceThreshold = 0.5f;

    public ApproachArmState(EnvironmentInteractionContext context, EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState stateKey) : base(context, stateKey)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        Debug.Log("Entering Approach State");
        _elapsedTime = 0.0f;
    }
    public override void ExitState() { }
    public override void UpdateState() 
    {
        // Quaternion to make the palm look at the ground
        Quaternion expectedGroundRotation = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward);
        _elapsedTime += Time.deltaTime;

        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(Context.CurrentIkTargetTransform.rotation, expectedGroundRotation, _rotationSpeed * Time.deltaTime);

        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(Context.CurrentMultiRotationConstraint.weight, _approachRotationWeight, _elapsedTime / _lerpDuration);
        Context.CurrentIkConstraint.weight = Mathf.Lerp(Context.CurrentIkConstraint.weight, _approachWeight, _elapsedTime / _lerpDuration);
    }
    public override EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState GetNextState() 
    {
        bool isOverStateLifeDuration = _elapsedTime >= _approachDuration;
        if (isOverStateLifeDuration || CheckShoouldReset())
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Reset_Arm;
        }

        bool isWithInArmReach = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.CurrentShoulderTransform.position) < _riseDistanceThreshold;
        bool isClosestPointColliderReal = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

        if (isWithInArmReach && isClosestPointColliderReal)
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Rise_Arm;
        }

        return StateKey;
    }
    public override void OnTriggerEnter(Collider other)
    {
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other)
    {
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other)
    {
        ResetIkTargetPositionTracking(other);
    }
}
