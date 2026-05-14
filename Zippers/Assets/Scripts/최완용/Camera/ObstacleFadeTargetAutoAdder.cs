#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 선택한 오브젝트 기준으로 ObstacleFadeTarget을 자동 추가하는 에디터 도구.
/// 런타임 코드가 아니므로 반드시 Assets/Editor 폴더 안에 넣어야 한다.
/// </summary>
public static class ObstacleFadeTargetAutoAdder
{
    private const string MenuRoot = "Tools/Obstacle Fade/";

    // 이름에 아래 키워드가 포함된 오브젝트는 ObstacleFadeTarget을 추가하지 않는다.
    private static readonly string[] ExcludeNameKeywords =
    {
        "Road",
        "Side",
        "Dirt",
        "Ground",
        "Grass"
    };

    /// <summary>
    /// 선택한 오브젝트 자체에 ObstacleFadeTarget을 추가한다.
    /// </summary>
    [MenuItem(MenuRoot + "선택 오브젝트에 추가")]
    private static void AddToSelectedObjects()
    {
        int addedCount = 0;
        int skippedCount = 0;

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            if (TryAddFadeTarget(selectedObject))
            {
                addedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        Debug.Log($"[ObstacleFadeTargetAutoAdder] 선택 오브젝트 처리 완료 / 추가: {addedCount}, 스킵: {skippedCount}");
    }

    /// <summary>
    /// 선택한 오브젝트의 바로 아래 자식들에게 ObstacleFadeTarget을 추가한다.
    /// MapWalls, Barricades 같은 묶음 오브젝트를 선택했을 때 사용한다.
    /// </summary>
    [MenuItem(MenuRoot + "선택 오브젝트의 직계 자식에 추가")]
    private static void AddToDirectChildren()
    {
        int addedCount = 0;
        int skippedCount = 0;

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            foreach (Transform child in selectedObject.transform)
            {
                if (TryAddFadeTarget(child.gameObject))
                {
                    addedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        Debug.Log($"[ObstacleFadeTargetAutoAdder] 직계 자식 처리 완료 / 추가: {addedCount}, 스킵: {skippedCount}");
    }

    /// <summary>
    /// 선택한 오브젝트의 모든 하위 오브젝트 중 Collider가 있는 오브젝트에 ObstacleFadeTarget을 추가한다.
    /// QuarterViewCamera가 Collider 기준으로 장애물을 감지하므로 가장 추천하는 방식이다.
    /// </summary>
    [MenuItem(MenuRoot + "선택 오브젝트의 모든 하위 Collider 오브젝트에 추가")]
    private static void AddToAllChildColliders()
    {
        int addedCount = 0;
        int skippedCount = 0;

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            Collider[] colliders = selectedObject.GetComponentsInChildren<Collider>(true);

            foreach (Collider collider in colliders)
            {
                if (collider == null)
                {
                    continue;
                }

                if (TryAddFadeTarget(collider.gameObject))
                {
                    addedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        Debug.Log($"[ObstacleFadeTargetAutoAdder] 하위 Collider 처리 완료 / 추가: {addedCount}, 스킵: {skippedCount}");
    }

    /// <summary>
    /// 선택한 오브젝트의 모든 하위 오브젝트 중 Renderer가 있는 오브젝트에 ObstacleFadeTarget을 추가한다.
    /// Collider가 없는 오브젝트까지 포함하고 싶을 때 사용한다.
    /// </summary>
    [MenuItem(MenuRoot + "선택 오브젝트의 모든 하위 Renderer 오브젝트에 추가")]
    private static void AddToAllChildRenderers()
    {
        int addedCount = 0;
        int skippedCount = 0;

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            Renderer[] renderers = selectedObject.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (TryAddFadeTarget(renderer.gameObject))
                {
                    addedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        Debug.Log($"[ObstacleFadeTargetAutoAdder] 하위 Renderer 처리 완료 / 추가: {addedCount}, 스킵: {skippedCount}");
    }

    /// <summary>
    /// GameObject에 ObstacleFadeTarget을 추가하고 Renderer 목록을 자동 등록한다.
    /// Road 또는 Side가 이름에 포함된 오브젝트는 제외한다.
    /// </summary>
    private static bool TryAddFadeTarget(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        // 길, 보도 관련 오브젝트는 투명화 대상에서 제외한다.
        if (HasExcludeKeyword(targetObject.name))
        {
            return false;
        }

        // 이미 붙어 있으면 중복 추가하지 않는다.
        if (targetObject.GetComponent<ObstacleFadeTarget>() != null)
        {
            return false;
        }

        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);

        // 투명화할 Renderer가 없으면 추가하지 않는다.
        if (renderers == null || renderers.Length == 0)
        {
            return false;
        }

        ObstacleFadeTarget fadeTarget = Undo.AddComponent<ObstacleFadeTarget>(targetObject);

        AssignFadeRenderers(fadeTarget, renderers);

        EditorUtility.SetDirty(targetObject);
        EditorUtility.SetDirty(fadeTarget);

        return true;
    }

    /// <summary>
    /// 오브젝트 이름에 제외 키워드가 포함되어 있는지 확인한다.
    /// </summary>
    private static bool HasExcludeKeyword(string objectName)
    {
        foreach (string keyword in ExcludeNameKeywords)
        {
            if (objectName.Contains(keyword))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// ObstacleFadeTarget 안의 _fadeRenderers 배열에 Renderer들을 자동 등록한다.
    /// </summary>
    private static void AssignFadeRenderers(ObstacleFadeTarget fadeTarget, Renderer[] renderers)
    {
        SerializedObject serializedObject = new SerializedObject(fadeTarget);
        SerializedProperty fadeRenderersProperty = serializedObject.FindProperty("_fadeRenderers");

        if (fadeRenderersProperty == null)
        {
            Debug.LogWarning("[ObstacleFadeTargetAutoAdder] ObstacleFadeTarget에서 _fadeRenderers 필드를 찾지 못했습니다.", fadeTarget);
            return;
        }

        fadeRenderersProperty.arraySize = renderers.Length;

        for (int i = 0; i < renderers.Length; i++)
        {
            SerializedProperty element = fadeRenderersProperty.GetArrayElementAtIndex(i);
            element.objectReferenceValue = renderers[i];
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif