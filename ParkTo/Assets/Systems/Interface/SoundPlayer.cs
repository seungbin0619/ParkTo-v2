using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public void PlaySound(int index = -1) => SFXManager.instance?.PlaySound(index);
}
