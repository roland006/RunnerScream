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
        [SerializeField] private GameObject MenuPanel;
        [SerializeField] private TMP_InputField Textbox;

        void Start()
        {
            // Убедись, что TitleId задан
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = "10C871"; // Подставь сюда свой ID
            }

            _customId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(_customId))
            {
                _customId = "Player_" + System.Guid.NewGuid().ToString();
            }

            Debug.Log("CustomId: " + _customId); // Для отладки
            LoginWithCustomID();
            
            // Register event listeners
            LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
        
            // Initialize the SDK with your App Key
            LevelPlay.Init("24583f3d5");



            PlayFabClientAPI.GetAdPlacements(new GetAdPlacementsRequest(),
                result =>
                {
                    foreach (var placement in result.AdPlacements)
                    {
                        Debug.Log($"Название: {placement.PlacementName}, ID: {placement.PlacementId}");
                    }
                },
                error => Debug.LogError(error.GenerateErrorReport()));
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
                LoadMenu();
            }
        }

        private void OnLoginFailure(PlayFabError error)
        {
            Debug.LogError("Ошибка входа: " + error.GenerateErrorReport());
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
            LoadMenu();
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
            MenuPanel.SetActive(true);
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