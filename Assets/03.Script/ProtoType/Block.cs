using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Block : MonoBehaviour
{
    public int count = 300;

    public BlockType blockType;
    
    public TMP_Text countText;
    internal Vector3 targetPosition;

    private void Start()
    {
         if(countText != null) countText.text = count.ToString();
    }

    public void Init(int _count)
    {
        count = _count;
        if (countText != null)  countText.text = count.ToString();

        if(GameLogicManager.instance.currentLevel < 30)
        {
            //blockType =
        }else

        if (GameLogicManager.instance.currentLevel < 60)
        {

        }else

        if (GameLogicManager.instance.currentLevel < 90)
        {

        }
        else
        if (GameLogicManager.instance.currentLevel < 120)
        {

        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            count--;
            if (countText != null) countText.text = count.ToString();
            if (count <= 0) Destroy(gameObject);
        }
    }
}
