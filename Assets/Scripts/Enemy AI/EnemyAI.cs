using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using Enemy_AI.States;
using Enemy_AI.UI;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private EnemyStateManager _stateManager;
    private Vision _visionManager;
    private Attackable _attackable;
    private EnemyUI _localCanvas;
    private FirstPersonController _fpsController;

    private Vector3 _lastSeenPlayerPosition;
    
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();


    private void Awake()
    {
        // Log error if patrol points are not set
        if (!patrolPoints.Any())
            Debug.LogError($"No patrol points assigned to {gameObject.name}");

        // Get components
        _stateManager = GetComponent<EnemyStateManager>();
        _visionManager = GetComponent<Vision>();
        _attackable = GetComponent<Attackable>();
        _localCanvas = GetComponent<EnemyUI>();
    }

    // Set up the enemy AI
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.Warp(patrolPoints.First().position);
        _fpsController = FindFirstObjectByType<FirstPersonController>();

        // Set up state manager
        if (_stateManager is not null)
        {
            _stateManager.Data.PatrolPoints = patrolPoints;
            _stateManager.SwitchState(EEnemyAIState.Patrolling);
        }

        // Set up vision manager
        if (_visionManager is not null)
        {
            _visionManager.AddPlayerSeenListener(OnPlayerSeenChanged);
        }

        // Set up attackable component
        if (_attackable is not null)
        {
            _attackable.OnHealthChanged.AddListener(OnHealthChanged);
            _attackable.OnDeath.AddListener(OnDeath);

            // Initialize the health bar
            if (_localCanvas is not null)
            {
                _localCanvas.SetHealthBarPercentage((_attackable.health / _attackable.maxHealth) * 100);
            }
        }
    }


    /// <summary>
    /// Called when the player is seen by the enemy, or has disappeared
    /// </summary>
    private void OnPlayerSeenChanged(bool seen, Transform newTransform)
    {
        // If we see the player, we want to run towards them
        if (seen && newTransform is not null)
        {
            _lastSeenPlayerPosition = newTransform.position;
            _stateManager.Data.ChasingTarget = newTransform;
            _stateManager.SwitchState(EEnemyAIState.Chasing);
        }
        // Otherwise, we have seen them and now lost them, so we want to search around where we last saw them and eventually return to patrolling
        else if (_lastSeenPlayerPosition != Vector3.zero)
        {
            _stateManager.Data.PatrolWasInterrupted = true;
            _stateManager.Data.SearchesLeft = 5;
            _stateManager.Data.SearchAroundPoint = _lastSeenPlayerPosition;
            _stateManager.SwitchState(EEnemyAIState.Searching);
        }
    }


    private void OnHealthChanged(float newHealth, float damageDealt)
    {
        //TODO: At some point we can have separate enemy stages on health levels idk
        if (_localCanvas is not null)
        {
            _localCanvas.SetHealthBarPercentage((newHealth / _attackable.maxHealth) * 100);

            // Don't show popups when health regened
            if (damageDealt > 0)
            {
                _localCanvas.ShowDamagePopup(damageDealt);
            }
        }
    }


    private void OnDeath()
    {
        // Hide the player's UI
        UIManager.Instance.HidePlayerUI();;

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Enable the victory screen
        UIManager.Instance.ShowVictoryScreen("The boss is dead and you waited safely until the police arrived. You win!");

        // Disable the FirstPersonController to prevent player inputs
        if (_fpsController != null)
        {
            _fpsController.enabled = false;
        }
    }
}