using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Handles player movement. Only moves sideways. Place on the object that holds the player's 3D model.
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController controller;
        [Tooltip("Will rotate based on the forward axis (the blue arrow) of this transform. Will use the main camera if empty.")]
        [SerializeField] private Transform movePlane;

        [Header("Settings")]
        [SerializeField, Min(0.5f)] private float moveSpeed = 2f;
        [SerializeField, Min(0.5f)] private float sprintSpeed = 5.7f;
        [SerializeField] private float speedChangeRate = 1f;
        [SerializeField, Range(0f, 1f)] private float rotationSpeed = 1f;

        private float rotationVelocity;

        /// <summary>
        /// If the current move speed is not within <c>[targetSpeed - SPEED_OFFSET, targetSpeed + SPEED_OFFSET]</c>,
        /// we'll smooth the move speed in order to reach target speed smoothly.
        /// This constant is used as the "deadzone".
        /// </summary>
        private const float SPEED_OFFSET = 0.1f;

        private void Start()
        {
            if (movePlane == null) movePlane = Camera.main.transform;
        }

        private void Reset()
        {
            // Grab the reference automatically.
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector2 moveInput = GetMoveVectorFromInput();
            
            // Rotate the player towards the input direction.
            RotatePlayer(moveInput);

            // Move the player using the input.
            Move(moveInput);
        }

        private void Move(Vector3 moveInput)
        {
            float speed = GetDesiredSpeed();

            // Calculate the movment vector using the speed and the move input.
            Vector3 nextMovement = speed * moveInput;

            // Calculate gravity.
            nextMovement -= new Vector3(0f, 9.81f, 0f);

            // Make sure the movement is frame-rate independent by multiplying by Time.deltaTime.
            nextMovement *= Time.deltaTime;

            // Move the controller.
            controller.Move(nextMovement);
        }

        /// <summary>
        /// Checks the input and returns a vector that represents the intended movement.
        /// </summary>
        /// <returns>A normalized vector where Y is always 0 (horizontal movement only).</returns>
        private Vector2 GetMoveVectorFromInput()
        {
            Vector2 move = Vector2.zero;

            if (Keyboard.current.aKey.isPressed) move = new(-1f, 0);
            if (Keyboard.current.dKey.isPressed) move = new(1f, 0);

            return move;
        }

        /// <summary>
        /// Gets the next speed to use in the <see cref="controller"/>'s <see cref="CharacterController.Move(Vector3)"/>.
        /// Checks input to determine whether we should simply move or sprint.
        /// </summary>
        /// <returns>The next speed value to use (smoothened).</returns>
        private float GetDesiredSpeed()
        {
            // Normally, we would only use the horizontal move speed, but since we can only ever move sideways, we can just
            // grab the magnitude of the controller's velocity.
            float currentSpeed = controller.velocity.magnitude;
            
            // Get the desired speed.
            // The input handling is to be replaced.
            float targetSpeed;
            if (Keyboard.current.shiftKey.isPressed) targetSpeed = sprintSpeed;
            else targetSpeed = moveSpeed;

            // If we're moving faster or slower than the desired speed, smoothly reach that value.
            // SPEED_OFFSET is the "deadzone".
            if (currentSpeed < targetSpeed - SPEED_OFFSET ||
                currentSpeed > targetSpeed + SPEED_OFFSET)
            {
                return Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            }
            // Otherwise, return the target speed.
            else return targetSpeed;
        }

        /// <summary>
        /// Rotates the object this script belongs to the input direction.
        /// </summary>
        /// <param name="move">The move input</param>
        private void RotatePlayer(Vector2 move)
        {
            // Don't apply rotation if there is no input because the player will just return to 0 rotation.
            if (move == Vector2.zero) return;
            
            // Get the angle between the input direction and the camera rotation.
            float targetRotation = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg + movePlane.eulerAngles.y;

            // Smoothly rotate towards that rotation (euler angles).
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSpeed);

            // Only rotate on the Y axis.
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }
    }
}