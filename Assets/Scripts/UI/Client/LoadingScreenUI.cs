using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ubv.ui.client
{
    public class LoadingScreenUI : LoadingScreen
    {
        [SerializeField] private TextMeshProUGUI m_percentageText;
        [SerializeField] private Slider m_progressBar;
        [SerializeField] private CanvasGroup m_canvasGroup;

        private float m_currentPercentage;

        private void Awake()
        {
            OnPercentageChanged += (float value) => 
            {
                m_currentPercentage = value;
            };
        }

        private void Update()
        {
            SetLoadPercentage(m_currentPercentage);
        }

        private void SetLoadPercentage(float percentage)
        {
            m_percentageText.text = (100 * percentage).ToString("00") + "%";
            m_progressBar.value = percentage;
        }

        public void FadeLoadingScreen(float targetValue, float duration)
        {
            StartCoroutine(FadeLoadingScreenCoroutine(targetValue, duration));
        }

        public void FadeIn(float duration)
        {
            gameObject.SetActive(true);
            StartCoroutine(FadeInCoroutine(duration));
        }

        private IEnumerator FadeInCoroutine(float duration)
        {
            yield return StartCoroutine(FadeLoadingScreenCoroutine(100, duration));
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
}
