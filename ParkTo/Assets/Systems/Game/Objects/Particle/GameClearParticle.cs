using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClearParticle : ParticleWithSound
{
    float delay = 0f, progress = 0f;
    bool isPlay = false;

    protected override void Start()
    {
        //base.Start();
        delay = Random.Range(0f, 1.5f);
    }

    private void Update() {
        if(isPlay) return;
        if(progress > delay) { 
            isPlay = true;

            particle.Play();
            SFXManager.instance.PlaySound(index);
        }

        progress += Time.deltaTime;
    }
}
