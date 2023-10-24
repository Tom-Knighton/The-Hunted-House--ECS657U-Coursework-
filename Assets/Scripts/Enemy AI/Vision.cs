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
        
        private List<Delegate> _callbacks = new();
        private bool _lastSeen;

        public void AddPlayerSeenListener(Action<bool, Transform> callback)
        {
            _callbacks.Add(callback);
        }
        
        private void Update()
        {
            Physics.OverlapSphereNonAlloc(transform.position, 5f, _visionResults, PlayerLayerMask);
            var player = _visionResults.FirstOrDefault();
            if (player is null || !player.enabled)
            {
                // We've lost sight of the player
                if (_lastSeen)
                {
                    _lastSeen = false;
                    UpdateCallbacks(false, null);
                }
                return;
            }
            
            var forwardDirection = (player.transform.position - transform.position).normalized;
            var angle = Vector3.Angle(forwardDirection, transform.forward);

            var seen = false;

            if (angle is < 89 and > -89f) // If player is within 90-ish degrees of forward vector (so enemy doesn't have eyes in back of head :))
            {
                var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                seen = !Physics.Raycast(transform.position, forwardDirection, distanceToPlayer, ObstacleMask);
            }
            
            if (seen)
            {
                Debug.DrawLine(transform.position, player.transform.position, Color.red);
            }
            
            if (_lastSeen != seen)
            {
                UpdateCallbacks(seen, seen ? player.transform : null);
                _lastSeen = seen;
            }
        }

        private void UpdateCallbacks(bool seen, Transform newTransform)
        {
            foreach (var callback in _callbacks)
            {
                callback.DynamicInvoke(seen, seen ? newTransform : null);
            }
        }
    }
}