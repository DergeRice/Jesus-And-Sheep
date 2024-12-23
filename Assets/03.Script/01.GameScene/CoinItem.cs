using UnityEngine;
using DG.Tweening;

public class CoinItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            GameLogicManager.instance.blockManager.RemoveBlock(GetComponent<Block>());

            var temp = FindFirstObjectByType<GameSceneSettingPanel>();
            temp.ShowGoldIndicator(true,false,10);

            
            SoundManager.instance.sfxAudioSource.PlayOneShot(SoundManager.instance.coinSound);
            Destroy(gameObject);
        }
    }
    
}
