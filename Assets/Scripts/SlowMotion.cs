using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlowMotion : MonoBehaviour
{
    public float slowMotionDuration = 5f;
    public float slowMotionScale = 0.4f;
    public float chargePerEnemy = 0.05f;
    public Image slowMotionBar;

    [HideInInspector] public float remainingSlowMotionTime;
    [HideInInspector] public bool isSlowMotionActive = false;

    void Start()
    {
        remainingSlowMotionTime = slowMotionDuration;
        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = 1f;
        }
    }

    public void ActivateSlowMotion()
    {
        if (remainingSlowMotionTime <= 0f || isSlowMotionActive) return;

        isSlowMotionActive = true;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        StartCoroutine(UpdateSlowMotionBarWhileActive());
    }

    public void DeactivateSlowMotion()
    {
        if (!isSlowMotionActive) return;

        isSlowMotionActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    IEnumerator UpdateSlowMotionBarWhileActive()
    {
        while (remainingSlowMotionTime > 0f && isSlowMotionActive)
        {
            remainingSlowMotionTime -= Time.unscaledDeltaTime;
            if (slowMotionBar != null)
            {
                slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration;
            }
            yield return null;
        }

        if (remainingSlowMotionTime <= 0f)
        {
            DeactivateSlowMotion();
        }
    }

    public void AddSlowMotionCharge()
    {
        remainingSlowMotionTime = Mathf.Min(remainingSlowMotionTime + (slowMotionDuration * chargePerEnemy), slowMotionDuration);
        if (slowMotionBar != null)
        {
            slowMotionBar.fillAmount = remainingSlowMotionTime / slowMotionDuration;
        }
    }

    public void PauseSlowMotion()
    {
        if (!isSlowMotionActive) return;

        isSlowMotionActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}