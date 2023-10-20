using System.Collections.Generic;
using UnityEngine;

namespace Enemy_AI.States
{
    public class EnemyStateContextData
    {
        public EEnemyAIState NextState { get; set; } = EEnemyAIState.Idle;
        
        #region Patrolling Data
        public IEnumerable<PatrolPoint> PatrolPoints { get; set; } = new List<PatrolPoint>();
        public int PatrolPointIndex { get; set; }
        public bool PatrolWasInterrupted { get; set; }
        #endregion

        #region Idle Data
        public float IdleTimer { get; set; } = 0f;
        #endregion

        #region Searching Data
        public Vector3 SearchAroundPoint { get; set; }
        public int? SearchesLeft { get; set; }
        #endregion

        #region Chasing Data

        public Transform ChasingTarget { get; set; }

        #endregion

    }
}