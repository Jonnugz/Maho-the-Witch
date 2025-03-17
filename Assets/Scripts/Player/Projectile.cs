using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip defaultHitSound;
    private float direction;
    private bool hit;
    private float lifetime;

    private Animator anim;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (hit) return;
        float movementSpeed = speed * Time.deltaTime * direction;
        transform.Translate(movementSpeed, 0, 0); 

        lifetime += Time.deltaTime;
        //if (lifetime > 10) gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        hit = true;
        boxCollider.enabled = false;
        anim.SetTrigger("explode");

        //deals dmg only to enemies
        if(collision.tag == "Enemy")
        {
            collision.GetComponent<Health>().TakeDamageServerRpc(1);

            //play enemy hitsound
            SoundManager.instance.PlaySound(enemyHitSound);
        }
        else
            //otherwise, play default hit sound
            SoundManager.instance.PlaySound(defaultHitSound);
    }

    public void SetDirection(float _direction)
    {
        lifetime = 0;
        direction = _direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        float localScaleX = transform.localScale.x;
        if (Mathf.Sign(localScaleX) != _direction)
            localScaleX = -localScaleX;

        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
        
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
