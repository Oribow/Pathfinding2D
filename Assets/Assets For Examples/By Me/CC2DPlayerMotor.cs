//#define Glide
#define DEBUG

using UnityEngine;
using Utility.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;

namespace CC2D
{
    public class CC2DPlayerMotor : CC2DMotor
    {
        #region Inspector vars
        [Header("External Reference")]
        [SerializeField]
        [Tooltip("Delay in fixed frames before isGrounded changes from true to false.")]
        int transToFallDelay = 20;
        [SerializeField]
        [Tooltip("Determines how long the jump button has to be hold before switching to gliding.")]
        float minGlideButtonHoldTime = 0.1f;

        [Header("Jumping:")]
        [SerializeField]
        float minJumpTime = 0.5f;
        [SerializeField]
        [Tooltip("The y velocity that gets assigned when the analog jump ends early.")]
        float jumpCutVelocity = 4;

        [Header("WallSliding:")]
        [SerializeField]
        [Tooltip("The min. time of user input away from the wall, which is needed for detaching from the wall.")]
        float wallStickiness = 0.1f;

        #endregion

        #region Private

        float _wallDetachingInput; //WallSlide specific

        //Coroutine
        Coroutine _delayedUnGrounding;

        #endregion

        protected override void FixedUpdate()
        {
            

            _prevMState = _cMState;
            //Check, if we are grounded
            if (actor.CharacterController2D.collisionState.wasGroundedLastFrame && !actor.CharacterController2D.isGrounded)
                OnIsNotGrounded();
            else if (actor.CharacterController2D.collisionState.becameGroundedThisFrame)
                OnBecameGrounded();

            if (IsFroozen)
                return;
            var currentInputEvent = _cMovementInput.GetNextEvent();

            switch (_cMState)
            {
                case MState.Walk:
                    //Handle sliding of a top step slope

                    if (actor.CharacterController2D.collisionState.standOnToSteepSlope || actor.CharacterController2D.manuallyCheckForSteepSlopes(-actor.CharacterController2D.collisionState.belowHit.normal.x))
                    {
                        HandleSlope();
                        if (currentInputEvent as JumpEvent != null)
                        {
                            _cMovementInput.ConsumeEvent(currentInputEvent);
                            StartLockedJump(jumpOfSteepSlopeLock);
                            return;
                        }
                    }

                    AccelerateHorizontal(ref walkHAcc, ref walkHFric, ref walkHMaxSpeed);
                    _cVelocity.y = -0.02f; //Small downwards velocity, to keep the CC2D grounded.

                    if (currentInputEvent as JumpEvent != null)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        StartJump();
                    }
                    else if (actor.CharacterController2D.collisionState.belowHit.collider.CompareTag(movingPlatformTag))
                    {
                        if (actor.CharacterController2D.collisionState.belowHit.collider.transform.rotation != Quaternion.identity || actor.CharacterController2D.collisionState.belowHit.collider.transform.localScale != new Vector3(1, 1, 1))
                            FakeTransformParent = actor.CharacterController2D.collisionState.belowHit.collider.transform.parent;
                        else
                            FakeTransformParent = actor.CharacterController2D.collisionState.belowHit.collider.transform;
                        transform.parent = actor.CharacterController2D.collisionState.belowHit.collider.transform.parent;
                        ReCalculateFakeParentOffset();
                    }
                    else
                    {
                        transform.parent = null;
                        FakeTransformParent = null;
                    }

                    if (currentInputEvent as CrouchEvent != null)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        StartCrouch();
                    }
                    break;

                case MState.Crouched:
                    AccelerateHorizontal(ref crouchHAcc, ref crouchHFric, ref crouchHMaxSpeed);
                    _cVelocity.y = -0.02f; //Small downwards velocity, to keep the CC2D grounded.
                    ApplyGravity(ref gravityAcceleration, ref fallCap);

                    if (_crouchTrigger > 0)
                        break;

                    if (currentInputEvent as CrouchEvent != null)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        EndCrouch();
                        StartWalk();
                    }
                    else if (currentInputEvent as JumpEvent != null)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        EndCrouch();
                        StartJump();
                    }

                    break;

                case MState.Fall:
                    AccelerateHorizontal(ref inAirHAcc, ref inAirHFric, ref inAirHMaxSpeed);
                    ApplyGravity(ref gravityAcceleration, ref fallCap);

                    //Possible transitions
#if Glide
                    if (!_cMovementInput.isJumpConsumed && _cMovementInput.jump && Time.time - _cMovementInput.timeOfLastJumpStateChange >= minGlideButtonHoldTime) //Should we glide?
                        {
                    StartGliding();
                    frontAnimator.SetBool("IsFalling", false);
                    }
                    else
