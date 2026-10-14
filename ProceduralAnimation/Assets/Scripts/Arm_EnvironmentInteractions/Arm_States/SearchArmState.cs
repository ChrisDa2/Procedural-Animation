using UnityEngine;

public class SearchArmState : EnvironmentInteractionArmState
{
    public float _approachDistanceThreshhold = 2.0f;
    public SearchArmState(EnvironmentInteractionContext context, EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState stateKey) : base(context, stateKey)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState() 
    {
        Debug.Log("Entering Search State");
    }
    public override void ExitState() { }
    public override void UpdateState() { }
    public override EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState GetNextState()
    {
        if (CheckShoouldReset())
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Reset_Arm;
        }

        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) < _approachDistanceThreshhold;
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

        if (isCloseToTarget && isClosestPointOnColliderValid)
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Approach_Arm;
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
