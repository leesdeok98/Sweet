using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class SpineOrderByY : MonoBehaviour
{
    [Header("Y값 기반 정렬 설정")]

    [Tooltip("기본 Sorting Order (전체를 위/아래로 밀어주는 오프셋)")]
    [SerializeField] private int baseOrder = 0;

    [Tooltip("Y값을 Sorting Order로 변환할 때 곱하는 값 (클수록 차이가 크게 남)")]
    [SerializeField] private int scale = 9;

    [Tooltip("비워두면 현재 Sorting Layer 유지, 입력하면 해당 Layer로 고정")]
    [SerializeField] private string sortingLayerName = "";

    private Renderer myRenderer;

    // 마지막으로 반영한 y 기반 order (불필요한 대입 방지용)
    private int lastAppliedOrder = int.MinValue;

    private void Awake()
    {
        myRenderer = GetComponent<Renderer>();

        // Sorting Layer는 매 프레임 바꿀 필요가 없어서 시작 시 1번만 적용
        if (!string.IsNullOrEmpty(sortingLayerName))
        {
            myRenderer.sortingLayerName = sortingLayerName;
        }
    }

    private void LateUpdate()
    {
        if (!myRenderer) return;

        // y가 작아질수록(아래로 갈수록) 더 앞으로 오게 하려면 order가 커지도록 만드는 게 핵심
        // 현재 수식: baseOrder - (y * scale)
        // y가 내려가면(음수 방향) -(-값) => +가 되어 order가 커져 앞에 옴
        int order = baseOrder - Mathf.RoundToInt(transform.position.y * scale);

        // 값이 바뀌었을 때만 반영 (성능/불필요한 세팅 방지)
        if (order != lastAppliedOrder)
        {
            myRenderer.sortingOrder = order;
            lastAppliedOrder = order;
        }
    }
}
