using System;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace Enemy_AI.States
{
    public class EnemyPatrolState : EnemyAIState
    {
        private PatrolPoint _nextPatrolPoint;
        
        public override void OnEnterState(EnemyStateManager context)
        {
            // Find the closest patrol point. Will be the next point in list usually, but if enemy has been disrupted from route then
            // this will be the closest one to their current position (rather than restarting from the beginning).
            var goToIndex = GetNextPatrolPointIndex(context.Data.PatrolPointIndex, context.Data.PatrolPoints.Count());
            var closestPatrolPoint = context.Data.PatrolWasInterrupted
                ? context.Data.PatrolPoints
                    .OrderBy(p => Vector3.Distance(p.position, context.transform.position))
                    .FirstOrDefault()
                : context.Data.PatrolPoints.ElementAtOrDefault(goToIndex);

            if (closestPatrolPoint is null)
            {
                return;
            }

            context.Data.PatrolPointIndex = goToIndex;
            _nextPatrolPoint = closestPatrolPoint;
            context.NavMeshAgent.SetDestination(_nextPatrolPoint.position);
        }

        public override void OnStateTick(EnemyStateManager context)
        {
            // If we've reached the next point, pause for the specified amount of time
            if (context.NavMeshAgent.remainingDistance <= context.NavMeshAgent.stoppingDistance)
            {
                context.Data.NextState = EEnemyAIState.Patrolling;
                context.Data.PatrolWasInterrupted = false; // Interrupted is false as we're just moving to next point
                context.EnterIdleState(_nextPatrolPoint.pauseForSeconds);
                return;
            }
        }

        public override void OnLeaveState(EnemyStateManager context)
        {
            
        }

        private int GetNextPatrolPointIndex(int current, int total)
        {
            return (current + 1) % total;
        }
    }
}