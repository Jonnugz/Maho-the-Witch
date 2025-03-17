using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private AudioClip jumpSound;
    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpCooldown;
    private float horizontalInput;

    public override void OnNetworkSpawn()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        isGrounded();
    }

    public void Update()
    {
        if (!IsOwner) return; // Ensure only the owning client controls movement
        
        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0.01f)
            FlipServerRpc(1);
        else if (horizontalInput < -0.01f)
            FlipServerRpc(-1);

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            JumpServerRpc();
            if (isGrounded() || onWall())
                SoundManager.instance.PlaySound(jumpSound);
        }

        MoveAnimServerRpc(horizontalInput);  // Send input to server to update animation
        
        if (wallJumpCooldown > 0.2f)
        {
            Vector2 newVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);
            MoveServerRpc(newVelocity);
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }
    }

    [ServerRpc]
    private void MoveAnimServerRpc(float input)
    {
        MoveAnimClientRpc(input);
    }
    
    [ClientRpc]
    private void MoveAnimClientRpc(float input)
    {
        anim.SetBool("run", input != 0);
        anim.SetBool("grounded", isGrounded());
        anim.SetBool("wall", onWall());
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 velocity)
    {
        body.linearVelocity = velocity;
        MoveClientRpc(velocity);
    }

    [ClientRpc]
    private void MoveClientRpc(Vector2 velocity)
    {
        if (!IsOwner) 
            body.linearVelocity = velocity;
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        if (isGrounded())
        {
            Vector2 jumpVelocity = new Vector2(body.linearVelocity.x, jumpPower);
            body.linearVelocity = jumpVelocity;
            anim.SetTrigger("jump");
            JumpClientRpc(jumpVelocity);
        }
        else if (onWall() && !isGrounded())
        {
            // Wall jump logic for the server
            Vector2 wallJumpVelocity = horizontalInput == 0 ? 
                new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 13) : 
                new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 13);

            // Flip the character to the opposite direction based on the wall jump direction
            FlipServerRpc(-Mathf.Sign(transform.localScale.x));
            body.linearVelocity = wallJumpVelocity;

            // Reset wall jump cooldown on the server
            wallJumpCooldown = 0;

            // Propagate the wall jump velocity to the client
            JumpClientRpc(wallJumpVelocity);
        }
    }

    [ClientRpc]
    private void JumpClientRpc(Vector2 velocity)
    {
        body.linearVelocity = velocity;
    }

    [ServerRpc]
    private void FlipServerRpc(float direction)
    {
        transform.localScale = new Vector3(direction, 1, 1);
        FlipClientRpc(direction);
    }

    [ClientRpc]
    private void FlipClientRpc(float direction)
    {
        if (!IsOwner) 
            transform.localScale = new Vector3(direction, 1, 1);
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack()
    {
        return !onWall();
    }
}
