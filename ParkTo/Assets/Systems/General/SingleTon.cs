using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTon<T>: MonoBehaviour where T : MonoBehaviour
{
    public static T instance;
    [SerializeField]
    private bool dontDestroy;

    protected virtual void Awake()
    {
        if (instance == null) instance = this as T;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (dontDestroy) DontDestroyOnLoad(gameObject);
    }
}
