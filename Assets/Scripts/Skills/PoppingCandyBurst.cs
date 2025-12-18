using UnityEngine;

public class PoppingCandyBurst : MonoBehaviour
{
    float colliderRadius;
    float range;
    float speed;
    int damage;

    GameObject defaultShardPrefab;
    GameObject[] shardPrefabs; 

    bool spawned;

    public void Initialize(
        float colliderRadius, float range, float speed, int damage,
        GameObject defaultShardPrefab, GameObject[] shardPrefabs)
    {
        this.colliderRadius = Mathf.Max(0.01f, colliderRadius);
        this.range = Mathf.Max(0.1f, range);
        this.speed = Mathf.Max(0.1f, speed);
        this.damage = Mathf.Max(1, damage);
        this.defaultShardPrefab = defaultShardPrefab;
        this.shardPrefabs = shardPrefabs;
    }

    void Start()
    {
        if (!spawned) Spawn();
    }

    void Spawn()
    {
        spawned = true;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject prefab = (shardPrefabs != null && i < shardPrefabs.Length) ? shardPrefabs[i] : null;
            if (prefab == null) prefab = defaultShardPrefab;

            GameObject shardGO;
            if (prefab != null)
            {
                shardGO = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                
            }
            else
            {
                
                shardGO = new GameObject($"PC_Sh{i}");
                shardGO.transform.SetParent(transform);
                var col = shardGO.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
            }

            var shard = shardGO.GetComponent<PoppingCandyShard>();
            if (shard == null) shard = shardGO.AddComponent<PoppingCandyShard>();

            shard.Setup(dir, speed, range, damage, colliderRadius);
        }

        Destroy(gameObject, range / speed + 0.1f);
    }
}
