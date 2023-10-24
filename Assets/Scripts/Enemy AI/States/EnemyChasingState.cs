using System.Collections;
using UnityEngine;

namespace Enemy_AI.States
{
    public class EnemyChasingState : EnemyAIState
    {
        private Vector3? _cachedPosition;
        private bool _canAttack = true;
        
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

            if (!_canAttack)
            {
                return;
            }
            
            if (Physics.Raycast(context.transform.position, context.transform.forward, out var hit, context.Data.attackRange, context.Data.attackLayerMask))
            {
                // Check if the hit object has an "Enemy" component
                var enemy = hit.collider.GetComponent<Attackable>();
                if (enemy is not null)
                {
                    enemy.Attack(context.Data.attackDamage);
                    
                    // Start cooldown
                    context.StartCoroutine(AttackCooldown(context.Data.attackCooldown));
                }
                
            }
        }

        public override void OnLeaveState(EnemyStateManager context)
        {
            _cachedPosition = null;
            context.NavMeshAgent.ResetPath();
        }

        private IEnumerator AttackCooldown(float cooldown)
        {
            _canAttack = false;
            yield return new WaitForSeconds(cooldown);
            _canAttack = true;
        }
    }
}