using System.Collections;
using UnityEngine;

namespace Enemy_AI.States
{
    public class EnemyIdleState : EnemyAIState
    {
        public override void OnEnterState(EnemyStateManager context)
        {
            context.StartCoroutine(WaitForLookAroundCoroutine(context.Data.IdleTimer));
            return;
            
            IEnumerator WaitForLookAroundCoroutine(float waitTime)
            {
                yield return new WaitForSecondsRealtime(waitTime);
                context.SwitchState(context.Data.NextState);
            }
        }

        public override void OnStateTick(EnemyStateManager context)
        {
        }

        public override void OnLeaveState(EnemyStateManager context)
        {
        }
    }
}