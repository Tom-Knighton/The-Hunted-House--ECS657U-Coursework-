using System.Collections;
using System.Linq;
using UnityEngine;

namespace Enemy_AI.States
{
    // State for when an enemy is chasing a target
    public class EnemyChasingState : EnemyAIState
    {
        private Vector3? _cachedPosition;
        private bool _canAttack = true;

        // Called when entering the chasing state
        public override void OnEnterState(EnemyStateManager context)
        {
            _cachedPosition = context.Data.ChasingTarget.position;
            context.NavMeshAgent.speed = 3f;
        }

        // Called every frame the enemy is in the chasing state
        public override void OnStateTick(EnemyStateManager context)
        {
            // Attempt to attack if within range and attack is available
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

                return; // Exit early if attack occurred
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

        // Called when exiting the chasing state
        public override void OnLeaveState(EnemyStateManager context)
        {
            _cachedPosition = null;
            context.NavMeshAgent.speed = 1f;
            context.NavMeshAgent.ResetPath();
        }

        // Cooldown routine for attacks
        private IEnumerator AttackCooldown(float cooldown)
        {
            _canAttack = false;
            yield return new WaitForSeconds(cooldown);
            _canAttack = true;
        }
    }
}