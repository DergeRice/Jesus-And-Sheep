using UnityEngine;

public class PlusItem : MonoBehaviour
{
    bool isAlreadyCollision = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball") && isAlreadyCollision == false)
        {
            GameLogicManager.instance.ChangeBallCount(1);

            isAlreadyCollision = true;
            GameLogicManager.instance.blockManager.RemoveBlock(GetComponent<Block>());
            Destroy(gameObject);
        }
    }
}
