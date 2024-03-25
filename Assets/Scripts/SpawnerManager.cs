using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager instance;
    public List<GameObject> spawnerList = new List<GameObject>();

    private Dictionary<ObjectType, Spawner> spawnerLookup;

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

        spawnerLookup = new Dictionary<ObjectType, Spawner>();
        foreach (GameObject obj in spawnerList)
        {
            Spawner spawnerComp = obj.GetComponent<Spawner>();
            if (spawnerComp != null)
            {
                spawnerLookup.Add(spawnerComp.GetObjectType(), spawnerComp);
            }
        }
    }

    public Spawner GetSpawnerOfType(ObjectType type)
    {
        return spawnerLookup[type];
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
