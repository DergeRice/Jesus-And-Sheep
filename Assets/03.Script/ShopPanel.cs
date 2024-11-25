using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Review;
#endif
public class ShopPanel : MonoBehaviour
{
    //public CardManager cardManager;
    #if UNITY_ANDROID
    private ReviewManager _reviewManager;

    
    // Start is called before the first frame update
    void Start()
    {
        _reviewManager = new ReviewManager();
    }
#endif
    // Update is called once per frame
    void Update()
    {
        
    }

    public void testAndroid()
    {
        // StoreReview.Open();
        #if UNITY_IOS
        if(UnityEngine.iOS.Device.RequestStoreReview() == true)
        {
            UnityEngine.iOS.Device.RequestStoreReview();
        }
        else
        {
            Application.OpenURL("https://apps.apple.com/kr/app/%EC%98%88%EC%88%98%EB%8B%98%EA%B3%BC-%EC%95%84%EC%9D%B4%EB%93%A4-%EC%88%98%EB%B0%95%EA%B2%8C%EC%9E%84/id6502446325"); 
        }
            Invoke(nameof(InvokeRemoveAds),3f);
        #elif UNITY_ANDROID
            // StoreReview.Open();
            Application.OpenURL($"https://play.google.com/store/apps/details?id={Application.identifier}"); 
            Invoke(nameof(InvokeRemoveAds),1.5f);
            // StartCoroutine(StartGooglePlayReview());
            // Debug.Log("cll");
        #endif

    }

    public void InvokeRemoveAds()
    {
        AdsInitializer.instance.RemoveAd(true);
    }


#if UNITY_ANDROID
    IEnumerator StartGooglePlayReview()
    {
        var reviewManager = new ReviewManager();
 
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
 
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("requestFlowOperation Error ::" + requestFlowOperation.Error.ToString());
            yield break;
        }
 
        var playReviewInfo = requestFlowOperation.GetResult();
 
        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        yield return launchFlowOperation;
 
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("launchFlowOperation Error ::" + launchFlowOperation.Error.ToString());
            yield break;
        }
 
    }
    #endif
}
