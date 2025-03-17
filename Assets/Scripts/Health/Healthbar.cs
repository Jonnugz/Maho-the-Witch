using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Healthbar : NetworkBehaviour
{
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;
    private Health playerHealth;

    private void Start()
    {
        StartCoroutine(FindLocalPlayer());
    }

    private IEnumerator FindLocalPlayer()
    {
        while (playerHealth == null)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.GetComponent<NetworkObject>().IsLocalPlayer)
                {
                    playerHealth = player.GetComponent<Health>();
                    RequestHealthUpdateServerRpc();
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f); // Retry every 0.5 seconds
        }
    }

    private void Update()
    {
        if (playerHealth != null)
        {
            currentHealthBar.fillAmount = playerHealth.currentHealth.Value / 10;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestHealthUpdateServerRpc(ServerRpcParams rpcParams = default)
    {
        UpdateHealthClientRpc(playerHealth.currentHealth.Value);
    }

    [ClientRpc]
    private void UpdateHealthClientRpc(float healthValue)
    {
        if (playerHealth != null)
        {
            totalHealthBar.fillAmount = healthValue / 10;
            currentHealthBar.fillAmount = healthValue / 10;
        }
    }
}
