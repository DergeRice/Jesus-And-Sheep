using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public Texture commonTexture;
    public GameEvent ��5��_�߰�_����3��_��ü��2��;
    public GameEvent ����_���پ��_���α���_��5������;
    public GameEvent ��_���ݸ�_�����_����_����;
    public GameEvent ȭ�鿡�ִ�_�����_ü��2��_��3���߰�;
    public GameEvent ����_10��_���_ü��_2��_��5���߰�;
    public GameEvent ���Ͻ���_��5���߰�;
    public GameEvent ���Ͻ���_��10���߰�;
    public GameEvent ���Ͻ���_��15���߰�;
    public GameEvent ����_�Ѱ�_���_ü��_3��_��3���߰�;
    public GameEvent ü��30�̸�_���_��ñ���_��1������;
    public GameEvent ü��100�̸�_���_��ñ���_��10������;
    public GameEvent ���ֵ鿡��50������_��5������;
    public GameEvent ���ֵ鿡��10������_��1������;
    public GameEvent ����3�����_100������_��3������;
    public GameEvent �������κ��5����ġ_��5���߰�;
    public GameEvent �������κ��10����ġ_����Ư����1���߰�;
    public GameEvent �ݹ�Ȯ����_��5�߰�_��2����;
    public GameEvent �ݹ�Ȯ����_��5�߰�_���3����ȯ;
    public GameEvent �ݹ�Ȯ����_Ư����1�߰�_��2����;
    public GameEvent �ݹ�Ȯ����_Ư����1�߰�_���5����ȯ;
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
