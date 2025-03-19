using UnityEngine;

public class EnemyCoinDrop : MonoBehaviour
{
    [Header("Lumi-Coins Drop Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.5f;
    public int coinAmount = 1;

    [Header("Visual Effect")]
    [Tooltip("Prefab que se instanciará cuando se suelte la moneda.")]
    public GameObject lumiCoinPrefab;

    public void TryDropCoins()
    {
        float roll = Random.value; // valor entre 0..1
        if (roll <= dropChance)
        {
            // Sumar Lumi-Coins al total
            CoinManager.AddCoins(coinAmount);

            // Instanciar el prefab de la moneda como efecto visual
            if (lumiCoinPrefab != null)
            {
                // Instanciar en la posición del enemigo (o donde lo desees)
                GameObject coinObj = Instantiate(lumiCoinPrefab, transform.position, Quaternion.identity);
                // El script LumiCoinFly en el prefab se encargará de moverlo hacia el jugador
            }
        }
    }
}
