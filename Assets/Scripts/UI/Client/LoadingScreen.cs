using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_percentageText;
    [SerializeField] private CanvasGroup m_canvasGroup;

    public float LoadPercentage;

    private void Update()
    {
        SetLoadPercentageText(LoadPercentage);
    }
    
    private void SetLoadPercentageText(float percentage)
    {
        m_percentageText.text = (100 * percentage).ToString("00.00") + "%";
    }

    public void FadeLoadingScreen(float targetValue, float duration)
    {
        StartCoroutine(FadeLoadingScreenCoroutine(targetValue, duration));
    }

    public void FadeAway(float duration)
    {
        StartCoroutine(FadeAwayCoroutine(duration));
    }

    private IEnumerator FadeAwayCoroutine(float duration)
    {
        yield return StartCoroutine(FadeLoadingScreenCoroutine(0, duration));
        gameObject.SetActive(false);
    }

    private IEnumerator FadeLoadingScreenCoroutine(float targetValue, float duration)
    {
        float startValue = m_canvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            m_canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        m_canvasGroup.alpha = targetValue;
    }
}
