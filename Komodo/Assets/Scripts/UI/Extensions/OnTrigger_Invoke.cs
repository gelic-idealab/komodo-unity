using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnTrigger_Invoke : MonoBehaviour
{
    public Image uiFadeImage;
    public float timeForFade = 1;
    public UnityEvent onCompletedFadeInEvent;
    public UnityEvent onCompletedFadeOutEvent;

    public static OnTrigger_Invoke firstInstance;
    public IEnumerator Start()
    {
        if (firstInstance == null)
        {
            firstInstance = this;

        }
        else yield break;

        FadeAlphaGraphicUI.CrossFadeAlphaFixed_Coroutine(uiFadeImage,1, 0.1f, null);
       // uiFadeImage.CrossFadeAlphaFixed(1, 0.1f, null);

        yield return new WaitUntil(() => GameStateManager.Instance.isAssetLoading_Finished);
        FadeAlphaGraphicUI.CrossFadeAlphaFixed_Coroutine(uiFadeImage, 0, 0.5f, null);
     //   uiFadeImage.CrossFadeAlphaFixed(0, 0.5f, null);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("MainCamera2"))
        {
            //uiFadeImage.CrossFadeAlphaFixed(1, timeForFade, () => { onCompletedFadeInEvent.Invoke(); OnCompleteFadeOut(); });
            FadeAlphaGraphicUI.CrossFadeAlphaFixed_Coroutine(uiFadeImage, 1, timeForFade, () => { onCompletedFadeInEvent.Invoke(); OnCompleteFadeOut(); });

        }
    }
    public void OnCompleteFadeOut()
    {
        FadeAlphaGraphicUI.CrossFadeAlphaFixed_Coroutine(uiFadeImage, 0, timeForFade, () => onCompletedFadeOutEvent.Invoke());
    //    uiFadeImage.CrossFadeAlphaFixed(0, timeForFade, () => onCompletedFadeOutEvent.Invoke());
    }

    
}
