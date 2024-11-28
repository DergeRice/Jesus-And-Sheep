using UnityEngine;

public class BoxItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            GameLogicManager.instance.turnEndAction += () =>
            {
                GameLogicManager.instance.isPlayerTurn = false;
                GameLogicManager.instance.eventManager.ShowTwoSelection();
            };
            GameLogicManager.instance.blockManager.RemoveBlock(GetComponent<Block>());
            Destroy(gameObject);
        }
    }
}
