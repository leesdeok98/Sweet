using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private Image _image;

    void Start()
    {
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// 유니티가 '클릭'을 감지할 때마다 이 함수를 호출합니다.
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 1. 스프라이트가 없으면 일단 통과 (혹시 모를 에러 방지)
        if (_image.sprite == null)
        {
            return true;
        }

        // 2. RectTransform 기준으로 로컬 좌표 계산
        RectTransform rectTransform = (RectTransform)transform;
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localPosition);

        // 3. 0~1 사이의 텍스처 좌표로 변환 (UV 좌표)
        float x = (localPosition.x - rectTransform.rect.x) / rectTransform.rect.width;
        float y = (localPosition.y - rectTransform.rect.y) / rectTransform.rect.height;

        // 4. (중요) 스프라이트의 실제 '텍스처'에서 픽셀 색상 읽기
        try
        {
            Color color = _image.sprite.texture.GetPixelBilinear(x, y);

            // ★★★ 여기가 수정된 부분입니다! ★★★
            // (1% 컷오프: 아주 희미한 테두리도 클릭으로 인정)
            return color.a > 0.01f; 
            // ★★★
        }
        catch (UnityException)
        {
            // (Read/Write Enabled가 꺼져있을 때 여기로 빠짐)
            Debug.LogWarning("AlphaRaycastFilter: 이미지 " + _image.sprite.name + "의 Read/Write Enabled 옵션을 켜주세요.");
            return true; // (오류 시 일단 클릭 허용)
        }
    }
}