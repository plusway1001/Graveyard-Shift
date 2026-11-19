#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > Graveyard Shift > Build Level 2
/// Copies Level_1_FINAL into a new scene "Level_2" and turns it into the one-night,
/// serve-4-ghosts level. Level_1_FINAL itself is never changed.
/// </summary>
public static class Level2Builder
{
    const string SourceScene = "Level 1 FINAL";
    const string NewScene = "Level 2";

    // Prefabs and sprites already used by Level_1_FINAL / the project.
    const string IngredientPrefabGuid = "8328d62a180e5084888a4f881357d201"; // "Ingredient" pickup (riots drop these)
    const string LifePrefabGuid = "622d6de1e9c3de94abaf3b3332bb4039";       // "Life Pickup" heart
    const string GhostSeatedSheetGuid = "c7b97e8d1514babc979980f6b2bd772c"; // ghost_A.png (seated)
    const string GhostEatingSheetGuid = "34fa606b69279a77c456026dfaf72305"; // ghost_B.png (eating)
    const string GhostStandSheetGuid = "00e98a8a670ecefe7223f7686fe2e52e";  // ghost_C.png (standing)

    static readonly List<string> log = new List<string>();
    static List<GameObject> all;

    [MenuItem("Tools/Graveyard Shift/Build Level 2")]
    public static void Build()
    {
        log.Clear();

        string src = FindScenePath(SourceScene);
        if (src == null) { Dialog("Couldn't find a scene called " + SourceScene + "."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string dst = System.IO.Path.GetDirectoryName(src).Replace('\\', '/') + "/" + NewScene + ".unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(dst) != null)
        {
            if (!EditorUtility.DisplayDialog("Build Level 2", NewScene + " already exists. Rebuild it from " + SourceScene + "? This replaces it.", "Rebuild", "Cancel")) return;
            AssetDatabase.DeleteAsset(dst);
        }
        if (!AssetDatabase.CopyAsset(src, dst)) { Dialog("Couldn't copy the scene."); return; }

        Scene scene = EditorSceneManager.OpenScene(dst, OpenSceneMode.Single);
        all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
        Physics2D.SyncTransforms();

        GameObject winScreen = RemoveTimeSystem();
        SetupHealthAndInventory();
        GameObject player = all.FirstOrDefault(g => g.CompareTag("Player"));

        SetupSlaughterRoom();
        SetupSafeRoom(player);
        SetupKitchen();
        SetupPitRoom();
        SetupDiningHall(winScreen);
        SetupBrainMeter();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings(dst);

        Debug.Log("[Level 2 Builder] Done:\n- " + string.Join("\n- ", log));
        EditorUtility.DisplayDialog("Level 2 built", "Saved " + dst + "\n\n• " + string.Join("\n• ", log.Take(30)), "OK");
    }

    // ---------------------------------------------------------------- time system
    static GameObject RemoveTimeSystem()
    {
        GameObject winScreen = null;

        foreach (var (mb, so) in WithProperty("realSecondsPerNight"))
        {
            so.FindProperty("realSecondsPerNight").floatValue = 999999f; // the clock never runs out
            so.ApplyModifiedPropertiesWithoutUndo();
            log.Add("Night clock on '" + mb.name + "' set so it never ends");
        }

        foreach (var (mb, so) in WithProperty("targetNights"))
        {
            var w = so.FindProperty("winScreen");
            if (w != null) winScreen = w.objectReferenceValue as GameObject;
            mb.enabled = false;
            log.Add("Revenue/night objective '" + mb.GetType().Name + "' turned off");
        }

        foreach (string n in new[] { "NightUI", "NightProgressBar", "NightProgressBar2", "RevenueProgressBar", "RevenueBarBG", "CurrentRevenuetext", "TimeText", "PhaseText", "NightCounterText" })
            Hide(Named(n), "time/revenue UI");

        if (winScreen == null) winScreen = Named("WinGame");
        return winScreen;
    }

    static void SetupHealthAndInventory()
    {
        foreach (var (mb, so) in WithProperty("startingLives"))
        {
            var lt = so.FindProperty("livesText");
            if (lt != null && lt.objectReferenceValue is Component c) Hide(c.gameObject, "lives number (hearts show health instead)");
        }
        var hearts = Named("HeartsContainer");
        if (hearts != null && !hearts.activeSelf) { hearts.SetActive(true); log.Add("Hearts HUD switched on"); }

        foreach (var (mb, so) in WithProperty("startingIngredients"))
        {
            so.FindProperty("startingIngredients").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            log.Add("Player starts with 0 ingredients (collect brains)");
        }
    }

    // ---------------------------------------------------------------- slaughter room
    static void SetupSlaughterRoom()
    {
        foreach (GameObject g in all.ToList())
        {
            string n = g.name.ToLower();
            if (n.Contains("ingredientspawn") || n.Contains("ingredient maker") || n.Contains("ingredientmaker"))
                Hide(g, "ingredient maker machine");
        }
        foreach (TMP_Text t in Texts())
            if (t.text.ToLower().Contains("ingredients maker") || t.text.Trim() == "111")
                Hide(t.gameObject, "machine label");

        Vector2 label = LabelPos("slaughter room", new Vector2(-195.9f, 16.6f));
        var markers = all.Where(g => g.name.StartsWith("Blocked")).Select(g => (Vector2)g.transform.position).ToList();
        var probes = new List<Vector2>();
        if (markers.Count > 0) probes.Add(new Vector2(markers.Average(p => p.x), markers.Average(p => p.y)));
        probes.AddRange(new[] { label + new Vector2(0, -8), label + new Vector2(0, -5), label + new Vector2(4, -8) });
        var (center, rect) = ProbeRoom(probes);

        GameObject anchor = new GameObject("L2_SlaughterRoomCenter");
        anchor.transform.position = center;

        // The riot zombies in the Slaughter Room: brains spawn around them.
        var zombies = all.Where(g => g != null && g.activeInHierarchy &&
                                     ((g.name.ToLower().Contains("riot") && g.name.ToLower().Contains("enem")) ||
                                      HasProperty(g, "chaseSpeed")))
                         .Select(g => g.transform).Distinct().ToList();
        // Keep only zombies near the Slaughter Room (not table riots elsewhere).
        Vector2 roomRef = markers.Count > 0 ? probes[0] : center;
        zombies = zombies.Where(z => Vector2.Distance(z.position, roomRef) < 25f).ToList();

        GameObject brains = new GameObject("L2_BrainSpawner");
        brains.transform.position = center;
        var sp = brains.AddComponent<L2PickupSpawner>();
        sp.pickupPrefab = LoadPrefab(IngredientPrefabGuid);
        sp.overrideSprite = FindAsset<Sprite>("L2_Brain");
        sp.overrideHeight = 0.9f;
        sp.roomCenter = anchor.transform;
        sp.spawnAround = zombies.ToArray();
        sp.aroundRadius = new Vector2(1.3f, 3f);
        if (zombies.Count == 0)
        {
            // Zombies may only appear at runtime: find them then, or use their spawn markers.
            sp.autoFindAnchorsNamed = "riot,enem";
            var blocked = all.Where(g => g.name.StartsWith("Blocked")).Select(g => g.transform).ToArray();
            if (blocked.Length > 0) { sp.spawnAround = blocked; sp.autoFindAnchorsNamed = ""; }
        }
        sp.maxAlive = 6; sp.respawnDelay = 4f; sp.minSpacing = 1.4f;
        log.Add("Brain spawner: 6 brains around " + zombies.Count + " riot zombies" + (sp.pickupPrefab ? "" : " WARNING: ingredient prefab not found"));

        // Room area: the riot spawn markers sit near the corners, so their box (plus a margin) is the room.
        Rect area = rect;
        if (markers.Count >= 2)
        {
            float x0 = markers.Min(p => p.x), x1 = markers.Max(p => p.x), y0 = markers.Min(p => p.y), y1 = markers.Max(p => p.y);
            area = Rect.MinMaxRect(x0 - 2.5f, y0 - 2.5f, x1 + 2.5f, y1 + 2.5f);
        }

        // Traps for the Slaughter Room (3 spikes + 2 banana peels), re-shuffled after every served ghost.
        var traps = new List<Transform>();
        GameObject spikeT = FindStaticSpikeTemplate();
        GameObject bananaT = all.FirstOrDefault(g => g.name.StartsWith("Banana Peel") && g.activeInHierarchy);
        for (int i = 0; spikeT != null && i < 3; i++) traps.Add(Clone(spikeT, area.center, "L2_SlaughterSpike_" + (i + 1)).transform);
        for (int i = 0; bananaT != null && i < 2; i++) traps.Add(Clone(bananaT, area.center, "L2_SlaughterBanana_" + (i + 1)).transform);
        var shuffler = new GameObject("L2_SlaughterTrapShuffler").AddComponent<L2TrapShuffler>();
        shuffler.roomName = "Slaughter Room";
        shuffler.area = area;
        shuffler.traps = traps;
        shuffler.keepClear = all.Where(g => g.name.StartsWith("Blocked")).Select(g => g.transform).ToList();
        shuffler.keepClearRadius = 2f;
        log.Add("Slaughter Room: " + traps.Count + " traps that change layout every time you serve a ghost");
    }

    // ---------------------------------------------------------------- safe room
    static void SetupSafeRoom(GameObject player)
    {
        Vector2 label = LabelPos("safe room", new Vector2(-169f, 13.5f));
        var probes = new List<Vector2>();
        if (player != null) probes.Add(player.transform.position);
        GameObject spawn = all.FirstOrDefault(g => g.name == "PlayerSpawn");
        if (spawn != null) probes.Insert(0, spawn.transform.position);
        probes.Add(label + new Vector2(0, -5));
        var (center, rect) = ProbeRoom(probes);

        HideHazardsIn(rect, "safe room");

        GameObject anchor = new GameObject("L2_SafeRoomCenter");
        anchor.transform.position = center;
        GameObject hearts = new GameObject("L2_HeartSpawner");
        hearts.transform.position = center;
        var sp = hearts.AddComponent<L2PickupSpawner>();
        sp.pickupPrefab = LoadPrefab(LifePrefabGuid);
        sp.roomCenter = anchor.transform;
        sp.maxAlive = 2; sp.respawnDelay = 15f; sp.minSpacing = 2.5f; sp.wallPadding = 1.5f;
        log.Add("Heart pickups in the Safe Room (2 at a time, respawn every 15s)" + (sp.pickupPrefab ? "" : " WARNING: life prefab not found"));
    }

    // ---------------------------------------------------------------- kitchen: exactly 3 pitfalls
    static void SetupKitchen()
    {
        Vector2 kitchenLabel = LabelPos("kitchen area", new Vector2(-146.9f, 14.5f));
        Vector2 counterLabel = LabelPos("dish counter", new Vector2(-136.5f, 14.7f));
        Vector2 guess = (kitchenLabel + counterLabel) * 0.5f + new Vector2(0, -4f);

        var pits = WithProperty("pitRoomSpawn").Select(p => p.mb.gameObject).Distinct().ToList();
        GameObject nearestPit = pits.OrderBy(p => Vector2.Distance(p.transform.position, guess)).FirstOrDefault();
        var probes = new List<Vector2> { guess, guess + new Vector2(0, -2) };
        if (nearestPit != null && Vector2.Distance(nearestPit.transform.position, guess) < 12f) probes.Insert(0, nearestPit.transform.position);
        var (center, rect) = ProbeRoom(probes);

        var kitchenPits = pits.Where(p => Grow(rect, 1f).Contains(p.transform.position)).ToList();
        GameObject template = kitchenPits.FirstOrDefault(p => !IsMover(p)) ?? pits.FirstOrDefault(p => !IsMover(p)) ?? pits.FirstOrDefault();
        if (template == null) { log.Add("WARNING: no pitfall found to copy"); return; }

        // Zig-zag: the player has to weave between them to reach the dish counter.
        Rect r = Shrink(rect, 1.6f);
        Vector2[] targets =
        {
            new Vector2(r.xMin + r.width * 0.22f, r.yMin + r.height * 0.68f),
            new Vector2(r.xMin + r.width * 0.50f, r.yMin + r.height * 0.32f),
            new Vector2(r.xMin + r.width * 0.78f, r.yMin + r.height * 0.68f),
        };

        var placed = new List<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = FindFreeNear(targets[i], 0.9f, center, counterLabel, 2.5f);
            GameObject pit = i < kitchenPits.Count && !IsMover(kitchenPits[i]) ? kitchenPits[i] : Clone(template, pos, "L2_KitchenPit_" + (i + 1));
            pit.transform.position = new Vector3(pos.x, pos.y, pit.transform.position.z);
            pit.transform.localScale = pit.transform.localScale * 2f; // bigger holes
            pit.SetActive(true);
            var chase = pit.GetComponent<L2Chaser>() ?? pit.AddComponent<L2Chaser>();
            chase.area = Shrink(rect, 1f);
            chase.speed = 1.6f;
            placed.Add(pit);
        }
        foreach (GameObject extra in kitchenPits.Where(p => !placed.Contains(p))) Hide(extra, "extra kitchen pitfall");

        // Kitchen traps (the 3 pits + any other fixed hazards inside) get a new layout after every served ghost.
        var kTraps = placed.Select(p => p.transform).ToList();
        string[] words = { "spike", "banana" };
        foreach (GameObject g in all.ToList())
        {
            if (g == null || !g.activeInHierarchy || IsMover(g)) continue;
            string low = g.name.ToLower();
            if (words.Any(w => low.StartsWith(w)) && rect.Contains(g.transform.position) && !kTraps.Contains(g.transform)) kTraps.Add(g.transform);
        }
        var counterPoint = new GameObject("L2_KitchenCounterPoint");
        counterPoint.transform.position = counterLabel + new Vector2(0, -1.2f);
        var ks = new GameObject("L2_KitchenTrapShuffler").AddComponent<L2TrapShuffler>();
        ks.roomName = "Kitchen";
        ks.area = rect;
        ks.traps = kTraps;
        ks.keepClear = new List<Transform> { counterPoint.transform };
        ks.spacing = 2.8f;
        log.Add("Kitchen: " + kTraps.Count + " traps that change layout every time you serve a ghost");
        log.Add("Kitchen has exactly 3 pitfalls, twice as big, that slowly follow you around the kitchen");
    }

    // ---------------------------------------------------------------- pitfall room: easy escape
    static void SetupPitRoom()
    {
        // Every pitfall's landing spot.
        var landings = WithProperty("pitRoomSpawn")
            .Select(x => x.so.FindProperty("pitRoomSpawn").objectReferenceValue as Transform)
            .Where(t => t != null).Distinct().ToList();
        var exits = WithProperty("requireInteract").Where(x => x.so.FindProperty("returnPoint") != null).ToList();
        if (landings.Count == 0 || exits.Count == 0) { log.Add("WARNING: pitfall room landing/exit not found"); return; }

        foreach (var (mb, so) in exits)
        {
            so.FindProperty("requireInteract").boolValue = false;
            var stay = so.FindProperty("minimumStayTime");
            if (stay != null) stay.floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Each exit only moves inside its OWN pitfall room: use the landing spot closest to it.
            Vector2 old = mb.transform.position;
            Transform landing = landings.OrderBy(l => Vector2.Distance(l.position, old)).First();
            if (Vector2.Distance(landing.position, old) > 40f) continue;

            var (center, rect) = ProbeRoom(new List<Vector2> { landing.position, landing.position + Vector3.up * 1.5f });
            HideHazardsIn(rect, "pitfall room", new List<GameObject> { mb.gameObject });

            Vector2 target = old;
            foreach (Vector2 off in new[] { new Vector2(3f, 0), new Vector2(-3f, 0), new Vector2(0, -3f), new Vector2(0, 3f),
                                            new Vector2(2.2f, 0), new Vector2(-2.2f, 0), new Vector2(0, -2.2f), new Vector2(0, 2.2f) })
            {
                Vector2 p = (Vector2)landing.position + off;
                if (L2RoomBounds.IsFree(p, 0.6f) && L2RoomBounds.ClearLine(landing.position, p)) { target = p; break; }
            }
            mb.transform.position = new Vector3(target.x, target.y, mb.transform.position.z);

            TMP_Text sign = Texts().Where(t => t.text.Trim().ToLower() == "exit")
                                   .OrderBy(t => Vector2.Distance(t.transform.position, old)).FirstOrDefault();
            if (sign != null && Vector2.Distance(sign.transform.position, old) < 8f) sign.transform.position += (Vector3)(target - old);
            log.Add("Pitfall exit '" + mb.name + "' moved next to its landing spot (walk into it to leave)");
        }
    }

    // ---------------------------------------------------------------- brain meter HUD
    static void SetupBrainMeter()
    {
        GameObject canvas = all.FirstOrDefault(g => g.name == "Canvas" && g.GetComponent<Canvas>() != null);
        GameObject ingText = Named("IngredientText");
        if (canvas == null || ingText == null) { log.Add("WARNING: couldn't add the brain meter (Canvas or IngredientText missing)"); return; }

        Sprite brain = FindAsset<Sprite>("L2_Brain");
        Sprite arrowSprite = FindAsset<Sprite>("L2_Arrow");

        var root = NewUI("L2_BrainMeter", canvas.transform, new Vector2(0.5f, 1f), new Vector2(0, -150f), new Vector2(520f, 64f));
        root.SetAsFirstSibling();
        var label = NewUI("Label", root, new Vector2(0.5f, 0.5f), new Vector2(-170f, 0), new Vector2(180f, 60f)).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = "Next dish:"; label.fontSize = 30; label.alignment = TextAlignmentOptions.MidlineRight; label.raycastTarget = false;
        label.color = new Color(0.98f, 0.91f, 0.28f);

        var pips = new List<UnityEngine.UI.Image>();
        for (int i = 0; i < 4; i++)
        {
            var img = NewUI("Brain " + (i + 1), root, new Vector2(0.5f, 0.5f), new Vector2(-40f + i * 64f, 0), new Vector2(56f, 48f)).gameObject.AddComponent<UnityEngine.UI.Image>();
            img.sprite = brain; img.preserveAspect = true; img.raycastTarget = false;
            pips.Add(img);
        }

        var toastRT = NewUI("L2_DishReadyToast", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 140f), new Vector2(1100f, 140f));
        var toast = toastRT.gameObject.AddComponent<TextMeshProUGUI>();
        toast.fontSize = 52; toast.alignment = TextAlignmentOptions.Center; toast.raycastTarget = false;
        toast.color = new Color(0.98f, 0.91f, 0.28f); toast.fontStyle = FontStyles.Bold;

        var arrowRT = NewUI("L2_DishArrow", canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(96f, 96f));
        var arrowImg = arrowRT.gameObject.AddComponent<UnityEngine.UI.Image>();
        arrowImg.sprite = arrowSprite; arrowImg.raycastTarget = false;

        // Where the dish counter is.
        Vector2 counterLabel = LabelPos("dish counter", new Vector2(-136.5f, 14.7f));
        GameObject counterGO = all.Where(g => g.name.ToLower().Contains("foodcounter") || g.name.ToLower().Contains("dish counter"))
                                  .OrderBy(g => Vector2.Distance(g.transform.position, counterLabel)).FirstOrDefault();
        Transform counter;
        if (counterGO != null && Vector2.Distance(counterGO.transform.position, counterLabel) < 6f) counter = counterGO.transform;
        else
        {
            var a = new GameObject("L2_DishCounterPoint");
            a.transform.position = counterLabel + new Vector2(0, -1.2f);
            counter = a.transform;
        }

        var meter = root.gameObject.AddComponent<L2BrainMeter>();
        meter.ingredientText = ingText.GetComponent<TMP_Text>();
        meter.pips = pips.ToArray();
        meter.toast = toast;
        meter.arrow = arrowRT;
        meter.dishCounter = counter;
        log.Add("Brain meter added: 4 brain icons fill up, then 'sent to the Kitchen' message + arrow to the Dish Counter");
    }

