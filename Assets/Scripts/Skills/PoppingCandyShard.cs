// PoppingCandyShard.cs (색상/투명도 인스펙터에서 바로 보이게)
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

    // ▼▼ 시각화 옵션
    [Header("Debug Visual")]
    [SerializeField] bool showVisual = true;

    // ✅ 알파 포함해서 인스펙터에서 바로 조절
    [SerializeField] Color visualColor = new Color(0.2f, 0.8f, 1f, 0.4f);

    // 생성 텍스처 해상도 (값 높을수록 가장자리 부드러움)
    [SerializeField] int circlePixels = 64;

    SpriteRenderer sr;

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
        col.usedByComposite = false;

        if (showVisual)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default"; // 필요시 "Effects" 등으로 변경
            sr.sortingOrder = 200;           // 위에 보이도록
            sr.sprite = GenerateCircleSprite(circlePixels); // 흰색 원 스프라이트 생성
            sr.color = visualColor;                         // ✅ 인스펙터 색/알파 적용
            // (URP에서 투명 블렌딩 이슈가 있으면, Material을 Sprites/Default로 둬야 함)
            UpdateVisualScale();
        }
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
        if (!other.CompareTag("Enemy") && !other.CompareTag("Boss")) return;
        Enemy e = other.GetComponent<Enemy>();
        if (e == null) return;
        if (hitSet.Contains(e)) return;

        hitSet.Add(e);
        e.TakeDamage(damage);
        // Destroy(gameObject); // 1히트 후 사라지게 하려면 주석 해제
    }

    void UpdateVisualScale()
    {
        if (sr == null || sr.sprite == null) return;
        float diameter = Mathf.Max(0.01f, radius * 2f);
        transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    // ★ 흰색 원 스프라이트 생성 (틴트로 색상/투명도 적용)
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
                    // 가장자리 부드럽게
                    float edge = Mathf.Clamp01((r - Mathf.Sqrt(d2)) / 2f);
                    // 흰색(틴트 적용 전)
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
        // PPU=100 → localScale로 반지름 직접 맞추는 방식 유지
        return Sprite.Create(tex, rect, pivot, 100f);
    }

    // 인스펙터에서 값 바꿨을 때 즉시 반영되게(플레이 중/편집 모드 모두)
#if UNITY_EDITOR
    void OnValidate()
    {
        if (sr != null)
        {
            sr.color = visualColor;  // 색/투명도 변경 즉시 반영
            UpdateVisualScale();
        }
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = radius;
        }
    }
#endif
}
