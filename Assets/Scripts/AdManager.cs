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
        _rewardedAd = new LevelPlayRewardedAd(StringAdUnitId);
        RegisterRewardedAdEvents();
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
        if (_rewardedAd != null)
        {
            _rewardedAd.LoadAd();
        }
    }
    
    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("реклама досмотрена, можно выдавать награду.");
        ReportAdActivity(AdActivity.End);
        RewardPlayer();
    }

    private void OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        if (_rewardedAd != null && _rewardedAd.IsAdReady())
        {
            Debug.Log("2. Реклама готова, начинаем показ.");
            ReportAdActivity(AdActivity.Start);
            _rewardedAd.ShowAd();
        }
        else
        {
            Debug.Log("2. ОШИБКА: Реклама не готова (IsAdReady вернул false).");
        }
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"❌ Ошибка загрузки рекламы: {error.AdUnitId}");
        Debug.LogError($"Код ошибки: {error.ErrorCode}");
        Debug.LogError($"Детали: {error.ErrorMessage}");
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
    }
    
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
        Debug.Log("Удачно запущено");
    }

    private void OnReportError(PlayFabError error)
    {
        Debug.LogError($"Ошибка: {error.GenerateErrorReport()}");
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
                
                Currency.PlayFabCurrency.GetCurrencyBalance();
            },
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    // Отписываемся от событий при уничтожении объекта
    void OnDestroy()
    {
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