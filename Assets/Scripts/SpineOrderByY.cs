using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpineOrderByY : MonoBehaviour
{
    [Header("Y기준 정렬 설정")]
    [Tooltip("씬 전체의 기준 오더 (기본값 0)")]
    public int baseOrder = 0;

    [Tooltip("Y값을 SortingOrder로 변환할 때의 스케일")]
    public int scale = 9;

    [Tooltip("고정할 Sorting Layer 이름 (비워두면 현재 레이어 유지)")]
    public string sortingLayerName = "";

    private Renderer myRenderer;

    void Awake()
    {
        myRenderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (!myRenderer) return;

        // y가 낮을수록(아래쪽일수록) 화면 앞에 오게끔 부호를 -로
        int order = baseOrder - Mathf.RoundToInt(transform.position.y * scale);
        myRenderer.sortingOrder = order;

        // 선택적으로 특정 Sorting Layer 고정
        if (!string.IsNullOrEmpty(sortingLayerName))
            myRenderer.sortingLayerName = sortingLayerName;
    }
}
