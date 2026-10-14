using UnityEngine;

public abstract class EnvironmentInteractionArmState : BaseState<EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState>
{
    protected EnvironmentInteractionContext Context;
    float _movingAwayOffset = 0.5f;
    bool _shouldReset;

    // Constructor
    public EnvironmentInteractionArmState(EnvironmentInteractionContext context, EnvironmentInteractionArmStateMachine.EEnvironmentInteractionArmState stateKey) : base(stateKey)
    {
        Context = context;
    }

    protected bool CheckShoouldReset()
    {
        if (_shouldReset)
        {
            // If should reset, skip other calculations
            Context.LowestDistence = float.PositiveInfinity;
            _shouldReset = false;
            return true;
        }

        //bool isPlayerStopped = Context.Rigidbody.linearVelocity == Vector3.zero;
        bool isMovingAway = CheckIsMovingAway();
        bool isBadAngle = CheckIsBadAngle();
        //bool isPlayerJumping = Context.Rigidbody.linearVelocity.y >= 1;       // Add this variable to the condition below if want to reset when jumping
        
        if (isMovingAway || isBadAngle)
        {
            Context.LowestDistence = float.PositiveInfinity;
            return true;
        }
        return false;
    }

    protected bool CheckIsBadAngle()
    {
        if (Context.CurrentIntersectingCollider == null)
        {
            // There's no object to interact on range
            return false;
        }

        Vector3 targetDirection = Context.ClosestPointOnColliderFromShoulder - Context.CurrentIkConstraint.transform.position;
        Vector3 shoulderDirection = Context.CurrentBodySide == EnvironmentInteractionContext.EbodySide.RIGHT ? Context.RootTransform.right : -Context.RootTransform.right;

        float dotProduct = Vector3.Dot(shoulderDirection, targetDirection.normalized);
        bool isBadAngle = dotProduct < 0.0f;

        return isBadAngle;
    }

    protected bool CheckIsMovingAway()
    {
        float currentDistanceToTarget = Vector3.Distance(Context.CurrentIkTargetTransform.position, Context.ClosestPointOnColliderFromShoulder);

        if (Context.CurrentIntersectingCollider == null)
        {
            // There's no object to interact on range
            return false;
        }

        if (currentDistanceToTarget <= Context.LowestDistence)
        {
            // Is getting closer to target
            Context.LowestDistence = currentDistanceToTarget;
            return false;
        }
        
        if (currentDistanceToTarget > Context.LowestDistence + _movingAwayOffset)
        {
            // Is moving away from target
            Context.LowestDistence = float.PositiveInfinity;
            return true;
        }

        return false;
    }

    private Vector3 GetClosestPointOnCollider(Collider intersectionCollider, Vector3 positionToCheck)
    {
        return intersectionCollider.ClosestPoint(positionToCheck);
    }

    protected void StartIkTargetPositionTracking(Collider intersectionCollider)
    {
        if (intersectionCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context.CurrentIntersectingCollider == null) 
        {
            Context.CurrentIntersectingCollider = intersectionCollider;
            Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectionCollider, Context.RootCollider.transform.position);
            Context.SetCurrentSideArm(closestPointFromRoot);

            SetIkTargetPosition();
        }
    }

    protected void UpdateIkTargetPosition(Collider intersectionCollider)
    {
        if (intersectionCollider == Context.CurrentIntersectingCollider)
        {
            SetIkTargetPosition();
        }
    }

    protected void ResetIkTargetPositionTracking(Collider intersectionCollider)
    {
        if (intersectionCollider == Context.CurrentIntersectingCollider)
        {
            Context.CurrentIntersectingCollider = null;
            Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
            _shouldReset = true;
        }
    }

    private void SetIkTargetPosition()
    {
        Context.UpdateClosestShoulderHeight();
        Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentIntersectingCollider, 
                new Vector3(Context.CurrentShoulderTransform.position.x, Context.CharacterShoulderHeight, Context.CurrentShoulderTransform.position.z));

        Vector3 rayDirection = Context.CurrentShoulderTransform.position - Context.ClosestPointOnColliderFromShoulder;
        Vector3 normalizedRayDirection = rayDirection.normalized;
        float offsetDistance = 0.05f;
        Vector3 offset = normalizedRayDirection * offsetDistance;

        Vector3 offsetPosition = Context.ClosestPointOnColliderFromShoulder + offset;
        Context.CurrentIkTargetTransform.position = new Vector3(offsetPosition.x, Context.InteractionPointYOffset, offsetPosition.z);
        // TODO: Add some space forward in the offset
    }
}
