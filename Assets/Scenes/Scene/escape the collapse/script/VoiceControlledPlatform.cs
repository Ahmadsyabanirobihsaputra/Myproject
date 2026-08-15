
using UnityEngine;

public class VoiceControlledPlatform : MonoBehaviour
{
    // =========================================================
    // PLAYER DETECTION
    // =========================================================

    [Header("Player Detection")]

    [Tooltip("Player that must be nearby to control the platform.")]
    public Transform player;

    [Tooltip("Maximum distance at which the player can control the platform.")]
    public float activationRange = 5f;


    // =========================================================
    // PUSH TO TALK
    // =========================================================

    [Header("Push To Talk")]

    [Tooltip("Key used to activate/deactivate voice control.")]
    public KeyCode pushToTalkKey = KeyCode.V;

    [Tooltip("Current voice control state.")]
    public bool voiceControlActive = false;


    // =========================================================
    // MICROPHONE
    // =========================================================

    [Header("Microphone Settings")]

    [Tooltip("Microphone device to use. Leave empty to use the default microphone.")]
    public string microphoneDevice = "";

    [Tooltip("Number of audio samples used to calculate volume.")]
    [Min(32)]
    public int sampleWindow = 128;

    [Tooltip("Multiplies the raw microphone volume.")]
    [Range(0.1f, 20f)]
    public float volumeMultiplier = 10f;

    [Tooltip("Ignore microphone volume below this value.")]
    [Range(0f, 1f)]
    public float noiseFloor = 0.02f;


    // =========================================================
    // VOLUME SMOOTHING
    // =========================================================

    [Header("Volume Smoothing")]

    [Tooltip("Higher values make the volume react faster.")]
    [Range(1f, 30f)]
    public float smoothingSpeed = 10f;


    // =========================================================
    // SCREAM DETECTION
    // =========================================================

    [Header("Scream Detection")]

    [Tooltip("Volume required to START detecting a scream.")]
    [Range(0f, 1f)]
    public float screamStartThreshold = 0.20f;

    [Tooltip("Volume required to STOP detecting a scream.")]
    [Range(0f, 1f)]
    public float screamStopThreshold = 0.10f;

    [Tooltip("How long the player must scream before the platform starts moving.")]
    [Min(0f)]
    public float requiredScreamTime = 0.15f;

    [Tooltip("How long the voice must stay below the stop threshold before the platform starts lowering.")]
    [Min(0f)]
    public float stopScreamTime = 0.30f;


    // =========================================================
    // PLATFORM MOVEMENT
    // =========================================================

    [Header("Platform Movement")]

    [Tooltip("How fast the platform moves vertically.")]
    public float moveSpeed = 2f;

    [Tooltip("Lowest local Y position relative to the starting position.")]
    public float minHeight = 0f;

    [Tooltip("Highest local Y position relative to the starting position.")]
    public float maxHeight = 8f;


    // =========================================================
    // DEBUG
    // =========================================================

    [Header("Debug")]

    [Tooltip("Raw microphone volume before filtering.")]
    [Range(0f, 1f)]
    public float rawVolume;

    [Tooltip("Volume after noise filtering and multiplier.")]
    [Range(0f, 1f)]
    public float processedVolume;

    [Tooltip("Smoothed microphone volume.")]
    [Range(0f, 1f)]
    public float currentVolume;

    [Tooltip("Shows whether the player is close enough.")]
    public bool playerNearby;

    [Tooltip("Shows whether voice control is active.")]
    public bool voiceActive;

    [Tooltip("Shows whether the platform currently considers the player screaming.")]
    public bool screamDetected;

    [Tooltip("Time the current scream has been detected.")]
    public float screamTimer;

    [Tooltip("Time the current silence has been detected.")]
    public float silenceTimer;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private AudioClip microphoneClip;

    private float startY;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        startY = transform.localPosition.y;

        FindPlayer();

