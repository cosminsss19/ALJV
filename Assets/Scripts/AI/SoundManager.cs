using UnityEngine;

namespace AI
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;

        public static SoundManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    var go = new GameObject("[SoundManager]");
                    _instance = go.AddComponent<SoundManager>();
                }
                return _instance;
            }
        }

        [Header("Detection Sounds")]
        public AudioClip SpottedClip;
        public AudioClip HeardClip;

        [Header("Volume")]
        [Range(0f, 1f)] public float Volume = 1f;

        private AudioSource _audio;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;

            _audio = GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = false;
            _audio.spatialBlend = 0f;
        }

        public void PlaySpotted()
        {
            PlayOnce(SpottedClip);
        }

        public void PlayHeard()
        {
            PlayOnce(HeardClip);
        }

        private void PlayOnce(AudioClip clip)
        {
            if (clip == null) return;
            if (_audio == null) return;
            if (_audio.isPlaying) return;
            _audio.clip = clip;
            _audio.volume = Volume;
            _audio.Play();
        }
    }
}
