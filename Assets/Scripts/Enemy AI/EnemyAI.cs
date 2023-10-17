using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
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
    /// To be run on each update, will move the AI towards the next patrol point
    /// </summary>
    private void DoPatrolUpdate()
    {
        // Look towards the next point
        transform.LookAt(_nextPatrolPoint.position);

        // Move towards next point
        var step = 10f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _nextPatrolPoint.position, step);
        
        // If we've reached the next point, pause for the specified amount of time
        if (Vector3.Distance(transform.position, _nextPatrolPoint.position) < 0.001f)
        {
            _lookAroundTimer = _nextPatrolPoint.pauseForSeconds;
            _state = EnemyState.LookAround;
            _previousState = EnemyState.Patrolling;
            _currentPatrolPointIndex = (_currentPatrolPointIndex + 1) % patrolPoints.Count;
            _nextPatrolPoint = patrolPoints[_currentPatrolPointIndex];
        }
    }

    private void DoLookAroundUpdate()
    {
        if (_isLookingAround)
            return;

        StartCoroutine(WaitForLookAroundCoroutine(_lookAroundTimer));
        _lookAroundTimer = 0f;
        _isLookingAround = true;
    }

    private IEnumerator WaitForLookAroundCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        _isLookingAround = false;
        _state = _previousState;
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
    }
}
