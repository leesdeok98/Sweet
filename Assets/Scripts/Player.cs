using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 inputVec; // 현재 입력 방향
    public float speed = 5f; // 이동 속도

    private Rigidbody2D rigid;
    SpriteRenderer spr;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (inputVec.x != 0)
        {
            spr.flipX = (inputVec.x < 0);
        }
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
}
