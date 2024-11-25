using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class Block : MonoBehaviour
{
    public int count = 300;

    public BlockType blockType;
    
    public TMP_Text countText;
    internal Vector3 targetPosition;

    public Action<Vector3> ballCollsionEffect;
    public Action allBlockBrokenCheck;

    private void Start()
    {
         if(countText != null) countText.text = count.ToString();
    }

    public void Init(int _count)
    {
        count = _count;
        if (countText != null)  countText.text = count.ToString();

        if(GameLogicManager.instance.currentLevel > 30 && Random.value > 0.1f)
        {
            blockType = BlockType.Double;
        }else

        if (GameLogicManager.instance.currentLevel > 60 && Random.value > 0.1f)
        {
            blockType = BlockType.Giant;
        }
        else

        if (GameLogicManager.instance.currentLevel > 90 && Random.value > 0.1f)
        {
            blockType = BlockType.Split;
        }
        else
        if (GameLogicManager.instance.currentLevel > 120 && Random.value > 0.1f)
        {
            blockType = BlockType.BottomIgnore;
        }

        TypeInit();

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            SoundManager.VibrateGame(EVibrate.weak);
            ballCollsionEffect.Invoke(collision.contacts[0].point);
            count--;
            if (countText != null) countText.text = count.ToString();
            if (count <= 0) 
            {
                allBlockBrokenCheck.Invoke();
                Destroy(gameObject);
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
                count *= 2;
                break;
            case BlockType.BottomIgnore:
                break;
            default:
                break;
        }
    }
}
