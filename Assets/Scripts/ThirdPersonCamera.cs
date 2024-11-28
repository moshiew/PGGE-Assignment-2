using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE;

public enum CameraType
{
    Track,
    Follow_Track_Pos,
    Follow_Track_Pos_Rot,
    Topdown,
    Follow_Independent,
}

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform mPlayer;

    TPCBase mThirdPersonCamera;
    // Get from Unity Editor.
    public Vector3 mPositionOffset = new Vector3(0.0f, 2.0f, -2.5f);
    public Vector3 mAngleOffset = new Vector3(0.0f, 0.0f, 0.0f);
    [Tooltip("The damping factor to smooth the changes in position and rotation of the camera.")]
    public float mDamping = 1.0f;

    public float mMinPitch = -30.0f;
    public float mMaxPitch = 30.0f;
    public float mRotationSpeed = 50.0f;
    public FixedTouchField mTouchField;

    public Transform player;
    public LayerMask obstacleLayer;
    public float sphereRadius = 0.5f;
    public float verticalOffset = 2.0f;
    public float distanceBehindPlayer = 3.0f;

    public CameraType mCameraType = CameraType.Follow_Track_Pos;
    Dictionary<CameraType, TPCBase> mThirdPersonCameraDict = new Dictionary<CameraType, TPCBase>();

    void Start()
    {
        // Set to CameraConstants class so that other objects can use.
        CameraConstants.Damping = mDamping;
        CameraConstants.CameraPositionOffset = mPositionOffset;
        CameraConstants.CameraAngleOffset = mAngleOffset;
        CameraConstants.MinPitch = mMinPitch;
        CameraConstants.MaxPitch = mMaxPitch;
        CameraConstants.RotationSpeed = mRotationSpeed;


        //mThirdPersonCamera = new TPCTrack(transform, mPlayer);
        //mThirdPersonCamera = new TPCFollowTrackPosition(transform, mPlayer);
        //mThirdPersonCamera = new TPCFollowTrackPositionAndRotation(transform, mPlayer);
        //mThirdPersonCamera = new TPCTopDown(transform, mPlayer);

        mThirdPersonCameraDict.Add(CameraType.Track, new TPCTrack(transform, mPlayer));
        mThirdPersonCameraDict.Add(CameraType.Follow_Track_Pos, new TPCFollowTrackPosition(transform, mPlayer));
        mThirdPersonCameraDict.Add(CameraType.Follow_Track_Pos_Rot, new TPCFollowTrackPositionAndRotation(transform, mPlayer));
        mThirdPersonCameraDict.Add(CameraType.Topdown, new TPCTopDown(transform, mPlayer));


        // We instantiate and add the new third-person camera to the dictionary
#if UNITY_STANDALONE
        mThirdPersonCameraDict.Add(CameraType.Follow_Independent, new TPCFollowIndependentRotation(transform, mPlayer));
#endif
#if UNITY_ANDROID
        mThirdPersonCameraDict.Add(CameraType.Follow_Independent, new TPCFollowIndependentRotation(transform, mPlayer, mTouchField));
#endif

        mThirdPersonCamera = mThirdPersonCameraDict[mCameraType];

    }

    private void Update()
    {
        // Update the game constant parameters every frame 
        // so that changes applied on the editor can be reflected
        CameraConstants.Damping = mDamping;
        //CameraConstants.CameraPositionOffset = mPositionOffset;
        CameraConstants.CameraAngleOffset = mAngleOffset;
        CameraConstants.MinPitch = mMinPitch;
        CameraConstants.MaxPitch = mMaxPitch;
        CameraConstants.RotationSpeed = mRotationSpeed;

        mThirdPersonCamera = mThirdPersonCameraDict[mCameraType];
    }

    void LateUpdate()
    {
        mThirdPersonCamera.Update();
        RepositionCamera();
    }

    void RepositionCamera()
    {
        // Calculates the desired position of the camera relative to the player
        // Positioned behind the player based on the player's forward direction and offset vertically
        Vector3 desiredPosition = player.position - player.forward * distanceBehindPlayer + Vector3.up * verticalOffset;

        RaycastHit hit;
        // Direction from the player to the desired camera position
        Vector3 directionToDesiredPosition = (desiredPosition - player.position).normalized;
        // Distance between the player and the desired camera position
        float distanceToDesiredPosition = Vector3.Distance(player.position, desiredPosition);

        // Check if there is an obstacle between the player and camera
        if (Physics.SphereCast(player.position, sphereRadius, directionToDesiredPosition, out hit, distanceToDesiredPosition, obstacleLayer))
        {
            // Adjust the camera to avoid clipping into the obstacle
            // New position is offset slightly away from the hitpoint based on the sphere's radius
            Vector3 hitPosition = hit.point + hit.normal * sphereRadius;

            // Maintain the vertical component of the desired position while adjusting the horizontal position to avoid the obstacle
            desiredPosition = new Vector3(hitPosition.x, desiredPosition.y, hitPosition.z);
        }

        // Interpolation to smoothly transit the camera's current position to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, mDamping * Time.deltaTime);
    }
}
