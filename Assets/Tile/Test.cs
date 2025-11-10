using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] tiles; // rows*cols 개수만큼 넣기

    [Header("설정값")]
    [SerializeField] private float unitSize = 10f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("그리드 크기")]
    [SerializeField] private int cols = 3; // 가로 타일 수
    [SerializeField] private int rows = 4; // 세로 타일 수(← 한 줄 늘렸음)

    private float halfSightX, halfSightY;
    private Vector2[] border;
    private Camera cam;

    // 계산용(그리드 절반/이동량)
    private float halfGridX, halfGridY, moveSpanX, moveSpanY;

    private void Start()
    {
        cam = Camera.main;

        // 카메라 시야 + 여유(0.5타일)
        halfSightY = cam.orthographicSize + unitSize * 0.5f;
        halfSightX = cam.orthographicSize * cam.aspect + unitSize * 0.5f;

        // 그리드 절반/이동량(열/행 기반)
        halfGridX = cols * unitSize * 0.5f;
        halfGridY = rows * unitSize * 0.5f;
        moveSpanX = cols * unitSize;
        moveSpanY = rows * unitSize;

        border = new Vector2[]
        {
            new Vector2(-unitSize * 1.5f,  unitSize * 1.5f),
            new Vector2( unitSize * 1.5f, -unitSize * 1.5f)
        };

        // ✅ 초기 배치: 짝/홀 모두 중심 정렬되도록 계산
        // (0,0)을 화면 중앙에 두고 cols×rows 격자로 배치
        float x0 = -(cols - 1) * 0.5f * unitSize;
        float y0 = (rows - 1) * 0.5f * unitSize;

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (index >= tiles.Length) return;
                tiles[index].transform.position = new Vector3(
                    x0 + c * unitSize,
                    y0 - r * unitSize,
                    0f
                );
                index++;
            }
        }

    }

    private void Update()
    {
        Vector3 delta = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) delta += Vector3.up;
        if (Input.GetKey(KeyCode.S)) delta += Vector3.down;
        if (Input.GetKey(KeyCode.A)) delta += Vector3.left;
        if (Input.GetKey(KeyCode.D)) delta += Vector3.right;

        delta *= moveSpeed * Time.deltaTime;

        player.transform.position += delta;
        cam.transform.position += delta;

        CheckBoundary();
    }

    private void CheckBoundary()
    {
        Vector3 pos = player.transform.position;

        if (border[1].x < pos.x + halfSightX)
        { border[0] += Vector2.right * unitSize; border[1] += Vector2.right * unitSize; }
        if (border[0].x > pos.x - halfSightX)
        { border[0] -= Vector2.right * unitSize; border[1] -= Vector2.right * unitSize; }

        if (border[0].y < pos.y + halfSightY)
        { border[0] += Vector2.up * unitSize; border[1] += Vector2.up * unitSize; }
        if (border[1].y > pos.y - halfSightY)
        { border[0] -= Vector2.up * unitSize; border[1] -= Vector2.up * unitSize; }

        ShiftTiles();
    }

    private void ShiftTiles()
    {
        Vector3 p = player.transform.position;

        for (int i = 0; i < tiles.Length; i++)
        {
            Vector3 t = tiles[i].transform.position;

            // 가로: 3열 → halfGridX = 1.5*unitSize, 이동량 = 3*unitSize
            if (t.x < p.x - halfGridX) tiles[i].transform.position += Vector3.right * moveSpanX;
            if (t.x > p.x + halfGridX) tiles[i].transform.position -= Vector3.right * moveSpanX;

            // 세로: 4행 → halfGridY = 2.0*unitSize, 이동량 = 4*unitSize
            if (t.y < p.y - halfGridY) tiles[i].transform.position += Vector3.up * moveSpanY;
            if (t.y > p.y + halfGridY) tiles[i].transform.position -= Vector3.up * moveSpanY;
        }
    }
}
