using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SpineOrderByY : MonoBehaviour
{
    [Header("Y���� ���� ����")]
    [Tooltip("�� ��ü�� ���� ���� (�⺻�� 0)")]
    public int baseOrder = 0;

    [Tooltip("Y���� SortingOrder�� ��ȯ�� ���� ������")]
    public int scale = 9;

    [Tooltip("������ Sorting Layer �̸� (����θ� ���� ���̾� ����)")]
    public string sortingLayerName = "";

    private Renderer myRenderer;

    void Awake()
    {
        myRenderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (!myRenderer) return;

        // y�� ��������(�Ʒ����ϼ���) ȭ�� �տ� ���Բ� ��ȣ�� -��
        int order = baseOrder - Mathf.RoundToInt(transform.position.y * scale);
        myRenderer.sortingOrder = order;

        // ���������� Ư�� Sorting Layer ����
        if (!string.IsNullOrEmpty(sortingLayerName))
            myRenderer.sortingLayerName = sortingLayerName;
    }
}
