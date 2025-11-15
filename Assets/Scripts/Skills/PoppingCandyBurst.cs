// PoppingCandyBurst.cs
using UnityEngine;

public class PoppingCandyBurst : MonoBehaviour
{
    float colliderRadius = 0.6f;
    float range = 6f;
    float speed = 11f;
    int damage = 5;

    bool spawned = false;

    // Bullet에서 호출
    public void Initialize(float colliderRadius, float range, float speed, int damage)
    {
        this.colliderRadius = Mathf.Max(0.01f, colliderRadius);
        this.range = Mathf.Max(0.1f, range);
        this.speed = Mathf.Max(0.1f, speed);
        this.damage = Mathf.Max(1, damage);
    }

    void Start()
    {
        SpawnIfNeeded();
    }

    void SpawnIfNeeded()
    {
        if (spawned) return;
        spawned = true;

        // 8방향(0~315, 45도 간격)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject shard = new GameObject($"PC_Sh{i}");
            shard.transform.position = transform.position;

            var comp = shard.AddComponent<PoppingCandyShard>();
            comp.Setup(dir, speed, range, damage, colliderRadius);

            // 부모로 묶어둠(정리용)
            shard.transform.SetParent(transform);
        }

        // 모든 자식이 자동 파괴되면 나도 파괴
        Destroy(gameObject, range / speed + 0.05f);
    }
}
