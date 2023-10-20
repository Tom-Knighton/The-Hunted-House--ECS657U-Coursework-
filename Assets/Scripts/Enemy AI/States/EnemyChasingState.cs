using UnityEngine;

namespace Enemy_AI.States
{
    public class EnemyChasingState : EnemyAIState
    {
        private Vector3? _cachedPosition;
        
        public override void OnEnterState(EnemyStateManager context)
        {
            _cachedPosition = context.Data.ChasingTarget.position;
        }

        public override void OnStateTick(EnemyStateManager context)
        {
            // Every update, move towards the target's current position
            if (context.Data.ChasingTarget != null)
            {
                _cachedPosition = context.Data.ChasingTarget.position;
            }

            if (_cachedPosition is null)
                return;
            
            context.NavMeshAgent.SetDestination(_cachedPosition.Value);
            
            // TODO: Eventually if we get close enough we should switch to attacking here...
        }

        public override void OnLeaveState(EnemyStateManager context)
        {
            _cachedPosition = null;
            context.NavMeshAgent.ResetPath();
        }
    }
}