using System.Collections.Generic;
using UnityEngine;

public class ActionBase : MonoBehaviour
{
    [SerializeField]
    private List<Action> actions;
    [System.NonSerialized]
    public Action currentAction;

    protected virtual void OnEnable()
    {
        foreach (Action action in actions)
            ActionManager.RegisterAction(action);
    }

    protected virtual void OnDisable()
    {
        foreach (Action action in actions)
            ActionManager.DeleteAction(action);
    }
}
