using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    // Manages the overall game state, including the game timer and activation of player/enemies
    public class GameManager: MonoBehaviour
    {
        [SerializeField] public FirstPersonController player; // A reference to the current player object
        [SerializeField] private List<EnemyAI> enemies; // A list of EnemyAI instances that *can* be spawned in
        [SerializeField] public bool debugMode = true; // Flag for debug mode
        public GameObject IntroBlockWall; // A reference to the intro blocking wall
        public static GameManager Instance; // Singleton instance

        private bool _introCompleted = false; // Whether or not the intro has been completed
        private DateTime _gameEndTime = DateTime.Parse("2023-01-02 06:00:00"); // 8am the next day
        private DateTime _currentTime;
        private int _enemiesKilled = 0; // The number of enemies killed in this round

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
            ChangedTo(GameSceneType.Inside);
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
                    RenderSettings.fogDensity = 0.025f;
                    break;
                case GameSceneType.Outside:
                    AudioManager.Instance.PlayOutsideRain();
                    RenderSettings.fogDensity = 0.12f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        // Called from EnemyAI when they die
        public void NotifyEnemyDied()
        {
            _enemiesKilled++;

            // If player has killed all the enemies, show the win screen
            if (_enemiesKilled >= enemies.Count)
            {
                // Hide the player's UI
                UIManager.Instance.HidePlayerUI();;

                // Unlock and show the cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Enable the victory screen
                UIManager.Instance.ShowVictoryScreen("You killed your captors and waited safely until the police arrived. You win!");

                // Disable the FirstPersonController to prevent player inputs
                GameManager.Instance.DisablePlayer();
            }
        }
    }

    public enum GameSceneType
    {
        Outside,
        Inside
    }
}