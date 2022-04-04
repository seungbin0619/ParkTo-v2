using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpScreen : MonoBehaviour
{
    public void OnAnimationExit()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
