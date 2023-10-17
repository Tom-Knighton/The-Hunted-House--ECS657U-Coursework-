using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();
    
    private void Awake()
    {
        if (!patrolPoints.Any())
            Debug.LogError($"No patrol points assigned to {gameObject.name}");

        transform.position = patrolPoints.First().position;
    }
}
