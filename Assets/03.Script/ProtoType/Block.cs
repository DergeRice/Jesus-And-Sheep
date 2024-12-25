using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Collections.Generic;

public class Block : MonoBehaviour
{
    private int _count = 300; // ���� �����͸� ������ �ʵ�

    public int Count
    {
        get => _count; // �� ��ȯ
        set
        {
            _count = value; // �� ����
            if (countText != null) countText.text = _count.ToString();
            if (_count <= 0) // ���� �˻�
            {
                _count = 0;
                countText.text = "";
                DestroyAnimation();
            }
        }
    }
    public int countMax;

    public BlockType blockType;

    public TMP_Text countText;
    public GameObject graphic;

    public Image fillHeart;

    public Action<Vector3> ballCollsionEffect;
    public Action allBlockBrokenCheck;

    public bool isDisappear = false;

    public int curX, curY;

    public SpriteRenderer spriteRenderer;

    public Sprite fat;
    public Sprite fatty;
    public List<Sprite> lovedSheep;

    private void Start()
    {
        if (countText != null) countText.text = Count.ToString();

        var tempScale = transform.localScale;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        transform.DOScale(tempScale, 0.2f);
        if (fillHeart != null) fillHeart.fillAmount = (countMax - Count) / (float)countMax;
    }

    void Update()
    {
        
    }

    public void Init(int _count)
    {
        Count = _count;
        countMax = _count;
        if (countText != null) countText.text = Count.ToString();

        if (GameLogicManager.instance.currentLevel > 30 && Random.value < 0.1f)
        {
            blockType = BlockType.Double;
            spriteRenderer.sprite = fat;
        }
        else

        if (GameLogicManager.instance.currentLevel > 100 && Random.value < 0.1f)
        {
            blockType = BlockType.Triple;
            spriteRenderer.sprite = fatty;
        }

        TypeInit();
        if (fillHeart != null)fillHeart.fillAmount = (countMax - Count) / (float)countMax;


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            //SoundManager.VibrateGame(EVibrate.weak);
            graphic.transform.DOKill();
            graphic.transform.localScale = Vector3.one;
            graphic.transform.DOShakeScale(0.2f, 0.2f);
            //Utils.DelayCall(() => { graphic.transform.localScale = Vector3.one; },0.2f);
            Count--;
            fillHeart.fillAmount = (countMax - Count) / (float)countMax;

            //Debug.Log(fillHeart.fillAmount);

            // SoundManager.instance.sfxAudioSource.PlayOneShot(SoundManager.instance.heartSound);
            SoundManager.instance.PlayBoingSound();
            if (countText != null) countText.text = Count.ToString();
            if (Count <= 0)
            {
                SoundManager.VibrateGame(EVibrate.weak);
                countText.text = "";
                allBlockBrokenCheck.Invoke();
                DestroyAnimation();
            }
        }
    }

    public void OnDrillCall()
    {
        //SoundManager.VibrateGame(EVibrate.weak);
        graphic.transform.DOKill();
        graphic.transform.localScale = Vector3.one;
        graphic.transform.DOShakeScale(0.2f, 0.2f);
        //Utils.DelayCall(() => { graphic.transform.localScale = Vector3.one; },0.2f);
        Count--;
        fillHeart.fillAmount = (countMax - Count) / (float)countMax;
        //Debug.Log(fillHeart.fillAmount);

        if (countText != null) countText.text = Count.ToString();
        if (Count <= 0)
        {
            countText.text = "";
            allBlockBrokenCheck.Invoke();
            DestroyAnimation();

        }
    }


    public void TypeInit(bool mulitifierInit = true)
    {
        switch (blockType)
        {
            case BlockType.Common:
                break;
            case BlockType.Double:
                spriteRenderer.sprite = fat;
                if(mulitifierInit == true) Count *= 2;
                break;
            case BlockType.Triple:
                spriteRenderer.sprite = fatty;
                if(mulitifierInit == true) Count *= 3;
                break;
            default:
                break;
        }
    }

    public void DestroyAnimation()
    {
        if (isDisappear == false)
        {
            spriteRenderer.sprite = lovedSheep[(int)blockType];
            SoundManager.instance.sfxAudioSource.PlayOneShot(SoundManager.instance.boingSound);
            GameLogicManager.instance.blockManager.RemoveBlock(this);
            GetComponent<Collider2D>().enabled = false;
            transform.DOShakeScale(0.5f);
            Destroy(gameObject, 0.5f);
            isDisappear = true;
        }
    }
}
