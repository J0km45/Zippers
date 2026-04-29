using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Font = TMPro.TMP_FontAsset;
using FontStyle = TMPro.FontStyles;
using Text = TMPro.TextMeshProUGUI;

/// <summary>
/// Figma/Game View(1920×1080) 기준 싱글플레이어 UI를 <b>선택한 Canvas 하위</b>에 오브젝트로만 생성한다.
/// </summary>
public static class SinglePlayerMenuBuilder
{
    private const string RootName = "SinglePlayerMenu";
    private const string PrefabPath = "Assets/Prefabs/UI/SinglePlayerMenu.prefab";

    private static Sprite _builtinUiSprite;

    [MenuItem("Tools/Zippers/SinglePlayer UI (Canvas 하위 생성)", priority = 10)]
    public static void CreateUnderSelectedCanvas()
    {
        Canvas canvas = null;
        if (Selection.activeGameObject != null)
            canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "SinglePlayer UI",
                "Hierarchy에서 Canvas 또는 Canvas의 자식을 선택한 뒤 다시 실행하세요.",
                "확인");
            return;
        }

        var canvasRt = canvas.GetComponent<RectTransform>();
        Transform directChild = canvasRt.Find(RootName);
        if (directChild != null)
        {
            if (!EditorUtility.DisplayDialog(
                    "SinglePlayer UI",
                    $"캔버스 하위에 '{RootName}' 이(가) 이미 있습니다. 삭제하고 다시 만들까요?",
                    "삭제 후 생성",
                    "취소"))
                return;
            Undo.DestroyObjectImmediate(directChild.gameObject);
        }

        EnsureEventSystemIfMissing();
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/UI");

        Color bgMain = Color.white;
        Color card = Color.white;
        Color line = new Color(0.18f, 0.18f, 0.18f, 1f);
        Color iconMuted = new Color(0.96f, 0.96f, 0.96f, 1f);
        Color textMain = new Color(0.08f, 0.08f, 0.08f, 1f);
        Color textMuted = new Color(0.38f, 0.38f, 0.38f, 1f);
        Color starOn = new Color(1f, 0.86f, 0.05f, 1f);
        Color starOff = new Color(0.76f, 0.76f, 0.76f, 1f);
        Font font = GetUiFont();

        int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Create SinglePlayer UI Full");

        var root = CreateRect(RootName, canvasRt);
        root.SetAsLastSibling();
        Undo.RegisterCreatedObjectUndo(root.gameObject, "SinglePlayerMenu Root");
        StretchFull(root);

        var rootBg = root.gameObject.AddComponent<Image>();
        rootBg.raycastTarget = false;
        rootBg.color = bgMain;

        CreateFullMockup(root, font, card, iconMuted, line, textMain, textMuted, starOn, starOff);

        EnsureFolder("Assets/Prefabs/UI");
        var saved = PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);
        if (saved != null)
            Debug.Log($"[SinglePlayer UI] 프리팹 저장: {PrefabPath}");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undo);
        Selection.activeGameObject = root.gameObject;
        Debug.Log("[SinglePlayer UI] 전체 레이아웃 생성 완료.");
    }

    private static void CreateFullMockup(
        RectTransform root,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff)
    {
        var frame = CreateRect("FullPage", root);
        SetAnchor(frame, Vector2.zero, Vector2.one, new Vector2(24, 24), new Vector2(-24, -24));

        var frameBg = frame.gameObject.AddComponent<Image>();
        frameBg.color = card;
        frameBg.raycastTarget = false;
        AddOutline(frame.gameObject, line, 2f);

        var left = CreateRect("LeftHalf", frame);
        SetAnchor(left, new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
        CreateLeftHalfMockup(left, font, card, iconMuted, line, textMain, textMuted, starOn, starOff);

        var divider = CreateRect("CenterDivider", frame);
        SetAnchor(divider, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(-1f, 0f), new Vector2(1f, 0f));
        var dividerImage = divider.gameObject.AddComponent<Image>();
        dividerImage.color = line;
        dividerImage.raycastTarget = false;

        var right = CreateRect("RightHalf", frame);
        SetAnchor(right, new Vector2(0.5f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
        CreateRightHalfMockup(right, font, card, iconMuted, line, textMain, textMuted, starOn, starOff);

        CreateEscClose(frame, font, textMain);
    }

    private static void CreateLeftOnlyMockup(
        RectTransform root,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff)
    {
        var page = CreateRect("LeftPage", root);
        SetAnchor(page, Vector2.zero, Vector2.one, new Vector2(24, 24), new Vector2(-24, -24));

        var pageBorder = page.gameObject.AddComponent<Image>();
        pageBorder.color = card;
        pageBorder.raycastTarget = false;
        AddOutline(pageBorder.gameObject, line, 2f);

        var v = page.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(22, 22, 0, 0);
        v.spacing = 0f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        CreateWireHeader(page, "클래스 선택", font, line, textMain);
        CreateClassListPanel(page, font, card, iconMuted, line, textMain, textMuted, starOn, starOff);
        CreateWireHeader(page, "이어 하기", font, line, textMain);
        CreateContinuePanelLeft(page, font, card, iconMuted, line, textMain, textMuted);
    }

    private static void CreateLeftHalfMockup(
        RectTransform parent,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(0, 0, 0, 0);
        v.spacing = 0f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        CreateWireHeader(parent, "클래스 선택", font, line, textMain);
        CreateClassListPanel(parent, font, card, iconMuted, line, textMain, textMuted, starOn, starOff);
        CreateWireHeader(parent, "이어 하기", font, line, textMain);
        CreateContinuePanelLeft(parent, font, card, iconMuted, line, textMain, textMuted);
    }

    private static void CreateRightHalfMockup(
        RectTransform parent,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff)
    {
        var top = CreateRect("RightTopArea", parent);
        SetAnchor(top, new Vector2(0f, 0.16f), Vector2.one, new Vector2(28, 18), new Vector2(-28, -26));

        var h = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(0, 0, 0, 0);
        h.spacing = 20f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;

        var visualColumn = CreateRect("CharacterColumn", top);
        var visualLe = visualColumn.gameObject.AddComponent<LayoutElement>();
        visualLe.flexibleWidth = 1.05f;
        visualLe.minWidth = 360f;
        CreateCharacterColumn(visualColumn, font, card, line, textMain, textMuted);

        var detailColumn = CreateRect("DetailColumn", top);
        var detailLe = detailColumn.gameObject.AddComponent<LayoutElement>();
        detailLe.flexibleWidth = 0.95f;
        detailLe.minWidth = 330f;
        CreateDetailColumn(detailColumn, font, card, iconMuted, line, textMain, textMuted, starOn, starOff);

        var newStart = CreateRect("NewStartButton", parent);
        SetAnchor(newStart, new Vector2(0.34f, 0.025f), new Vector2(0.66f, 0.15f), Vector2.zero, Vector2.zero);
        var newStartBg = newStart.gameObject.AddComponent<Image>();
        newStartBg.color = Color.white;
        AddOutline(newStart.gameObject, line, 2f);
        var newStartButton = newStart.gameObject.AddComponent<Button>();
        newStartButton.targetGraphic = newStartBg;
        newStartButton.navigation = new Navigation { mode = Navigation.Mode.None };
        var newStartText = CreateText("Label", newStart, font, 40, FontStyle.Normal, TextAnchor.MiddleCenter, textMain);
        StretchFull(newStartText.rectTransform);
        newStartText.text = "새로 시작";

    }

    private static void CreateEscClose(RectTransform parent, Font font, Color textMain)
    {
        var esc = CreateText("EscClose", parent, font, 16, FontStyle.Normal, TextAnchor.MiddleRight, textMain);
        SetAnchor(esc.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-132f, 3f), new Vector2(-6f, 27f));
        esc.text = "[ESC] CLOSE";
    }

    private static void CreateCharacterColumn(RectTransform parent, Font font, Color card, Color line, Color textMain, Color textMuted)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = 10f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var title = CreateText("RoleTitle", parent, font, 30, FontStyle.Normal, TextAnchor.MiddleLeft, textMain);
        title.gameObject.AddComponent<LayoutElement>().minHeight = 42f;
        title.text = "근접";

        var tags = CreateRect("TagRow", parent);
        tags.gameObject.AddComponent<LayoutElement>().minHeight = 34f;
        var tagH = tags.gameObject.AddComponent<HorizontalLayoutGroup>();
        tagH.spacing = 8f;
        tagH.childControlWidth = false;
        tagH.childControlHeight = true;
        tagH.childForceExpandWidth = false;
        string[] tagLabels = { "근접 전투", "전선 돌파", "군중 제어" };
        foreach (string tag in tagLabels)
            CreateTextChip(tags, tag, font, line, textMain, 92f);

        var illust = CreateRect("CharacterIllustration", parent);
        var illustLe = illust.gameObject.AddComponent<LayoutElement>();
        illustLe.flexibleHeight = 1f;
        illustLe.minHeight = 500f;
        var illustBg = illust.gameObject.AddComponent<Image>();
        illustBg.color = card;
        illustBg.raycastTarget = false;
        AddOutline(illust.gameObject, line, 2f);

        var illustText = CreateText("Placeholder", illust, font, 24, FontStyle.Normal, TextAnchor.MiddleCenter, textMuted);
        StretchFull(illustText.rectTransform);
        illustText.text = "캐릭터\n일러스트";
    }

    private static void CreateDetailColumn(
        RectTransform parent,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff)
    {
        var v = parent.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(0, 0, 56, 0);
        v.spacing = 0f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        var panel = CreateRect("DetailPanel", parent);
        panel.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
        var panelBg = panel.gameObject.AddComponent<Image>();
        panelBg.color = card;
        panelBg.raycastTarget = false;
        AddOutline(panel.gameObject, line, 2f);

        var stack = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        stack.padding = new RectOffset(12, 12, 12, 12);
        stack.spacing = 0f;
        stack.childControlWidth = true;
        stack.childControlHeight = true;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;

        CreateDetailHeader(panel, "기본 능력치", font, line, textMain, 28f);
        CreateDetailStatTable(panel, font, line, textMain, starOn, starOff);
        CreateDetailHeader(panel, "스킬", font, line, textMain, 26f);
        CreateSkillCards(panel, font, iconMuted, line, textMain, textMuted);
        CreateDetailHeader(panel, "플레이 스타일", font, line, textMain, 32f);
        CreateParagraphBox(panel, "최전선에서 적을 압도하는 돌파형 클래스입니다.\n다수의 좀비를 밀어내고 아군이 진격할 수 있는 길을 엽니다.\n위험을 감수하고 적진에 진입을 감당하세요.", font, line, textMain, 94f);
        CreateDetailHeader(panel, "장점 / 단점", font, line, textMain, 30f);
        CreateProsCons(panel, font, line, textMain);
    }

    private static void CreateDetailHeader(RectTransform parent, string label, Font font, Color line, Color textMain, float height)
    {
        var header = CreateRect("Header_" + label, parent);
        var le = header.gameObject.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
        var bg = header.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(header.gameObject, line, 1f);

        var text = CreateText("Label", header, font, 18, FontStyle.Normal, TextAnchor.MiddleLeft, textMain);
        SetAnchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
        text.text = label;
    }

    private static void CreateDetailStatTable(RectTransform parent, Font font, Color line, Color textMain, Color starOn, Color starOff)
    {
        var table = CreateRect("BasicStatTable", parent);
        table.gameObject.AddComponent<LayoutElement>().minHeight = 120f;
        var bg = table.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(table.gameObject, line, 1f);

        var v = table.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(16, 16, 8, 8);
        v.spacing = 0f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = true;

        string[] labels = { "생존", "전투", "기동력", "유틸" };
        int[] values = { 5, 3, 2, 2 };
        for (int i = 0; i < labels.Length; i++)
            CreateStatStarRow(table, labels[i], font, values[i], textMain, starOn, starOff);
    }

    private static void CreateSkillCards(RectTransform parent, Font font, Color iconMuted, Color line, Color textMain, Color textMuted)
    {
        var row = CreateRect("SkillCards", parent);
        row.gameObject.AddComponent<LayoutElement>().minHeight = 180f;
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 8f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;

        CreateSkillCard(row, "주무기", "무기 아이콘", "근접 무기로 전방의\n적을 빠르게 베어냅니다.", font, iconMuted, line, textMain, textMuted);
        CreateSkillCard(row, "액티브", "액티브 스킬\n아이콘", "전방으로 돌진하며\n적을 밀쳐냅니다.", font, iconMuted, line, textMain, textMuted);
        CreateSkillCard(row, "패시브", "패시브 스킬\n아이콘", "피해를 일부 감소하고\n버티는 힘이 증가합니다.", font, iconMuted, line, textMain, textMuted);
    }

    private static void CreateSkillCard(RectTransform parent, string title, string iconLabel, string desc, Font font, Color iconMuted, Color line, Color textMain, Color textMuted)
    {
        var card = CreateRect("Skill_" + title, parent);
        card.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var bg = card.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(card.gameObject, line, 1f);

        var v = card.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(8, 8, 8, 8);
        v.spacing = 6f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;

        CreateTextChip(card, title, font, line, textMain, 72f);
        CreateIconBox(card, iconLabel, font, 90f, iconMuted, line, textMain);
        var body = CreateText("Desc", card, font, 12, FontStyle.Normal, TextAnchor.UpperCenter, textMuted);
        body.gameObject.AddComponent<LayoutElement>().minHeight = 54f;
        body.text = desc;
    }

    private static void CreateParagraphBox(RectTransform parent, string body, Font font, Color line, Color textMain, float height)
    {
        var box = CreateRect("Paragraph", parent);
        var le = box.gameObject.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
        var bg = box.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(box.gameObject, line, 1f);

        var text = CreateText("Body", box, font, 15, FontStyle.Normal, TextAnchor.MiddleLeft, textMain);
        SetAnchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));
        text.text = body;
    }

    private static void CreateProsCons(RectTransform parent, Font font, Color line, Color textMain)
    {
        var row = CreateRect("ProsCons", parent);
        row.gameObject.AddComponent<LayoutElement>().minHeight = 96f;
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 0f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;

        CreateListBox(row, "장점", "• 빠른 근접 돌파\n• 방어 능력이 우수\n• 많은 전투에서 생존 우수", font, line, textMain);
        CreateListBox(row, "단점", "• 원거리 대응 능력 부족\n• 기동력이 상대적으로 낮음\n• 원거리 피해에 취약", font, line, textMain);
    }

    private static void CreateListBox(RectTransform parent, string title, string body, Font font, Color line, Color textMain)
    {
        var box = CreateRect(title, parent);
        box.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var bg = box.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(box.gameObject, line, 1f);

        var text = CreateText("Text", box, font, 13, FontStyle.Normal, TextAnchor.UpperLeft, textMain);
        SetAnchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));
        text.text = title + "\n" + body;
    }

    private static void CreateWireHeader(RectTransform parent, string label, Font font, Color line, Color textMain)
    {
        var header = CreateRect("Header_" + label, parent);
        var le = header.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 64f;
        le.preferredHeight = 64f;

        var box = header.gameObject.AddComponent<Image>();
        box.color = Color.white;
        box.raycastTarget = false;
        AddOutline(header.gameObject, line, 2f);

        var text = CreateText("Label", header, font, 28, FontStyle.Normal, TextAnchor.MiddleCenter, textMain);
        SetAnchor(text.rectTransform, new Vector2(0f, 0f), new Vector2(0.26f, 1f), Vector2.zero, Vector2.zero);
        text.text = label;
    }

    private static void CreateClassListPanel(
        RectTransform parent,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff)
    {
        var panel = CreateRect("ClassSelectionPanel", parent);
        var le = panel.gameObject.AddComponent<LayoutElement>();
        le.flexibleHeight = 1f;
        le.minHeight = 520f;

        var bg = panel.gameObject.AddComponent<Image>();
        bg.color = card;
        bg.raycastTarget = false;
        AddOutline(panel.gameObject, line, 2f);

        var v = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(18, 18, 18, 18);
        v.spacing = 18f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = true;

        string[] classNames = { "근접 클래스", "소총 클래스", "샷건 클래스", "유틸 클래스" };
        int[][] stats =
        {
            new[] { 5, 3, 2, 2 },
            new[] { 2, 4, 3, 3 },
            new[] { 3, 4, 2, 2 },
            new[] { 3, 2, 2, 5 }
        };

        for (int i = 0; i < classNames.Length; i++)
            CreateLeftClassRow(panel, classNames[i], font, card, iconMuted, line, textMain, textMuted, starOn, starOff, stats[i]);
    }

    private static void CreateLeftClassRow(
        RectTransform parent,
        string className,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted,
        Color starOn,
        Color starOff,
        int[] stats)
    {
        var row = CreateRect("ClassRow_" + className, parent);
        var rowLe = row.gameObject.AddComponent<LayoutElement>();
        rowLe.minHeight = 112f;
        rowLe.flexibleHeight = 1f;

        var bg = row.gameObject.AddComponent<Image>();
        bg.color = card;
        AddOutline(row.gameObject, line, 2f);

        var button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        button.transition = Selectable.Transition.ColorTint;
        var colors = ColorBlock.defaultColorBlock;
        colors.normalColor = card;
        colors.highlightedColor = new Color(0.95f, 0.97f, 1f, 1f);
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(18, 18, 14, 14);
        h.spacing = 18f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        h.childAlignment = TextAnchor.MiddleLeft;

        CreateIconBox(row, className + "\n아이콘", font, 112f, iconMuted, line, textMain);
        CreateIconBox(row, "주무기\n아이콘", font, 92f, iconMuted, line, textMain);
        CreateIconBox(row, "액티브 스킬\n아이콘", font, 92f, iconMuted, line, textMain);
        CreateIconBox(row, "패시브 스킬\n아이콘", font, 92f, iconMuted, line, textMain);
        CreateStatTable(row, font, line, textMuted, starOn, starOff, stats);
    }

    private static void CreateIconBox(RectTransform parent, string label, Font font, float width, Color fill, Color line, Color textMain)
    {
        var box = CreateRect("IconBox", parent);
        var le = box.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 76f;

        var bg = box.gameObject.AddComponent<Image>();
        bg.color = fill;
        bg.raycastTarget = false;
        AddOutline(box.gameObject, line, 1.5f);

        var text = CreateText("Label", box, font, 15, FontStyle.Normal, TextAnchor.MiddleCenter, textMain);
        StretchFull(text.rectTransform);
        text.text = label;
    }

    private static void CreateStatTable(RectTransform parent, Font font, Color line, Color textMuted, Color starOn, Color starOff, int[] stats)
    {
        var table = CreateRect("StatTable", parent);
        var le = table.gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minWidth = 230f;
        le.minHeight = 84f;

        var bg = table.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(table.gameObject, line, 1.5f);

        var v = table.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(6, 6, 4, 4);
        v.spacing = 0f;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = true;

        string[] labels = { "생존", "전투", "기동력", "유틸" };
        for (int i = 0; i < labels.Length; i++)
            CreateStatStarRow(table, labels[i], font, Mathf.Clamp(stats[i], 0, 5), textMuted, starOn, starOff);
    }

    private static void CreateStatStarRow(RectTransform parent, string label, Font font, int filled, Color textMuted, Color starOn, Color starOff)
    {
        var row = CreateRect("Stat_" + label, parent);
        var rowLe = row.gameObject.AddComponent<LayoutElement>();
        rowLe.minHeight = 18f;
        rowLe.flexibleHeight = 1f;

        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childAlignment = TextAnchor.MiddleLeft;

        var name = CreateText("Name", row, font, 14, FontStyle.Normal, TextAnchor.MiddleLeft, textMuted);
        var nameLe = name.gameObject.AddComponent<LayoutElement>();
        nameLe.minWidth = 54f;
        nameLe.preferredWidth = 54f;
        name.text = label;

        for (int i = 0; i < 5; i++)
        {
            var star = CreateText("Star", row, font, 18, FontStyle.Normal, TextAnchor.MiddleCenter, i < filled ? starOn : starOff);
            var starLe = star.gameObject.AddComponent<LayoutElement>();
            starLe.minWidth = 20f;
            starLe.preferredWidth = 20f;
            star.text = i < filled ? "★" : "☆";
        }
    }

    private static void CreateContinuePanelLeft(
        RectTransform parent,
        Font font,
        Color card,
        Color iconMuted,
        Color line,
        Color textMain,
        Color textMuted)
    {
        var panel = CreateRect("ContinuePanel", parent);
        var le = panel.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 178f;
        le.preferredHeight = 178f;

        var bg = panel.gameObject.AddComponent<Image>();
        bg.color = card;
        bg.raycastTarget = false;
        AddOutline(panel.gameObject, line, 2f);

        var h = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(28, 28, 28, 28);
        h.spacing = 28f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        h.childAlignment = TextAnchor.MiddleLeft;

        CreateIconBox(panel, "클래스\n아이콘", font, 132f, iconMuted, line, textMain);

        var saveInfo = CreateRect("SaveInfo", panel);
        var infoLe = saveInfo.gameObject.AddComponent<LayoutElement>();
        infoLe.minWidth = 430f;
        infoLe.flexibleWidth = 1f;

        var infoBg = saveInfo.gameObject.AddComponent<Image>();
        infoBg.color = Color.white;
        infoBg.raycastTarget = false;
        AddOutline(saveInfo.gameObject, line, 1.5f);

        var infoV = saveInfo.gameObject.AddComponent<VerticalLayoutGroup>();
        infoV.padding = new RectOffset(16, 16, 10, 10);
        infoV.spacing = 8f;
        infoV.childControlWidth = true;
        infoV.childControlHeight = true;
        infoV.childForceExpandWidth = true;
        infoV.childForceExpandHeight = false;

        var top = CreateRect("SaveTopLine", saveInfo);
        top.gameObject.AddComponent<LayoutElement>().minHeight = 28f;
        var topH = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        topH.spacing = 12f;
        topH.childControlWidth = false;
        topH.childControlHeight = true;
        CreateTextChip(top, "스테이지 : 1", font, line, textMain, 130f);
        CreateTextChip(top, "진행 : 3 / 7", font, line, textMain, 140f);

        CreateTextChip(saveInfo, "마지막 저장 : mm.dd 00:00", font, line, textMain, 260f);

        var metrics = CreateRect("Metrics", saveInfo);
        metrics.gameObject.AddComponent<LayoutElement>().minHeight = 30f;
        var metricH = metrics.gameObject.AddComponent<HorizontalLayoutGroup>();
        metricH.spacing = 8f;
        metricH.childControlWidth = false;
        metricH.childControlHeight = true;

        string[] values = { "68 %", "105", "42", "2" };
        foreach (string value in values)
            CreateMetricCell(metrics, value, font, line, textMain, textMuted);

        var start = CreateRect("GameStartButton", panel);
        var startLe = start.gameObject.AddComponent<LayoutElement>();
        startLe.minWidth = 160f;
        startLe.preferredWidth = 160f;

        var startBg = start.gameObject.AddComponent<Image>();
        startBg.color = Color.white;
        AddOutline(start.gameObject, line, 1.5f);

        var button = start.gameObject.AddComponent<Button>();
        button.targetGraphic = startBg;
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        var startText = CreateText("Label", start, font, 26, FontStyle.Normal, TextAnchor.MiddleCenter, textMain);
        StretchFull(startText.rectTransform);
        startText.text = "게임 시작";
    }

    private static void CreateTextChip(RectTransform parent, string label, Font font, Color line, Color textMain, float width)
    {
        var chip = CreateRect("Chip", parent);
        var le = chip.gameObject.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 28f;
        le.preferredHeight = 28f;

        var bg = chip.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(chip.gameObject, line, 1f);

        var text = CreateText("Label", chip, font, 15, FontStyle.Normal, TextAnchor.MiddleCenter, textMain);
        StretchFull(text.rectTransform);
        text.text = label;
    }

    private static void CreateMetricCell(RectTransform parent, string value, Font font, Color line, Color textMain, Color textMuted)
    {
        var cell = CreateRect("Metric", parent);
        var le = cell.gameObject.AddComponent<LayoutElement>();
        le.minWidth = 82f;
        le.preferredWidth = 82f;
        le.minHeight = 28f;

        var bg = cell.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        AddOutline(cell.gameObject, line, 1f);

        var icon = CreateText("Icon", cell, font, 9, FontStyle.Normal, TextAnchor.MiddleCenter, textMuted);
        SetAnchor(icon.rectTransform, Vector2.zero, new Vector2(0.32f, 1f), Vector2.zero, Vector2.zero);
        icon.text = "Icon";

        var text = CreateText("Value", cell, font, 18, FontStyle.Normal, TextAnchor.MiddleCenter, textMain);
        SetAnchor(text.rectTransform, new Vector2(0.3f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
        text.text = value;
    }

    private static void AddOutline(GameObject go, Color color, float distance)
    {
        var outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(distance, -distance);
        outline.useGraphicAlpha = false;
    }

    private static void CreateFooterCenterTextButton(RectTransform footer, string name, string label, Font font)
    {
        var btnRoot = CreateRect(name, footer);
        btnRoot.gameObject.AddComponent<LayoutElement>().minWidth = 160f;
        var clearImg = btnRoot.gameObject.AddComponent<Image>();
        clearImg.color = new Color(1f, 1f, 1f, 0.001f);
        clearImg.raycastTarget = true;
        var btn = btnRoot.gameObject.AddComponent<Button>();
        btn.targetGraphic = clearImg;
        btn.transition = Selectable.Transition.ColorTint;
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.95f, 0.95f, 1f, 1f);
        btn.colors = cb;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var txt = CreateText("Label", btnRoot, font, 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.2f, 0.45f, 0.85f, 1f));
        StretchFull(txt.rectTransform);
        txt.text = label;
    }

    private static void CreateClassRowFigma(RectTransform parent, string name, Font font, Color card, Color pillC, Color on, Color off, int[] fourStats)
    {
        var row = CreateRect(name, parent);
        row.gameObject.AddComponent<LayoutElement>().minHeight = 128f;
        var rowBg = row.gameObject.AddComponent<Image>();
        rowBg.color = card;
        var btn = row.gameObject.AddComponent<Button>();
        btn.targetGraphic = rowBg;
        btn.transition = Selectable.Transition.ColorTint;
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = card;
        cb.highlightedColor = new Color(0.94f, 0.97f, 1f, 1f);
        btn.colors = cb;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };

        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(10, 10, 8, 8);
        h.spacing = 12f;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;

        var pillCol = CreateRect("Pills", row);
        pillCol.gameObject.AddComponent<LayoutElement>().minWidth = 36f;
        var pv = pillCol.gameObject.AddComponent<VerticalLayoutGroup>();
        pv.spacing = 4f;
        pv.childAlignment = TextAnchor.MiddleCenter;
        pv.childControlWidth = true;
        pv.childControlHeight = true;
        pv.childForceExpandHeight = false;
        for (int p = 0; p < 4; p++)
        {
            var pGo = CreateRect("Pill_" + p, pillCol);
            var ple = pGo.gameObject.AddComponent<LayoutElement>();
            ple.minWidth = 28f;
            ple.preferredWidth = 28f;
            ple.minHeight = 40f;
            ple.preferredHeight = 40f;
            var pim = pGo.gameObject.AddComponent<Image>();
            pim.sprite = GetBuiltinUiSprite();
            pim.type = Image.Type.Sliced;
            pim.color = pillC;
            pim.raycastTarget = false;
        }

        var gridHost = CreateRect("DotGrid4x4", row);
        gridHost.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var grid = gridHost.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.cellSize = new Vector2(14f, 14f);
        grid.spacing = new Vector2(5f, 5f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        for (int r = 0; r < 4; r++)
        {
            int filled = Mathf.Clamp(fourStats[r], 0, 4);
            for (int c = 0; c < 4; c++)
            {
                var cell = CreateRect($"d_{r}_{c}", gridHost);
                var im = cell.gameObject.AddComponent<Image>();
                im.sprite = GetBuiltinUiSprite();
                im.color = c < filled ? on : off;
                im.raycastTarget = false;
            }
        }
    }

    private static void CreateContinueBlockFigma(RectTransform parent, Font font, Color card, Color iconC, Color textMain, Color textMuted)
    {
        var root = CreateRect("ContinueBlock", parent);
        root.gameObject.AddComponent<LayoutElement>().minHeight = 128f;
        var bg = root.gameObject.AddComponent<Image>();
        bg.color = card;
        bg.raycastTarget = false;
        var h = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(10, 10, 8, 8);
        h.spacing = 12f;
        h.childControlWidth = true;
        h.childForceExpandWidth = true;
        h.childControlHeight = true;
        h.childForceExpandHeight = true;

        var ic = CreateRect("SaveAvatar", root);
        ic.gameObject.AddComponent<LayoutElement>().minWidth = 88f;
        ic.gameObject.AddComponent<LayoutElement>().minHeight = 88f;
        var im = ic.gameObject.AddComponent<Image>();
        im.sprite = GetBuiltinUiSprite();
        im.color = iconC;

        var center = CreateRect("SaveTexts", root);
        center.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        var cv = center.gameObject.AddComponent<VerticalLayoutGroup>();
        cv.spacing = 4f;
        cv.childControlWidth = true;
        cv.childForceExpandWidth = true;
        CreateInfoLine(center, "Stage", "스테이지 : 1", font, 17, textMain);
        CreateInfoLine(center, "Prog", "진행 : 3 / 7", font, 17, textMain);
        CreateInfoLine(center, "Save", "마지막 저장 : 04.27 00:00", font, 14, textMuted);

        var grid2 = CreateRect("Metrics2x2", center);
        grid2.gameObject.AddComponent<LayoutElement>().minHeight = 52f;
        var g = grid2.gameObject.AddComponent<GridLayoutGroup>();
        g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        g.constraintCount = 2;
        g.cellSize = new Vector2(72f, 22f);
        g.spacing = new Vector2(6f, 6f);
        g.startCorner = GridLayoutGroup.Corner.UpperLeft;
        string[] vals = { "68%", "105", "42", "2" };
        for (int i = 0; i < 4; i++)
        {
            var cell = CreateRect("m" + i, grid2);
            var t = CreateText("t", cell, font, 15, FontStyle.Normal, TextAnchor.MiddleLeft, textMain);
            StretchFull(t.rectTransform);
            t.text = vals[i];
        }

        var btnBox = CreateRect("StartGame", root);
        btnBox.gameObject.AddComponent<LayoutElement>().minWidth = 136f;
        var bImg = btnBox.gameObject.AddComponent<Image>();
        bImg.color = new Color(0.35f, 0.58f, 0.92f, 1f);
        var b = btnBox.gameObject.AddComponent<Button>();
        b.targetGraphic = bImg;
        var bt = CreateText("L", btnBox, font, 17, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        StretchFull(bt.rectTransform);
        bt.text = "게임 시작";
        b.navigation = new Navigation { mode = Navigation.Mode.None };
    }

    private static void CreateLabeledDotRow(RectTransform parent, string label, Font font, int filled, Color textMain, Color on, Color off)
    {
        var row = CreateRect("Stat_" + label, parent);
        row.gameObject.AddComponent<LayoutElement>().minHeight = 22f;
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 8f;
        h.childControlWidth = false;
        h.childControlHeight = true;
        var lt = CreateText("L", row, font, 15, FontStyle.Normal, TextAnchor.MiddleLeft, textMain);
        lt.gameObject.AddComponent<LayoutElement>().minWidth = 52f;
        lt.text = label;
        filled = Mathf.Clamp(filled, 0, 5);
        for (int s = 0; s < 5; s++)
            AddRoundDot(row, s < filled, on, off, 13f);
    }

    private static void AddRoundDot(Transform row, bool on, Color cOn, Color cOff, float sz)
    {
        var dot = CreateRect("dot", row);
        var le = dot.gameObject.AddComponent<LayoutElement>();
        le.minWidth = sz;
        le.minHeight = sz;
        le.preferredWidth = sz;
        le.preferredHeight = sz;
        var im = dot.gameObject.AddComponent<Image>();
        im.sprite = GetBuiltinUiSprite();
        im.type = Image.Type.Simple;
        im.preserveAspect = true;
        im.color = on ? cOn : cOff;
        im.raycastTarget = false;
    }

    private static void CreateSkillColumnFigma(RectTransform parent, string cap, string name, string desc, Font font, Color card, Color ovalC, Color main, Color muted)
    {
        var col = CreateRect("Sk_" + cap, parent);
        col.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        col.gameObject.AddComponent<LayoutElement>().minWidth = 96f;
        var bg = col.gameObject.AddComponent<Image>();
        bg.color = card;
        bg.raycastTarget = false;
        var v = col.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(8, 8, 8, 8);
        v.spacing = 6f;
        v.childControlWidth = true;
        v.childForceExpandWidth = true;

        var oval = CreateRect("IconOval", col);
        var ole = oval.gameObject.AddComponent<LayoutElement>();
        ole.minHeight = 36f;
        ole.preferredHeight = 36f;
        ole.minWidth = 72f;
        ole.preferredWidth = 72f;
        var oim = oval.gameObject.AddComponent<Image>();
        oim.sprite = GetBuiltinUiSprite();
        oim.type = Image.Type.Sliced;
        oim.color = ovalC;
        oim.raycastTarget = false;

        var n = CreateText("Name", col, font, 15, FontStyle.Bold, TextAnchor.UpperLeft, main);
        n.gameObject.AddComponent<LayoutElement>().minHeight = 40f;
        n.richText = true;
        n.text = $"{cap}  {name}";

        var d = CreateText("Desc", col, font, 14, FontStyle.Normal, TextAnchor.UpperLeft, muted);
        d.gameObject.AddComponent<LayoutElement>().minHeight = 64f;
        d.text = desc;
    }

    private static void CreateInfoLine(RectTransform parent, string n, string text, Font font, int size, Color c)
    {
        var go = CreateRect(n, parent);
        go.gameObject.AddComponent<LayoutElement>().minHeight = 22f;
        var t = CreateText("t", go, font, size, FontStyle.Normal, TextAnchor.UpperLeft, c);
        StretchFull(t.rectTransform);
        t.alignment = ToTmpAlignment(TextAnchor.MiddleLeft);
        t.text = text;
    }

    private static void CreateColumnTextBlock(RectTransform parent, string head, string body, Color textDefault, Color headCol)
    {
        var go = CreateRect("Col_" + head, parent);
        go.gameObject.AddComponent<LayoutElement>().minWidth = 160f;
        go.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        Font font = GetUiFont();
        var t = CreateText("t", go, font, 15, FontStyle.Normal, TextAnchor.UpperLeft, textDefault);
        StretchFull(t.rectTransform);
        t.richText = true;
        t.text = $"<b><color=#{ColorUtility.ToHtmlStringRGB(headCol)}>{head}</color></b>\n{body}";
    }

    private static void CreateSectionLabel(RectTransform parent, string text, Font font, Color c)
    {
        var go = CreateRect("Sec_" + text, parent);
        go.gameObject.AddComponent<LayoutElement>().minHeight = 26f;
        var t = CreateText("t", go, font, 18, FontStyle.Bold, TextAnchor.MiddleLeft, c);
        StretchFull(t.rectTransform);
        t.text = text;
    }

    private static void CreateMiniLabel(RectTransform parent, string text, Font font, Color c)
    {
        var go = CreateRect("Lbl_" + text, parent);
        go.gameObject.AddComponent<LayoutElement>().minHeight = 22f;
        var t = CreateText("t", go, font, 16, FontStyle.Bold, TextAnchor.MiddleLeft, c);
        StretchFull(t.rectTransform);
        t.text = text;
    }

    private static Text CreateText(string name, RectTransform parent, Font font, int size, FontStyle st, TextAnchor al, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "UI");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = st;
        t.alignment = ToTmpAlignment(al);
        t.color = col;
        t.richText = true;
        t.enableWordWrapping = true;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static TextAlignmentOptions ToTmpAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
            default:
                return TextAlignmentOptions.Center;
        }
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "UI");
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }

    private static void EnsureEventSystemIfMissing()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(es, "EventSystem");
    }

    private static Font GetUiFont()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font != null) return font;

        font = Resources.Load<Font>("Fonts & Materials/LiberationSans SDF");
        if (font != null) return font;

        Debug.LogWarning("[SinglePlayer UI] TMP 폰트 에셋을 찾지 못했습니다. TextMeshPro 기본 폰트를 사용합니다.");
        return TMP_Settings.defaultFontAsset;
    }

    private static Sprite GetBuiltinUiSprite()
    {
        if (_builtinUiSprite != null) return _builtinUiSprite;
        _builtinUiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (_builtinUiSprite == null)
            _builtinUiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        return _builtinUiSprite;
    }

    private static void EnsureFolder(string path)
    {
        if (Directory.Exists(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "Assets", Path.GetFileName(path));
    }
}
