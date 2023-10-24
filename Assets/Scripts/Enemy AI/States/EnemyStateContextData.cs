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

        #region Attacking Data

        public float attackDamage { get; set; }
        public float attackCooldown { get; set; }
        public float attackRange { get; set; }
        public LayerMask attackLayerMask { get; set; }

        #endregion

    }
}