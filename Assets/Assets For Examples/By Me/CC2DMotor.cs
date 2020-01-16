//#define Glide
//#define DEBUG

using UnityEngine;
using Utility.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;

namespace CC2D
{
    public abstract class CC2DMotor : MonoBehaviour
    {
        [SerializeField]
        protected PlayerActor actor;

        #region Inspector vars
        [Header("External Reference")]
        [SerializeField]
        [Tooltip("Will only be used for flipping the sprite, based on its movement.")]
        protected Transform spriteRoot;
        [Header("Control easer:")]
        [SerializeField]
        [Tooltip("Will ground the player at start.")]
        protected bool startWrappedDown = true;

        [Header("Gravity:")]
        [SerializeField]
        protected float gravityAcceleration = 20;

        [Header("Walk:")]
        [SerializeField]
        protected float walkHAcc = 5; //horizontal speed
        [SerializeField]
        protected float walkHFric = 5; //horizontal speed
        [SerializeField]
        protected float walkHMaxSpeed = 10;
        [SerializeField]
        protected float steepSlopeGravity = 5;
        [SerializeField]
        [Tooltip("If you jump of a steep slope, you will not be able to move horizontal for a time frame determined by this variable.")]
        protected float jumpOfSteepSlopeLock = 0.75f;
        [SerializeField]
        protected string movingPlatformTag;

        [Header("Fall:")]
        [SerializeField]
        protected float inAirHAcc = 11; //horizontal speed
        [SerializeField]
        protected float inAirHFric = 5;
        [SerializeField]
        protected float inAirHMaxSpeed = 11;
        [SerializeField]
        [Tooltip("Max velocity, that can be reached by falling.")]
        protected float fallCap = 100;

        [Header("Jumping:")]
        [SerializeField]
        protected float jumpVAcc = 20;

        [Header("Gliding:")]
        [SerializeField]
        protected float glideVVelocity = 3f;
        [SerializeField]
        protected float glideHAcc = 11; //horizontal speed
        [SerializeField]
        protected float glideHFric = 5;
        [SerializeField]
        protected float glideHMaxSpeed = 11;

        [Header("WallSliding:")]
        [SerializeField]
        protected LayerMask wallSlideable = 1; //Everything
        [SerializeField]
        protected float wallSlidingVVelocity = 3f;

        [Header("WallJump:")]
        [SerializeField]
        protected float walljumpVVelocity = 10;
        [SerializeField]
        protected float walljumpHVelocity = 5;
        [SerializeField]
        protected float walljumpHFric = 5;
        [SerializeField]
        [Tooltip("How much time the player input be discarded.")]
        protected float walljumpLockedTime = 1;

        [Header("Climbing:")]
        [SerializeField]
        protected string climbableTag = "Climbable";
        [SerializeField]
        protected float climbingVVelocity = 5;

        [Header("Physics Interaction:")]
        [SerializeField]
        protected float bounciness = 0;
        [SerializeField]
        [Tooltip("Used to slowly damp impulses from other rigidbodys.")]
        protected float standartDrag = 0.3f;

        [Header("Crouch:")]
        [SerializeField]
        protected float crouchHAcc = 3; //horizontal speed
        [SerializeField]
        protected float crouchHFric = 3; //horizontal speed
        [SerializeField]
        protected float crouchHMaxSpeed = 5;

        #endregion

        #region Public
        public enum MState
        {
            WallSlide, //Slower fall
            Jump,
            Glide, //Slower fall
            Fall,
            Walk,
            Climb, //Move up, down, right, left
            LockedJump, //No horizontal movement input!
            Crouched
        }

        /// <summary>
        /// Property that holds the current controller input. It's asserted, that it's never set to null.
        /// Setting it to zero will result in an error.
        /// </summary>
        public MovementInput CurrentMovementInput { get { return _cMovementInput; } }

        public int FacingDir { get { return _cFacingDir; } }

        public void AddVelocity(Vector2 velocity, float damp, Velocity2D.VelocityAllowsThisState velocityAllowsThisState)
        {
            _allExternalVelocitys.Add(new Velocity2D(velocity, damp, velocityAllowsThisState));
        }

