using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenCapture : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [SerializeField] private GameObject diePanel;        // 화면 전체 검은 사망 패널
    [SerializeField] private RawImage screenshotImage;   // 사망창 안의 흰 네모 RawImage
    [SerializeField] private Transform player;           // 플레이어 트랜스폼

    [Header("캡쳐 범위 (플레이어 주변, 픽셀 단위)")]
    [Tooltip("플레이어 주변을 몇 x 몇 픽셀 크기로 잘라낼지")]
    [SerializeField] private int captureWidth = 512;
    [SerializeField] private int captureHeight = 512;

    private Texture2D capturedTex;

    // Player.Die()에서 호출
    public void ShowDeathScreen()
    {
        StartCoroutine(CaptureAndShow());
    }

    private IEnumerator CaptureAndShow()
    {
        // 패널이 찍히지 않게 잠깐 끔
        if (diePanel != null)
            diePanel.SetActive(false);

        // 한 프레임 렌더 끝날 때까지 대기
        yield return new WaitForEndOfFrame();

        if (player == null)
        {
            Debug.LogWarning("[DeathScreenCapture] player가 비어 있습니다.");
            yield break;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[DeathScreenCapture] Camera.main이 없습니다.");
            yield break;
        }

        int screenW = Screen.width;
        int screenH = Screen.height;

        // 원하는 캡쳐 크기 (스크린보다 클 수 없게 한 번 더 제한)
        int regionW = Mathf.Min(captureWidth, screenW);
        int regionH = Mathf.Min(captureHeight, screenH);

        // 플레이어 위치를 스크린 좌표로 변환
        Vector3 screenPos = cam.WorldToScreenPoint(player.position);

        // 플레이어를 중심으로 한 사각형
        float x = screenPos.x - regionW / 2f;
        float y = screenPos.y - regionH / 2f;

        // 화면 밖으로 튀어나가지 않게 클램프
        if (x < 0) x = 0;
        if (y < 0) y = 0;
        if (x + regionW > screenW) x = screenW - regionW;
        if (y + regionH > screenH) y = screenH - regionH;

        Rect captureRect = new Rect(x, y, regionW, regionH);

        // 텍스처 생성/재사용
        if (capturedTex == null || capturedTex.width != regionW || capturedTex.height != regionH)
        {
            if (capturedTex != null)
                Destroy(capturedTex);

            capturedTex = new Texture2D(regionW, regionH, TextureFormat.RGB24, false);
        }

        // 해당 영역만 픽셀 읽기
        capturedTex.ReadPixels(captureRect, 0, 0);
        capturedTex.Apply();

        // RawImage에 적용
        if (screenshotImage != null)
        {
            screenshotImage.texture = capturedTex;
            // 필요하면 아래를 켜서 텍스처 크기에 맞게 네모 크기를 자동 조정할 수도 있음
            // screenshotImage.SetNativeSize();
        }

        // 이제 사망 패널 보여주기
        if (diePanel != null)
            diePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (capturedTex != null)
            Destroy(capturedTex);
    }
}
