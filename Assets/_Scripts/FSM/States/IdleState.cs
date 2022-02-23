using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleState", menuName = "Unity_FSM/States/Idle",order =1)]
public class IdleState : AbstractFSMState
{
    public override bool EnterState()
    {
        base.EnterState();
        Debug.Log("ENTERED IDLE STATE");
        return false;
    }
    public override void UpdateState()
    {
        Debug.Log("UPDATING IDLE STATE");
    }
    public override bool ExitState()
    {
        base.EnterState();
        Debug.Log("EXITING IDLE STATE");
        return false;
    }
}
