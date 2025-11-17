using System.Collections;
using UnityEngine;

public class StrawberryPopCoreSkill : MonoBehaviour
{
    [Header("PopCore Settings")]
    public GameObject bulletPrefab;     // 발사할 총알 프리팹
    [Range(0f, 1f)] public float popChancePer = 0.3f;      // 발동 확률
    public int extraBulletCount = 8;    // 추가로 발사할 총알 개수
    public float bulletSpeed = 5f;      // 총알 속도

    Transform player;

    void Awake()
    {
        player = transform; // 이 스킬은 Player에 붙어 있어야 함
    }

    // Player에서 호출할 함수
    public void UseSkill()
    {
        float roll = Random.value; 

        Debug.Log($"PopCore 확률 체크 (0~99): {roll} / 필요 확률: {popChancePer}");

        // 확률 체크
        if (roll <= popChancePer)
        {
            Debug.Log("PopCore 발동!");
            FirePopCore();
        }
    }

    //  실제 사방으로 탄 발사 메서드
    void FirePopCore()
    {
        Debug.Log("딸기 팝코어 실행");

        int total = extraBulletCount;
        float angleStep = 360f / total;

        for (int i = 0; i < total; i++)
        {
            float angle = i * angleStep;
            SpawnBullet(angle);
        }
    }

    // 총알 생성 메서드
    void SpawnBullet(float angle)
    {
        GameObject newBullet = Instantiate(bulletPrefab, player.position, Quaternion.identity);

        // 각도 ->  방향벡터 변환
        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 총알에 속도 설정
        Rigidbody2D rb = newBullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = dir * bulletSpeed;
        }
    }
}
