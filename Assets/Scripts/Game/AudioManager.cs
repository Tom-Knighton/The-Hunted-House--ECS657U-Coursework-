using UnityEngine;

namespace Game
{
    public class AudioManager: MonoBehaviour
    {
        public AudioSource source;
        private AudioSource _rainSource;
        
        public AudioClip phoneCall;

        public AudioClip rainOutside;
        public AudioClip rainInside;
        
        [Header("Footstep Audio")]
        [SerializeField] public AudioClip[] woodClips = default;
        [SerializeField] public AudioClip[] concreteClips = default;
        [SerializeField] public AudioClip[] grassClips = default;
        
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
                source = GetComponent<AudioSource>();

                _rainSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                PlayInsideRain();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);// Destroy duplicate
                return;
            }
        }
        
        public void PlayPhoneCall()
        {
            source.clip = phoneCall;
            source.Play();
        }

        /// <summary>
        /// Plays the outside rain sound as a 2d sound
        /// </summary>
        public void PlayOutsideRain()
        {
            _rainSource.loop = true;
            _rainSource.clip = rainOutside;
            _rainSource.Play();
        }
        
        /// <summary>
        /// Plays the inside/muffled rain sound as a 2d sound
        /// </summary>
        public void PlayInsideRain()
        {
            _rainSource.loop = true;
            _rainSource.clip = rainInside;
            _rainSource.Play();
        }
    }
}