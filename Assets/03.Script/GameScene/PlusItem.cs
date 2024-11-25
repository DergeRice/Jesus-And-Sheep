using UnityEngine;

public class PlusItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            GameLogicManager.instance.ballCount++;
            Destroy(gameObject);
        }
    }
}
