using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using Enemy_AI.States;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private EnemyStateManager _stateManager;
    private Vision _visionManager;
    
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();
    
    private void Awake()
    {
        if (!patrolPoints.Any())
            Debug.LogError($"No patrol points assigned to {gameObject.name}");
        
        
        _stateManager = GetComponent<EnemyStateManager>();
        _visionManager = GetComponent<Vision>();
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.Warp(patrolPoints.First().position);
        
        if (_stateManager is not null)
        {
            _stateManager.Data.PatrolPoints = patrolPoints;
            _stateManager.SwitchState(EEnemyAIState.Patrolling);
        }
        
        if (_visionManager is not null)
        {
            _visionManager.AddPlayerSeenListener((seen) =>
            {
                Debug.Log($"Seen now {seen}");
            });
        }

    }
}
