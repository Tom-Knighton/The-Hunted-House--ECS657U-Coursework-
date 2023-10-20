using System;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy_AI.States
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyStateManager : MonoBehaviour
    {
        /// <summary>
        /// The current state of the AI's behaviour
        /// </summary>
        public EnemyAIState CurrentState;

        /// <summary>
        /// Shared data accessible by each state
        /// </summary>
        public EnemyStateContextData Data = new();

        [NonSerialized]
        public NavMeshAgent NavMeshAgent;

        private EnemyAIState _patrollingState = new EnemyPatrolState();
        private EnemyAIState _idlingState = new EnemyIdleState();
        private EnemyAIState _searchingState = new EnemySearchState();
        private EnemyAIState _chasingState = new EnemyChasingState();
        
        private void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();
        }
        
        private void Update()
        {
            // Runs the current state's Update()/tick method every Update
            CurrentState?.OnStateTick(this);
        }

        /// <summary>
        /// Leaves the current state and enters a new one
        /// </summary>
        /// <param name="state">The EEnemyAIState, manager decides which state implementation this belongs to</param>
        public void SwitchState(EEnemyAIState state)
        {
            CurrentState?.OnLeaveState(this);
            CurrentState = state switch
            {
                EEnemyAIState.Patrolling => _patrollingState,
                EEnemyAIState.Idle => _idlingState,
                EEnemyAIState.Searching => _searchingState,
                EEnemyAIState.Chasing => _chasingState,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Invalid Enemy AI state")
            };
            CurrentState.OnEnterState(this);
        }

        public void EnterIdleState(float waitFor)
        {
            CurrentState.OnLeaveState(this);
            CurrentState = _idlingState;
            Data.IdleTimer = waitFor;
            CurrentState.OnEnterState(this);
        }
    }
}