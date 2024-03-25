using System.Collections;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SpawnUnit : MonoBehaviour
{
    // This implementation could potentially be replaced w/ inheritance (already pre-emptively set up) if we wanted units with unique behavior
    // But for now we are simply using an enum to track unit type
    public ObjectType type;
    public static int idCount = 0;
    
    [SerializeField] protected float speed;

    [Header("Spawn Optimizaton: Lower Limit")]
    [SerializeField] protected int lowerLoadLimit;
    [SerializeField] protected float minSpawnCooldown;
    [SerializeField] protected float minPercentDisabled;
    [Header("Spawn Optimizaton: Upper Limit")]
    [SerializeField] protected int upperLoadLimit;
    [SerializeField] protected float maxSpawnCooldown;
    [SerializeField] protected float maxPercentDisabled;

    protected Vector3 dest = Vector3.zero;
    protected Collider m_coll;
    protected Vector3 velocity = Vector3.zero;
    protected int spawnID;
    protected float spawnCooldown;
    protected float spawnCooldownTimer = 0f;
    protected bool onCooldown = true;
    //protected bool offCooldown = false;
    protected Vector3 lastCollisionPt = Vector3.zero;
    protected bool spawnInProgress = false;
    protected bool disableNewSpawns = false;

    public int GetSpawnID() { return spawnID; }
    protected void Awake()
    {
        m_coll = GetComponent<Collider>();

        // Assign an id to this unit. This will be used to determine which unit spawns a clone upon collision
        spawnID = idCount;
        idCount++;

        spawnCooldown = minSpawnCooldown;
    }
    
    public virtual void Initialize() // To be called when spawned into scene
    {
        CalculateNewDestination();
        spawnCooldownTimer = 0f;
        onCooldown = true;
    }

    private void FixedUpdate()
    {
        if (!spawnInProgress && !disableNewSpawns && onCooldown)
        {
            if (spawnCooldownTimer > spawnCooldown)
            {
                onCooldown = false;
                spawnCooldownTimer = 0f;
            }
            spawnCooldownTimer += Time.deltaTime;
        }

        // Manually cool down our spawn rate
        Spawner sp = SpawnerManager.instance.GetSpawnerOfType(type);
        int spawnCnt = sp.spawnCount;
        if (spawnCnt > lowerLoadLimit)
        {
            disableNewSpawns = (Random.value > (1 - CalculateDisabledRate(spawnCnt))); // % chance of disabling new spawns
            spawnCooldown = CalculateSpawnRate(spawnCnt); // Adjust time between spawns
        }
    }
    private float CalculateSpawnRate(int spawnCnt)
    {
        float ratio = (spawnCnt - lowerLoadLimit) / (upperLoadLimit - lowerLoadLimit);
        float percent = Mathf.Lerp(minSpawnCooldown, maxSpawnCooldown, ratio);
        return percent;
    }
    private float CalculateDisabledRate(int spawnCnt)
    {
        float ratio = (spawnCnt - lowerLoadLimit) / (upperLoadLimit - lowerLoadLimit);
        float percent = Mathf.Lerp(minPercentDisabled, maxPercentDisabled, ratio);
        return percent;
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
            Vector3 toDest = dest - transform.position;

            if (toDest.sqrMagnitude < 0.2f*0.2f ||
                transform.position.x < BoundsInfo.areaBounds.min.x ||
                transform.position.x > BoundsInfo.areaBounds.max.x ||
                transform.position.z < BoundsInfo.areaBounds.min.z ||
                transform.position.z > BoundsInfo.areaBounds.max.z)
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
            SpawnerManager.instance.GetSpawnerOfType(type).DecreaseSpawnCount();
        }
        // Handle collision with same type unit
        else if (!disableNewSpawns)
        {
            // Reflect this unit's movement around the collision normal
            Vector3 refl = Vector3.Reflect(velocity.normalized, collisionPt.normal);
            refl.y = 0f;
            refl.Normalize();

            CalculateNewDestination(refl);

            lastCollisionPt = collisionPt.point;

            // Have only one unit spawn a clone
            if (!onCooldown && this.spawnID > other.GetSpawnID())
            {
                Spawner sp = SpawnerManager.instance.GetSpawnerOfType(type);
                sp.SpawnUnit(collisionPt.point);
                onCooldown = true;
            }
        }
    }

    protected void OnCollisionStay(Collision collision) // If units spawn inside each other, separate them
    {
        SpawnUnit other = collision.gameObject.GetComponent<SpawnUnit>();
        if (other == null)
            return;

        Vector3 toOtherUnit = collision.gameObject.transform.position - transform.position;
        toOtherUnit.y = 0f;
        toOtherUnit.Normalize();

        CalculateNewDestination(-1f * toOtherUnit);
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
        dir.Normalize();

        // Scale the direction vector by a random number, but confine it to the bounds of the area
        float xBound;
        if (dir.x < 0f)
            xBound = (transform.position.x - BoundsInfo.areaBounds.min.x) / Mathf.Abs(dir.x);
        else
            xBound = (BoundsInfo.areaBounds.max.x - transform.position.x) / Mathf.Abs(dir.x);

        float zBound;
        if (dir.z < 0f)
            zBound = (transform.position.z - BoundsInfo.areaBounds.min.z) / Mathf.Abs(dir.z);
        else
            zBound = (BoundsInfo.areaBounds.max.z - transform.position.z) / Mathf.Abs(dir.z);

        float bound = Mathf.Min(xBound, zBound);

        float mult = Random.Range(1f, bound);
        Vector3 scaledDir = dir * mult;

        Vector3 pos = transform.position + scaledDir;
        return pos;
    }
}
