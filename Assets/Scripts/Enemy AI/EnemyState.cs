using System;
using UnityEngine;

namespace Enemy_AI
{
    /// <summary>
    /// The state the enemy is currently operating. I.e. 'patrolling' 'attacking' 'hiding' etc...
    /// </summary>
    
    [Serializable]
    [SerializeField]
    public enum EEnemyAIState
    {
        Patrolling,
        Idle,
        Searching,
        Chasing
    }
}