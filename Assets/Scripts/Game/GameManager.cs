using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    // Manages the overall game state, including the game timer and activation of player/enemies
    public class GameManager: MonoBehaviour
    {
        private DateTime _gameEndTime = DateTime.Parse("2023-01-02 06:00:00"); // 8am the next day
        private DateTime _currentTime;
        
        [SerializeField] public FirstPersonController player;
        [SerializeField] private List<EnemyAI> enemies;

        public static GameManager Instance; // Singleton instance

        [SerializeField] public bool debugMode = true; // Flag for debug mode

        private bool _introCompleted = false;
        public GameObject IntroBlockWall;

        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);// Destroy duplicate
                return;
            }
        }

        private void Start()
        {
            // Set up the game based on whether it's in debug mode
            if (debugMode)
            {
                EnablePlayers(); // Enable player and enemies
                UIManager.Instance.FadeScreenIn(0);
                UIManager.Instance.ShowPlayerUI(); // Show the player UI
                UIManager.Instance.SetCountdownVisibility(true);
                _introCompleted = true;
                EnableEnemies();
            }
            else
            {
                // Disable player and enemies for non-debug mode
                player.enabled = false;
                foreach (var enemy in enemies)
                {
                    enemy.gameObject.SetActive(false);
                }
                UIManager.Instance.ShowOpeningScrawl();  // Show the opening narrative
            }
            _currentTime = DateTime.Parse("2023-01-01 20:00:00");
        }

        private void Update()
        {
            if (!_introCompleted) return;
            
            UpdateTime();
        }

        // Updates the in-game time and UI
        private void UpdateTime()
        {
            // Advance the current time by a scaled amount of real time
            _currentTime = _currentTime.AddSeconds(Time.deltaTime * 60f);
            // Update the UI with the time left until the game ends
            UIManager.Instance.UpdateTimeLeft(_gameEndTime - _currentTime);

            // Check if the current time has reached or passed the game end time
            if (_currentTime >= _gameEndTime)
            {
                // End the game and show the victory screen
                UIManager.Instance.HidePlayerUI();
                UIManager.Instance.ShowVictoryScreen("The police arrived and arrested your captor!");
            }
        }

        // Enables the player and enemies
        public void EnablePlayers()
        {
            player.enabled = true; // Enable the player controller
            player.UpdateUIOnRespawn(); // Update the player's UI
        }

        // Disables the player
        public void DisablePlayer()
        {
            player.enabled = false;
        }

        // Enables each enemy
        public void EnableEnemies()
        {
            IntroBlockWall.SetActive(false);
            UIManager.Instance.SetCountdownVisibility(true);
            _introCompleted = true;
            foreach (var enemy in enemies)
            {
                enemy.gameObject.SetActive(true); // Activate each enemy
            }
        }

        /// <summary>
        /// Notifies the game manager that the game has moved inside or outside
        /// </summary>
        public void ChangedTo(GameSceneType type)
        {
            switch (type)
            {
                case GameSceneType.Inside:
                    AudioManager.Instance.PlayInsideRain();
                    break;
                case GameSceneType.Outside:
                    AudioManager.Instance.PlayOutsideRain();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    public enum GameSceneType
    {
        Outside,
        Inside
    }
}