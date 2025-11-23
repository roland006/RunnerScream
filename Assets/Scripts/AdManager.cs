using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Unity.Services.LevelPlay;

public class AdManager : MonoBehaviour
{
    private string _placementId = "F008E07BEB7457EF";
    private string _rewardId = "27FE990FE2FC3E4E";
    private LevelPlayRewardedAd _rewardedAd;
    [SerializeField] private string _adUnitId = "your_ad_unit_id"; // Ваш Ad Unit ID

    void Start()
    {
        // Подписываемся на успешную инициализацию LevelPlay
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        
        // Инициализируем LevelPlay с указанием форматов рекламы
        /*LevelPlayAdFormat[] adFormats = { LevelPlayAdFormat.REWARDED };
        LevelPlay.Init("your_app_key_here", adFormats: adFormats);*/
        
        
    }

    private void OnLevelPlayInitialized(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay инициализирован!");
        
        // Создаем экземпляр rewarded ad после инициализации
        _rewardedAd = new LevelPlayRewardedAd(_adUnitId);
        
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
        if (_rewardedAd != null && _rewardedAd.IsAdReady())
        {
            Debug.Log("Показываем рекламу...");
            // Сообщить PlayFab о начале показа
            ReportAdActivity(AdActivity.Start);
            
            // Показать рекламу через LevelPlayRewardedAd
            _rewardedAd.ShowAd();
        }
        else
        {
            Debug.Log("Реклама не готова");
            LoadRewardedAd(); // Попробовать загрузить снова
        }
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

    private void OnAdDisplayFailed(LevelPlayAdInfo adInfo,LevelPlayAdError error)
    {
        Debug.LogError($"Ошибка показа рекламы: {error.ErrorCode}");
    }

    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log($"Реклама просмотрена до конца. Награда: {reward.Amount} {reward.Name}");
        
        // Сообщить PlayFab о завершении
        ReportAdActivity(AdActivity.End);
        
        // Выдать награду
        RewardPlayer();
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
            PlacementId = _placementId,
            RewardId = _rewardId,
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
            PlacementId = _placementId,
            RewardId = _rewardId
        };

        PlayFabClientAPI.RewardAdActivity(request,
            result => {
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