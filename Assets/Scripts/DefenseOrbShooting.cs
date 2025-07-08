using UnityEngine;
using System.Collections;
using TMPro;

public class DefenseOrbShooting : MonoBehaviour
{
    [Header("Prefabs de Orbes")]
    public GameObject defenseOrbPrefab;
    public GameObject orbRedPrefab;
    public GameObject orbBluePrefab;
    public GameObject orbGreenPrefab;
    public GameObject orbYellowPrefab;

    [Header("Valores por Defecto")]
    [SerializeField] private int defaultMagazineSize = 4;
    [SerializeField] private float defaultReloadTime = 2f;
    [SerializeField] private int defaultOrbDurability = 3;
    
    [Header("Configuración General")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;
    public float fireRate = 0.2f;
    
    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public WeaponReloadIndicator reloadIndicator;
    
    [Header("Animaciones")]
    public ShipBodyOrbsIdle8Directions orbsIdleScript;
    public ShipBodyOrbsAttack8Directions orbsAttackScript;
    public float orbsAttackAnimationDuration = 0.5f;
    
    // --- Variables de estado y mejoras ---
    [HideInInspector] public int magazineSize;
    [HideInInspector] public float reloadTime;
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;
    public Color currentColor = Color.white;
    private float nextFireTime = 0f;
    private bool isPlayingOrbsAttackAnim = false;
    
    // --- Claves de PlayerPrefs ---
    private const string ORBS_DURABILITY_KEY = "Orbs_Durability";
    private const string ORBS_MAG_KEY = "Orbs_Magazine";
    private const string ORBS_RELOAD_KEY = "Orbs_ReloadTime";

    void Start()
    {
        // Cargar mejoras desde PlayerPrefs o usar valores por defecto
        magazineSize = PlayerPrefs.GetInt(ORBS_MAG_KEY, defaultMagazineSize);
        reloadTime = PlayerPrefs.GetFloat(ORBS_RELOAD_KEY, defaultReloadTime);
        
        currentAmmo = magazineSize;
        UpdateAmmoText();
    }
    
    public void ShootOrb()
    {
        if (currentColor == Color.white || isReloading || currentAmmo <= 0 || Time.time < nextFireTime)
        {
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 direction = (mouseWorldPos - transform.position).normalized;
        float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 spawnPosition = transform.position + (direction * orbitRadius);

        GameObject chosenPrefab = null;
        if (currentColor == Color.red) chosenPrefab = orbRedPrefab;
        else if (currentColor == Color.blue) chosenPrefab = orbBluePrefab;
        else if (currentColor == Color.green) chosenPrefab = orbGreenPrefab;
        else if (currentColor == Color.yellow) chosenPrefab = orbYellowPrefab;
        if (chosenPrefab == null) chosenPrefab = defenseOrbPrefab;

        GameObject orbObj = Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);

        if (orbObj.TryGetComponent(out DefenseOrb newOrb))
        {
            newOrb.currentAngle = newAngle;
            newOrb.orbitRadius = orbitRadius;
            newOrb.orbitSpeed = -orbitSpeed;
            newOrb.orbColor = currentColor;
            newOrb.durability = PlayerPrefs.GetInt(ORBS_DURABILITY_KEY, defaultOrbDurability);
        }
        
        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoText();
        StartCoroutine(PlayOrbsAttackAnimation());
    }
    
    public IEnumerator Reload()
    {
        if (currentAmmo == magazineSize || isReloading) yield break;

        isReloading = true;
        UpdateAmmoText();
        if (reloadIndicator != null) reloadIndicator.ResetIndicator();

        float reloadTimer = 0f;
        while (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
            if (reloadIndicator != null) reloadIndicator.UpdateIndicator(reloadTimer / reloadTime);
            yield return null;
        }

        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoText();
        if (reloadIndicator != null) reloadIndicator.ResetIndicator();
    }

    IEnumerator PlayOrbsAttackAnimation()
    {
        if (isPlayingOrbsAttackAnim) yield break;
        isPlayingOrbsAttackAnim = true;
        
        if (orbsIdleScript != null) orbsIdleScript.enabled = false;
        if (orbsAttackScript != null) orbsAttackScript.enabled = true;

        yield return new WaitForSeconds(orbsAttackAnimationDuration);

        if (orbsAttackScript != null) orbsAttackScript.enabled = false;
        if (orbsIdleScript != null) orbsIdleScript.enabled = true;
        
        isPlayingOrbsAttackAnim = false;
    }

    private void UpdateAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isReloading ? "Orbe: RELOADING" : $"Orbe: {currentAmmo}/{magazineSize}";
    }

    public void UpdateCurrentColor()
    {
        if (Input.GetKey(KeyCode.W)) SetCurrentColor(Color.yellow);
        else if (Input.GetKey(KeyCode.A)) SetCurrentColor(Color.blue);
        else if (Input.GetKey(KeyCode.S)) SetCurrentColor(Color.green);
        else if (Input.GetKey(KeyCode.D)) SetCurrentColor(Color.red);
        else SetCurrentColor(Color.white);
    }

    void SetCurrentColor(Color color)
    {
        currentColor = color;
    }
}