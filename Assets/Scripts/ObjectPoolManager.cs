using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance;
    
    [System.Serializable]
    public struct ObjectPool
    {
        public ObjectType type;
        public GameObject pooledObject;
        public int poolSize;
        [HideInInspector] public List<GameObject> objList;
    }
    public List<ObjectPool> objectPools = new List<ObjectPool>();

    private Dictionary<ObjectType, ObjectPool> poolLookup = new Dictionary<ObjectType, ObjectPool>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        foreach (var pool in objectPools)
        {
            // Keep a lookup table of each object type and its associated pool
            poolLookup.Add(pool.type, pool);

            // Create initial pool(s), parented to this manager
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.pooledObject, transform);
                obj.SetActive(false);
                pool.objList.Add(obj);
            }
        }
    }

    public GameObject GetPooledObject(ObjectType type)
    {
        ObjectPool pool = poolLookup[type];

        for (int i=0; i< pool.poolSize; i++)
        {
            if (!pool.objList[i].activeInHierarchy)
            {
                pool.objList[i].transform.parent = null;
                pool.objList[i].SetActive(true);
                return pool.objList[i];
            }
        }
        return null;
    }

    public GameObject AddToPool(ObjectType type)
    {
        ObjectPool pool = poolLookup[type];

        GameObject obj = Instantiate(pool.pooledObject);
        obj.SetActive(true);
        pool.objList.Add(obj);

        return obj;
    }

    public void Release(GameObject obj)
    {
        obj.transform.parent = transform;
        obj.SetActive(false);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
