using UnityEngine;
using UnityEngine.AI;
using HorrorGame.Audio;
using HorrorGame.Core;
using HorrorGame.Interfaces;

namespace HorrorGame.Enemies
{
    /// <summary>
    /// ABSTRACT CLASS untuk semua musuh. Kelas turunan wajib mengimplementasikan UpdateBehaviour().
    /// Pemain harus memiliki Tag "Player".
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Statistik")]
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float detectionRange = 12f;
        [SerializeField] protected float attackRange = 1.6f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected int damage = 20;

        protected NavMeshAgent agent;
        protected Transform player;
        protected IDamageable playerTarget;

        private float nextAttackTime;

        public EnemyState State { get; protected set; } = EnemyState.Idle;

        protected float DistanceToPlayer =>
            player == null ? float.MaxValue : Vector3.Distance(transform.position, player.position);

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
        }

        protected virtual void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerTarget = playerObject.GetComponent<IDamageable>();
            }
        }

        private void Update()
        {
            if (player == null) return;

            if (!GameManager.IsPlaying || playerTarget == null || playerTarget.IsDead)
            {
                if (agent.isOnNavMesh) agent.isStopped = true;
                return;
            }

            if (agent.isOnNavMesh) agent.isStopped = false;
            UpdateBehaviour();
        }

        /// <summary>Perilaku utama musuh, dipanggil setiap frame saat game berjalan.</summary>
        protected abstract void UpdateBehaviour();

        protected virtual bool CanSeePlayer()
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 target = player.position + Vector3.up;
            Vector3 direction = target - origin;

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, detectionRange))
                return hit.transform == player || hit.transform.IsChildOf(player);

            return false;
        }

        protected virtual void TryAttack()
        {
            if (DistanceToPlayer > attackRange || Time.time < nextAttackTime) return;

            nextAttackTime = Time.time + attackCooldown;
            playerTarget.TakeDamage(damage);

            if (AudioManager.Instance != null) AudioManager.Instance.Play("attack");
        }
    }
}
