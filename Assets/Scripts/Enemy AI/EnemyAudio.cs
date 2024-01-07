using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy_AI
{
    [RequireComponent(typeof(AudioSource))]
    public class EnemyAudio : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _footstepTimer;
        private int[] _woodIndices;
        private int[] _concreteIndices;
        private int[] _grassIndices;
        private int _currentWoodFootstepIndex;
        private int _currentConcreteFootstepIndex;
        private int _currentGrassFootstepIndex;
        
        private void Start()
        {
            _audioSource = gameObject.GetComponent<AudioSource>();
            _woodIndices = GenerateRandomIndex(AudioManager.Instance.woodClips.Length);
            _concreteIndices = GenerateRandomIndex(AudioManager.Instance.concreteClips.Length);
            _grassIndices = GenerateRandomIndex(AudioManager.Instance.grassClips.Length);
        }

        private void Update()
        {
            HandleFootsteps();
        }

        // Play a grunt sound when attacking
        public void AttackSound()
        {
            _audioSource.PlayOneShot(AudioManager.Instance.GetRandom(AudioManager.Instance.EnemyAttackClips));
        }
        
        // Play a grunt when getting hit
        public void GetHitSound()
        {
            _audioSource.PlayOneShot(AudioManager.Instance.GetRandom(AudioManager.Instance.EnemyGetHitClips));
        }

        private void HandleFootsteps()
        {
            _footstepTimer -= Time.deltaTime;

            if (_footstepTimer > 0) return;
            
            // Raycast to determine surface type
            if (Physics.Raycast(gameObject.transform.position, Vector3.down, out var hit, 3))
            {
                // Adjust volume for crouch
                _audioSource.pitch = Random.Range(0.9f, 1.1f);

                // Play sound based on surface
                switch (hit.collider.tag)
                {
                    case "Footsteps/WOOD":
                        _audioSource.PlayOneShot(
                            AudioManager.Instance.woodClips[_woodIndices[_currentWoodFootstepIndex]]);
                        ShiftIndex(ref _currentWoodFootstepIndex, AudioManager.Instance.woodClips.Length,
                            ref _woodIndices);
                        break;
                    case "Footsteps/CONCRETE":
                        _audioSource.PlayOneShot(
                            AudioManager.Instance.concreteClips[_concreteIndices[_currentConcreteFootstepIndex]]);
                        ShiftIndex(ref _currentConcreteFootstepIndex, AudioManager.Instance.concreteClips.Length,
                            ref _concreteIndices);
                        break;
                    case "Footsteps/GRASS":
                        _audioSource.PlayOneShot(
                            AudioManager.Instance.grassClips[_grassIndices[_currentGrassFootstepIndex]]);
                        ShiftIndex(ref _currentGrassFootstepIndex, AudioManager.Instance.grassClips.Length,
                            ref _grassIndices);
                        break;
                }
            }

            // Reset footstep timer
            _footstepTimer = 0.5f;
        }
        
        // Generate randomized indices for audio clips
        private static int[] GenerateRandomIndex(int clipCount)
        {
            var availableIndices = new List<int>();
            // Populate list with clip indices
            for (var i = 0; i < clipCount; i++)
            {
                availableIndices.Add(i);
            }

            var randomizedIndices = new int[clipCount];
            // Shuffle indices
            for (var i = 0; i < clipCount; i++)
            {
                var randomIndex = Random.Range(0, availableIndices.Count);
                randomizedIndices[i] = availableIndices[randomIndex];
                availableIndices.RemoveAt(randomIndex);
            }

            return randomizedIndices;
        }
        
        // Increment and reset index if needed
        private static void ShiftIndex(ref int currentIndex, int clipLength, ref int[] indicesArray)
        {
            currentIndex++;

            // Reset if exceeds length
            if (currentIndex >= clipLength)
            {
                currentIndex = 0;
                indicesArray = GenerateRandomIndex(clipLength);
            }
        }
    }
}