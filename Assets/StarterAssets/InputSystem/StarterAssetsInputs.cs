using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

        [Header("Menu Settings")]
        public bool isMenuOpen = false;
		public bool InputBlocked = false;
#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
		{
            if (!InputBlocked)
				MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
            if (!InputBlocked)
                JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
            if (!InputBlocked)
                SprintInput(value.isPressed);
		}

        // --- MODIFICATION FOR OFFLINE ACTION ---
        /// <summary>
        /// Add Player can Fire a bullet
        /// </summary>
        public bool fire;
        public void OnFire(InputValue value)
        {
            if (!InputBlocked)
                fire = value.isPressed;
        }

		public bool heal;

		public void OnHeal(InputValue value)
		{
            if (!InputBlocked)
                heal = value.isPressed;
		}

        public void OnPause(InputValue value)
        {
            if (value.isPressed)
            {
                ToggleMenuMode();
            }
        }
        // --- END MODIFICATION ---
#endif


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}
		
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

        // --- MODIFICATION FOR PAUSE ACTION ---
        public void ToggleMenuMode()
        {
            isMenuOpen = !isMenuOpen;
            SetInputState(!isMenuOpen);
        }

        public void SetInputState(bool isGameActive)
        {
            cursorLocked = isGameActive;
            cursorInputForLook = isGameActive;
            InputBlocked = !isGameActive;

            SetCursorState(cursorLocked);

			if (InputBlocked)
				look = Vector2.zero;
        }
        // --- END MODIFICATION ---
    }

}