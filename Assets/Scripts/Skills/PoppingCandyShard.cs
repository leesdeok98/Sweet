// PoppingCandyShard.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PoppingCandyShard : MonoBehaviour
{
    Vector2 dir;
    float speed;
    float maxDistance;
    int damage;
    float radius;

    Vector3 startPos;
    HashSet<Enemy> hitSet = new HashSet<Enemy>();
    CircleCollider2D col;

    [Header("Debug Visual")]
    [SerializeField] bool showVisual = true;
    //[SerializeField] Color visualColor = new Color(0.2f, 0.8f, 1f, 0.4f);
    [SerializeField] int circlePixels = 64;

    SpriteRenderer sr;

    //  Enemy 레이어 번호
    private int enemyLayer;

    public void Setup(Vector2 direction, float speed, float maxDistance, int damage, float colliderRadius)
    {
        this.dir = direction.normalized;
        this.speed = speed;
        this.maxDistance = maxDistance;
        this.damage = damage;
        this.radius = colliderRadius;
    }

    void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        //sr.sortingOrder = 15;

        //  Enemy 레이어 캐싱
        enemyLayer = LayerMask.NameToLayer("Enemy");

        // 프리팹에 SpriteRenderer가 없다면 자동 생성해 원형 시각화
        if (showVisual && GetComponent<SpriteRenderer>() == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 200;
            sr.sprite = GenerateCircleSprite(circlePixels);
            //sr.color = visualColor;
        }
        else
        {
            sr = GetComponent<SpriteRenderer>();
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.PoppingCandy_SFX, 0.1f);
    }

    void Start()
    {
        startPos = transform.position;
        col.radius = radius;
        UpdateVisualScale();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        transform.position += (Vector3)(dir * speed * dt);

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ★ Enemy 레이어가 아니면 무시
        if (other.gameObject.layer != enemyLayer) return;

        var e = other.GetComponent<Enemy>();
        if (e == null) return;
        if (hitSet.Contains(e)) return;

        hitSet.Add(e);
        e.TakeDamage(damage);
        // 한 번만 맞게 하려면 아래 주석 해제:
        // Destroy(gameObject);
    }

    void UpdateVisualScale()
    {
        if (sr == null || sr.sprite == null) return;
        float diameter = Mathf.Max(0.01f, radius * 2f);
        transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    Sprite GenerateCircleSprite(int size)
    {
        size = Mathf.Max(16, size);
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        float r = size * 0.5f - 1f;
        float r2 = r * r;
        Vector2 c = new Vector2(r + 0.5f, r + 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int idx = y * size + x;
                float dx = x - c.x;
                float dy = y - c.y;
                float d2 = dx * dx + dy * dy;

                if (d2 <= r2)
                {
                    float edge = Mathf.Clamp01((r - Mathf.Sqrt(d2)) / 2f);
                    pixels[idx] = new Color(1f, 1f, 1f, 0.5f + edge * 0.5f);
                }
                else
                {
                    pixels[idx] = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        var rect = new Rect(0, 0, size, size);
        var pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(tex, rect, pivot, 100f);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        //if (sr != null) sr.color = visualColor;
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = radius;
        }
        UpdateVisualScale();
    }
#endif
}
