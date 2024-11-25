using System;
using System.Threading.Tasks;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static async void DelayCall(Action action, float delaySeconds)
    {
        // 지연 시간만큼 대기 (밀리초 단위)
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

        // 메인 스레드에서 액션 실행
        action?.Invoke();
    }
}
