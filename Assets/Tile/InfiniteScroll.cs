// InfiniteScroll.cs
using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject[] tiles; // rows*cols 개수만큼 넣기

    [Header("설정값")]
    [SerializeField] private float unitSize = 10f;

    [Header("그리드 크기")]
    [SerializeField] private int cols = 3; // 가로 타일 수
    [SerializeField] private int rows = 4; // 세로 타일 수

    private float halfSightX, halfSightY;
    private Vector2[] border;
    private Camera cam;

    // 계산용(그리드 절반/이동량)
    private float halfGridX, halfGridY, moveSpanX, moveSpanY;

    private void Start()
    {
        cam = Camera.main;
        if (!cam)
        {
            Debug.LogError("[InfiniteScroll] Camera.main이 없습니다.");
            enabled = false;
            return;
        }

        // 🔹 player가 비어있으면 자동으로 씬에서 Player 찾기
        if (!player)
        {
            var p = FindObjectOfType<Player>();
            if (p != null)
            {
                player = p.gameObject;
            }
            else
            {
                Debug.LogError("[InfiniteScroll] player 참조가 비었고, 씬에서 Player도 못 찾았습니다.");
                enabled = false;
                return;
            }
        }

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("[InfiniteScroll] tiles 배열이 비었습니다.");
            enabled = false;
            return;
        }

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
        float x0 = -(cols - 1) * 0.5f * unitSize;
        float y0 = (rows - 1) * 0.5f * unitSize;

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (index >= tiles.Length) break;
                var t = tiles[index];
                if (!t)
                {
                    index++;
                    continue; // 파괴/누락 슬롯 무시
                }

                t.transform.position = new Vector3(
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
        // 🔹 런타임 도중에 player가 없어졌다면 다시 한 번 찾아보기
        if (!player)
        {
            var p = FindObjectOfType<Player>();
            if (p != null)
                player = p.gameObject;
        }

        if (!player || tiles == null || tiles.Length == 0) return;

        CheckBoundary();
    }

    private void CheckBoundary()
    {
        if (!player) return;

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
        Vector3 p = player ? player.transform.position : Vector3.zero;

        for (int i = 0; i < tiles.Length; i++)
        {
            var tile = tiles[i];
            if (!tile) continue; // 파괴/비활성/누락 슬롯 스킵

            Vector3 tpos = tile.transform.position;

            // 가로
            if (tpos.x < p.x - halfGridX)
                tile.transform.position = new Vector3(tpos.x + moveSpanX, tpos.y, tpos.z);
            else if (tpos.x > p.x + halfGridX)
                tile.transform.position = new Vector3(tpos.x - moveSpanX, tpos.y, tpos.z);

            // 세로
            tpos = tile.transform.position; // 가로 이동 반영 후 최신 좌표
            if (tpos.y < p.y - halfGridY)
                tile.transform.position = new Vector3(tpos.x, tpos.y + moveSpanY, tpos.z);
            else if (tpos.y > p.y + halfGridY)
                tile.transform.position = new Vector3(tpos.x, tpos.y - moveSpanY, tpos.z);
        }
    }

    // (선택) 에디터에서 null 슬롯이 있으면 눈에 띄게 경고
    private void OnValidate()
    {
        if (tiles == null) return;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                Debug.unityLogger.LogWarning(
                    "[InfiniteScroll]",
                    $"tiles[{i}] 가 비어있습니다(또는 파괴됨)."
                );
            }
        }
    }
}
