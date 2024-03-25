using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private ObjectType objectType;
    [SerializeField] private Transform spawnPosition;

    public int spawnCount { get; private set; }

    private void OnMouseDown()
    {
        SpawnUnit(spawnPosition.position);
    }

    public void SpawnUnit(Vector3 pos)
    {
        GameObject obj = ObjectPoolManager.instance.GetPooledObject(objectType);
        if (obj == null)
        {
            Debug.Log("Max Pool count reached; creating new object of type "+objectType);
            obj = ObjectPoolManager.instance.AddToPool(objectType);
        }
        // Move unit to position
        obj.transform.SetParent(transform);
        float halfHeight = obj.GetComponent<MeshRenderer>().bounds.extents.y;
        obj.transform.position = new Vector3(pos.x, halfHeight, pos.z);

        // Start unit movement
        var unit = obj.GetComponent<SpawnUnit>();
        if (unit != null)
        {
            unit.Initialize();
        }

        spawnCount++;
    }

    public ObjectType GetObjectType() { return objectType; }

    public void DecreaseSpawnCount() { spawnCount--; }
}
