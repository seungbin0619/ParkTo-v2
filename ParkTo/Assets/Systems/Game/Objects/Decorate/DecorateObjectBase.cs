using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorateObjectBase : MonoBehaviour
{
    Transform shadow;
    void Awake()
    {
        shadow = transform.GetChild(0);

        transform.localScale = Vector3.zero;
        shadow.localScale = Vector3.zero;
    }

    void Start() {
        StartCoroutine(Show());
    }

    IEnumerator Show() {
        yield return Wait();
        yield return ShowEffect();
    }

    protected virtual IEnumerator ShowEffect() {
        float progress = 0, duration = 0.4f;

        while(progress < duration) {
            yield return YieldDictionary.WaitForEndOfFrame;

            progress += Time.deltaTime;
            float clamp = progress / duration;

            transform.localScale = shadow.localScale = 
                LineAnimation.LerpBound(Vector3.zero, Vector3.one, clamp, 0.3f, 0.7f);
        }

        transform.localScale = shadow.localScale = Vector3.one;
    }

    IEnumerator Wait() {
        yield return YieldDictionary.WaitForSeconds(Random.Range(0, 30) * 0.01f);
    }
}
