using UnityEngine;

public class PlusItem : MonoBehaviour
{
    public GameObject particle;
    bool isAlreadyCollision = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball") && isAlreadyCollision == false)
        {
            GameLogicManager.instance.ChangeBallCount(1);

            isAlreadyCollision = true;
            GameLogicManager.instance.blockManager.RemoveBlock(GetComponent<Block>());
            SoundManager.instance.sfxAudioSource.PlayOneShot(SoundManager.instance.plusItemSound);

            Instantiate(particle,transform.position,Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
