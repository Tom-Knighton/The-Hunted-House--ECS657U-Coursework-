using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Enemy_AI
{
    // Component for handling vision detection of the player
    public class Vision : MonoBehaviour
    {
        public LayerMask PlayerLayerMask;
        public LayerMask ObstacleMask;
        
        private Collider[] _visionResults = new Collider[1];
        
        private List<Delegate> _callbacks = new();
        private bool _lastSeen;

        // Method to add a listener for player detection changes
        public void AddPlayerSeenListener(Action<bool, Transform> callback)
        {
            _callbacks.Add(callback);
        }
        
        private void Update()
        {
            // Check for the player within a sphere around the enemy
            Physics.OverlapSphereNonAlloc(transform.position, 5f, _visionResults, PlayerLayerMask);
            var player = _visionResults.FirstOrDefault();

            try
            {
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

                // Calculate direction and angle to the player
                var forwardDirection = (player.transform.position - transform.position).normalized;
                var angle = Vector3.Angle(forwardDirection, transform.forward);

                var seen = false;

                // Check if the player is within the enemy's field of view
                if (angle is < 100 and > -99f) // If player is within 90-ish degrees of forward vector (so enemy doesn't have eyes in back of head :))
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
            catch
            {
                player = null;
                _visionResults = new Collider[1];
                _lastSeen = false;
                UpdateCallbacks(_lastSeen, null);
            }
            
        }

        // Invoke all registered callbacks with the visibility status
        private void UpdateCallbacks(bool seen, Transform newTransform)
        {
            foreach (var callback in _callbacks)
            {
                callback.DynamicInvoke(seen, seen ? newTransform : null);
            }
        }
    }
}