using UnityEngine;
using UnityEngine.UI;

public class LoadingMove : MonoBehaviour
{
    public Slider loadingSlider;      // 연결된 Slider
    public RectTransform movingImage; // 움직일 이미지의 RectTransform
    public Vector2 startPosition;    // 이미지의 시작 위치
    public Vector2 endPosition;      // 이미지의 끝 위치

    void Start()
    {
        if (movingImage != null)
        {
            // 시작 위치를 이미지의 초기 위치로 설정
            startPosition = movingImage.anchoredPosition;
        }
    }

    void Update()
    {
        if (loadingSlider != null && movingImage != null)
        {
            // 슬라이더 값에 따라 이미지의 위치를 선형적으로 보간
            movingImage.anchoredPosition = Vector2.Lerp(startPosition, endPosition, loadingSlider.value);
        }
    }
}
