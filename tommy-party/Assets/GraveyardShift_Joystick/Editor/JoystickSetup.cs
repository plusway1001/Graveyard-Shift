#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu: Tools > Graveyard Shift > Add Mobile Joystick
/// Builds the joystick in the open scene, links it to PlayerMovement and hides the old arrow buttons.
/// </summary>
public static class JoystickSetup
{
    [MenuItem("Tools/Graveyard Shift/Add Mobile Joystick")]
    private static void AddJoystick()
    {
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player == null)
        {
            EditorUtility.DisplayDialog("Joystick Setup", "No PlayerMovement found in the open scene.", "OK");
            return;
        }

        GameObject arrows = FindInScene("MovementControls");
        Canvas canvas = arrows != null ? arrows.GetComponentInParent<Canvas>(true) : null;
        if (canvas == null) canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Joystick Setup", "No Canvas found in the open scene.", "OK");
            return;
        }
        canvas = canvas.rootCanvas;

        VirtualJoystick existing = canvas.GetComponentInChildren<VirtualJoystick>(true);
        if (existing != null)
        {
            Undo.RecordObject(player, "Link Joystick");
            player.joystick = existing;
            EditorUtility.SetDirty(player);
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("Joystick Setup", "A joystick already exists. It has been linked to the Player.", "OK");
            return;
        }

        Sprite baseSprite = FindSprite("Joystick_Base");
        Sprite knobSprite = FindSprite("Joystick_Knob");
        Sprite fallback = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Touch zone: invisible image over the bottom-left of the screen.
        GameObject areaGO = new GameObject("JoystickArea", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick));
        Undo.RegisterCreatedObjectUndo(areaGO, "Add Mobile Joystick");
        areaGO.layer = LayerMask.NameToLayer("UI");
        RectTransform area = areaGO.GetComponent<RectTransform>();
        area.SetParent(canvas.transform, false);
        area.SetAsFirstSibling(); // behind other buttons so Pause / Dash / Interact still get taps
        area.anchorMin = new Vector2(0f, 0f);
        area.anchorMax = new Vector2(0.45f, 0.65f);
        area.pivot = new Vector2(0.5f, 0.5f);
        area.offsetMin = Vector2.zero;
        area.offsetMax = Vector2.zero;
        Image areaImg = areaGO.GetComponent<Image>();
        areaImg.color = new Color(1f, 1f, 1f, 0f); // invisible, still catches touches

        // Ring.
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        bgGO.layer = areaGO.layer;
        RectTransform bg = bgGO.GetComponent<RectTransform>();
        bg.SetParent(area, false);
        bg.anchorMin = bg.anchorMax = new Vector2(0f, 0f);
        bg.pivot = new Vector2(0.5f, 0.5f);
        bg.sizeDelta = new Vector2(300f, 300f);
        bg.anchoredPosition = new Vector2(250f, 220f);
        Image bgImg = bgGO.GetComponent<Image>();
        bgImg.sprite = baseSprite != null ? baseSprite : fallback;
        bgImg.color = baseSprite != null ? Color.white : new Color(0.1f, 0.05f, 0.2f, 0.6f);
        bgImg.raycastTarget = false;

        // Knob.
        GameObject knobGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        knobGO.layer = areaGO.layer;
        RectTransform knob = knobGO.GetComponent<RectTransform>();
        knob.SetParent(bg, false);
        knob.anchorMin = knob.anchorMax = new Vector2(0.5f, 0.5f);
        knob.pivot = new Vector2(0.5f, 0.5f);
        knob.sizeDelta = new Vector2(140f, 140f);
        knob.anchoredPosition = Vector2.zero;
        Image knobImg = knobGO.GetComponent<Image>();
        knobImg.sprite = knobSprite != null ? knobSprite : fallback;
        knobImg.color = knobSprite != null ? Color.white : new Color(0.12f, 0.4f, 0.86f, 1f);
        knobImg.raycastTarget = false;

        VirtualJoystick joy = areaGO.GetComponent<VirtualJoystick>();
        joy.background = bg;
        joy.handle = knob;

        Undo.RecordObject(player, "Link Joystick");
        player.joystick = joy;
        EditorUtility.SetDirty(player);

        if (arrows != null)
        {
            Undo.RecordObject(arrows, "Hide Arrow Buttons");
            arrows.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        Selection.activeGameObject = areaGO;
        Debug.Log("[Joystick Setup] Joystick added and linked to " + player.name +
                  (arrows != null ? ". Old arrow buttons (MovementControls) hidden." : "."));
    }

    private static Sprite FindSprite(string name)
    {
        foreach (string guid in AssetDatabase.FindAssets(name + " t:Sprite"))
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
            if (s != null) return s;
        }
        return null;
    }

    private static GameObject FindInScene(string name)
    {
        foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform t = FindDeep(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
#endif
