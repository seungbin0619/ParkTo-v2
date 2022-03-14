using System.Collections.Generic;
using ActionData = System.Tuple<Action, object[]>;

public class ActionManager : SingleTon<ActionManager>
{
    private static readonly Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private static readonly List<ActionData> playLists = new List<ActionData>();

    public static bool IsPlaying { private set; get; }
    public static bool IsCompleted => playLists.Count == 0;

    public static void RegisterAction(Action action)
    {
        if (action.id == "") return;
        if (actions.ContainsKey(action.id)) return;

        actions.Add(action.id, action);
    }

    public static void DeleteAction(Action action)
    {
        if (!actions.ContainsKey(action.id)) return;
        actions.Remove(action.id);
    }

    public static void AddAction(string id, params object[] args)
    {
        if (!actions.ContainsKey(id)) return;
        if (actions[id].argCount != args.Length) return;

        playLists.Add(new ActionData(actions[id], args));
        print(id);
    }

    public static void Play()
    {
        if (IsCompleted) return;
        IsPlaying = true;

        Next();
    }

    private static void Next()
    {
        if (IsCompleted)
        {
            IsPlaying = false;
            return;
        }

        ActionData actionData = playLists[0];
        Action action = actionData.Item1;
        playLists.RemoveAt(0);

        action.Complete = () => Next();
        action.behaviors.Invoke(action, actionData.Item2);
    }
}
