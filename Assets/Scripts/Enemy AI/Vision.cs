using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Enemy_AI
{
    public class Vision : MonoBehaviour
    {
        public LayerMask PlayerLayerMask;
        public LayerMask ObstacleMask;
        
        private Collider[] _visionResults = new Collider[1];
        
        private void Update()
        {
            Physics.OverlapSphereNonAlloc(transform.position, 5f, _visionResults, PlayerLayerMask);
            var player = _visionResults.FirstOrDefault();
            if (player is null)
            {
                return;
            }
            
            var forwardDirection = (player.transform.position - transform.position).normalized;
            var angle = Vector3.Angle(forwardDirection, transform.forward);
            
            if (angle is > 89 or < -89f)
            {
                Debug.Log("Out of vision cone");
                return;
            }
            
            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (!Physics.Raycast(transform.position, forwardDirection, distanceToPlayer, ObstacleMask))
            {
                Debug.DrawLine(transform.position, player.transform.position, Color.red);
            }
            else
            {
                Debug.Log("See but blocked");
            }
            
        }
    }
}