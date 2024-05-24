using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.Serialization;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        [Header("Options")]
        public bool FlipX;
        public float MoveSpeed;


        [Header("Unsorted")]
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/
        public Collider2D collider2d;
        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] public Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;
        public SpellProjectile spellPrefab;
        public float spellForce = 20f;

        public float flyingForce = 10f;
        public float currentAngle = 45f;
        public float angleChange = 30f; // The amount the angle will change 
        public float initialAngle = 45f;
        [Header("Controls")]
        public KeyCode Jump;
        public KeyCode Left;
        public KeyCode Down;
        public KeyCode Right;
        public KeyCode AttackLong;
        public KeyCode AttackShort;
        public KeyCode ToggleFlying;
        private Rigidbody2D rb;


        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
            currentAngle = initialAngle;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                if (isFlying)
                {
                    HandleFlyingControls();
                }
                else
                {
                    HandleGroundControls();
                }

                if (Input.GetKeyDown(AttackShort))
                {
                    animator.SetTrigger("attackClose");
                }
                else if (Input.GetKeyDown(AttackLong))
                {
                    animator.SetTrigger("attackLong");
                    CastSpell();
                }

                if (Input.GetKeyDown(ToggleFlying) && !isFlying)
                {
                    ActivateFlyingMode();
                }
                else if (Input.GetKeyDown(ToggleFlying) && isFlying)
                {
                    DeactivateFlyingMode();
                    UpdateSpriteRotation(PlayerDirection());
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        private void HandleGroundControls()
        {
            if (Input.GetKey(Left))
                move.x = -MoveSpeed;
            else if (Input.GetKey(Right))
                move.x = MoveSpeed;
            else
                move.x = 0;

            if (jumpState == JumpState.Grounded && Input.GetKeyDown(Jump))
                jumpState = JumpState.PrepareToJump;
            else if (Input.GetKeyUp(Jump))
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }

        private void HandleFlyingControls()
        {
            if (Input.GetKey(Jump))
            {
                currentAngle += angleChange * Time.deltaTime;
            }
            else if (Input.GetKey(Down))
            {
                currentAngle -= angleChange * Time.deltaTime;
            }
            else if (Input.GetKey(Left))
            {
                spriteRenderer.flipX = !FlipX;
            }
            else if (Input.GetKey(Right))
            {
                spriteRenderer.flipX = FlipX;
            }

            Fly();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = FlipX;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = !FlipX;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        Vector2 PlayerDirection() => (spriteRenderer.flipX ? Vector2.left : Vector2.right) * (FlipX ? -1 : 1);

        private void CastSpell()
        {
            SpellProjectile spellInstance = Instantiate(spellPrefab, transform.position, transform.rotation);
            spellInstance.caster = gameObject;
            Rigidbody2D spellRb = spellInstance.GetComponent<Rigidbody2D>();
            Vector2 castDirection = PlayerDirection();
            spellInstance.CastDirection = castDirection;
            spellInstance.isLaunched = true;

            // Apply the force in the direction the character is facing
            spellRb.AddForce(castDirection * spellForce, ForceMode2D.Impulse);
        }

        private void ActivateFlyingMode()
        {
            // Set the flag to true
            isFlying = true;
            // rb.gravityScale = 0;

            rb.velocity = new Vector2(rb.velocity.x, 0);
            var playerDirection = PlayerDirection();
            Vector2 direction = Quaternion.Euler(0, 0, playerDirection.x * currentAngle) * PlayerDirection();
            rb.AddForce(direction * flyingForce, ForceMode2D.Impulse);
        }

        public void DeactivateFlyingMode()
        {
            isFlying = false;
            currentAngle = initialAngle;
        }

        private void Fly()
        {
            var playerDirection = PlayerDirection();
            Vector2 direction = Quaternion.Euler(0, 0, playerDirection.x * currentAngle) * PlayerDirection();
            rb.AddForce(direction * flyingForce);
            UpdateSpriteRotation(direction);
        }

        private void UpdateSpriteRotation(Vector2 flyingDirection)
        {
            float angle = Mathf.Atan2(flyingDirection.y, flyingDirection.x) * Mathf.Rad2Deg;

            if (spriteRenderer.flipX && angle > 0)
            {
                angle -= 90;
            }

            transform.rotation = Quaternion.AngleAxis(angle * PlayerDirection().x, Vector3.forward);
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}