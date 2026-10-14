using UnityEngine;

public class SearchLegState : EnvironmentInteractionLegState
{
    public float _approachDistanceThreshhold = 2.0f;
    public SearchLegState(EnvironmentInteractionContext context, EnvironmentInteractionLegStateMachine.EEnvironmentInteractionLegState stateKey) : base(context, stateKey)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState() 
    {
        Debug.Log("Entering Search State");
    }
    public override void ExitState() { }
    public override void UpdateState() { }
    public override EnvironmentInteractionLegStateMachine.EEnvironmentInteractionLegState GetNextState()
    {
        if (CheckShoouldReset())
        {
            return EnvironmentInteractionLegStateMachine.EEnvironmentInteractionLegState.Reset_Leg;
        }

        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) < _approachDistanceThreshhold;
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;

        if (isCloseToTarget && isClosestPointOnColliderValid)
        {
            return EnvironmentInteractionLegStateMachine.EEnvironmentInteractionLegState.Approach_Leg;
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
