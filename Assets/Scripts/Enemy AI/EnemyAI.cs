using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

    public float MovementSpeed = 1f;

    private NavMeshAgent _agent;
    private EnemyState _state = EnemyState.Patrolling;
    private EnemyState _previousState;

    #region Patrol Variables
    
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();
    private PatrolPoint _nextPatrolPoint;
    private int _currentPatrolPointIndex = 0;
    
    #endregion
    
    #region Look Around Variables
    private float _lookAroundTimer = 0f;
    private bool _isLookingAround = false;
    #endregion
    
    private void Awake()
    {
        if (!patrolPoints.Any())
            Debug.LogError($"No patrol points assigned to {gameObject.name}");

        transform.position = patrolPoints.First().position;
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        // Calls patrol immediately
        StartPatrolRoute();
    }

    private void Update()
    {
        switch (_state)
        {
            case EnemyState.Patrolling:
                DoPatrolUpdate();
                break;
            case EnemyState.LookAround:
                DoLookAroundUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// To be run on each update, will check if the AI has reached its destination
    /// </summary>
    private void DoPatrolUpdate()
    {
        // If we've reached the next point, pause for the specified amount of time
        if (_agent.remainingDistance <= 0.1f)
        {
            _lookAroundTimer = _nextPatrolPoint.pauseForSeconds;
            _state = EnemyState.LookAround;
            _previousState = EnemyState.Patrolling;
            Debug.Log("Reached dest");
        }
    }

    /// <summary>
    /// To be run on each update, will start a coroutine if not already started that will wait for the specified amount
    /// </summary>
    private void DoLookAroundUpdate()
    {
        if (_isLookingAround)
            return;

        StartCoroutine(WaitForLookAroundCoroutine(_lookAroundTimer));
        _lookAroundTimer = 0f;
        _isLookingAround = true;
        return;

        IEnumerator WaitForLookAroundCoroutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            _isLookingAround = false;
            _state = _previousState;
            if (_state == EnemyState.Patrolling)
                GoToNextPatrolPoint();
        }
    }

    

    /// <summary>
    /// Starts the enemy along their assigned patrol route.
    /// </summary>
    private void StartPatrolRoute()
    {
        // Find the closest patrol point. Will be the first one on load, but if enemy has been disrupted from route then
        // this will be the closest one to their current position (rather than restarting from the beginning).
        var closestPatrolPoint = patrolPoints
            .OrderBy(p => Vector3.Distance(p.position, transform.position))
            .First();

        _nextPatrolPoint = closestPatrolPoint;
        _state = EnemyState.Patrolling;
        _agent.SetDestination(_nextPatrolPoint.position);
    }

    /// <summary>
    /// Finds the next patrol point in the list and sets the agent's destination to it.
    /// </summary>
    private void GoToNextPatrolPoint()
    {
        Debug.Log("Going to next patrol point");
        _currentPatrolPointIndex = (_currentPatrolPointIndex + 1) % patrolPoints.Count;
        _nextPatrolPoint = patrolPoints[_currentPatrolPointIndex];
        
        _agent.SetDestination(_nextPatrolPoint.position);
    }
}
