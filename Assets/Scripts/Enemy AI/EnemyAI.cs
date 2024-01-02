using System;
using System.Collections.Generic;
using System.Linq;
using Enemy_AI;
using Enemy_AI.States;
using Enemy_AI.UI;
using Game;
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
    private Animator _animator;
    private EnemyAudio _audio;

    private Vector3 _lastSeenPlayerPosition;
    
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();
    
    private static readonly int Speed = Animator.StringToHash("Speed");

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
        _animator = GetComponent<Animator>();
        _audio = GetComponent<EnemyAudio>();
    }

    // Set up the enemy AI
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.Warp(patrolPoints.First().position);

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

    private void Update()
    {
        var speed = _agent.velocity.magnitude * _agent.speed;
        _animator.SetFloat(Speed, speed);
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

            AudioManager.Instance.PlaySpottedSound();
        }
        // Otherwise, we have seen them and now lost them, so we want to search around where we last saw them and eventually return to patrolling
        else
        {
            _stateManager.Data.PatrolWasInterrupted = true;
            _stateManager.Data.SearchesLeft = 3;
            _stateManager.Data.SearchAroundPoint = _lastSeenPlayerPosition;
            _stateManager.SwitchState(EEnemyAIState.Searching);
        }
    }


    private void OnHealthChanged(float newHealth, float damageDealt)
    {
        if (_localCanvas is not null)
        {
            _localCanvas.SetHealthBarPercentage((newHealth / _attackable.maxHealth) * 100);

            // Don't show popups when health regened
            if (damageDealt > 0)
            {
                _localCanvas.ShowDamagePopup(damageDealt);

                _stateManager.SafeTriggerAnimator("GetAttacked");
                // If we were attacked by player, assume we can now see them/feel it and turn around if necessary
                if (_stateManager.CurrentEState != EEnemyAIState.Chasing)
                {
                    _stateManager.Data.ChasingTarget = FirstPersonController.instance.transform;
                    _stateManager.Data.PatrolWasInterrupted = true;
                    _stateManager.SwitchState(EEnemyAIState.Chasing);
                }
                _audio?.GetHitSound();
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
        GameManager.Instance.DisablePlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Door door))
        {
            door.OnInteract();
        }
    }
}