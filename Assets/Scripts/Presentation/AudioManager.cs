using UnityEngine;

namespace KidQuiz.Presentation
{
    // Owns all audio playback and the mute state. Muted audio doesn't just
    // play silently - one-shot sources check IsMuted before starting playback at
    // all, and AudioListener.volume is set to 0 as a hard backstop. The music
    // source itself keeps playing while muted; only its audible volume changes,
    // so unmuting never restarts the loop. Mute state is in-memory only and
    // always starts unmuted - it is not persisted between sessions.
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip correctSfx;
        [SerializeField] private AudioClip wrongSfx;
        [SerializeField] private AudioClip buttonPress;

        public bool IsMuted { get; private set; }

        private void Awake()
        {
            IsMuted = true;
            ApplyMuteState();
        }

        public void ToggleMuted()
        {
            IsMuted = !IsMuted;
            ApplyMuteState();
        }

        public void PlayMusic()
        {
            if (musicSource == null || backgroundMusic == null)
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

        public void PlayCorrect()
        {
            PlayOneShot(correctSfx);
        }

        public void PlayWrong()
        {
            PlayOneShot(wrongSfx);
        }

        public void PlayButton()
        {
            PlayOneShot(buttonPress);
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
        }
    }
}
