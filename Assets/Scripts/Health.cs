// Health.cs  (기존 PlayerUnderfootHP.cs였다면 파일명도 Health.cs로 맞추세요)
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Player player;      // 플레이어 컴포넌트
    [SerializeField] private Image fillImage;    // Fill 오브젝트의 Image (Image Type = Filled)

    [Header("Options")]
    [SerializeField] private bool smooth = true;     // 부드럽게 보간할지
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool hideWhenFull = false; // 풀피면 숨기기 (선택)

    float targetFill = 1f;
    float currentFill = 1f;

    void Reset()
    {
        if (player == null) player = GetComponentInParent<Player>();
        if (fillImage == null && transform.Find("Fill") != null)
            fillImage = transform.Find("Fill").GetComponent<Image>();
    }

    void Awake()
    {
        if (player == null) player = GetComponentInParent<Player>();
    }

    void Update()
    {
        if (player == null || fillImage == null) return;

        // 체력 비율만 계산해서 반영
        targetFill = Mathf.Approximately(player.maxHealth, 0f) ? 0f :
                     Mathf.Clamp01(player.health / player.maxHealth);

        currentFill = smooth
            ? Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed)
            : targetFill;

        fillImage.fillAmount = currentFill;

        if (hideWhenFull)
            gameObject.SetActive(currentFill < 0.999f || currentFill <= 0f);
    }

    // 즉시 반영하고 싶을 때 외부에서 호출 가능(예: 피해 직후)
    public void ForceRefresh()
    {
        if (player == null || fillImage == null) return;
        currentFill = targetFill = Mathf.Clamp01(player.health / player.maxHealth);
        fillImage.fillAmount = currentFill;
    }
}
