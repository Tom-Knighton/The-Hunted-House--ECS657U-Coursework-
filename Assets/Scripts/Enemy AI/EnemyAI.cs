using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using Enemy_AI.States;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyStateManager))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private EnemyStateManager _stateManager;
    
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();
    
    private void Awake()
    {
        if (!patrolPoints.Any())
            Debug.LogError($"No patrol points assigned to {gameObject.name}");
        
        
        _stateManager = GetComponent<EnemyStateManager>();
        _stateManager.Data.PatrolPoints = patrolPoints;
        _stateManager.SwitchState(EEnemyAIState.Patrolling);

    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.Warp(patrolPoints.First().position);
    }

    private void Update()
    {
    }
}
