using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinAmountText;

    private void OnEnable()
    {
        GameEvents.OnCoinUpdated += UpdateCoinAmount;
        UpdateCoinAmount();
    }
    private void OnDisable()
    {
        GameEvents.OnCoinUpdated -= UpdateCoinAmount;
    }

    public void UpdateCoinAmount()
    {
        coinAmountText.text = GamePrefs.FormatCoin(GamePrefs.Coin);
    }
}
