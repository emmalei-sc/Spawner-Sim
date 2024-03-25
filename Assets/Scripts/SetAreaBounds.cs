using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAreaBounds : MonoBehaviour
{
    private void Awake()
    {
        BoundsInfo.areaBounds = GetComponent<Collider>().bounds;
        Debug.Log("Area Bounds: Max "+BoundsInfo.areaBounds.max + " Min "+BoundsInfo.areaBounds.min);
    }
}
