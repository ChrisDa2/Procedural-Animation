using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    public BaseState<EState> CurrentState { get; protected set; }

    protected bool IsTransitioningState = false;

    void Start() 
    { 
        CurrentState.EnterState();
    }

    void FixedUpdate() 
    {
        EState nextStateKey = CurrentState.GetNextState();
        //Debug.Log("Now Im in " + CurrentState + " and next is " + nextStateKey);

        if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        } 
        else if (!IsTransitioningState)
        {
            TransitionToState(nextStateKey);
        }
    }

    // Ends current state and begins the given state
    private void TransitionToState(EState newStateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[newStateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
    }

    // Triggers
    void OnTriggerEnter(Collider other) 
    {
        CurrentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other) 
    {
        CurrentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other) 
    {
        CurrentState.OnTriggerExit(other);
    }
}
