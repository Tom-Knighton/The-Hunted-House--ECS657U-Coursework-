using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        private int _lastSeenTries = 0;

        // Method to add a listener for player detection changes
        public void AddPlayerSeenListener(Action<bool, Transform> callback)
        {
            _callbacks.Add(callback);
        }
        
        private void FixedUpdate()
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
                if (angle is < 130 and > -130f) // If player is within 90-ish degrees of forward vector (so enemy doesn't have eyes in back of head :))
                {
                    var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                    var raycastPos = transform.position;
                    raycastPos.y += 1;
                    seen = !Physics.Raycast(raycastPos, forwardDirection, distanceToPlayer, ObstacleMask);
                    
                    if (seen)
                    {
                        Debug.DrawLine(raycastPos, player.transform.position, Color.red);
                    }
                }
                
                // If last seen does not match current,
                // if we can see the player, immediately notify AI manager,
                // If we lost player, wait 250 FixedUpdates (about 5s depending on editor settings) and if we still can't, then notify manager
                if (_lastSeen != seen)
                {
                    if (seen)
                    {
                        UpdateCallbacks(true, player.transform);
                        _lastSeen = seen;
                        _lastSeenTries = 0;
                        Debug.Log("Seen!");
                    }
                    else
                    {
                        _lastSeenTries++;
                        if (_lastSeenTries > 250)
                        {
                            UpdateCallbacks(false, null);
                            _lastSeen = seen;
                            _lastSeenTries = 0;
                            Debug.Log("Lost!");
                        }
                    }
                }
            }
            catch
            {
                player = null;
                _visionResults = new Collider[1];
                Debug.LogError("Vision broke");
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