using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    public float shakeDuration = 0.2f; // Duración del efecto
    public float shakeMagnitude = 0.3f; // Intensidad del efecto

    private Transform camTransform;
    private Vector3 originalPos;

    void Awake()
    {
        // Singleton para acceder desde otros scripts
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        camTransform = GetComponent<Transform>();
        originalPos = camTransform.localPosition;
    }

    public void ShakeCamera()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            camTransform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        camTransform.localPosition = originalPos;
    }
}