using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleWithSound : MonoBehaviour
{
    [SerializeField]
    protected int index = -1;
    protected ParticleSystem particle;

    private void Awake() => particle ??= GetComponent<ParticleSystem>();

    protected virtual void Start() {
        particle.Play();
        SFXManager.instance.PlaySound(index);
    }
}
