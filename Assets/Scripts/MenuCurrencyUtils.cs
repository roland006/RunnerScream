using UnityEngine;
using Currency;
using Unity.Services.LevelPlay;

public class MenuCurrencyUtils : MonoBehaviour
{
    [SerializeField] private AdManager adManager;

    void Start()
    {
        PlayFabCurrency.GetCurrencyBalance();
    }

    public void AddSoftCurrency(int amountToAdd)
    {
        PlayFabCurrency.AddCurrency("SC", amountToAdd);
    }

    public void AddHardCurrency(int amountToAdd)
    {
        adManager.ShowRewardedAd();
    }

    public void SubtractSoftCurrency(int amountToSubstract)
    {
        PlayFabCurrency.SafeSubtractCurrency("SC", amountToSubstract);
    }

    public void SubtractHardCurrency(int amountToSubstract)
    {
        PlayFabCurrency.SafeSubtractCurrency("HC", amountToSubstract);
    }
}