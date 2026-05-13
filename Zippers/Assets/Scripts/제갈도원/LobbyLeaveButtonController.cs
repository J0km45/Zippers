using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyLeaveButtonController : MonoBehaviour
{
    [SerializeField] private Button _leaveButton;

    private bool _isLeaving;

    public static LobbyLeaveButtonController EnsureInScene()
    {
        LobbyLeaveButtonController existing = FindObjectOfType<LobbyLeaveButtonController>(true);
        if (existing != null) return existing;

        Button leaveButton = FindLeaveButton();
        if (leaveButton == null) return null;

        LobbyLeaveButtonController controller = leaveButton.gameObject.AddComponent<LobbyLeaveButtonController>();
        controller._leaveButton = leaveButton;
        return controller;
    }

    private void Awake()
    {
        if (_leaveButton == null)
        {
            _leaveButton = FindLeaveButton();
        }
    }

    private void OnEnable()
    {
        if (_leaveButton != null)
        {
            _leaveButton.onClick.AddListener(OnLeaveClicked);
        }
    }

    private void OnDisable()
    {
        if (_leaveButton != null)
        {
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }
    }

    private async void OnLeaveClicked()
    {
        if (_isLeaving) return;
        _isLeaving = true;

        if (_leaveButton != null)
        {
            _leaveButton.interactable = false;
        }

        if (LobbyManager.Instance != null && LobbyManager.Instance.CurrentSession != null)
        {
            await LobbyManager.Instance.LeaveSessionAsync();
            return;
        }

        SceneLoader.LoadLocal(SceneId.RoomList);
    }

    private static Button FindLeaveButton()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid()) continue;

            if (IsLeaveButtonName(button.gameObject.name) || HasLeaveButtonText(button.transform))
            {
                return button;
            }
        }

        return null;
    }

    private static bool IsLeaveButtonName(string objectName)
    {
        return objectName.Contains("Leave")
               || objectName.Contains("Exit")
               || objectName.Contains("Back")
               || objectName.Contains("나가기");
    }

    private static bool HasLeaveButtonText(Transform buttonRoot)
    {
        TMP_Text[] texts = buttonRoot.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            string text = texts[i].text;
            if (string.IsNullOrEmpty(text)) continue;

            if (text.Contains("나가기")
                || text.Contains("Leave")
                || text.Contains("Exit")
                || text.Contains("Back"))
            {
                return true;
            }
        }

        return false;
    }
}
