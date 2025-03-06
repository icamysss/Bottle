using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField] private float shakeIntensity = 0f;  
    [SerializeField] private float shakeDuration = 0f;  

    private float shakeTimer;
    private CinemachineBasicMultiChannelPerlin noiseProfile;

    private void Start()
    {
        noiseProfile = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera()
    {
        noiseProfile.m_AmplitudeGain = shakeIntensity;

        shakeTimer = shakeDuration;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            noiseProfile.m_AmplitudeGain = Mathf.Lerp(shakeIntensity, 0f, 1f - (shakeTimer / shakeDuration));
        }
    }
}
