using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Audio;
using HorrorGame.Core;

namespace HorrorGame.Enemies
{
    /// <summary>INHERITANCE: GhostEnemy -> EnemyBase. Patroli antar waypoint, mengejar jika melihat pemain.</summary>
    public class GhostEnemy : EnemyBase
    {
        [Header("Patroli")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();
        [SerializeField] private float patrolSpeed = 1.8f;

        [Header("Kejar")]
        [SerializeField] private float chaseSpeed = 4.2f;
        [SerializeField] private float loseSightTime = 4f;

        private int waypointIndex;
        private float loseSightTimer;

        protected override void Start()
        {
            base.Start();
            EnterPatrol();
        }

        protected override void UpdateBehaviour()
        {
            bool seesPlayer = DistanceToPlayer <= detectionRange && CanSeePlayer();

            if (State == EnemyState.Chase)
                Chase(seesPlayer);
            else if (seesPlayer)
                EnterChase();
            else
                Patrol();
        }

        private void EnterPatrol()
        {
            State = EnemyState.Patrol;
            agent.speed = patrolSpeed;

            if (waypoints.Count > 0 && agent.isOnNavMesh)
                agent.SetDestination(waypoints[waypointIndex].position);
        }

        private void EnterChase()
        {
            State = EnemyState.Chase;
            agent.speed = chaseSpeed;
            loseSightTimer = 0f;

            if (AudioManager.Instance != null) AudioManager.Instance.Play("ghost_spot");
        }

        private void Patrol()
        {
            if (waypoints.Count == 0 || !agent.isOnNavMesh) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Count;
                agent.SetDestination(waypoints[waypointIndex].position);
            }
        }

        private void Chase(bool seesPlayer)
        {
            loseSightTimer = seesPlayer ? 0f : loseSightTimer + Time.deltaTime;

            if (loseSightTimer > loseSightTime)
            {
                EnterPatrol();
                return;
            }

            agent.SetDestination(player.position);
            TryAttack();
        }
    }
}
