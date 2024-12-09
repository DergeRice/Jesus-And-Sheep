using UnityEngine;
using DG.Tweening;


public class IntroScreen : MonoBehaviour
{
    public CanvasGroup CG;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RunOnceOnStart()
    {
        // 게임 실행 시 최초로 실행할 코드
        // 인스턴스를 사용하려면, 이곳에서 컴포넌트를 가져와 호출해야 합니다.
        FindObjectOfType<IntroScreen>()?.PlayIntro();
    }

    // 실제로 인스턴스 메서드를 사용하여 화면 효과를 적용
    public void PlayIntro()
    {
        CG.gameObject.SetActive(true);
        CG.DOFade(0, 4f);
        Utils.DelayCall(() => { CG.gameObject.SetActive(false); }, 4f);
    }

}
