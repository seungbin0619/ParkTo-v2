using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    void Start()
    {
        ActionManager.AddAction("Move", "Game");
        ActionManager.AddAction("FadeOut", 0.5f);

        ActionManager.Play();
    }
}
