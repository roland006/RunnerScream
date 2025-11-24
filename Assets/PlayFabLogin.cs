using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.LevelPlay;

namespace DataBase
{
    public class PlayFabLogin : MonoBehaviour
    {
        private string _customId;
        [SerializeField] private GameObject CreateNicknamePanel;
        [SerializeField] private TMP_InputField Textbox;

        private string StringPlacementId = "F008E07BEB7457EF"; // это Placement ID из строки браузера в Playfab. Про начисление награды
        private string StringAppId = "24583f3d5"; // это AppKey из LevelPlay, про подключение рекламы
        private string StringTitleId = "10C871"; // это TitleId из плейфаба. Обозначающий айдишник игры

        void Start()
        {
            _customId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(_customId))
            {
                _customId = "Player_" + System.Guid.NewGuid().ToString();
            }
        
            LoginWithCustomID();
        }

        void LoginWithCustomID()
        {
            var request = new LoginWithCustomIDRequest
            {
                CustomId = _customId,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true // Запрашиваем профиль игрока при входе
                }
            };

            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }

        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Успешный вход! PlayFabId: " + result.PlayFabId);
            string displayName = null;
            // Пытаемся получить ник из данных профиля
            if (result.InfoResultPayload?.PlayerProfile != null)
            {
                displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
            }

            // Проверяем, новый ли игрок (оба условия)
            bool isNewlyCreated = result.NewlyCreated == true;
            bool hasDisplayName = !string.IsNullOrEmpty(displayName);

            if (isNewlyCreated || !hasDisplayName)
            {
                Debug.Log("Привет, новичок! Твой постоянный ник еще не задан.");
                CreateNicknamePanel.SetActive(true);
            }
            else
            {
                // Игрок не новый, ник уже есть -> загружаем его
                Debug.Log("С возвращением, " + displayName + "!");
                InitAds();
            }
        }

        private void OnLoginFailure(PlayFabError error)
        {
            Debug.LogError("Ошибка входа: " + error.GenerateErrorReport());
        }

        void InitAds()
        {
            // Убедись, что TitleId задан
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = StringTitleId;
            }

            // Register event listeners
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

            // Initialize the SDK with your App Key
            LevelPlay.Init(StringAppId);

            // Вызов метода для получения конкретного размещения
            GetSpecificAdPlacement();
    
            LoadMenu();
        }

        public void GetSpecificAdPlacement()
        {
            var request = new GetAdPlacementsRequest
            {
                AppId = StringAppId,
                Identifier = new NameIdentifier()
                {
                    Id = StringPlacementId
                }
            };

            PlayFabClientAPI.GetAdPlacements(request,
                result => {
                    if (result.AdPlacements.Count > 0)
                    {
                        var placement = result.AdPlacements[0];
                        Debug.Log($"Найдено размещение: {placement.PlacementName}");
                        // Здесь можно сохранить PlacementId и RewardId для дальнейшего использования
                    }
                    else
                    {
                        Debug.Log("Размещение не найдено");
                    }
                },
                error => Debug.LogError(error.GenerateErrorReport())
            );
        }

        public void SetDisplayNameForNewPlayer()
        {
            if (Textbox.text != null)
            {
                var request = new UpdateUserTitleDisplayNameRequest
                {
                    DisplayName = Textbox.text
                };
                PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdateSuccess,
                    OnDisplayNameUpdateError);
            }
            else
            {
                Debug.Log("Ник не может быть пустым");
            }
        }

        void OnDisplayNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log("Ник успешно установлен: " + result.DisplayName);
            InitAds();
        }

        void OnDisplayNameUpdateError(PlayFabError error)
        {
            Debug.LogError("Не удалось установить ник: " + error.GenerateErrorReport());

            // Обрабатываем ошибку, например, если ник занят
            if (error.Error == PlayFabErrorCode.NameNotAvailable)
            {
                Debug.LogError("Этот ник уже занят, выбери другой.");
                // Показать сообщение игроку в UI
            }
        }

        void LoadMenu()
        {
            CreateNicknamePanel.SetActive(false);
            SceneManager.LoadScene(1);
        }


        private void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
        {
            Debug.Log("LevelPlay SDK initialized successfully.");
            // You can now also use the 'config' object if needed.
        }

        // Ensure the error handler also has the correct signature
        private void SdkInitializationFailedEvent(LevelPlayInitError error)
        {
            Debug.LogError("LevelPlay SDK initialization failed: " + error);
        }
    }
}