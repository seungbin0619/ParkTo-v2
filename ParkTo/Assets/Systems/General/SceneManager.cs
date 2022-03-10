using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : ActionBase
{
    private AsyncOperation operation;

    public void MoveScene(Action action, object[] args)
    {
        string sceneName = args[0].ToString();
        operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        operation.completed += (e) =>
        {
            action.Complete();
            operation = null;
        };
    }
}
