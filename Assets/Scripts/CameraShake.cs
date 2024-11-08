using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    // Variables para el efecto de shake
    public float shakeDuration = 0.2f; // Duración del efecto de shake
    public float shakeMagnitude = 0.3f; // Magnitud del efecto de shake

    // Variables para el efecto de retroceso
    public float recoilDistance = 0.1f; // Distancia del retroceso
    public float recoilSpeed = 10f;     // Velocidad del retroceso

    private Coroutine recoilCoroutine;

    void Awake()
    {
        // Implementación del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShakeCamera()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    // Método corregido para el retroceso de cámara
    public void RecoilCamera(Vector3 recoilDirection)
    {
        // Si ya hay una corrutina de retroceso en ejecución, la detenemos
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }
        recoilCoroutine = StartCoroutine(Recoil(recoilDirection));
    }

    IEnumerator Recoil(Vector3 recoilDirection)
    {
        // Usar la posición actual de la cámara al iniciar el retroceso
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = startPosition - recoilDirection.normalized * recoilDistance;
        float elapsedTime = 0f;
        float recoilDuration = recoilDistance / recoilSpeed;

        // Movimiento hacia atrás (retroceso)
        while (elapsedTime < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;

        // Retorno a la posición original
        elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(targetPosition, startPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = startPosition;
        recoilCoroutine = null;
    }
}