using Unity.Netcode;
using UnityEngine;

public class MeleeEnemy : NetworkBehaviour
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float range;
    [SerializeField] private int damage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;
    private float cooldownTimer = Mathf.Infinity;

    // References
    private Animator anim;
    private EnemyPatrol enemyPatrol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
    }

    private void Update()
    {
        if (!IsServer) return; // Only the server controls AI behavior

        cooldownTimer += Time.deltaTime;

        if (PlayerInSight(out Health playerHealth))
        {
            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
                TriggerAttackClientRpc();
                DamagePlayerServerRpc(playerHealth.NetworkObjectId);
            }
        }

        if (enemyPatrol != null)
            enemyPatrol.enabled = !PlayerInSight(out _);
    }

    private bool PlayerInSight(out Health playerHealth)
    {
        playerHealth = null;
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, Vector2.left, 0, playerLayer);

        if (hit.collider != null)
        {
            playerHealth = hit.transform.GetComponent<Health>();
            return playerHealth != null;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }

    [ServerRpc]
    private void DamagePlayerServerRpc(ulong playerId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject netObj))
        {
            Health playerHealth = netObj.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamageServerRpc(damage);
            }
        }
    }

    [ClientRpc]
    private void TriggerAttackClientRpc()
    {
        anim.SetTrigger("meleeAttack");
    }
}
