using UnityEngine;

namespace KidQuiz.Presentation
{
    // Owns all audio playback and the mute preference. Muted audio doesn't just
    // play silently - sources check IsMuted before starting playback at all, and
    // AudioListener.volume is set to 0 as a hard backstop.
    public sealed class AudioManager : MonoBehaviour
    {
        private const string MutedPrefKey = "KidQuiz.SoundMuted";

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip correctSfx;
        [SerializeField] private AudioClip wrongSfx;

        public bool IsMuted { get; private set; }

        private void Awake()
        {
            IsMuted = PlayerPrefs.GetInt(MutedPrefKey, 0) == 1;
            ApplyMuteState();
        }

        public void ToggleMuted()
        {
            IsMuted = !IsMuted;
            PlayerPrefs.SetInt(MutedPrefKey, IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState();
        }

        public void PlayMusic()
        {
            if (musicSource == null || backgroundMusic == null || IsMuted)
            {
                return;
            }

            if (musicSource.clip != backgroundMusic)
            {
                musicSource.clip = backgroundMusic;
            }
            musicSource.loop = true;

            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void PlayCorrect()
        {
            PlayOneShot(correctSfx);
        }

        public void PlayWrong()
        {
            PlayOneShot(wrongSfx);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (sfxSource == null || clip == null || IsMuted)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        private void ApplyMuteState()
        {
            AudioListener.volume = IsMuted ? 0f : 1f;

            if (IsMuted)
            {
                StopMusic();
            }
        }
    }
}
