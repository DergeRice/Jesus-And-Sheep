using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;

    public int zaebal;


    private void Start()
    {
        ��5��_�߰�_����3��_��ü��2��.explaination =
            "�� 5���� �߰��ϰ�, ���� 3�� ���� ������ ����� ü���� 2�谡 �˴ϴ�.";
        ��5��_�߰�_����3��_��ü��2��.action += () =>
        {
            GameLogicManager.instance.ballCount += 5;
            GameLogicManager.instance.blockManager.doubleSheepHpCount = 3;
        };

        ����_���پ��_���α���_��5������.explaination =
            "���� �տ� �ִ� ���� ����� ��� ��� �����մϴ�. �� 5�� ����";
        ����_���پ��_���α���_��5������.action += () =>
        {
            GameLogicManager.instance.ballCount += 5;
            //var blockList = GameLogicManager.instance.blockManager.GetBlocksInSameYLineWithMaxY() = 3;
        };
    }
}