#endif
                    if (ShouldWallSlide())
                    {
                        StartWallSliding();

                    }
                    break;

                case MState.Jump:
                    AccelerateHorizontal(ref inAirHAcc, ref inAirHFric, ref inAirHMaxSpeed);
                    ApplyGravity(ref gravityAcceleration, ref fallCap);

                    //Possible transitions
                    if (_cVelocity.y <= jumpCutVelocity) //Finished jumping, gravity catch us!
                        StartFalling();
                    else if (actor.CharacterController2D.collisionState.above) // Probably hit the ceiling. Abort Jump to avoid "hovering at the ceiling"!
                    {
                        _cVelocity.y = 0;
                        StartFalling();
                    }
                    break;

                case MState.LockedJump:
                    ApplyGravity(ref gravityAcceleration, ref fallCap);

                    //Possible transitions
                    if (Time.time - _stateStartTime >= _cJumpLockTime)
                    {
                        //Switch seamless to normal jumping!
                        _cMState = MState.Jump;
                    }
                    else if (actor.CharacterController2D.collisionState.above) // Probably hit the ceiling. Abort Jump to avoid "hovering at the ceiling"!
                    {
                        _cVelocity.y = 0;
                        StartFalling();
                    }
                    break;

                case MState.Glide:
                    AccelerateHorizontal(ref glideHAcc, ref glideHFric, ref glideHMaxSpeed);
                    if (currentInputEvent as JumpEvent == null)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        StartFalling();
                    }
                    break;
                case MState.WallSlide:
                    if (_cFacingDir * _cMovementInput.horizontalRaw < 0) //They don't share the same sign and horizontalRaw != 0
                    {
                        _wallDetachingInput += Time.deltaTime;
                        if (_wallDetachingInput >= wallStickiness) // Input time exceed wall stickiness. Detach from wall!
                            StartFalling();
                    }
                    else
                        _wallDetachingInput = 0; // No opposite to wall input. Reset wall detaching counter

                    if (currentInputEvent as JumpEvent != null)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        StartWallJump();
                    }
                    if (!actor.CharacterController2D.manuallyCheckForCollisionsH(_cFacingDir * 0.01f)) //The wall suddenly ended in mid air!
                        StartFalling();
                    break;

                case MState.Climb:
                    AccelerateHorizontal(ref inAirHAcc, ref inAirHFric, ref inAirHMaxSpeed);
                    _cVelocity.y = _cMovementInput.verticalRaw * climbingVVelocity;

                    //Possible transitions
                    //To allow jumping, even when not grounded, but only when some sort of x movement is applied.
                    //For a straight up jump, climbing shouldn't be used!
                    if (currentInputEvent as JumpEvent != null && _cVelocity.x != 0)
                    {
                        _cMovementInput.ConsumeEvent(currentInputEvent);
                        StartJump();
                    }
                    break;
            }
            //Solely determined by input
            AdjustFacingDirToVelocity();
            MoveCC2DByVelocity();
        }

        protected override void OnBecameGrounded()
        {
            Debug.Assert(_crouchTrigger == 0);
            _isGrounded = true;
            //Stop the delayed ground routine, as we are already grounded.
            if (_delayedUnGrounding != null)
                StopCoroutine(_delayedUnGrounding);
            //Switch to the default grounded mState, except when we are climbing.
            if (_cMState != MState.Climb)
                StartWalk();
        }

        protected override void OnIsNotGrounded()
        {
            Debug.Assert(_crouchTrigger == 0);

            if (_cMState != MState.Climb)
                _delayedUnGrounding = StartCoroutine(DelayForFixedFrames((object data) => { _isGrounded = false; }, transToFallDelay));
            else
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

#if DEBUG
        #region Debug

        void OnGUI()
        {
            if (gameObject.CompareTag("Player"))
            {
                GUILayout.Label("cState = " + _cMState.ToString());
                GUILayout.Label("MoveOutput = " + _cVelocity);
                GUILayout.Label("Real velocity = " + actor.CharacterController2D.velocity);
                GUILayout.Label("isGrounded = " + _isGrounded + "( raw = " + actor.CharacterController2D.isGrounded + ")");
                GUILayout.Label("currentLyTouchingClimbables = " + _climbableTriggerCount);
                GUILayout.Label("isOnSlope = " + actor.CharacterController2D.collisionState.standOnToSteepSlope);
                GUILayout.Label("isFakedParents = " + (_fakeParent != null));
            }
        }
        #endregion
#endif
    }
}
