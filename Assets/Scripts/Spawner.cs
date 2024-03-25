using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager.ObjectType objectType;
    //[SerializeField] private Vector3 spawnOffset;
    [SerializeField] private Transform spawnPosition;

    private void OnMouseDown()
    {
        SpawnUnit();
    }

    private void SpawnUnit()
    {
        GameObject obj = ObjectPoolManager.instance.GetPooledObject(objectType);
        if (obj == null)
        {
            Debug.Log("Max Pool count reached; creating new object of type "+objectType);
            obj = ObjectPoolManager.instance.AddToPool(objectType);
        }
        // Move unit to spawn position
        obj.transform.SetParent(transform);
        float halfHeight = obj.GetComponent<MeshRenderer>().bounds.extents.y;
        obj.transform.position = new Vector3(spawnPosition.position.x, halfHeight, spawnPosition.position.z);

        // Start unit movement
        var unit = obj.GetComponent<SpawnUnit>();
        if (unit != null)
        {
            unit.Initialize();
        }
    }
}
