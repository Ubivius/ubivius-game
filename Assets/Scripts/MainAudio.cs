using UnityEngine;
using System.Collections;

namespace ubv.client.audio
{
    public class MainAudio : MonoBehaviour
    {
        private static MainAudio m_instance = null;
        private AudioSource m_audioSource;

        private void Awake()
        {
            m_audioSource = GetComponent<AudioSource>();
            m_instance = this;
        }

        public static void PlayOnce(AudioClip audio, float volume = 1f)
        {
            m_instance.m_audioSource.PlayOneShot(audio, volume);
        }

        public static void SetMainTrack(AudioClip audio, float volume = 1f)
        {
            m_instance.m_audioSource.clip = audio;
            m_instance.m_audioSource.Play();
        }
    }
}