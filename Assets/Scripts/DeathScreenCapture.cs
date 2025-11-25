using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenCapture : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [SerializeField] private GameObject diePanel;        // 사망 패널
    [SerializeField] private RawImage screenshotImage;   // 사망 장면 캡쳐를 띄울 RawImage

    private Texture2D capturedTex;

    // Player.Die()에서 호출할 함수
    public void ShowDeathScreen()
    {
        Debug.Log("[DeathScreenCapture] ShowDeathScreen 호출됨");
        StartCoroutine(CaptureAndShow());
    }

    private IEnumerator CaptureAndShow()
    {
        Debug.Log("[DeathScreenCapture] 캡쳐 시작");
        // 켜져 있으면 끄기 (캡쳐에 패널이 찍히지 않게)
        if (diePanel != null)
            diePanel.SetActive(false);

        // 이번 프레임이 전부 그려질 때까지 기다림(카메라 렌더 끝난 시점)
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        // 텍스쳐가 없거나 해상도 바뀌면 새로 생성
        if (capturedTex == null || capturedTex.width != width || capturedTex.height != height)
        {
            if (capturedTex != null)
                Destroy(capturedTex);

            capturedTex = new Texture2D(width, height, TextureFormat.RGB24, false);
        }

        // 화면 전체 픽셀 읽기
        capturedTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        capturedTex.Apply();

        // RawImage에 넣기
        if (screenshotImage != null)
            screenshotImage.texture = capturedTex;

        // 이제 사망 패널 보여주기
        if (diePanel != null)
            diePanel.SetActive(true);
            
        if(screenshotImage == null)
           Debug.LogWarning("[DeathScreenCapture] screenshotImage가 비어있음");
    }

    private void OnDestroy()
    {
        if (capturedTex != null)
            Destroy(capturedTex);
    }
}
