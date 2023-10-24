using System;

namespace Enemy_AI.States
{
    [Serializable]
    public abstract class EnemyAIState
    {
        /// <summary>
        /// Called when state becomes active
        /// </summary>
        public abstract void OnEnterState(EnemyStateManager context);
        
        /// <summary>
        /// Runs on every update
        /// </summary>
        public abstract void OnStateTick(EnemyStateManager context);
        
        /// <summary>
        /// Called when the state leaves the active state
        /// </summary>
        public abstract void OnLeaveState(EnemyStateManager context);
    }
}