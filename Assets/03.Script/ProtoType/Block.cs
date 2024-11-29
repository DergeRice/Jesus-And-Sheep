using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;

public class Block : MonoBehaviour
{
    private int _count = 300; // ���� �����͸� ������ �ʵ�

    public int Count
    {
        get => _count; // �� ��ȯ
        set
        {
            _count = value; // �� ����
            if(countText != null) countText.text = _count.ToString();
            if (_count < 0) // ���� �˻�
            {
                _count = 0;
                DestroyAnimation();
            }
        }
    }

    public BlockType blockType;
    
    public TMP_Text countText;
    internal Vector3 targetPosition;

    public Action<Vector3> ballCollsionEffect;
    public Action allBlockBrokenCheck;

    public bool isDisappear = false;

    public int curX, curY;

    private void Start()
    {
         if(countText != null) countText.text = Count.ToString();

        var tempScale = transform.localScale;
        transform.localScale = new Vector3(0.1f,0.1f,0.1f);
        transform.DOScale(tempScale, 0.2f);
    }

    public void Init(int _count)
    {
        Count = _count;
        if (countText != null)  countText.text = Count.ToString();

        if(GameLogicManager.instance.currentLevel > 30 && Random.value < 0.1f)
        {
            blockType = BlockType.Double;
        }else

        if (GameLogicManager.instance.currentLevel > 60 && Random.value < 0.1f)
        {
            blockType = BlockType.Giant;
        }
        else

        if (GameLogicManager.instance.currentLevel > 90 && Random.value < 0.1f)
        {
            blockType = BlockType.Split;
        }
        else
        if (GameLogicManager.instance.currentLevel > 120 && Random.value < 0.1f)
        {
            blockType = BlockType.BottomIgnore;
        }

        TypeInit();

    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            //SoundManager.VibrateGame(EVibrate.weak);
            transform.DOShakeScale(0.2f,0.2f);
            ballCollsionEffect.Invoke(collision.contacts[0].point);
            Count--;
            if (countText != null) countText.text = Count.ToString();
            if (Count <= 0) 
            {
                allBlockBrokenCheck.Invoke();
                DestroyAnimation();
            }
        }
    }

    public void TypeInit()
    {
        switch (blockType)
        {
            case BlockType.Common:
                break;
            case BlockType.Giant:
                break;
            case BlockType.Split:
                break;
            case BlockType.Double:
                Count *= 2;
                break;
            case BlockType.BottomIgnore:
                break;
            default:
                break;
        }
    }

    public void DestroyAnimation()
    {
        if(isDisappear == false)
        {
            GameLogicManager.instance.blockManager.RemoveBlock(this);
            GetComponent<Collider2D>().enabled = false;
            transform.DOShakeScale(0.5f);
            Destroy(gameObject,0.5f);
            isDisappear = true;
        }
    }
}
