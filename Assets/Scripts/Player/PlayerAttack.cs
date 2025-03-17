using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform wavePoint;
    [SerializeField] private AudioClip attackSound;
    private float cooldownTimer = Mathf.Infinity;
    private Animator anim;
    private PlayerMovement playerMovement;
    
    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!IsOwner) return; // Only the local player processes attack input
        
        if (Input.GetMouseButtonDown(0) && cooldownTimer > attackCooldown && playerMovement.canAttack())
        {
            RequestAttackServerRpc();
            cooldownTimer = 0; 
        }

        cooldownTimer += Time.deltaTime;
    }

    [ServerRpc]
    private void RequestAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        AttackClientRpc(); // Sync animation & sound for all clients

        GameObject wavePrefab = Resources.Load<GameObject>("Wave"); // Load from Resources folder
        if (wavePrefab == null)
        {
            Debug.LogError("Wave prefab not found in Resources folder!");
            return;
        }

        // Spawn projectile on the server
        GameObject magicWave = Instantiate(wavePrefab, wavePoint.position, Quaternion.identity);
        NetworkObject networkObject = magicWave.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn(true); // Ensure all clients see it
            SetProjectileDirectionClientRpc(networkObject.NetworkObjectId, Mathf.Sign(transform.localScale.x));
        }
        else
        {
            Debug.LogError("NetworkObject component missing on Wave prefab!");
        }
    }

    [ClientRpc]
    private void AttackClientRpc()
    {
        SoundManager.instance.PlaySound(attackSound);
        anim.SetTrigger("attack");
    }

    [ClientRpc]
    private void SetProjectileDirectionClientRpc(ulong projectileId, float direction)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(projectileId, out NetworkObject netObj))
        {
            netObj.GetComponent<Projectile>().SetDirection(direction);
        }
    }
}
