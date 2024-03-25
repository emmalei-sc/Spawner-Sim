using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class SpawnUnit : MonoBehaviour
{
    // This implementation could potentially be replaced w/ inheritance (already pre-emptively set up) if we wanted units with unique behavior
    // But for now we are simply using an enum to track unit type
    public ObjectPoolManager.ObjectType type;
    public static int idCount = 0;
    
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float spawnBufferTime = 0.5f;

    protected Vector3 dest = Vector3.zero;
    protected Collider m_coll;
    protected Vector3 velocity = Vector3.zero;
    protected int spawnID;
    protected float spawnCooldownTimer = 0f;
    protected bool canSpawn = false;
    protected Vector3 lastCollisionPt = Vector3.zero;
    public int GetSpawnID() { return spawnID; }
    protected void Awake()
    {
        m_coll = GetComponent<Collider>();

        // Assign an id to this unit. This will be used to determine which unit spawns a clone upon collision
        spawnID = idCount;
        idCount++;
    }
    
    public virtual void Initialize() // To be called when spawned into scene
    {
        CalculateNewDestination();
        spawnCooldownTimer = 0f;
        canSpawn = true;
    }

    private void FixedUpdate()
    {
        if (!canSpawn)
        {
            if (spawnCooldownTimer > spawnBufferTime)
            {
                canSpawn = true;
                spawnCooldownTimer = 0f;
            }
            spawnCooldownTimer += Time.deltaTime;
        }
    }

    protected virtual void Update()
    {
        if (dest == Vector3.zero || velocity == Vector3.zero)
        {
            CalculateNewDestination();
        }
        else
        {
            transform.position += velocity * Time.deltaTime;

            if (Vector3.Distance(transform.position, dest) < 0.2f)
            {
                CalculateNewDestination();
            }
        }
    }
    

    protected void OnCollisionEnter(Collision collision)
    {
        SpawnUnit other = collision.gameObject.GetComponent<SpawnUnit>();
        if (other == null)
            return;

        ContactPoint collisionPt = collision.GetContact(0);

        // Handle collision with different type unit
        if (other.type != type)
        {
            ObjectPoolManager.instance.Release(gameObject);
        }
        // Handle collision with same type unit
        else
        {
            // Reflect this unit's movement around the collision normal
            Vector3 refl = Vector3.Reflect(velocity.normalized, collisionPt.normal);
            refl.y = 0f;
            refl.Normalize();
            //Debug.DrawLine(collisionPt.point, collisionPt.point + collisionPt.normal, Color.red, 10f);
            //Debug.DrawLine(collisionPt.point, collisionPt.point + refl, Color.green, 10f);

            CalculateNewDestination(refl);

            lastCollisionPt = collisionPt.point;

            /*// Have only one unit spawn a clone
            if (canSpawn && this.spawnID > other.GetSpawnID())
            {
                SpawnNewUnit(collisionPt.point);
                Debug.Log("spawn new unit here");
                canSpawn = false;
            }*/
        }
    }

    protected void OnCollisionStay(Collision collision) // If units spawn inside each other, separate them
    {
        SpawnUnit other = collision.gameObject.GetComponent<SpawnUnit>();
        if (other == null)
            return;

        ContactPoint collisionPt = collision.GetContact(0);
        Vector3 toOtherUnit = collision.gameObject.transform.position - transform.position;
        //Debug.DrawLine(transform.position, transform.position + toOtherUnit, Color.blue, 10f);
        toOtherUnit.y = 0f;
        toOtherUnit.Normalize();

        CalculateNewDestination(-1f * toOtherUnit);
        lastCollisionPt = collisionPt.point;
        canSpawn = false;
    }
    protected void OnCollisionExit(Collision collision)
    {
        SpawnUnit other = collision.gameObject.GetComponent<SpawnUnit>();
        if (other == null)
            return;

        if (lastCollisionPt == Vector3.zero) // If the collision pt was somehow not set, set it to a previous position in our path
            lastCollisionPt = transform.position - (velocity * m_coll.bounds.extents.x);

        // Have only one unit spawn a clone
        if (canSpawn && this.spawnID > other.GetSpawnID())
        {
            StartCoroutine(DelayedSpawn(spawnBufferTime, lastCollisionPt));
            Debug.Log("spawn new unit");
            canSpawn = false;
        }
    }

    IEnumerator DelayedSpawn(float seconds, Vector3 pos)
    {
        yield return new WaitForSeconds(seconds);
        SpawnNewUnit(pos);
    }

    private void SpawnNewUnit(Vector3 pos)
    {
        GameObject obj = ObjectPoolManager.instance.GetPooledObject(type);
        if (obj == null)
        {
            Debug.Log("Max Pool count reached; creating new object of type " + type);
            obj = ObjectPoolManager.instance.AddToPool(type);
        }
        // Move unit into position
        float halfHeight = obj.GetComponent<MeshRenderer>().bounds.extents.y;
        obj.transform.position = new Vector3(pos.x, halfHeight, pos.z);

        var unit = obj.GetComponent<SpawnUnit>();
        if (unit != null)
        {
            unit.Initialize();
        }
    }
    protected void CalculateNewDestination(Vector3 inDir = default) // optional param to find a position in direction inDir
    {
        // Set new destination and velocity
        if (inDir != default)
            dest = GetRandomPositionInDirection(inDir);
        else
            dest = GetRandomPosition();

        Vector3 dir = dest - transform.position;
        dir.Normalize();
        dir.y = 0f;
        velocity = dir * speed;
    }

    private Vector3 GetRandomPosition()
    {
        float x = UnityEngine.Random.Range(BoundsInfo.areaBounds.min.x, BoundsInfo.areaBounds.max.x);
        float z = UnityEngine.Random.Range(BoundsInfo.areaBounds.min.z, BoundsInfo.areaBounds.max.z);
        Vector3 pos = new Vector3(x, m_coll.bounds.extents.y, z);

        return pos;
    }
    private Vector3 GetRandomPositionInDirection(Vector3 dir)
    {
        // Scale the direction vector by a random number, but confine it to the bounds of the area
        float xMult = 0f;
        float xBound = 0f;
        if (dir.x < 0f)
            xBound = (transform.position.x - BoundsInfo.areaBounds.min.x) / Mathf.Abs(dir.x);
        else
            xBound = (BoundsInfo.areaBounds.max.x - transform.position.x) / Mathf.Abs(dir.x);
        xMult = UnityEngine.Random.Range(1f, xBound);
        xMult = xBound;

        float zMult = 0f;
        float zBound = 0f;
        if (dir.z < 0f)
            zBound = (transform.position.z - BoundsInfo.areaBounds.min.z) / Mathf.Abs(dir.z);
        else
            zBound = (BoundsInfo.areaBounds.max.z - transform.position.z) / Mathf.Abs(dir.z);
        zMult = UnityEngine.Random.Range(1f, zBound);
        zMult = zBound;

        Vector3 pos = new Vector3(dir.x * xMult, m_coll.bounds.extents.y, dir.z * zMult);
        //Debug.Log(gameObject.name + ": Getting Max Random Position " + pos + " in Direction: " + dir);
        return pos;
    }
}
