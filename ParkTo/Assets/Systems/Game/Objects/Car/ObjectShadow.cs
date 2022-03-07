using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShadow : MonoBehaviour
{
    private Vector3 shadowDistance;

    [SerializeField] private Transform parent;
    [SerializeField] private float distance = 0.1f;

    private void Awake()
    {
        shadowDistance = new Vector3(0, -distance, 0);
    }

    private void Update()
    {
        transform.position = parent.position + shadowDistance;
    }
}
