using System;
using UnityEngine;

namespace Enemy_AI
{
    [Serializable]
    public class PatrolPoint
    {
        public Transform position;
        public float pauseForSeconds = 0f;
    }
}