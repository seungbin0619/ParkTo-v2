using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : SingleTon<EventManager>
{
    public GameEvent PrevMove;
    public GameEvent OnMove;

    public GameEvent OnChange;

    public GameEvent OnSelectTrigger;
    public GameEvent OnUnselectTrigger;
    public GameEvent OnSelectedTriggerStateChange;
}
