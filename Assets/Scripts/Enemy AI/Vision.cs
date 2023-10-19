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
            
            var forwardAngle = (player.transform.position - transform.position).normalized;
            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (!Physics.Raycast(transform.position, forwardAngle, distanceToPlayer, ObstacleMask))
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