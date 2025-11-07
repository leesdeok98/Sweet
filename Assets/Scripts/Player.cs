using UnityEngine;

public class Player : MonoBehaviour
{
    // 나 이성덕은 바보다 - 성덕
    public Vector2 inputVec; // ���� �Է� ����
    public float speed = 5f; // �̵� �ӵ�

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
