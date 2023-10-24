using System.Collections;
using System.Linq;
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
            
            if (_canAttack)
            {
                var hit = Physics
                    .OverlapSphere(context.transform.position, context.Data.attackRange, context.Data.attackLayerMask)
                    .FirstOrDefault();
                if (hit is not null)
                {
                    // Check if the hit object has an "Enemy" component
                    var enemy = hit.GetComponent<Attackable>();
                    if (enemy is not null)
                    {
                        enemy.Attack(context.Data.attackDamage);
                    
                        // Start cooldown
                        context.StartCoroutine(AttackCooldown(context.Data.attackCooldown));
                    }
                }

                return;
            }
            
            // Every update, move towards the target's current position
            if (context.Data.ChasingTarget != null)
            {
                _cachedPosition = context.Data.ChasingTarget.position;
            }

            if (_cachedPosition is null)
                return;


            // Work around a NavMeshAgent issue which causes the agent to ignore stopping distance
            context.NavMeshAgent.SetDestination(_cachedPosition.Value);
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