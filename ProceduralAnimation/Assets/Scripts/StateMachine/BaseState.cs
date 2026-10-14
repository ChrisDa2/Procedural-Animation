using UnityEngine;
using System;

public abstract class BaseState<EState> where EState : Enum
{
    // Constructor
    public BaseState(EState key)
    {
        StateKey = key;
    }

    // Get & Set
    public EState StateKey { get; private set; }

    // Abstract methods
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract EState GetNextState();
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);
}
