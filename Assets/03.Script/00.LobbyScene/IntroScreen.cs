using UnityEngine;
using DG.Tweening;


public class IntroScreen : MonoBehaviour
{
    public CanvasGroup CG;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RunOnceOnStart()
    {
        // ���� ���� �� ���ʷ� ������ �ڵ�
        // �ν��Ͻ��� ����Ϸ���, �̰����� ������Ʈ�� ������ ȣ���ؾ� �մϴ�.
        FindObjectOfType<IntroScreen>()?.PlayIntro();
    }

    // ������ �ν��Ͻ� �޼��带 ����Ͽ� ȭ�� ȿ���� ����
    public void PlayIntro()
    {
        CG.gameObject.SetActive(true);
        CG.DOFade(0, 4f);
        Utils.DelayCall(() => { CG.gameObject.SetActive(false); }, 4f);
    }

}
