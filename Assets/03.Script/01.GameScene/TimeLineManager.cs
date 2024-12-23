using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimeLineManager : MonoBehaviour
{
   public PlayableAsset success, fail;

   public void PlayTimeLine(bool success)
   {
        GetComponent<PlayableDirector>().playableAsset = success ? this.success : fail;
        GetComponent<PlayableDirector>().Play();
   }
}