        public void AddVelocity(Velocity2D velocity)
        {
            _allExternalVelocitys.Add(velocity);
        }

        public void FreezeAndResetMovement(bool freeze)
        {
            this.IsFroozen = freeze;
            ResetPlayerMovementInput();
        }

        public void FreezeMovement(bool freeze)
        {
            this.IsFroozen = freeze;
        }

        /// <summary>
        /// If assigned to something different from zero, this motor will act as if it were a child of the assigned object.
        /// </summary>
        public Transform FakeTransformParent { get { return _fakeParent; } set { _fakeParent = value; } }

        public Vector2 Velocity { get { return _cVelocity; } }
        public MState MotorState { get { return _cMState; } }
        public MState PrevMotorState { get { return _prevMState; } }
        public float MaxWalkSpeed { get { return walkHMaxSpeed; } }

        public void ResetPlayerMovementInput()
        {
            _cVelocity.x = 0;
            _cVelocity.y = Mathf.Min(_cVelocity.y, 0);
        }

        #endregion

        #region Private

        /// <summary>
        /// Current movement state
        /// </summary>
        protected MState _cMState;
        protected MState _prevMState;
        /// <summary>
        /// Will change with delay from grounded to not grounded, to help the player.
        /// </summary>
        protected bool _isGrounded;
        /// <summary>
        /// Current velocity, acceleration output.
        /// </summary>
        protected Vector2 _cVelocity;
        /// <summary>
        /// Holds the time the current state started (if the state sets this value).
        /// </summary>
        protected float _stateStartTime;

        protected int _climbableTriggerCount; //Climbing specific. Counts the amount of triggers we are currently touching.
        protected int _cFacingDir;
        Vector3 _fakeParentOffset;
        List<Velocity2D> _allExternalVelocitys;
        Vector2 _totalExternalVelocity;
        protected bool IsFroozen;
        protected int _crouchTrigger;
        protected MovementInput _cMovementInput;
        protected float _cJumpLockTime;

        //External reference
        protected Transform _fakeParent;

        #endregion

        protected virtual void Awake()
        {
            _cMovementInput = new MovementInput();
            _cFacingDir = 1; // Assume the sprite starts looking at the right side.
            _allExternalVelocitys = new List<Velocity2D>(1);
            triggeredColliderHash = new List<int>(2);
            if (startWrappedDown)
            {
                actor.CharacterController2D.warpToGrounded();
                StartWalk();
            }
            else
                StartFalling();
        }

        protected virtual void Update()
        {
            HandleFakeParenting();
        }

        protected abstract void FixedUpdate();

        List<int> triggeredColliderHash;
        protected void OnTriggerExit2D(Collider2D obj)
        {
            if (obj.CompareTag(climbableTag))
            {
                if (!triggeredColliderHash.Remove(obj.GetHashCode()))
                    return;
                _climbableTriggerCount--;
                if (_climbableTriggerCount == 0) //No more climbable triggers are touching us. Abort climbing.
                {
                    if (_cMState != MState.Jump)
                        StartFalling();
                }
            }
            else if (obj.CompareTag("Crouch"))
            {
                if (!triggeredColliderHash.Remove(obj.GetHashCode()))
                    return;
                _crouchTrigger--;
                Debug.Assert(_crouchTrigger >= 0);
                if (_crouchTrigger == 0)
                {
                    EndCrouch();
                    StartWalk();
                }
            }
        }

        protected void OnTriggerEnter2D(Collider2D obj)
        {
            if (obj.CompareTag(climbableTag))
            {
                if (triggeredColliderHash.Contains(obj.GetHashCode()))
                    return;
                if (_cMState != MState.Climb) //If we aren't already climbing, start now!
                {
                    StartClimbing();
                }
                _climbableTriggerCount++;
                triggeredColliderHash.Add(obj.GetHashCode());
            }
            else if (obj.CompareTag("Crouch"))
            {
                if (triggeredColliderHash.Contains(obj.GetHashCode()))
                    return;
                _crouchTrigger++;
                if (_cMState != MState.Crouched)
                    StartCrouch();
                triggeredColliderHash.Add(obj.GetHashCode());
            }
        }

