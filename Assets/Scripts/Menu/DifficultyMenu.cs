using System.Collections.Generic;
using Game;
using TMPro;
using UnityEngine;
using Toggle = UnityEngine.UI.Toggle;

namespace Menu
{
    public class DifficultyMenu: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI KeysLabel; 
        
        private readonly Dictionary<string, GameSettings> _presets = new Dictionary<string, GameSettings>
        {
            { "Hard", new GameSettings
                {
                    EnemySettings = new EnemySpawnSettings
                    {
                        MainBossAttack = 20,
                        MainBossHealth = 1000,
                        MainBossRegenRate = 5,
                        MiniBossAttack = 17,
                        MiniBossHealth = 100,
                        MiniBossRegenRate = 5
                    },
                    KeysRequired = 7,
                    PlayerAttackMultiplier = 1
                }
            },
            { "Normal", new GameSettings
                {
                    EnemySettings = new EnemySpawnSettings
                    {
                        MainBossAttack = 15,
                        MainBossHealth = 500,
                        MainBossRegenRate = 3,
                        MiniBossAttack = 13,
                        MiniBossHealth = 100,
                        MiniBossRegenRate = 3
                    },
                    KeysRequired = 5,
                    PlayerAttackMultiplier = 1
                }
            },
            { "Easy", new GameSettings
                {
                    EnemySettings = new EnemySpawnSettings
                    {
                        MainBossAttack = 10,
                        MainBossHealth = 100,
                        MainBossRegenRate = 3,
                        MiniBossAttack = 5,
                        MiniBossHealth = 50,
                        MiniBossRegenRate = 0
                    },
                    KeysRequired = 3,
                    PlayerAttackMultiplier = 1.5f
                }
            }
        };


        public void OnToggle(Toggle toggle)
        {
            if (!toggle.isOn)
                return;
            
            switch (toggle.name)
            {
                case "EasyToggle":
                {
                    var preset = _presets["Easy"];
                    CurrentGameSettings.Settings = preset;
                    KeysLabel.text = $"You will have to find {preset.KeysRequired} Keys to escape.";
                }
                    break;
                case "NormalToggle":
                {
                    var preset = _presets["Normal"];
                    CurrentGameSettings.Settings = preset;
                    KeysLabel.text = $"You will have to find {preset.KeysRequired} Keys to escape.";
                } 
                    break;
                case "HardToggle":
                {
                    var preset = _presets["Hard"];
                    CurrentGameSettings.Settings = preset;
                    KeysLabel.text = $"You will have to find {preset.KeysRequired} Keys to escape.";
                } 
                    break;
                default:
                    var fallback = _presets["Normal"];
                    CurrentGameSettings.Settings = fallback;
                    KeysLabel.text = $"You will have to find {fallback.KeysRequired} Keys to escape.";
                    break;
            }
        }
    }
}