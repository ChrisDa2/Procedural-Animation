using UnityEngine;

public class RiseArmState : EnvironmentInteractionArmState
{
    float _elapsedTime = 0.0f;
    float _lerpDuration = 5.0f;
    float _riseWeight = 1.0f;
    Quaternion _expectedRotation;
    float _maxDistance = 0.5f;
    protected LayerMask _interactableLayerMask = LayerMask.GetMask("Interactable");
    float _rotationSpeed = 1000f;
    float _touchDistanceThreshold = 0.05f;
    float _touchTimeThreshold = 1f;


    public RiseArmState(EnvironmentInteractionContext context, EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState stateKey) : base(context, stateKey)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState() 
    {
        Debug.Log("Entering Rise State");
        _elapsedTime = 0.0f;
    }
    public override void ExitState() { }
    public override void UpdateState() 
    { 
        CalculateExpectedHndRotation();
        //Debug.Log("Closes Point On Collider Y " + Context.ClosestPointOnColliderFromShoulder.y);
        Context.InteractionPointYOffset = Mathf.Lerp(Context.InteractionPointYOffset, Context.ClosestPointOnColliderFromShoulder.y, _elapsedTime/_lerpDuration);

        Context.CurrentIkConstraint.weight = Mathf.Lerp(Context.CurrentIkConstraint.weight, _riseWeight, _elapsedTime / _lerpDuration);
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(Context.CurrentMultiRotationConstraint.weight, _riseWeight, _elapsedTime / _lerpDuration);

        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(Context.CurrentIkTargetTransform.rotation, _expectedRotation, _rotationSpeed * Time.deltaTime);

        _elapsedTime += Time.deltaTime;
    }

    private void CalculateExpectedHndRotation()
    {
        Vector3 startPos = Context.CurrentShoulderTransform.position;
        Vector3 endPos = Context.ClosestPointOnColliderFromShoulder;
        Vector3 direction = (endPos - startPos).normalized;

        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, _maxDistance, _interactableLayerMask))
        {
            Vector3 surfaceNormal = hit.normal;
            Vector3 targetForward = -surfaceNormal;
            _expectedRotation = Quaternion.LookRotation(targetForward, Vector3.up);
        }
    }

    public override EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState GetNextState()
    {
        if (CheckShoouldReset())
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Reset_Arm;
        }

        if (Vector3.Distance(Context.CurrentIkTargetTransform.position, Context.ClosestPointOnColliderFromShoulder) < _touchDistanceThreshold
            && _elapsedTime >= _touchTimeThreshold)
        {
            return EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState.Touch_Arm;
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
