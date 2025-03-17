using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerRespawn : NetworkBehaviour
{
    [SerializeField] private AudioClip checkpoint;
    private static Transform currentCheckpoint; // Shared across all players
    private Health playerHealth;
    private UIManager uiManager;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        uiManager = FindObjectOfType<UIManager>();
    }

    public void CheckRespawn()
    {
        if (currentCheckpoint == null)
        {
            uiManager.GameOver(); // Show game over screen
            return;
        }

        // Server-side: Restore the health of the player who died
        playerHealth.RespawnServerRpc();

        // Teleport all players to the current checkpoint
        TeleportAllPlayersToCheckpointClientRpc(currentCheckpoint.position);

        // Move the player who died to the checkpoint
        transform.position = currentCheckpoint.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint"))
        {
            ActivateCheckpointServerRpc(collision.gameObject.GetComponent<NetworkObject>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateCheckpointServerRpc(NetworkObjectReference checkpointObjectRef)
    {
        if (!checkpointObjectRef.TryGet(out NetworkObject checkpointObject)) return;

        // Update the global checkpoint for the server (host)
        currentCheckpoint = checkpointObject.transform;

        // Notify clients about the new checkpoint
        ActivateCheckpointClientRpc(checkpointObject.NetworkObjectId);
    }

    [ClientRpc]
    private void ActivateCheckpointClientRpc(ulong checkpointId)
    {
        // Use NetworkObjectId to get the correct checkpoint object on the client
        GameObject checkpointObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[checkpointId].gameObject;

        if (checkpointObj != null)
        {
            // Update the checkpoint for the client
            currentCheckpoint = checkpointObj.transform;

            // Disable checkpoint collider to avoid re-triggering
            checkpointObj.GetComponent<Collider2D>().enabled = false;

            // Trigger checkpoint animation on the client
            checkpointObj.GetComponent<Animator>().SetTrigger("appear");

            // Play the checkpoint sound on the client
            SoundManager.instance.PlaySound(checkpoint);
        }
    }

    [ClientRpc]
    private void TeleportAllPlayersToCheckpointClientRpc(Vector3 checkpointPosition)
    {
        // Get all players in the scene and teleport them to the checkpoint position
        foreach (var player in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (player.TryGetComponent(out PlayerRespawn playerRespawn))
            {
                playerRespawn.transform.position = checkpointPosition;
            }
        }
    }
}
