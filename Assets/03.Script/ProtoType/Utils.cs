using System;
using System.Threading.Tasks;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static async void DelayCall(Action action, float delaySeconds)
    {
        // ���� �ð���ŭ ��� (�и��� ����)
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

        // ���� �����忡�� �׼� ����
        action?.Invoke();
    }
}
