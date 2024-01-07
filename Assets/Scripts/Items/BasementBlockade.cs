using System;
using UnityEngine;

namespace Items
{
    public class BasementBlockade: MonoBehaviour
    {
        // Blocks the player from moving out of the basement until they pick up the phone
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                UIManager.Instance.ShowHint("I should use the phone to call for help first...");
            }
        }
    }
}