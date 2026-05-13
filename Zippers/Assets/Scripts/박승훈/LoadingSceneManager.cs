using UnityEngine;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [Space(3)] [Header("UI 컴포넌트")]
    [SerializeField] private LoadingPanel _loadingPanel;
    [SerializeField] private ChangeSceneController _changeSceneController;
    [Space(3)] [Header("데이터 컴포넌트")]
    [SerializeField] private DataManager _dataManager;
    [SerializeField] private DataLoadController _dataLoadController;

    private void Awake()
        => Init();
        
    private void OnEnable()
    {
        _changeSceneController.OnChangeScene += _dataManager.DataLoad;
    }

    private void Start()
    {
        LocalDataAccess.Instance.Game.OnReady += _changeSceneController.OnExitScene;
        LocalDataAccess.Instance.Game.OnReady += _loadingPanel.OnProceedLoading;

        _loadingPanel.TotalProgrss = _dataManager.PendingSHeetCount;
        
        _loadingPanel.PrintImage();
        _changeSceneController.OnEnterScene();
        
    }

    private void OnDisable()
    {
        if (LocalDataAccess.Instance != null)
        {
            LocalDataAccess.Instance.Game.OnReady -= _changeSceneController.OnExitScene;
            LocalDataAccess.Instance.Game.OnReady -= _loadingPanel.OnProceedLoading;
        }
        _changeSceneController.OnChangeScene -= _dataManager.DataLoad;
    }

    private void Init()
    {
        _loadingPanel = GetComponentInChildren<LoadingPanel>();
        if(_loadingPanel == null)
            DebugTool.Warning("로딩 판넬 컴포넌트가 없습니다.", DebugType.Missing);
        
        _changeSceneController = GetComponentInChildren<ChangeSceneController>();
        if (_changeSceneController == null)
            DebugTool.Warning("씬 전환 컨트롤러 컴포넌트가 없습니다.", DebugType.Missing);
        
        if (_dataLoadController == null)
            DebugTool.Warning("데이터 로드 컨트롤러 컴포넌트가 없습니다.", DebugType.Missing);
        
        if(_dataManager == null)
            DebugTool.Warning("데이터 로드 매니저 컴포넌트가 없습니다.", DebugType.Missing);
    }
}