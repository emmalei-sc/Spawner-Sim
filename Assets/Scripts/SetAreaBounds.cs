using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAreaBounds : MonoBehaviour
{
    private void Awake()
    {
        BoundsInfo.areaBounds = GetComponent<Collider>().bounds;
    }
}
