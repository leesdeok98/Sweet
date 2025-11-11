using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed = 5f;

    private Rigidbody2D rigid;
    private SpriteRenderer spr;

    // 스킬 보유 상태
    public bool hasIcedJellySkill = false;
    public bool hasSugarShield = false;
    public bool hasDarkChip = false;
    public bool hasRollingChocolateBar = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (inputVec.x != 0)
            spr.flipX = (inputVec.x < 0);
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("플레이어가 " + damage + " 만큼 데미지를 입음");
    }

    void Die()
    {
        GameManager.instance.GameOver();
        gameObject.SetActive(false);
    }
}