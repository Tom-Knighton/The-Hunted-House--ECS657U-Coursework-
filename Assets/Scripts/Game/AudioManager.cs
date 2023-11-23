using UnityEngine;

namespace Game
{
    public class AudioManager: MonoBehaviour
    {
        public AudioSource source;
        
        public AudioClip phoneCall;
        
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
                source = GetComponent<AudioSource>();
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

    }
}