using UnityEngine;

public class CoinManager : MonoBehaviour
{
    // Clave para guardar las Lumi-Coins en PlayerPrefs
    private const string LUMI_COINS_KEY = "LumiCoins";
    
    // Propiedad para leer/escribir la cantidad de coins
    public static int CurrentCoins
    {
        get
        {
            return PlayerPrefs.GetInt(LUMI_COINS_KEY, 0);
        }
        private set
        {
            PlayerPrefs.SetInt(LUMI_COINS_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Suma la cantidad indicada de Lumi-Coins al total y las guarda.
    /// </summary>
    public static void AddCoins(int amount)
    {
        int newTotal = CurrentCoins + amount;
        CurrentCoins = newTotal;
        Debug.Log("Lumi-Coins añadidas: " + amount + ", Total: " + newTotal);
    }

    /// <summary>
    /// Método para restablecer las Lumi-Coins a cero (si lo deseas).
    /// </summary>
    public static void ResetCoins()
    {
        CurrentCoins = 0;
        Debug.Log("Lumi-Coins restablecidas a 0.");
    }
}
