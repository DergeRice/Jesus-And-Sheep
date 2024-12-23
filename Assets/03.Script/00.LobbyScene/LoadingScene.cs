using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Slider loadingSlider; // Slider를 Inspector에서 연결
    public List<string> stringList; // 랜덤으로 선택할 문자열 리스트
    public TMP_Text displayText;        // 텍스트를 표시할 UI Text 컴포넌트
    public float interval = 1.2f;   // 문자열 변경 주기

    private void Start()
    {
        if (stringList.Count > 0 && displayText != null)
        {
            StartCoroutine(ChangeStringCoroutine());
        }
        else
        {
            Debug.LogWarning("StringList가 비어있거나 Text 컴포넌트가 연결되지 않았습니다.");
        }

         StartCoroutine(LoadSceneCoroutine());
    }

    private IEnumerator ChangeStringCoroutine()
    {
        while (true)
        {
            // 리스트에서 랜덤 문자열 선택
            string randomString = stringList[Random.Range(0, stringList.Count)];
            // 선택된 문자열을 텍스트에 표시
            displayText.text = randomString;
            // 지정된 시간 대기
            yield return new WaitForSeconds(interval);
        }
    }


    private IEnumerator LoadSceneCoroutine()
    {
        float elapsedTime = 0f;
        float duration = 3f; // 90%까지 차는 데 걸리는 시간
        float delay = 3f;    // 90%에서 100%로 차는 대기 시간

        // Slider를 선형적으로 90%까지 채움
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            loadingSlider.value = Mathf.Lerp(0f, 0.9f, elapsedTime / duration);
            yield return null;
        }

        // 1초 대기 후 Slider를 100%로 채움
        yield return new WaitForSeconds(delay);
        loadingSlider.value = 1f;

        // 다음 씬으로 이동
        SceneManager.LoadScene("01.LobbySceneSheep"); // "NextSceneName"을 전환할 씬 이름으로 변경
    }
}
