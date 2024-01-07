using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class RoomData
    {
        [SerializeField] public string RoomName;
        [SerializeField] public bool MustHaveKey = false;
        [SerializeField] public List<Transform> KeyPositions;
    }
}