        StartMicrophone();
    }


    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            Debug.Log(
                "[VoiceControlledPlatform] Player found: "
                + player.name
            );
        }
        else
        {
            Debug.LogWarning(
                "[VoiceControlledPlatform] Player with tag 'Player' not found!"
            );
        }
    }


    // =========================================================
    // MICROPHONE
    // =========================================================

    private void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning(
                "[VoiceControlledPlatform] No microphone detected!"
            );

            return;
        }

        if (string.IsNullOrEmpty(microphoneDevice))
        {
            microphoneDevice = Microphone.devices[0];
        }

        microphoneClip = Microphone.Start(
            microphoneDevice,
            true,
            1,
            44100
        );

        Debug.Log(
            "[VoiceControlledPlatform] Microphone started: "
            + microphoneDevice
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        CheckPushToTalk();

        CheckPlayerDistance();

        voiceActive =
            playerNearby &&
            voiceControlActive;


        // -----------------------------------------------------
        // PLAYER NOT NEARBY
        // -----------------------------------------------------

        if (!playerNearby)
        {
            ResetVoiceDetection();

            MovePlatformDown();

            return;
        }


        // -----------------------------------------------------
        // VOICE CONTROL OFF
        // -----------------------------------------------------

        if (!voiceControlActive)
        {
            ResetVoiceDetection();

            MovePlatformDown();

            return;
        }


        // -----------------------------------------------------
        // MICROPHONE
        // -----------------------------------------------------

        if (microphoneClip == null)
            return;


        // Get raw microphone volume
        rawVolume = GetMicrophoneVolume();


        // Process volume
        processedVolume =
            ProcessVolume(rawVolume);


        // Smooth volume
        currentVolume = Mathf.Lerp(
            currentVolume,
            processedVolume,
            smoothingSpeed * Time.deltaTime
        );


        // -----------------------------------------------------
        // SCREAM DETECTION
        // -----------------------------------------------------

        DetectScream();


        // -----------------------------------------------------
        // PLATFORM MOVEMENT
        // -----------------------------------------------------

        if (screamDetected)
        {
            MovePlatformUp();
        }
        else
        {
            MovePlatformDown();
        }
    }


    // =========================================================
    // PUSH TO TALK
    // =========================================================

    private void CheckPushToTalk()
    {
        if (Input.GetKeyDown(pushToTalkKey))
        {
            voiceControlActive =
                !voiceControlActive;

            Debug.Log(
                "[VoiceControlledPlatform] Voice Control: "
                + (voiceControlActive
                    ? "ON"
                    : "OFF")
            );

            ResetVoiceDetection();
        }
    }


    // =========================================================
    // PLAYER DISTANCE
    // =========================================================

    private void CheckPlayerDistance()
    {
        if (player == null)
        {
            playerNearby = false;
            return;
        }

        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );

        playerNearby =
            distance <= activationRange;
    }


    // =========================================================
    // VOLUME PROCESSING
    // =========================================================

    private float ProcessVolume(float volume)
    {
        // Remove quiet background noise
        if (volume <= noiseFloor)
        {
            return 0f;
        }

        // Remove noise floor
        float adjustedVolume =
            volume - noiseFloor;

        // Apply sensitivity multiplier
        adjustedVolume *= volumeMultiplier;

        return Mathf.Clamp01(adjustedVolume);
    }


    // =========================================================
    // SCREAM DETECTION
    // =========================================================

    private void DetectScream()
    {
        // -----------------------------------------------------
        // ALREADY SCREAMING
        // -----------------------------------------------------

        if (screamDetected)
        {
            // Voice is still loud enough
            if (currentVolume >= screamStopThreshold)
            {
                silenceTimer = 0f;

                return;
            }

            // Voice has dropped below stop threshold
            silenceTimer += Time.deltaTime;

            if (silenceTimer >= stopScreamTime)
            {
                screamDetected = false;

                screamTimer = 0f;
                silenceTimer = 0f;
            }

            return;
        }


        // -----------------------------------------------------
        // NOT CURRENTLY SCREAMING
        // -----------------------------------------------------

        if (currentVolume >= screamStartThreshold)
        {
            screamTimer += Time.deltaTime;

            silenceTimer = 0f;

            if (screamTimer >= requiredScreamTime)
            {
                screamDetected = true;

                screamTimer = 0f;

                Debug.Log(
                    "[VoiceControlledPlatform] SCREAM DETECTED"
                );
            }
        }
        else
        {
            screamTimer = 0f;
        }
    }


    // =========================================================
    // RESET DETECTION
    // =========================================================

    private void ResetVoiceDetection()
    {
        screamDetected = false;

        screamTimer = 0f;

        silenceTimer = 0f;

        currentVolume = 0f;

        processedVolume = 0f;
    }


    // =========================================================
    // MICROPHONE VOLUME
    // =========================================================

    private float GetMicrophoneVolume()
    {
        if (microphoneClip == null)
            return 0f;

        int microphonePosition =
            Microphone.GetPosition(
                microphoneDevice
            );

        if (microphonePosition < sampleWindow)
            return 0f;

        float[] samples =
            new float[sampleWindow];

        microphoneClip.GetData(
            samples,
            microphonePosition - sampleWindow
        );

        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rms =
            Mathf.Sqrt(
                sum / samples.Length
            );

        return Mathf.Clamp01(
            rms * 10f
        );
    }


    // =========================================================
    // MOVE PLATFORM UP
    // =========================================================

    private void MovePlatformUp()
    {
        Vector3 position =
            transform.localPosition;

        float targetY =
            startY + maxHeight;

        position.y =
            Mathf.MoveTowards(
                position.y,
                targetY,
                moveSpeed * Time.deltaTime
            );

        transform.localPosition =
            position;
    }


    // =========================================================
    // MOVE PLATFORM DOWN
    // =========================================================

    private void MovePlatformDown()
    {
        Vector3 position =
            transform.localPosition;

        float targetY =
            startY + minHeight;

        position.y =
            Mathf.MoveTowards(
                position.y,
                targetY,
                moveSpeed * Time.deltaTime
            );

        transform.localPosition =
            position;
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            activationRange
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(microphoneDevice))
        {
            if (Microphone.IsRecording(
                microphoneDevice))
            {
                Microphone.End(
                    microphoneDevice
                );
            }
        }
    }
}