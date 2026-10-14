using UnityEngine;

public class TouchArmState : EnvironmentInteractionArmState
{
    public float _elapsedTime = 0f;
    float _resetThreshold = 5f;
    public TouchArmState(EnvironmentInteractionContext context, EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState stateKey) : base(context, stateKey)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState() 
    {
        Debug.Log("Entering Touch State");
        _elapsedTime = 0f;
    }
    public override void ExitState() { }
    public override void UpdateState() 
    { 
        _elapsedTime += Time.deltaTime;
        Context.InteractionPointYOffset = Context.ClosestPointOnColliderFromShoulder.y;
    }
    public override EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState GetNextState()
    {
        if (_elapsedTime > _resetThreshold || CheckShoouldReset()) 
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Reset_Arm;
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
