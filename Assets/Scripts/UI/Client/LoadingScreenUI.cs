using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ubv.ui.client
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_percentageText;
        [SerializeField] private CanvasGroup m_canvasGroup;

        [SerializeField]
        private LoadingScreen m_loadingScreen;

        private float m_currentPercentage;

        private void Awake()
        {
            m_loadingScreen.OnPercentageChanged += (float value) => 
            {
                m_currentPercentage = value;
            };
        }

        private void Update()
        {
            SetLoadPercentageText(m_currentPercentage);
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
}
