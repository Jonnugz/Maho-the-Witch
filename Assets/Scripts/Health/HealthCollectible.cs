using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
	[SerializeField] private float healthValue;
    [SerializeField] private AudioClip healthCollectSound;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			collision.GetComponent<Health>().AddHealthServerRpc(healthValue);
			gameObject.SetActive(false);
			SoundManager.instance.PlaySound(healthCollectSound);
		}
	}
}
