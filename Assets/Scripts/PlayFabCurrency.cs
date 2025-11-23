using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Currency
{
    public class PlayFabCurrency : MonoBehaviour
    {
        // Получить баланс валют
        public static void GetCurrencyBalance()
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                result =>
                {
                    foreach (var currency in result.VirtualCurrency)
                    {
                        Debug.Log($"Валюта {currency.Key}: {currency.Value}");
                    }
                },
                error => Debug.LogError(error.GenerateErrorReport())
            );
        }
        public static void AddCurrency(string currencyCode, int amount)
        {
            var request = new AddUserVirtualCurrencyRequest
            {
                VirtualCurrency = currencyCode, 
                Amount = amount
            };

            PlayFabClientAPI.AddUserVirtualCurrency(request,
                result => { Debug.Log($"Добавлено {amount} {currencyCode}. Новый баланс: {result.Balance}"); },
                error => Debug.LogError(error.GenerateErrorReport())
            );
        }
        
        // 1. Метод для проверки и списания валюты
        public static void SafeSubtractCurrency(string currencyCode, int amountToSubtract)
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                getInventoryResult => {
                    // Проверяем, есть ли у игрока такая валюта и достаточно ли средств
                    if (getInventoryResult.VirtualCurrency.TryGetValue(currencyCode, out int currentBalance))
                    {
                        if (currentBalance >= amountToSubtract)
                        {
                            // Если средств достаточно, выполняем списание
                            SubtractCurrency(currencyCode, amountToSubtract);
                        }
                        else
                        {
                            Debug.LogError($"Недостаточно средств. Текущий баланс {currencyCode}: {currentBalance}, требуется: {amountToSubtract}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Валюта {currencyCode} не найдена в инвентаре игрока.");
                    }
                },
                error => {
                    Debug.LogError("Ошибка при получении инвентаря: " + error.GenerateErrorReport());
                }
            );
        }

        // 2. Метод для непосредственного списания валюты
        private static void SubtractCurrency(string currencyCode, int amount)
        {
            var subtractRequest = new SubtractUserVirtualCurrencyRequest
            {
                VirtualCurrency = currencyCode,
                Amount = amount
            };

            PlayFabClientAPI.SubtractUserVirtualCurrency(subtractRequest,
                subtractResult => {
                    Debug.Log($"Списание успешно. Новый баланс {currencyCode}: {subtractResult.Balance}");
                },
                subtractError => {
                    Debug.LogError("Ошибка при списании валюты: " + subtractError.GenerateErrorReport());
                }
            );
        }
    }
}