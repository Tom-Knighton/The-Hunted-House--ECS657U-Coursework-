using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Game
{
    // Manages the overall game state, including the game timer and activation of player/enemies
    public class GameManager: MonoBehaviour
    {
        public GameSettings GameSettings => CurrentGameSettings.Settings; // The settings currently used in the game round
        [SerializeField] public FirstPersonController player; // A reference to the current player object
        [SerializeField] private List<EnemyAI> enemies; // A list of EnemyAI instances that *can* be spawned in
        [SerializeField] public bool debugMode = true; // Flag for debug mode
        public GameObject IntroBlockWall; // A reference to the intro blocking wall
        public GameObject KeyPrefab; // A reference to the prefab to spawn keys with
        public List<RoomData> KeyRoomData; // Places keys can be spawned
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
            SpawnKeys();
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
                enemy.StartEnemy(GameSettings.EnemySettings); // Activate each enemy
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

        /// <summary>
        /// Procedurally spawn the keys required by the difficulty setting.
        /// The keys will spawn first where they are required, then any extra keys are spawned in
        /// </summary>
        private void SpawnKeys()
        {
            var spawnedKeysAt = new List<Transform>();
            var keysToSpawn = GameSettings.KeysRequired;
            var roomsRequired = KeyRoomData.Where(r => r.MustHaveKey);

            if (roomsRequired.Count() > keysToSpawn)
            {
                Debug.LogError("The number of keys to spawn is less than required by the number of rooms, the keys for the min difficulty should be increased");
            }

            var random = new Random();
            // First, spawn a key in all the rooms that MUST have a key
            foreach (var room in roomsRequired)
            {

                var randomLocation = room.KeyPositions.ElementAtOrDefault(random.Next(0, room.KeyPositions.Count - 1));
                if (randomLocation is null)
                {
                    Debug.LogError("Cannot spawn a key, a room may be setup with no key positions");
                    continue;
                }

                Instantiate(KeyPrefab, randomLocation.position, randomLocation.rotation);
                spawnedKeysAt.Add(randomLocation);
            }
            
            // If there are any keys left to spawn, spawn them in randomly
            if (spawnedKeysAt.Count < keysToSpawn)
            {
                var allLocations = KeyRoomData.SelectMany(r => r.KeyPositions).ToList();
                for (var i = 0; i < keysToSpawn - spawnedKeysAt.Count; i++)
                {
                    var safeLocations = allLocations.Where(l => spawnedKeysAt.Contains(l) == false).ToList();
                    if (!safeLocations.Any())
                    {
                        Debug.LogError("Could not spawn required keys...");
                        GameSettings.KeysRequired = spawnedKeysAt.Count;
                        return;
                    }

                    var locToSpawn = safeLocations.ElementAtOrDefault(random.Next(0, safeLocations.Count - 1));
                    Instantiate(KeyPrefab, locToSpawn.position, locToSpawn.rotation);
                    spawnedKeysAt.Add(locToSpawn);
                }
            }
        }
    }

    public enum GameSceneType
    {
        Outside,
        Inside
    }
}