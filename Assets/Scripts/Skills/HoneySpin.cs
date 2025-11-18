using UnityEngine;
using Spine.Unity;   // ★ 스파인용 네임스페이스 추가

/// <summary>
/// 허니스핀 구체 1개를 관리하는 스크립트
/// - 플레이어 주변을 원운동
/// - Enemy(일반 몬스터 + 보스)에 닿으면
///     → damage 만큼 데미지
///     → slowPercent 만큼 이동속도 감소 디버프 (slowDuration초)
/// - 스파인 애니메이션(loop) 재생 + 이동 방향에 맞춰 회전
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HoneySpin : MonoBehaviour
{
    // ───────── Spine 관련 ─────────
    [Header("Spine")]
    [Tooltip("이 오브젝트나 자식에 붙어 있는 SkeletonAnimation. 비워두면 자동으로 찾음")]
    public SkeletonAnimation skeleton;

    [Tooltip("재생할 애니메이션 이름 (Spine에서 만든 애니 이름 그대로)")]
    public string spinAnimationName = "loop";   // 아트팀이 정한 이름으로 바꿔도 됨

    [Tooltip("스파인 애니메이션 재생 속도 (1 = 기본 속도)")]
    public float spineTimeScale = 1f;

    // ───────── 궤도/공격 관련 ─────────
    private Transform target;      // 플레이어 Transform
    private float radius;          // 플레이어로부터 거리
    private float rotateSpeed;     // 초당 회전 각도 (deg/sec)
    private float angle;           // 현재 각도 (deg)

    private float damage;          // 충돌 시 데미지
    private float slowPercent;     // 0.3f = -30%
    private float slowDuration;    // 디버프 지속 시간 (초)

    private Collider2D col;

    [Header("Visual Rotation")]
    [Tooltip("이동 방향(궤도 접선 방향)을 바라보게 회전할지 여부")]
    public bool faceMoveDirection = true;

    [Tooltip("스프라이트 기본 방향 보정(도 단위). \n기본 이미지가 오른쪽(+X)을 보고 있으면 0, \n위(+Y)를 보고 있으면 90 정도로 조정")]
    public float visualRotationOffset = 0f;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true; // 허니스핀은 트리거 충돌
        }

        // ★ SkeletonAnimation 자동 할당
        if (skeleton == null)
        {
            // 같은 오브젝트 또는 자식에서 찾아보기
            skeleton = GetComponent<SkeletonAnimation>();
            if (skeleton == null)
                skeleton = GetComponentInChildren<SkeletonAnimation>();
        }

        // ★ 스파인 애니메이션 재생 설정
        if (skeleton != null && !string.IsNullOrEmpty(spinAnimationName))
        {
            skeleton.timeScale = spineTimeScale;
            skeleton.AnimationState.SetAnimation(0, spinAnimationName, true); // loop = true
        }
        else
        {
            Debug.LogWarning("[HoneySpin] SkeletonAnimation 또는 spinAnimationName이 비어 있습니다.");
        }
    }

    /// <summary>
    /// SkillManager에서 생성 직후 한 번 호출해서 전체 파라미터 세팅
    /// </summary>
    public void Initialize(
        Transform target,
        float radius,
        float rotateSpeed,
        float startAngleDeg,
        float damage,
        float slowPercent,
        float slowDuration
    )
    {
        this.target = target;
        this.radius = radius;
        this.rotateSpeed = rotateSpeed;
        this.angle = startAngleDeg;
        this.damage = damage;
        this.slowPercent = slowPercent;
        this.slowDuration = slowDuration;

        // 시작 위치 즉시 반영
        UpdatePositionAndRotation();
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 각도 증가
        angle += rotateSpeed * Time.deltaTime;

        UpdatePositionAndRotation();
    }

    /// <summary>
    /// 현재 angle, radius를 기준으로 플레이어 주변 원운동 위치 + 회전 계산
    /// </summary>
    void UpdatePositionAndRotation()
    {
        if (target == null) return;

        float rad = angle * Mathf.Deg2Rad;
        Vector2 center = target.position;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

        // 위치
        Vector2 pos = center + offset;
        transform.position = pos;

        // 회전 (이미지 고정이 아니라 꼬리/머리 방향을 맞추고 싶을 때)
        if (faceMoveDirection)
        {
            // 원운동의 접선 방향 (속도 방향 벡터)
            // angle이 커질수록 반시계 방향 기준:
            // 접선 벡터 v = (-sin, +cos)
            Vector2 tangent = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));

            float z = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            z += visualRotationOffset; // 스파인 기본 방향 보정

            transform.rotation = Quaternion.Euler(0f, 0f, z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy(보스 포함)만 타격
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        // 1) 데미지
        enemy.TakeDamage(damage);

        // 2) 이동속도 감소 디버프
        try
        {
            enemy.ApplySlow(slowPercent, slowDuration);
        }
        catch
        {
            Debug.LogWarning("[HoneySpin] Enemy에 ApplySlow(float, float)가 없습니다. IcedJelly와 동일하게 구현해 주세요.");
        }
    }
}
