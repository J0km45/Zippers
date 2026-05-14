using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 조인 코드로 특정 방에 바로 참여하는 모달 다이얼로그.
/// 코드는 호스트가 방 생성 후 ISession.Code 로 받음 (RoomUI 등에서 표시 후 공유).
/// </summary>
public class JoinByCodeDialogController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private TMP_Text _warningText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private bool _isProcessing;

    public static JoinByCodeDialogController EnsureInScene()
    {
        JoinByCodeDialogController existing = FindObjectOfType<JoinByCodeDialogController>(true);
        if (existing != null)
        {
            existing.AutoWireMissingReferences();
            return existing;
        }

        Button confirmButton = FindButtonByName("CodeEnterButton", "EnterCodeButton", "JoinCodeButton");
        if (confirmButton == null) return null;

        GameObject host = FindDialogRoot(confirmButton.transform);
        JoinByCodeDialogController controller = host.AddComponent<JoinByCodeDialogController>();
        controller._panel = host;
        controller._confirmButton = confirmButton;
        controller.AutoWireMissingReferences();
        return controller;
    }

    private void Awake()
    {
        AutoWireMissingReferences();
    }

    private void OnEnable()
    {
        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (_confirmButton != null) _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        if (_cancelButton != null) _cancelButton.onClick.RemoveListener(Close);
    }

    public void Open()
    {
        if (_panel != null) _panel.SetActive(true);
        ResetFields();
    }

    public void Close()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void ResetFields()
    {
        if (_codeInput != null) _codeInput.text = string.Empty;
        SetWarning(string.Empty);
        _isProcessing = false;
        if (_confirmButton != null) _confirmButton.interactable = true;
    }

    private async void OnConfirmClicked()
    {
        if (_isProcessing) return;

        string code = _codeInput != null ? _codeInput.text?.Trim().ToUpperInvariant() : null;
        if (string.IsNullOrEmpty(code))
        {
            SetWarning("조인 코드를 입력하세요");
            return;
        }
        if (LobbyManager.Instance == null)
        {
            DebugTool.Error("LobbyManager.Instance 가 null - 씬에 LobbyManager 가 없습니다.", DebugType.Network, this);
            SetWarning("로비 매니저 미초기화");
            return;
        }

        _isProcessing = true;
        if (_confirmButton != null) _confirmButton.interactable = false;
        SetWarning("참여 중...");

        DebugTool.Log($"초대 코드 입장 시도: {code}", DebugType.Network);
        bool success = await LobbyManager.Instance.JoinSessionByCodeAsync(code);
        DebugTool.Log($"초대 코드 입장 결과: {(success ? "성공" : "실패")} / code={code}", DebugType.Network);

        if (this == null) return;

        _isProcessing = false;
        if (success)
        {
            Close();
        }
        else
        {
            if (_confirmButton != null) _confirmButton.interactable = true;
            SetWarning("참여 실패. 코드를 확인하세요.");
        }
    }

    private void SetWarning(string message)
    {
        if (_warningText != null) _warningText.text = message;
    }

    private void AutoWireMissingReferences()
    {
        if (_confirmButton == null)
        {
            _confirmButton = FindButtonByName("CodeEnterButton", "EnterCodeButton", "JoinCodeButton");
        }

        Transform searchRoot = _confirmButton != null ? FindDialogRoot(_confirmButton.transform).transform : transform;

        if (_panel == null && searchRoot != null)
        {
            _panel = searchRoot.gameObject;
        }

        if (_codeInput == null && searchRoot != null)
        {
            TMP_InputField[] inputs = searchRoot.GetComponentsInChildren<TMP_InputField>(true);
            if (inputs.Length > 0)
            {
                _codeInput = inputs[0];
            }
        }

        if (_warningText == null && searchRoot != null)
        {
            _warningText = FindTextByName(searchRoot, "WarningText", "StatusText", "MessageText");
        }

        if (_cancelButton == null && searchRoot != null)
        {
            _cancelButton = FindButtonUnder(searchRoot, "CancelButton", "CloseButton", "BackButton");
        }
    }

    private static GameObject FindDialogRoot(Transform child)
    {
        Transform current = child;
        while (current != null)
        {
            if (current.name.Contains("Code") && (current.name.Contains("Panel") || current.name.Contains("Dialog")))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return child.parent != null ? child.parent.gameObject : child.gameObject;
    }

    private static Button FindButtonByName(params string[] names)
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;

            for (int j = 0; j < names.Length; j++)
            {
                if (button.gameObject.name == names[j])
                {
                    return button;
                }
            }
        }

        return null;
    }

    private static Button FindButtonUnder(Transform root, params string[] names)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null) continue;

            for (int j = 0; j < names.Length; j++)
            {
                if (button.gameObject.name == names[j])
                {
                    return button;
                }
            }
        }

        return null;
    }

    private static TMP_Text FindTextByName(Transform root, params string[] names)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null) continue;

            for (int j = 0; j < names.Length; j++)
            {
                if (text.gameObject.name == names[j])
                {
                    return text;
                }
            }
        }

        return null;
    }
}
