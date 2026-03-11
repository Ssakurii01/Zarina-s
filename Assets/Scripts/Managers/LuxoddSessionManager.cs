using UnityEngine;

#if LUXODD_PLUGIN
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
using Luxodd.Game.Scripts.Game.Leaderboard;
#endif

public class LuxoddSessionManager : MonoBehaviour
{
    public static LuxoddSessionManager Instance { get; private set; }

#if LUXODD_PLUGIN
    [Header("Luxodd Plugin References (drag from UnityPluginPrefab)")]
    [SerializeField] private WebSocketService _webSocketService;
    [SerializeField] private WebSocketCommandHandler _commandHandler;
    [SerializeField] private HealthStatusCheckService _healthCheckService;
#endif

#pragma warning disable CS0414, CS0067
    private string _playerName = "";
    private int _playerBalance = 0;
    private bool _isConnected = false;

    public delegate void PlayerInfoEvent(string name, int balance);
    public static event PlayerInfoEvent OnPlayerInfoReceived;
#pragma warning restore CS0414, CS0067

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
#if LUXODD_PLUGIN
        ConnectToServer();
#endif
    }

#if LUXODD_PLUGIN
    private void ConnectToServer()
    {
        if (_webSocketService == null) return;

        _webSocketService.ConnectToServer(
            onSuccessCallback: () =>
            {
                _isConnected = true;
                if (_healthCheckService != null)
                    _healthCheckService.Activate();
                GetPlayerProfile();
                GetPlayerBalance();
                SendLevelBegin(1);
            },
            onErrorCallback: () =>
            {
                Debug.LogError("Luxodd: Connection failed!");
            }
        );
    }

    private void GetPlayerProfile()
    {
        _commandHandler.SendProfileRequestCommand(
            onSuccessCallback: (name) =>
            {
                _playerName = name;
                OnPlayerInfoReceived?.Invoke(_playerName, _playerBalance);
            },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Profile failed [{code}]: {msg}")
        );
    }

    private void GetPlayerBalance()
    {
        _commandHandler.SendUserBalanceRequestCommand(
            onSuccessCallback: (credits) =>
            {
                _playerBalance = (int)credits;
                OnPlayerInfoReceived?.Invoke(_playerName, _playerBalance);
            },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Balance failed [{code}]: {msg}")
        );
    }
#endif

    public void SendLevelBegin(int level)
    {
#if LUXODD_PLUGIN
        if (!_isConnected) return;
        _commandHandler.SendLevelBeginRequestCommand(
            level: level,
            onSuccessCallback: () => { },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Level begin failed [{code}]: {msg}")
        );
#endif
    }

    public void SendLevelEnd(int level, int score)
    {
#if LUXODD_PLUGIN
        if (!_isConnected) return;
        _commandHandler.SendLevelEndRequestCommand(
            level: level,
            score: score,
            onSuccessCallback: () => { },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Level end failed [{code}]: {msg}")
        );
#endif
    }

    public void ShowContinuePopup()
    {
#if LUXODD_PLUGIN
        if (!_isConnected || _webSocketService == null) return;
        _webSocketService.SendSessionOptionContinue((action) =>
        {
            switch (action)
            {
                case SessionOptionAction.Continue:
                    GameManager.Instance?.RestartGame();
                    break;
                case SessionOptionAction.End:
                    BackToSystem();
                    break;
            }
        });
#endif
    }

    public void FetchLeaderboard()
    {
#if LUXODD_PLUGIN
        if (!_isConnected) return;
        _commandHandler.SendLeaderboardRequestCommand(
            onSuccessCallback: (response) => { },
            onFailureCallback: (code, msg) =>
                Debug.LogError($"Luxodd: Leaderboard failed [{code}]: {msg}")
        );
#endif
    }

    public void BackToSystem()
    {
#if LUXODD_PLUGIN
        if (_webSocketService != null)
            _webSocketService.BackToSystem();
#else
        GameManager.Instance?.EndSession();
#endif
    }
}
