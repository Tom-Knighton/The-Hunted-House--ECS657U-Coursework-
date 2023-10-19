using System.Collections.Generic;

namespace Enemy_AI.States
{
    public class EnemyStateContextData
    {
        public EEnemyAIState NextState { get; set; } = EEnemyAIState.Idle;
        
        public IEnumerable<PatrolPoint> PatrolPoints { get; set; } = new List<PatrolPoint>();
        public int PatrolPointIndex { get; set; }
        public bool PatrolWasInterrupted { get; set; }
        
        public float IdleTimer { get; set; } = 0f;
    }
}