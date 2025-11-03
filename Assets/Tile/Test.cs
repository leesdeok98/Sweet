using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] tiles; // 3x3 타일 (총 9개)

    [Header("설정값")]
    [SerializeField] private float unitSize = 10f; // 타일 한 변 길이
    [SerializeField] private float moveSpeed = 10f;

    private float halfSight;
    private Vector2[] border;

    private void Start()
    {
        // 카메라 절반 높이 기준 + 여유 공간
        halfSight = Camera.main.orthographicSize + unitSize * 0.5f;

        border = new Vector2[]
        {
            new Vector2(-unitSize * 1.5f, unitSize * 1.5f),
            new Vector2(unitSize * 1.5f, -unitSize * 1.5f)
        };

        // 3x3 타일 초기 배치
        int index = 0;
        for (int y = 1; y >= -1; y--)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (index >= tiles.Length) return;
                tiles[index].transform.position = new Vector3(x * unitSize, y * unitSize, 0);
                index++;
            }
        }
    }

    private void Update()
    {
        Vector3 delta = Vector3.zero;

        // 입력 처리
        if (Input.GetKey(KeyCode.W)) delta += Vector3.up;
        if (Input.GetKey(KeyCode.S)) delta += Vector3.down;
        if (Input.GetKey(KeyCode.A)) delta += Vector3.left;
        if (Input.GetKey(KeyCode.D)) delta += Vector3.right;

        delta *= moveSpeed * Time.deltaTime;

        // 플레이어와 카메라 이동
        player.transform.position += delta;
        Camera.main.transform.position += delta;

        CheckBoundary();
    }

    private void CheckBoundary()
    {
        Vector3 pos = player.transform.position;

        // 대각선 포함 모든 축 체크
        if (border[1].x < pos.x + halfSight)
        {
            border[0] += Vector2.right * unitSize;
            border[1] += Vector2.right * unitSize;
        }
        if (border[0].x > pos.x - halfSight)
        {
            border[0] -= Vector2.right * unitSize;
            border[1] -= Vector2.right * unitSize;
        }
        if (border[0].y < pos.y + halfSight)
        {
            border[0] += Vector2.up * unitSize;
            border[1] += Vector2.up * unitSize;
        }
        if (border[1].y > pos.y - halfSight)
        {
            border[0] -= Vector2.up * unitSize;
            border[1] -= Vector2.up * unitSize;
        }

        // 타일 재배치
        ShiftTiles();
    }

    private void ShiftTiles()
    {
        Vector3 playerPos = player.transform.position;

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector3 pos = tiles[i].transform.position;

            // x축 이동
            if (pos.x < playerPos.x - unitSize * 1.5f)
                tiles[i].transform.position += Vector3.right * unitSize * 3;
            if (pos.x > playerPos.x + unitSize * 1.5f)
                tiles[i].transform.position -= Vector3.right * unitSize * 3;

            // y축 이동
            if (pos.y < playerPos.y - unitSize * 1.5f)
                tiles[i].transform.position += Vector3.up * unitSize * 3;
            if (pos.y > playerPos.y + unitSize * 1.5f)
                tiles[i].transform.position -= Vector3.up * unitSize * 3;
        }
    }
}