        protected void OnCollisionStay2D(Collision2D col)
        {

            Rigidbody2D oRi = col.collider.GetComponent<Rigidbody2D>();
            if (oRi == null)
                return;

            // Calculate relative velocity
            Vector2 rv = _cVelocity - col.relativeVelocity;

            // Calculate relative velocity in terms of the normal direction
            float velAlongNormal = Mathf.Abs(Vector2.Dot(rv, col.contacts[0].normal));


            // Do not resolve if velocities are separating
            //if (velAlongNormal <= 0)
            //    return;

            // Calculate restitution
            float e = Mathf.Min((col.collider.sharedMaterial == null) ? 0 : col.collider.sharedMaterial.bounciness, bounciness);

            // Calculate impulse scalar
            float j = -(1 + e) * velAlongNormal;
            j /= 1 / actor.Rigidbody2D.mass + 1 / oRi.mass;

            // Apply impulse
            Vector2 impulse = j * col.contacts[0].normal;
            oRi.AddForceAtPosition(impulse, col.contacts[0].point, ForceMode2D.Force);
            //actor.CharacterController2D.move((-impulse * 1 / actor.CharacterController2D.rigidBody2D.mass) * Time.deltaTime, false);
            //AddVelocity(-impulse * 1 / actor.CharacterController2D.rigidBody2D.mass, standartDrag, (MState mStaet) => { return true; });
        }

        protected virtual void OnBecameGrounded()
        {
            Debug.Assert(_crouchTrigger == 0);
            _isGrounded = true;

            //Switch to the default grounded mState, except when we are climbing.
            if (_cMState != MState.Climb)
                StartWalk();
        }

        protected virtual void OnIsNotGrounded()
        {
            Debug.Assert(_crouchTrigger == 0);
            _isGrounded = false;

            if (_cMState == MState.Walk)
            {
                StartFalling();
            }
            else if (_cMState == MState.Crouched)
            {
                EndCrouch();
                StartFalling();
            }
            FakeTransformParent = null;
        }

        protected void FlipFacingDir()
        {
            spriteRoot.localScale = new Vector3(-spriteRoot.localScale.x, spriteRoot.localScale.y, spriteRoot.localScale.z);
            _cFacingDir *= -1;
        }

        protected void AdjustFacingDirToVelocity()
        {
            if (_cVelocity.x * _cFacingDir < 0)
                FlipFacingDir();
        }

        protected void MoveCC2DByVelocity()
        {
            _totalExternalVelocity = CalculateTotalExternalAccerleration();
            if (_fakeParent != null)
            {
                _fakeParentOffset += actor.CharacterController2D.calcMoveVector((_cVelocity + _totalExternalVelocity) * Time.fixedDeltaTime, _cMState == MState.Jump);
            }
            else
                actor.CharacterController2D.move((_cVelocity + _totalExternalVelocity) * Time.fixedDeltaTime, _cMState == MState.Jump);

            

            //We turned out to be slower then our external velocity demanded us. We presumably hit something, so reset forces.
            if (_totalExternalVelocity.x == 0)
                return;
            if (_totalExternalVelocity.x > 0)
            {
                if (actor.CharacterController2D.collisionState.right)
                {
                    _allExternalVelocitys.Clear();
                    return;
                }
            }
            else
            {
                if (actor.CharacterController2D.collisionState.left)
                {
                    _allExternalVelocitys.Clear();
                    return;
                }
            }
            if (_totalExternalVelocity.y == 0)
                return;
            if (_totalExternalVelocity.y > 0)
            {
                if (actor.CharacterController2D.collisionState.above)
                {
                    _allExternalVelocitys.Clear();
                    return;
                }
            }
            else
            {
                if (actor.CharacterController2D.collisionState.below)
                {
                    _allExternalVelocitys.Clear();
                    return;
                }
            }
        }

