using UnityEngine;
using Currency;

public class MenuCurrencyUtils : MonoBehaviour
{
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
        PlayFabCurrency.AddCurrency("HC", amountToAdd);
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