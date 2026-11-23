using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

namespace Player
{
    /// <summary>
    /// Has a <see cref="CinemachineCamera"/> that moves on a <see cref="Spline"/> and uses the player's position
    /// to place the camera on the correct point of the <see cref="Spline"/>.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineSplineDolly camera;
        [SerializeField] private Transform player;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        /// <summary>
        /// Camera updates should happen on LateUpdate().
        /// </summary>
        private void LateUpdate()
        {
            ResolveCameraPosition();
        }

        /// <summary>
        /// Places the camera on the approriate spot depending on where the player is located.
        /// </summary>
        private void ResolveCameraPosition()
        {
            float splinePosition;

            // Get the X position coordinates from the player, the start point and the end point.
            float horizontalPosition = player.position.x;
            float minPosition = startPoint.position.x;
            float maxPosition = endPoint.position.x;

            // Set the spline position to the minimum if we're past the left-most point of the playable area.
            if (horizontalPosition < minPosition) splinePosition = 0f;
            // Set the spline position to the maximum if we're past the right-most point of the playable area.
            else if (horizontalPosition > maxPosition) splinePosition = 1f;
            // Otherwise, set the spline position based on how far away we are from the ending position.
            else splinePosition = Mathf.Lerp(0f, 1f, (horizontalPosition - minPosition) / maxPosition);

            // Make sure the spline position is always normalized.
            splinePosition = Mathf.Clamp01(splinePosition);
            
            // Apply the position to the camera.
            camera.CameraPosition = splinePosition;
        }
    }
}