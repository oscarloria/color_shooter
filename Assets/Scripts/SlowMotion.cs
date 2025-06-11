using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering; // ¡NUEVO! Necesario para Volumes
using UnityEngine.Rendering.Universal; // ¡NUEVO! Necesario para los efectos de URP

public class SlowMotion : MonoBehaviour
{
    [Header("Configuración")]
    public float slowMotionDuration = 5f;
    public float slowMotionScale = 0.4f;
    public float chargePerEnemy = 0.05f;

    [Header("UI")]
    public Image slowMotionBar;
    
    // --- NUEVO: Referencias para Feedback Visual ---
    [Header("Feedback Visual (Opcional)")]
    [Tooltip("Arrastra aquí el objeto 'Global Volume' de tu escena.")]
    public Volume postProcessVolume;
    [Tooltip("Qué tan rápido aparecen y desaparecen los efectos visuales.")]
    public float effectFadeSpeed = 2f;
    
    // --- NUEVO: Referencias a los efectos específicos ---
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    // Propiedades de estado
    private float remainingSlowMotionTime;
    private bool isSlowMotionActive = false;
    private Coroutine runningCoroutine;
    private Coroutine visualFeedbackCoroutine; // Corrutina para el feedback

    void Start()
    {
        remainingSlowMotionTime = slowMotionDuration;
        UpdateSlowMotionBarUI();
        
        // --- NUEVO: Inicializar los efectos ---
        // Buscamos los efectos en el perfil del Volume que asignamos.
        if (postProcessVolume != null) 
        {
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            
            // Asegurarse de que los efectos están desactivados al empezar
            if(vignette) vignette.active = false;
            if(chromaticAberration) chromaticAberration.active = false;
        }
    }

    public void Toggle()
    {
        if (isSlowMotionActive)
        {
            StopEffect();
        }
        else if (remainingSlowMotionTime > 0)
        {
            StartEffect();
        }
    }

    public void AddSlowMotionCharge()
    {
        if (isSlowMotionActive) return;
        remainingSlowMotionTime = Mathf.Min(remainingSlowMotionTime + (slowMotionDuration * chargePerEnemy), slowMotionDuration);
        UpdateSlowMotionBarUI();
    }
    
    private void StartEffect()
    {
        if (isSlowMotionActive) return;
        isSlowMotionActive = true;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        runningCoroutine = StartCoroutine(ConsumeCharge());
        
        // --- NUEVO: Activar el feedback visual ---
        if (visualFeedbackCoroutine != null) StopCoroutine(visualFeedbackCoroutine);
        visualFeedbackCoroutine = StartCoroutine(FadeEffects(true)); // Fade IN
    }

    private void StopEffect()
    {
        if (!isSlowMotionActive) return;
        isSlowMotionActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }
        
        // --- NUEVO: Desactivar el feedback visual ---
        if (visualFeedbackCoroutine != null) StopCoroutine(visualFeedbackCoroutine);
        visualFeedbackCoroutine = StartCoroutine(FadeEffects(false)); // Fade OUT
    }

    private IEnumerator ConsumeCharge()
    {
        while (remainingSlowMotionTime > 0f)
        {
            remainingSlowMotionTime -= Time.unscaledDeltaTime;
            UpdateSlowMotionBarUI();
            yield return null;
        }
        StopEffect();
    }
    
    private void UpdateSlowMotionBarUI()
    {
        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration;
        }
    }

    // --- NUEVO: Corrutina para el fundido de los efectos ---
    private IEnumerator FadeEffects(bool fadeIn)
    {
        // Si no tenemos los componentes, no hacemos nada.
        if (vignette == null || chromaticAberration == null) yield break;

        // Activar los componentes para que sean modificables
        vignette.active = true;
        chromaticAberration.active = true;

        float timer = 0f;
        
        // Definir valores iniciales y finales
        float startVignette = vignette.intensity.value;
        float startAberration = chromaticAberration.intensity.value;
        float endVignette = fadeIn ? 0.4f : 0f;
        float endAberration = fadeIn ? 0.5f : 0f;

        // Duración del fundido
        float fadeDuration = 1f / effectFadeSpeed;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            // Interpolar suavemente los valores
            vignette.intensity.value = Mathf.Lerp(startVignette, endVignette, t);
            chromaticAberration.intensity.value = Mathf.Lerp(startAberration, endAberration, t);

            yield return null;
        }

        // Asegurar que los valores finales son exactos
        vignette.intensity.value = endVignette;
        chromaticAberration.intensity.value = endAberration;

        // Si estamos en fade out, desactivar los componentes al final para optimizar
        if (!fadeIn)
        {
            vignette.active = false;
            chromaticAberration.active = false;
        }
        
        visualFeedbackCoroutine = null;
    }
}