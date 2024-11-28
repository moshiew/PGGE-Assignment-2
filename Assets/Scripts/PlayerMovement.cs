using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public CharacterController mCharacterController;
    public Animator mAnimator;

    public float mWalkSpeed = 1.5f;
    public float mRotationSpeed = 50.0f;
    public bool mFollowCameraForward = false;
    public float mTurnRate = 10.0f;

    public AudioSource audioSource;
    public AudioClip[] metalClips;
    public AudioClip[] dirtClips;
    public AudioClip[] sandClips;
    public AudioClip jumpSfx;
    public AudioClip groanSFX;
    public AudioClip dancesfx;
    public AudioClip deathSFX;
    public float stepRate = 0.65f;
    public float nextStepRate = 0.35f;
    public bool isMoving;
    public bool isDamaged;
    public bool isDancing = false;
    private string currentGroundTag = "Ground";

#if UNITY_ANDROID
    public FixedJoystick mJoystick;
#endif

    private float hInput;
    private float vInput;
    public float speed;
    private bool jump = false;
    private bool crouch = false;
    public float mGravity = -30.0f;
    public float mJumpHeight = 1.0f;

    private Vector3 mVelocity = new Vector3(0.0f, 0.0f, 0.0f);

    void Start()
    {
        mCharacterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleInputs();
        Move();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    public void HandleInputs()
    {
        // We shall handle our inputs here.
    #if UNITY_STANDALONE
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
    #endif

    #if UNITY_ANDROID
        hInput = 2.0f * mJoystick.Horizontal;
        vInput = 2.0f * mJoystick.Vertical;
    #endif

        speed = mWalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = mWalkSpeed * 2.0f;
            stepRate = 0.4f;
        }
        else
        {
            stepRate = 0.65f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jump = false;
        }

        if (Input.GetKey(KeyCode.LeftControl) && crouch == false)
        {
            crouch = !crouch;
            Crouch();
            Debug.Log("Crouch: " + crouch);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(deathSFX);
            mAnimator.SetTrigger("Die");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && isDancing == false)
        {
            isDancing = true;
            mAnimator.SetBool("Dance", true);
            StartCoroutine(PlayDanceSound());
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            mAnimator.SetTrigger("Headbutt");
        }

        isMoving = (vInput != 0);

        if (isMoving && Time.time >= nextStepRate)
        {
            PlayGroundSound();
            nextStepRate = Time.time + stepRate;
        }

    }
    IEnumerator PlayDanceSound()
    {
        audioSource.clip = dancesfx;
        audioSource.volume = 0.5f;
        audioSource.Play();

        yield return new WaitForSeconds(13.0f);

        if (isDancing) // If dancing is true, stop dancing
        {
            StopDance();
        }
    }

    void StopDance()
    {
        isDancing = false;
        mAnimator.SetBool("Dance", false);
        audioSource.Stop();
    }

    public void Move()
    {
        if (crouch) return;

        // We shall apply movement to the game object here.
        if (mAnimator == null) return;
        if (mFollowCameraForward)
        {
            // rotate Player towards the camera forward.
            Vector3 eu = Camera.main.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.Euler(0.0f, eu.y, 0.0f),
                mTurnRate * Time.deltaTime);
        }
        else
        {
            transform.Rotate(0.0f, hInput * mRotationSpeed * Time.deltaTime, 0.0f);
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward).normalized;
        forward.y = 0.0f;

        Vector3 movement = forward * vInput* speed * Time.deltaTime;
        movement.y = mVelocity.y * Time.deltaTime;

        mCharacterController.Move(movement);
        
        mAnimator.SetFloat("PosX", 0);
        mAnimator.SetFloat("PosZ", vInput * speed / (2.0f * mWalkSpeed));

        if(jump)
        {
            Jump();
            jump = false;
        }
    }

    void Jump()
    {
        audioSource.PlayOneShot(jumpSfx);
        audioSource.PlayOneShot(groanSFX);
        audioSource.volume = 0.5f;
        mAnimator.SetTrigger("Jump");
        mVelocity.y -= Mathf.Sqrt(mJumpHeight * -2f * mGravity);
    }

    private Vector3 HalfHeight;
    private Vector3 tempHeight;
    void Crouch()
    {
        mAnimator.SetBool("Crouch", crouch);
        if (crouch)
        {
            tempHeight = CameraConstants.CameraPositionOffset;
            HalfHeight = tempHeight;
            HalfHeight.y *= 0.5f;
            CameraConstants.CameraPositionOffset = HalfHeight;
            Debug.Log("Player Crouching!");
        }
        else
        {
            CameraConstants.CameraPositionOffset = tempHeight;
            Debug.Log("Player NOT Crouching!");
        }
    }

    void ApplyGravity()
    {
        // apply gravity.
        mVelocity.y += mGravity * Time.deltaTime;
        if (mCharacterController.isGrounded && mVelocity.y < 0)
            mVelocity.y = 0f;

        mCharacterController.Move(mVelocity * Time.deltaTime);
    }

    public void ResetCharacter()
    {
        mAnimator.SetTrigger("Reset");
    }

    public void Quit()
    {
        Application.Quit();
    }

    void PlayGroundSound()
    {
        if (!audioSource || dirtClips.Length == 0) return;
        if (!audioSource || sandClips.Length == 0) return;
        if (!audioSource || metalClips.Length == 0) return;

        AudioClip stepClip = null;

        switch (currentGroundTag)
        {
            case "Dirt":
                stepClip = dirtClips[Random.Range(0, dirtClips.Length)];
                Debug.Log(currentGroundTag);
                break;

            case "Sand":
                stepClip = sandClips[Random.Range(0, sandClips.Length)];
                Debug.Log(currentGroundTag);
                break;

            case "Ground":
                stepClip = metalClips[Random.Range(0, metalClips.Length)];
                Debug.Log(currentGroundTag);
                break;
        }

        if (stepClip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.volume = Random.Range(0.5f, 0.8f);
            audioSource.PlayOneShot(stepClip);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Update ground tag on collision
        currentGroundTag = hit.collider.tag;
    }

}
