using System.Linq;
using System.Numerics;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

namespace Enemy_AI.States
{
    public class EnemySearchState : EnemyAIState
    {
        private const float SearchAroundRadius = 1.5f;
        
        public override void OnEnterState(EnemyStateManager context)
        {
            var searchAroundPoint = context.Data.SearchAroundPoint;
            if (searchAroundPoint == Vector3.zero)
            {
                return;
            }

            context.NavMeshAgent.stoppingDistance = 0;
            context.NavMeshAgent.SetDestination(searchAroundPoint);
            context.Data.SearchesLeft -= 1;
        }

        public override void OnStateTick(EnemyStateManager context)
        {
            // If agent has reached the point to search around, get a random location within a minimum distance 
            if (context.NavMeshAgent.remainingDistance <= 0.1f)
            {
                // If we've done all the searches needed, go back to patrolling
                if (context.Data.SearchesLeft <= 0)
                {
                    context.Data.SearchesLeft = null;
                    context.SwitchState(EEnemyAIState.Patrolling);
                    return;
                }
                
                // Find a random point to search around
                var randomPoint = Random.insideUnitSphere * SearchAroundRadius;
                var dir = randomPoint + context.transform.position;
                if (NavMesh.SamplePosition(dir, out var hit, SearchAroundRadius, 1))
                {
                    randomPoint = hit.position;
                }
                // Wait for 5 secs at this location
                context.Data.NextState = EEnemyAIState.Searching;
                context.Data.SearchAroundPoint = randomPoint;
                context.EnterIdleState(5f);
            }
        }

        public override void OnLeaveState(EnemyStateManager context)
        {
            context.NavMeshAgent.stoppingDistance = 1.5f;
        }
    }
}