using System;
using UnityEngine;

namespace Game
{
    public class GameManager: MonoBehaviour
    {
        private DateTime _gameEndTime = DateTime.Parse("2023-01-02 08:00:00"); // 8am the next day
        private DateTime _currentTime;
        
        private void Start()
        {
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
    }
}