using UnityEngine;
using DG.Tweening;

public class BoxItem : MonoBehaviour
{
    public GameObject dropBox;
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
            var temp = Instantiate(dropBox, transform.position, Quaternion.identity);
            GameLogicManager.instance.removeObjsAfterTurnEnd.Add(temp);
            temp.transform.DOMoveY(-4.5f,3f).SetEase(Ease.OutBounce);

            SoundManager.instance.sfxAudioSource.PlayOneShot(SoundManager.instance.chestItemSound);
            Destroy(gameObject);
        }
    }
    
}
