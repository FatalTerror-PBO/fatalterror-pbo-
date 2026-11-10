using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Enemies
{
    /// <summary>
    /// INHERITANCE: StalkerEnemy -> EnemyBase.
    /// Hanya bergerak ketika TIDAK sedang dilihat pemain (gaya "weeping angel").
    /// </summary>
    public class StalkerEnemy : EnemyBase
    {
        [SerializeField] private float stalkSpeed = 5f;

        private Camera playerCamera;

        protected override void Start()
        {
            base.Start();
            playerCamera = Camera.main;
            agent.speed = stalkSpeed;
            State = EnemyState.Chase;
        }

        protected override void UpdateBehaviour()
        {
            if (playerCamera == null) playerCamera = Camera.main;

            if (IsSeenByPlayer())
            {
                agent.isStopped = true;
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(player.position);
            TryAttack();
        }

        private bool IsSeenByPlayer()
        {
            if (playerCamera == null) return false;

            Vector3 target = transform.position + Vector3.up;
            Vector3 viewport = playerCamera.WorldToViewportPoint(target);

            bool inView = viewport.z > 0f && viewport.x > 0f && viewport.x < 1f && viewport.y > 0f && viewport.y < 1f;
            if (!inView) return false;

            // Pastikan tidak terhalang tembok
            if (Physics.Linecast(playerCamera.transform.position, target, out RaycastHit hit))
                return hit.transform.IsChildOf(transform);

            return true;
        }
    }
}
