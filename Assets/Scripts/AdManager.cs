using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Unity.Services.LevelPlay;

public class AdManager : MonoBehaviour
{
    private string StringPlacementId = "F008E07BEB7457EF"; // это Placement ID из ссылки URL в Playfab. 
    private string StringRewardId = "27FE990FE2FC3E4E"; // награда в Placement из ссылки URL в Playfab
    private string StringAdUnitId = "1mnkp5jrqnmrrjfd"; // ID Ad Unit из levelPlay -> Settings
    private string StringAppId = "24583f3d5"; // это AppKey из LevelPlay, про подключение рекламы

    private LevelPlayRewardedAd _rewardedAd;

    void Start()
    {
        // Подписываемся на успешную инициализацию LevelPlay
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
    }

    private void OnLevelPlayInitialized(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay инициализирован!");

        // Создаем экземпляр rewarded ad после инициализации
        _rewardedAd = new LevelPlayRewardedAd(StringAdUnitId);

        // Подписываемся на события LevelPlayRewardedAd
        RegisterRewardedAdEvents();

        // Загружаем рекламу
        LoadRewardedAd();
    }

    private void RegisterRewardedAdEvents()
    {
        _rewardedAd.OnAdLoaded += OnAdLoaded;
        _rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        _rewardedAd.OnAdDisplayed += OnAdDisplayed;
        _rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        _rewardedAd.OnAdRewarded += OnAdRewarded;
        _rewardedAd.OnAdClosed += OnAdClosed;
    }

    // Метод для показа рекламы по нажатию кнопки
    public void ShowRewardedAd()
    {
        Debug.Log("1. Кнопка нажата, вызов метода ShowRewardedAd.");

        if (_rewardedAd != null && _rewardedAd.IsAdReady())
        {
            Debug.Log("2. Реклама готова, начинаем показ.");
            ReportAdActivity(AdActivity.Start);
            _rewardedAd.ShowAd();
        }
        else
        {
            Debug.Log("2. ОШИБКА: Реклама не готова (IsAdReady вернул false).");
            // Здесь можно добавить логику повторной загрузки рекламы
            LoadRewardedAd();
        }
    }

// В обработчике успешного просмотра (OnAdRewarded)
    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("4. Событие OnAdRewarded: реклама досмотрена, можно выдавать награду.");
        ReportAdActivity(AdActivity.End);
        RewardPlayer();
    }

    // Метод для загрузки рекламы
    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.LoadAd();
        }
    }

    // ===== ОБРАБОТЧИКИ СОБЫТИЙ LEVELPLAYREWARDEDAD =====

    private void OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Реклама загружена и готова к показу");
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Ошибка загрузки рекламы: {error.ErrorCode}");
    }

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Реклама открыта");
        ReportAdActivity(AdActivity.Opened);
    }

    private void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Ошибка показа рекламы: {error.ErrorCode}");
    }


    private void OnAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Реклама закрыта");
        ReportAdActivity(AdActivity.Closed);

        // Перезагружаем рекламу для следующего показа
        LoadRewardedAd();
    }

    // ===== МЕТОДЫ PLAYFAB (остаются без изменений) =====

    private void ReportAdActivity(AdActivity activity)
    {
        var request = new ReportAdActivityRequest
        {
            PlacementId = StringPlacementId,
            RewardId = StringRewardId,
            Activity = activity
        };
        PlayFabClientAPI.ReportAdActivity(request, OnReportSuccess, OnReportError);
    }

    private void OnReportSuccess(ReportAdActivityResult result)
    {
        Debug.Log("Активность успешно отчетена");
    }

    private void OnReportError(PlayFabError error)
    {
        Debug.LogError($"Ошибка отчета: {error.GenerateErrorReport()}");
    }

    public void RewardPlayer()
    {
        var request = new RewardAdActivityRequest
        {
            PlacementId = StringPlacementId,
            RewardId = StringRewardId
        };

        PlayFabClientAPI.RewardAdActivity(request,
            result =>
            {
                Debug.Log("Игрок получил награду!");
                // Обнови UI с валютой
                Currency.PlayFabCurrency.GetCurrencyBalance(); // Обновляем баланс
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // Отписываемся от событий при уничтожении объекта
    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;

        if (_rewardedAd != null)
        {
            _rewardedAd.OnAdLoaded -= OnAdLoaded;
            _rewardedAd.OnAdLoadFailed -= OnAdLoadFailed;
            _rewardedAd.OnAdDisplayed -= OnAdDisplayed;
            _rewardedAd.OnAdDisplayFailed -= OnAdDisplayFailed;
            _rewardedAd.OnAdRewarded -= OnAdRewarded;
            _rewardedAd.OnAdClosed -= OnAdClosed;

            _rewardedAd.Dispose();
        }
    }
}