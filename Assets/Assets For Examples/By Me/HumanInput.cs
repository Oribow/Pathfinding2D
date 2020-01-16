using UnityEngine;
using System;

namespace CC2D
{
    public class HumanInput : MonoBehaviour
    {
        [SerializeField]
        PlayerActor actor;

        [SerializeField]
        [Tooltip("Max time a jump will be buffered.")]
        float maxJumpExecutionDelay = 0.5f;

        MovementInput bufferedInput;
        bool allowMovementInput = true;
        bool allowEquipmentInput = true;

        void Start ()
        {
            bufferedInput = actor.CC2DMotor.CurrentMovementInput;
        }

        void Update()
        {
            if (allowMovementInput)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    bufferedInput.AddEvent(new JumpEvent(maxJumpExecutionDelay));
                }
                else if (Input.GetButtonDown("Crouch"))
                {
                    bufferedInput.AddEvent(new CrouchEvent());
                }
            }
        }

        void FixedUpdate()
        {
            if (!allowMovementInput)
                return;

            bufferedInput.horizontalRaw = Input.GetAxisRaw("Horizontal");
            bufferedInput.verticalRaw = Input.GetAxisRaw("Vertical");

            bufferedInput.horizontal = Input.GetAxis("Horizontal");
            bufferedInput.vertical = Input.GetAxis("Vertical");
        }

        public void SetAllowAllInput(bool enabled)
        {
            SetAllowEquipmentInput(enabled);
            SetAllowMovementInput(enabled);
        }

        public void SetAllowMovementInput(bool enabled)
        {
            allowMovementInput = enabled;
            if (!allowMovementInput)
                ResetPlayerMovementInput();
        }

        public void SetAllowEquipmentInput(bool enabled)
        {
            allowEquipmentInput = enabled;
        }

        public void ResetPlayerMovementInput()
        {
            bufferedInput.horizontal = 0;
            bufferedInput.horizontalRaw = 0;
            bufferedInput.vertical = 0;
            bufferedInput.verticalRaw = 0;
        }
    }
}
