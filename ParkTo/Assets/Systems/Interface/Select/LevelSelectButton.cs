using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    public void Select(int index) => SelectManager.instance.SelectIndex(index);
}
