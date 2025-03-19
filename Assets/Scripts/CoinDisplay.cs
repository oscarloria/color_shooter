using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    void Update()
    {
        if (coinText != null)
        {
            coinText.text = "Lumi-Coins: " + CoinManager.CurrentCoins;
        }
    }
}
