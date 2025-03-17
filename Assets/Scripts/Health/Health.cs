using Unity.Netcode;
using System.Collections;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private float startingHealth;
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    private Animator anim;
    private bool dead;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private float numFlashes;
    private SpriteRenderer spriteRend;
    private bool invulnerable;

    [Header("Components")]
    [SerializeField] private Behaviour[] components;

    [Header("Hurt Sound")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip hurtSound2;
    
    [Header("Death Sound")]
    [SerializeField] private AudioClip deathSound;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
		Physics2D.IgnoreLayerCollision(8, 9, false);
        if (IsServer)
        {
            currentHealth.Value = startingHealth;
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        if (invulnerable || dead) return;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, startingHealth);
        TakeDamageClientRpc();

        if (currentHealth.Value > 0)
        {
            StartCoroutine(Invulnerability());
        }
        else
        {
            if (!dead)
            {
                dead = true;
                DieClientRpc();
                DisableEnemyServerRpc();
            }
        }
    }

    [ClientRpc]
    private void TakeDamageClientRpc()
    {
        anim.SetTrigger("hurt");
        SoundManager.instance.PlaySound(hurtSound);
        SoundManager.instance.PlaySound(hurtSound2);
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        anim.SetTrigger("die");
        foreach (Behaviour component in components)
            component.enabled = false;
        SoundManager.instance.PlaySound(deathSound);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisableEnemyServerRpc()
    {
    	// Check if the prefab has the "Enemy" tag
		if (gameObject.CompareTag("Enemy"))
		{
			DisableEnemyClientRpc();
			gameObject.SetActive(false); // Ensures it's also disabled on the server
		}
    }

    [ClientRpc]
    private void DisableEnemyClientRpc()
    {
        gameObject.SetActive(false);
    }

    [ServerRpc]
    public void AddHealthServerRpc(float value)
    {
        currentHealth.Value = Mathf.Clamp(currentHealth.Value + value, 0, startingHealth);
    }

    [ServerRpc]
    public void RespawnServerRpc()
    {
        dead = false;
        currentHealth.Value = startingHealth;
        RespawnClientRpc();
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        anim.ResetTrigger("die");
        anim.Play("Idle");
        StartCoroutine(Invulnerability());
        foreach (Behaviour component in components)
            component.enabled = true;
    }

    private IEnumerator Invulnerability()
    {
        invulnerable = true;
        Physics2D.IgnoreLayerCollision(8, 9, true);

        for (int i = 0; i < numFlashes; i++)
        {
            spriteRend.color = new Color(1, 1, 1, 0.8f);
            yield return new WaitForSeconds(iFramesDuration / (numFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numFlashes * 2));
        }

        Physics2D.IgnoreLayerCollision(8, 9, false);
        invulnerable = false;
    }

	//Test Damage
	// private void Update()
	// {
	//     if (Input.GetKeyDown(KeyCode.E))
	//         TakeDamageServerRpc(1);
	// }
}
