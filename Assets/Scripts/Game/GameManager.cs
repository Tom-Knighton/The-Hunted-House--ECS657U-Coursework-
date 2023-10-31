using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class GameManager: MonoBehaviour
    {
        private DateTime _gameEndTime = DateTime.Parse("2023-01-02 08:00:00"); // 8am the next day
        private DateTime _currentTime;

        [SerializeField] private FirstPersonController player;
        [SerializeField] private List<EnemyAI> enemies;

        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            player.enabled = false;
            foreach (var enemy in enemies)
            {
                enemy.gameObject.SetActive(false);
            }
            
            UIManager.Instance.ShowOpeningScrawl();

            _currentTime = DateTime.Parse("2023-01-01 18:00:00"); // Set time to 6pm so we can get roughly a day night cycle
        }

        private void Update()
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            _currentTime = _currentTime.AddMilliseconds(Time.deltaTime * 10000f);
            UIManager.Instance.UpdateTimeLeft(_gameEndTime - _currentTime);
            
            if (_currentTime >= _gameEndTime)
            {
                UIManager.Instance.HidePlayerUI();
                UIManager.Instance.ShowVictoryScreen("The police arrived and arrested your captor!");
            }
        }

        public void EnablePlayers()
        {
            player.enabled = true;
            foreach (var enemy in enemies)
            {
                enemy.gameObject.SetActive(true);
            }
        }
    }
}