using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class FadeAlphaGraphicUI 
{

    public static void CrossFadeAlphaFixed(Image graphic, float alpha, float duration, Action callback)
    {
        //try
        //{
        //  MonoBehaviour monoBehaviour = graphic.GetComponent<MonoBehaviour>();

        CrossFadeAlphaFixed_Coroutine(graphic, alpha, duration, callback);
        //  }
        //catch
        //{
        //    Debug.LogWarning("CrossFade funcionality has not finished.....");
        //}
        //  monoBehaviour.StartCoroutine(CrossFadeAlphaFixed_Coroutine(graphic, alpha, duration, callback));
    }

    //public static  void CrossFadeAlphaFixed_Coroutine(Image img, float alpha, float duration, Action callback)
    public static async void CrossFadeAlphaFixed_Coroutine(Image img, float alpha, float duration, Action callback)
    {
        if (img == null)
            return;

        Color currentColor = img.color;
        Color visibleColor = img.color;
        visibleColor.a = alpha;
        float counter = 0;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            img.color = Color.Lerp(currentColor, visibleColor, counter / duration);
            await Task.Delay(1);
         //   yield return null;
        }

        if (callback != null) callback();

    }

    // How Use, e.g
    // Image.CrossFadeAlphaFixed(1f, 1f. false, () => print("Finished"));
}