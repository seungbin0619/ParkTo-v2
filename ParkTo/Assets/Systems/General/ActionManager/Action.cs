using UnityEngine.Events;

[System.Serializable]
public class Action
{
    public string id;
    public int argCount;
    public UnityEvent<Action, object[]> behaviors;

    public delegate void CompleteEventHandler();
    public CompleteEventHandler Complete;
}