    static RectTransform NewUI(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    // ---------------------------------------------------------------- dining hall
    static void SetupDiningHall(GameObject winScreen)
    {
        foreach (var q in Object.FindObjectsByType<CustomerQueue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            q.enabled = false;
            log.Add("Customer queue turned off (ghosts start seated)");
        }

        GameObject manager = new GameObject("L2_LevelManager");
        var goal = manager.AddComponent<L2ServeGoal>();
        goal.target = 4;
        goal.winScreen = winScreen;
        if (winScreen != null) winScreen.SetActive(false);
        goal.progressText = MakeGoalText();

        var ghosts = manager.AddComponent<L2SeatedGhosts>();
        ghosts.ghostCount = 4;
        ghosts.goal = goal;
        ghosts.ghostPrefab = FindGhostPrefabAsset();
        ghosts.seatedFrames = LoadSprites(GhostSeatedSheetGuid);
        ghosts.eatingFrames = LoadSprites(GhostEatingSheetGuid);
        ghosts.leavingFrames = LoadSprites(GhostStandSheetGuid);
        ghosts.eatClip = FindAsset<AudioClip>("Ghost_Eating_1");
        ghosts.leaveClip = FindAsset<AudioClip>("Ghost_Leaving_1");
        ghosts.spreadAcrossTables = false; // any free seat in the dining hall
        if (ghosts.seatedFrames.Length == 0) log.Add("WARNING: ghost_A sprites not found (ghosts would be invisible)");
        log.Add("Ghost sounds: eating " + (ghosts.eatClip ? "OK" : "missing") + ", leaving " + (ghosts.leaveClip ? "OK" : "missing"));
        log.Add("4 ghosts seated at random dining hall seats; serve all 4 to win" +
                (ghosts.ghostPrefab ? " (prefab: " + ghosts.ghostPrefab.name + ")" : " (ghost prefab will be found from the queue at runtime)"));

        var tables = Object.FindObjectsByType<RestaurantTable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        log.Add(tables.Length + " tables found in the dining hall");

        // Banana peels in the aisles between tables.
        GameObject banana = all.FirstOrDefault(g => g.name.StartsWith("Banana Peel") && g.activeInHierarchy);
        Vector2 label = LabelPos("dining hall", new Vector2(-142.2f, 44.1f));
        var (center, rect) = ProbeRoom(new List<Vector2> { label, label + new Vector2(0, -2), label + new Vector2(0, 2) });
        if (banana != null)
        {
            var spots = new List<Vector2>();
            for (int a = 0; a < 360 && spots.Count < 3; a += 30)
            {
                foreach (float rad in new[] { 2.5f, 4f })
                {
                    Vector2 p = center + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * rad;
                    bool nearTable = tables.Any(t => Vector2.Distance(t.transform.position, p) < 2f);
                    if (!nearTable && rect.Contains(p) && L2RoomBounds.IsFree(p, 0.5f) && spots.All(s => Vector2.Distance(s, p) > 3f))
                    { spots.Add(p); break; }
                }
            }
            for (int i = 0; i < spots.Count; i++) Clone(banana, spots[i], "L2_AisleBanana_" + (i + 1));
            log.Add(spots.Count + " banana peels in the dining hall aisles");
        }
    }

    static TMP_Text MakeGoalText()
    {
        GameObject canvas = all.FirstOrDefault(g => g.name == "Canvas" && g.GetComponent<Canvas>() != null);
        if (canvas == null) return null;
        var go = new GameObject("L2_GoalText", typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(canvas.transform, false);
        rt.SetAsFirstSibling(); // menus and win/lose panels draw on top
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -20f);
        rt.sizeDelta = new Vector2(1100f, 150f);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = 44;
        t.alignment = TextAlignmentOptions.Top;
        t.color = new Color(0.98f, 0.91f, 0.28f);
        t.raycastTarget = false;
        t.text = "Ghosts served: 0/4";
        return t;
    }

    // ---------------------------------------------------------------- helpers
    static (Vector2 center, Rect rect) ProbeRoom(List<Vector2> probes)
    {
        foreach (Vector2 p in probes)
        {
            if (!L2RoomBounds.IsFree(p, 0.2f)) continue;
            Rect r = L2RoomBounds.Measure(p);
            if (r.width >= 5f && r.height >= 5f && r.width <= 60f && r.height <= 60f) return (r.center, r);
        }
        Vector2 c = probes.Count > 0 ? probes[0] : Vector2.zero;
        log.Add("Note: couldn't measure the room near " + c + ", used a 12x8 area");
        return (c, new Rect(c - new Vector2(6, 4), new Vector2(12, 8)));
    }

    static Vector2 FindFreeNear(Vector2 target, float radius, Vector2 roomCenter, Vector2 avoid, float avoidDist)
    {
        for (int ring = 0; ring < 6; ring++)
            for (int a = 0; a < 360; a += 45)
            {
                Vector2 p = target + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * (ring * 0.6f);
                if (L2RoomBounds.IsFree(p, radius) && Vector2.Distance(p, avoid) > avoidDist && L2RoomBounds.ClearLine(roomCenter, p)) return p;
                if (ring == 0) break;
            }
        return target;
    }

    static void HideHazardsIn(Rect rect, string where, List<GameObject> keep = null)
    {
        string[] words = { "spike", "banana", "clown", "fire oven", "mudthrower" };
        int n = 0;
        foreach (GameObject g in all)
        {
            if (g == null || !g.activeInHierarchy || (keep != null && keep.Contains(g))) continue;
            string low = g.name.ToLower();
            if (!words.Any(w => low.StartsWith(w) || low.Contains(w))) continue;
            if (!rect.Contains(g.transform.position)) continue;
            g.SetActive(false); n++;
        }
        if (n > 0) log.Add(n + " hazards removed from the " + where);
    }

    static GameObject FindSpikeTemplate()
    {
        var spikes = WithProperty("cycleSpikes").Select(x => x.mb.gameObject).ToList();
        return spikes.FirstOrDefault(s => s.activeInHierarchy) ?? spikes.FirstOrDefault();
    }

    static GameObject FindStaticSpikeTemplate()
    {
        var spikes = WithProperty("cycleSpikes").Select(x => x.mb.gameObject).Where(g => !IsMover(g)).ToList();
        foreach (GameObject g in spikes)
        {
            foreach (MonoBehaviour mb in g.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                var mode = new SerializedObject(mb).FindProperty("movementMode");
                if (mode != null && mode.propertyType == SerializedPropertyType.Enum && mode.enumValueIndex == 0 && g.activeInHierarchy) return g;
            }
        }
        return spikes.FirstOrDefault(g => g.activeInHierarchy) ?? spikes.FirstOrDefault();
    }

    static bool MakePatrol(GameObject spike, Vector2 center, Vector2 size)
    {
        foreach (MonoBehaviour mb in spike.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var so = new SerializedObject(mb);
            var mode = so.FindProperty("movementMode");
            if (mode == null || mode.propertyType != SerializedPropertyType.Enum) continue;
            int idx = System.Array.FindIndex(mode.enumNames, e => e.ToLower().Contains("patrol") || e.ToLower().Contains("area") || e.ToLower().Contains("random"));
            if (idx < 0) return false;
            mode.enumValueIndex = idx;
            SetVec(so.FindProperty("patrolAreaCenter"), center);
            SetVec(so.FindProperty("patrolAreaSize"), size);
            var cyc = so.FindProperty("cycleSpikes"); if (cyc != null) cyc.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
        return false;
    }

    static void SetVec(SerializedProperty p, Vector2 v)
    {
        if (p == null) return;
        if (p.propertyType == SerializedPropertyType.Vector2) p.vector2Value = v;
        else if (p.propertyType == SerializedPropertyType.Vector3) p.vector3Value = v;
    }

    static bool IsMover(GameObject g)
    {
        for (Transform t = g.transform; t != null; t = t.parent)
            foreach (MonoBehaviour mb in t.GetComponents<MonoBehaviour>())
                if (mb != null && new SerializedObject(mb).FindProperty("waypoints") != null) return true;
        return false;
    }

    static GameObject Clone(GameObject template, Vector2 pos, string name)
    {
        GameObject c = Object.Instantiate(template);
        c.name = name;
        c.transform.SetParent(null, true);
        c.transform.position = new Vector3(pos.x, pos.y, template.transform.position.z);
        c.transform.localScale = template.transform.lossyScale;
        c.SetActive(true);
        all.Add(c);
        return c;
    }

    static IEnumerable<(MonoBehaviour mb, SerializedObject so)> WithProperty(string prop)
    {
        foreach (GameObject g in all)
            foreach (MonoBehaviour mb in g.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                var so = new SerializedObject(mb);
                if (so.FindProperty(prop) != null) yield return (mb, so);
            }
    }

    static bool HasProperty(GameObject g, string prop)
    {
        foreach (MonoBehaviour mb in g.GetComponents<MonoBehaviour>())
            if (mb != null && new SerializedObject(mb).FindProperty(prop) != null) return true;
        return false;
    }

    static IEnumerable<TMP_Text> Texts() => all.Where(g => g != null).SelectMany(g => g.GetComponents<TMP_Text>());

    static Vector2 LabelPos(string contains, Vector2 fallback)
    {
        TMP_Text t = Texts().FirstOrDefault(x => x.GetComponentInParent<Canvas>() == null && x.text.ToLower().Contains(contains));
        return t != null ? (Vector2)t.transform.position : fallback;
    }

    static GameObject Named(string n) => all.FirstOrDefault(g => g != null && g.name == n);

    static void Hide(GameObject g, string why)
    {
        if (g == null || !g.activeSelf) return;
        g.SetActive(false);
        log.Add("Hid " + g.name + " (" + why + ")");
    }

    static Rect Grow(Rect r, float d) => Rect.MinMaxRect(r.xMin - d, r.yMin - d, r.xMax + d, r.yMax + d);
    static Rect Shrink(Rect r, float d) => Rect.MinMaxRect(r.xMin + d, r.yMin + d, r.xMax - d, r.yMax - d);

    static GameObject LoadPrefab(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static Sprite[] LoadSprites(string guid)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path)) return new Sprite[0];
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    static T FindAsset<T>(string name) where T : Object
    {
        foreach (string g in AssetDatabase.FindAssets(name + " t:" + typeof(T).Name))
        {
            T a = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g));
            if (a != null) return a;
        }
        return null;
    }

    static GameObject FindGhostPrefabAsset()
    {
        GameObject best = null;
        foreach (string g in AssetDatabase.FindAssets("t:Prefab"))
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
            if (!L2SeatedGhosts.IsGhost(go)) continue;
            if (best == null || go.name.ToLower().Contains("ghost")) best = go;
        }
        return best;
    }

    static string Norm(string s) => s.Replace(" ", "").Replace("_", "").Replace("-", "").ToLower();

    static string FindScenePath(string name)
    {
        // Matches "Level 1 FINAL", "Level_1_FINAL", "Level1FINAL"...
        foreach (string g in AssetDatabase.FindAssets("t:Scene"))
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (Norm(System.IO.Path.GetFileNameWithoutExtension(p)) == Norm(name)) return p;
        }
        // Fallback: the scene that is open right now.
        string open = SceneManager.GetActiveScene().path;
        if (!string.IsNullOrEmpty(open) && Norm(open) != Norm("Assets/Scenes/" + NewScene + ".unity")) return open;
        return null;
    }

    static void AddToBuildSettings(string path)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        log.Add("Added " + NewScene + " to Build Settings");
    }

    static void Dialog(string msg) => EditorUtility.DisplayDialog("Build Level 2", msg, "OK");
}
#endif