        protected bool ShouldWallSlide()
        {
            if (actor.CharacterController2D.collisionState.right) //we hit a wall in that direction
            {
                if (Mathf.Approximately(actor.CharacterController2D.collisionState.rightHit.normal.y, 0)) // no sliding on overhangs!
                    if (wallSlideable.IsLayerWithinMask(actor.CharacterController2D.collisionState.rightHit.collider.gameObject.layer))
                        return true; //the walls layer is contained in all allowed layers.
            }
            else if (actor.CharacterController2D.collisionState.left) //we hit a wall in that direction
                if (actor.CharacterController2D.collisionState.leftHit.normal.y >= 0) // no sliding on overhangs!
                    if (wallSlideable.IsLayerWithinMask(actor.CharacterController2D.collisionState.leftHit.collider.gameObject.layer))
                        return true; //the walls layer is contained in all allowed layers.
            return false;
        }

        protected void HandleSlope()
        {
            _cVelocity = new Vector2(actor.CharacterController2D.collisionState.belowHit.normal.y, -actor.CharacterController2D.collisionState.belowHit.normal.x) * steepSlopeGravity * actor.CharacterController2D.collisionState.belowHit.normal.x;
        }

        protected void HandleFakeParenting()
        {
            if (_fakeParent != null)
            {
                transform.position = _fakeParent.position + _fakeParentOffset;
            }
        }

        protected void ReCalculateFakeParentOffset()
        {
            if (_fakeParent != null)
            {
                _fakeParentOffset = transform.position - _fakeParent.position;

            }
        }

        protected void AccelerateHorizontal(ref float acc, ref float fric, ref float cap)
        {
            if (_cMovementInput.horizontalRaw != 0)
            {
                if (_cMovementInput.horizontalRaw > 0)
                {
                    _cVelocity.x = Mathf.Abs(_cVelocity.x);
                    _cVelocity.x += acc * Time.fixedDeltaTime;
                    _cVelocity.x = Mathf.Min(cap, _cVelocity.x);
                }
                else
                {
                    _cVelocity.x = -Mathf.Abs(_cVelocity.x);
                    _cVelocity.x -= acc * Time.fixedDeltaTime;
                    _cVelocity.x = Mathf.Max(-cap, _cVelocity.x);
                }
            }
            else if (_cVelocity.x != 0) //No Input? Apply friction. Wait, what? Thats horrible!!
            {
                /*if (_cVelocity.x < 0)
                {
                    _cVelocity.x += fric * Time.fixedDeltaTime;
                    if (_cVelocity.x > 0)
                        _cVelocity.x = 0;
                }
                else
                {
                    _cVelocity.x -= fric * Time.fixedDeltaTime;
                    if (_cVelocity.x < 0)
                        _cVelocity.x = 0;
                }*/
                _cVelocity.x = 0;
            }

        }

        protected void ApplyGravity(ref float gravity, ref float cap)
        {
            _cVelocity.y -= gravity * Time.fixedDeltaTime;
            Mathf.Max(cap, gravity);
        }

        protected void ApplyFrictionHorizontal(ref float fric)
        {
            if (_cVelocity.x == 0)
                return;

            if (_cVelocity.x > 0)
            {
                _cVelocity.x -= fric * Time.fixedDeltaTime;
                if (_cVelocity.x < 0)
                    _cVelocity.x = 0;
            }
            else
            {
                _cVelocity.x += fric * Time.fixedDeltaTime;
                if (_cVelocity.x > 0)
                    _cVelocity.x = 0;
            }
        }

        Vector2 CalculateTotalExternalAccerleration()
        {
            Vector2 result = Vector2.zero;
            Velocity2D pForce;
            //Go through all stored forces. Add them to the result and damp them.
            for (int iForce = 0; iForce < _allExternalVelocitys.Count; iForce++)
            {
                pForce = _allExternalVelocitys[iForce];
                result += pForce.Velocity;
                pForce.DampVelocity(Time.fixedDeltaTime);
                if (pForce.IsVelocityZero() || !pForce.velocityAllowsThisState(_cMState))//Force is zero remove it!
                {
                    _allExternalVelocitys.RemoveAt(iForce);
                    iForce--;
                }
            }
            return result;
        }

        #region Methods to start each state with

