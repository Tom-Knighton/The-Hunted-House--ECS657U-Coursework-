using System.Collections.Generic;
using Enemy_AI;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    public List<PatrolPoint> patrolPoints = new();
}
