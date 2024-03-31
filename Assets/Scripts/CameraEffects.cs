using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public static CameraEffects ins;
    public Camera camera;
    public Vector3 originalCameraPosition;
    public Vector3 shakeOffset = Vector3.zero;
    public Vector3 bounceOffset = Vector3.zero;

    void Awake()
    {
        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (this != ins)
        {
            Destroy(gameObject);
        }

        originalCameraPosition = camera.transform.localPosition;
    }

    public void ShakeCamera(float duration, float intensity)
    {
        StartCoroutine(Shake(duration, intensity));
    }

    IEnumerator Shake(float duration, float intensity)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            shakeOffset = new Vector3(x, 0, 0);
            UpdateCameraPosition();
            elapsed += Time.deltaTime;

            yield return null;
        }

        shakeOffset = Vector3.zero;
        UpdateCameraPosition();
    }

    public void BounceCamera(float height, float speed)
    {
        StartCoroutine(Bounce(height, speed));
    }

    IEnumerator Bounce(float height, float speed)
    {
        float elapsedTime = 0f;
        float halfDuration = height / speed;
        float quarterDuration = halfDuration / 4;
        while (elapsedTime < quarterDuration)
        {
            bounceOffset = Vector3.Lerp(Vector3.zero, new Vector3(0, height, 0), (elapsedTime / quarterDuration));
            UpdateCameraPosition();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        // Start descent with cubic ease out
        while (elapsedTime < quarterDuration)
        {
            // Cubic ease out calculation
            float progress = elapsedTime / halfDuration;
            float cubicEaseOut = 1 - Mathf.Pow(1 - progress, 3);
        
            bounceOffset = Vector3.Lerp(new Vector3(0, height, 0), Vector3.zero, cubicEaseOut);
            UpdateCameraPosition();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bounceOffset = Vector3.zero;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Apply both offsets to the camera's position
        camera.transform.localPosition = originalCameraPosition + shakeOffset + bounceOffset;
    }
}