        protected virtual void StartFalling()
        {
            _prevMState = _cMState;
            _cMState = MState.Fall;
        }

        protected virtual void StartWalk()
        {
            _prevMState = _cMState;
            _cMState = MState.Walk;
        }

        protected virtual void StartJump()
        {
            _isGrounded = false;
            _stateStartTime = Time.time;
            _cVelocity.y = jumpVAcc;
            //frontAnimator.SetTrigger("Jump");
            _prevMState = _cMState;
            _cMState = MState.Jump;
        }

        protected virtual void StartLockedJump(float lockedTime)
        {
            _cJumpLockTime = lockedTime;
            _isGrounded = false;
            _stateStartTime = Time.time;
            _cVelocity.y = jumpVAcc;
            _prevMState = _cMState;
            _cMState = MState.LockedJump;
        }

        float crouchScaleFactor = 0.50f;  // hack to show player as crouched
        protected virtual void StartCrouch()
        {
            _prevMState = _cMState;
            //Crouch the char down, only for debug!
            spriteRoot.localScale = new Vector3(spriteRoot.localScale.x * crouchScaleFactor, spriteRoot.localScale.y * crouchScaleFactor, spriteRoot.localScale.z);
            _cMState = MState.Crouched;
        }

        protected virtual void EndCrouch()
        {
            _prevMState = _cMState;
            //Stop crouch the char down, only for debug!
            spriteRoot.localScale = new Vector3(spriteRoot.localScale.x / crouchScaleFactor, spriteRoot.localScale.y / crouchScaleFactor, spriteRoot.localScale.z);
        }

        protected virtual void StartGliding()
        {
            _cVelocity.y = -glideVVelocity;
            _prevMState = _cMState;
            _cMState = MState.Glide;
        }

        protected virtual void StartWallSliding()
        {
            _cVelocity.x = 0; // No side movement in this state!
            _cVelocity.y = -wallSlidingVVelocity;
            _prevMState = _cMState;
            _cMState = MState.WallSlide;
        }

        protected virtual void StartWallJump()
        {
            _cVelocity.x = walljumpHVelocity * -_cFacingDir;
            _cVelocity.y = walljumpVVelocity;
            _stateStartTime = Time.time;
            _prevMState = _cMState;
            _cMState = MState.Jump;
        }

        protected virtual void StartClimbing()
        {
            _cMState = MState.Climb;
            _prevMState = _cMState;
        }

        #endregion

        #region Coroutines

        protected delegate void DelayedAction(object data);
        /// <summary>
        /// Executes the given "action" a by "delay" specified number of fixed frames later.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="data">The data, that will be supplied to the "action" method.</param>
        /// <param name="delay">Determines how many fixed frames the "action" should be delayed.</param>
        /// <returns></returns>
        protected IEnumerator DelayForFixedFrames(DelayedAction action, int delay, object data = null)
        {
            int frameCounter = 0;
            while (frameCounter < delay)
            {
                yield return null;
                frameCounter++;
            }
            action(data);
        }

        #endregion
    }

    public class Velocity2D
    {
        public delegate bool VelocityAllowsThisState(CC2DMotor.MState MState);
        public Vector2 Velocity { get { return vel; } }
        public VelocityAllowsThisState velocityAllowsThisState;
        Vector2 vel;
        float damp;


        public Velocity2D(Vector2 velocity, float damp, VelocityAllowsThisState velocityAllowsThisState)
        {
            this.vel = velocity;
            this.damp = damp;
            this.velocityAllowsThisState = velocityAllowsThisState;
        }

        /// <summary>
        /// Will damp the stored force and return it. (Damped force is also saved internally. Another call will return a even minor force!)
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public void DampVelocity(float deltaTime)
        {
            vel = vel * (1 - deltaTime * damp);
            //velocity += -velocity.normalized * (Mathf.Sqrt(velocity.magnitude) * drag);
        }

        /// <summary>
        /// Returns true, if the force is approximately zero.
        /// </summary>
        /// <returns></returns>
        public bool IsVelocityZero()
        {
            return Mathf.Approximately(vel.x, 0) && Mathf.Approximately(vel.y, 0);
        }
    }
}
