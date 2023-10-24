using System;
using UnityEngine;

namespace Enemy_AI
{
    [Serializable]
    public class PatrolPoint
    {
        public Transform transform;
        public float pauseForSeconds = 0f;
        
        public Vector3 position => transform.position;
    }
}