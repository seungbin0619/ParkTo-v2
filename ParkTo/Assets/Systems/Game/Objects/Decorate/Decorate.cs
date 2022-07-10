using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decorate : MonoBehaviour
{
    private const float Delta = 0.1f;
    void Start()
    {
        Vector3 position = transform.position;
        position += new Vector3(Random.Range(-Delta, Delta), Random.Range(-Delta, Delta));

        transform.position = position;
    }
}
