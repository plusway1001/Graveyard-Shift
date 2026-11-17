#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the medium difficulty Graveyard Shift level and saves it as a prefab.
/// Open from the menu: Tools > Graveyard Shift > Level Generator.
/// MUST be in a folder named "Editor".
/// Any prefab slot you leave empty gets a simple coloured placeholder instead.
/// </summary>
public class GraveyardLevelGenerator : EditorWindow
{
    // =====================================================================
    // LAYOUT (tile units, 1 tile = 1 world unit). Edit these to reshape the level.
    // RectInt(x, y, width, height) = bottom-left corner + size.
    // =====================================================================
    private static readonly RectInt SafeRoom        = new RectInt(-6, -5, 12, 10);
    private static readonly RectInt SlaughterRoom   = new RectInt(-36, -8, 20, 16);
    private static readonly RectInt KitchenRoom     = new RectInt(18, -6, 16, 12);
    private static readonly RectInt MiniSafeRoom    = new RectInt(23, 16, 7, 7);
    private static readonly RectInt TableRoom       = new RectInt(16, 33, 20, 16);
    private static readonly RectInt PitRoom         = new RectInt(20, -40, 14, 12);

    private static readonly RectInt CorridorWest        = new RectInt(-16, -1, 10, 3);  // Safe -> Slaughter
    private static readonly RectInt CorridorEast        = new RectInt(6, -1, 12, 3);    // Safe -> Kitchen
    private static readonly RectInt CorridorKitchenMini = new RectInt(25, 6, 3, 10);    // Kitchen -> Mini safe
    private static readonly RectInt CorridorMiniTable   = new RectInt(25, 23, 3, 10);   // Mini safe -> Table room

    // =====================================================================
    // SETTINGS
    // =====================================================================
    public string prefabPath = "Assets/Prefabs/Levels/Level_Medium.prefab";
    public bool placeInScene = true;
    public string wallLayerName = "Walls";

    public Sprite floorSprite;
    public Sprite wallSprite;
    public Color floorColor = new Color(0.2f, 0.18f, 0.24f);
    public Color wallColor = new Color(0.08f, 0.07f, 0.1f);

    public GameObject spikePrefab;
    public GameObject pitfallPrefab;
    public GameObject bananaPrefab;
    public GameObject tablePrefab;
    public GameObject riotEnemyPrefab;
    public GameObject riotProjectilePrefab;
    public GameObject hidingSpotPrefab;
    public GameObject spawnerMachinePrefab;
    public GameObject ingredientPrefab;
    public GameObject dishCounterPrefab;
    public GameObject healthDropPrefab;
    public GameObject clownPrefab;
    public GameObject ovenFirePrefab;
    public GameObject entrancePrefab;

    private Vector2 scroll;

    // Build state
    private GameObject root;
    private Sprite whiteSprite;
    private int wallLayer = -1;
    private Transform pitRoomSpawn;
    private Transform pitReturnPoint;

    [MenuItem("Tools/Graveyard Shift/Level Generator")]
    public static void Open()
    {
        GetWindow<GraveyardLevelGenerator>("Level Generator");
    }

    // =====================================================================
    // WINDOW
    // =====================================================================
    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);
        placeInScene = EditorGUILayout.Toggle("Also Place In Scene", placeInScene);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Walls and Floors", EditorStyles.boldLabel);
        wallLayerName = EditorGUILayout.TextField("Wall Layer Name", wallLayerName);
        floorSprite = (Sprite)EditorGUILayout.ObjectField("Floor Sprite (optional)", floorSprite, typeof(Sprite), false);
        wallSprite = (Sprite)EditorGUILayout.ObjectField("Wall Sprite (optional)", wallSprite, typeof(Sprite), false);
        floorColor = EditorGUILayout.ColorField("Placeholder Floor Colour", floorColor);
        wallColor = EditorGUILayout.ColorField("Placeholder Wall Colour", wallColor);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Your Prefabs (leave empty = placeholder)", EditorStyles.boldLabel);
        spikePrefab = Slot("Spike Trap", spikePrefab);
        pitfallPrefab = Slot("Pitfall", pitfallPrefab);
        bananaPrefab = Slot("Banana Skin", bananaPrefab);
        tablePrefab = Slot("Table (with seat)", tablePrefab);
        riotEnemyPrefab = Slot("Riot Enemy", riotEnemyPrefab);
        riotProjectilePrefab = Slot("Riot Projectile", riotProjectilePrefab);
        hidingSpotPrefab = Slot("Hiding Spot", hidingSpotPrefab);
        spawnerMachinePrefab = Slot("Ingredient Spawner Machine", spawnerMachinePrefab);
        ingredientPrefab = Slot("Ingredient Pickup", ingredientPrefab);
        dishCounterPrefab = Slot("Dish Counter", dishCounterPrefab);
        healthDropPrefab = Slot("Health Drop (LifePickup)", healthDropPrefab);
        clownPrefab = Slot("Pop-up Clown", clownPrefab);
        ovenFirePrefab = Slot("Oven Fire", ovenFirePrefab);
        entrancePrefab = Slot("Customer Entrance / Queue", entrancePrefab);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Medium Level Prefab", GUILayout.Height(34)))
            Generate();

        EditorGUILayout.EndScrollView();
    }

    private static GameObject Slot(string label, GameObject value)
    {
        return (GameObject)EditorGUILayout.ObjectField(label, value, typeof(GameObject), false);
    }

    // =====================================================================
    // GENERATE
    // =====================================================================
    private void Generate()
    {
        if (string.IsNullOrWhiteSpace(prefabPath) || !prefabPath.EndsWith(".prefab"))
        {
            Debug.LogError("Level Generator: Prefab Path must end with .prefab");
            return;
        }

        whiteSprite = GetWhiteSprite();
        wallLayer = LayerMask.NameToLayer(wallLayerName);

        if (wallLayer < 0)
            Debug.LogWarning("Level Generator: layer '" + wallLayerName + "' does not exist. Walls will be on Default. Create the layer and regenerate so the player's dash collides with walls.");

        root = new GameObject("Level_Medium");

        try
        {
            Transform floors = Child(root.transform, "Floors");
            Transform walls = Child(root.transform, "Walls");

            Transform safe = Child(root.transform, "SafeRoom");
            Transform kitchen = Child(root.transform, "KitchenRoom");
            Transform mini = Child(root.transform, "MiniSafeRoom");
            Transform table = Child(root.transform, "TableServingRoom");
            Transform slaughter = Child(root.transform, "SlaughterRoom");
            Transform pit = Child(root.transform, "PitFallRoom");
            Transform cWest = Child(root.transform, "Corridor_Safe_to_Slaughter");
            Transform cEast = Child(root.transform, "Corridor_Safe_to_Kitchen");
            Transform cKitchenMini = Child(root.transform, "Corridor_Kitchen_to_MiniSafe");
            Transform cMiniTable = Child(root.transform, "Corridor_MiniSafe_to_Table");

            // Markers needed by several rooms.
            pitRoomSpawn = Marker(pit, "PitRoomSpawn", new Vector2(22.5f, -30.5f));
            pitReturnPoint = Marker(kitchen, "PitReturnPoint", new Vector2(20f, 4.5f));

            BuildFloorsAndWalls(floors, walls);

            BuildSafeRoom(safe);
            BuildCorridorWest(cWest);
            BuildCorridorEast(cEast);
            BuildKitchen(kitchen);
            BuildCorridorKitchenMini(cKitchenMini);
            BuildMiniSafeRoom(mini);
            BuildCorridorMiniTable(cMiniTable);
            BuildTableRoom(table);
            BuildSlaughterRoom(slaughter);
            BuildPitRoom(pit);

            SaveRoot();
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            if (root != null)
                DestroyImmediate(root);
        }
    }

    private void SaveRoot()
    {
        string folder = Path.GetDirectoryName(Path.Combine(ProjectRoot(), prefabPath));
        Directory.CreateDirectory(folder);

        GameObject saved;

        if (placeInScene)
        {
            saved = PrefabUtility.SaveAsPrefabAssetAndConnect(
                root, prefabPath, InteractionMode.UserAction);
        }
        else
        {
            saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();

        if (saved != null)
        {
            Selection.activeObject = saved;
            EditorGUIUtility.PingObject(saved);
        }

        Debug.Log("Level Generator: saved level prefab to " + prefabPath);
    }

    // =====================================================================
    // FLOORS AND WALLS
    // =====================================================================
    private void BuildFloorsAndWalls(Transform floors, Transform walls)
    {
        var rects = new List<(string name, RectInt rect)>
        {
            ("Floor_SafeRoom", SafeRoom),
            ("Floor_SlaughterRoom", SlaughterRoom),
            ("Floor_Kitchen", KitchenRoom),
            ("Floor_MiniSafeRoom", MiniSafeRoom),
            ("Floor_TableRoom", TableRoom),
            ("Floor_PitRoom", PitRoom),
            ("Floor_CorridorWest", CorridorWest),
            ("Floor_CorridorEast", CorridorEast),
            ("Floor_CorridorKitchenMini", CorridorKitchenMini),
            ("Floor_CorridorMiniTable", CorridorMiniTable),
        };

        var floorCells = new HashSet<Vector2Int>();
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

        foreach (var entry in rects)
        {
            RectInt r = entry.rect;

            for (int x = r.xMin; x < r.xMax; x++)
                for (int y = r.yMin; y < r.yMax; y++)
                    floorCells.Add(new Vector2Int(x, y));

            minX = Mathf.Min(minX, r.xMin);
            minY = Mathf.Min(minY, r.yMin);
            maxX = Mathf.Max(maxX, r.xMax);
            maxY = Mathf.Max(maxY, r.yMax);

            Color floorTint = floorSprite != null ? Color.white : floorColor;

            MakeTile(entry.name, floors, r.center, r.size,
                     floorSprite != null ? floorSprite : whiteSprite,
                     floorTint, -10, false);
        }

        // A wall is any empty cell touching a floor cell (including diagonals).
        Func<int, int, bool> isWall = (x, y) =>
        {
            if (floorCells.Contains(new Vector2Int(x, y)))
                return false;

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (floorCells.Contains(new Vector2Int(x + dx, y + dy)))
                        return true;

            return false;
        };

        minX -= 2; minY -= 2; maxX += 2; maxY += 2;

        Sprite wallSpriteToUse = wallSprite != null ? wallSprite : whiteSprite;
        Color wallTint = wallSprite != null ? Color.white : wallColor;

        // Merge wall cells into as few boxes as possible:
        // first into horizontal runs, then stack identical runs upwards.
        var active = new Dictionary<(int x, int len), int>();
        int wallCount = 0;

        Action<int, int, int, int> emit = (x, len, startY, endY) =>
        {
            Vector2 size = new Vector2(len, endY - startY);
            Vector2 center = new Vector2(x + len / 2f, startY + size.y / 2f);
            wallCount++;
            MakeTile("Wall_" + wallCount, walls, center, size, wallSpriteToUse, wallTint, 0, true);
        };

        for (int y = minY; y <= maxY; y++)
        {
            var current = new List<(int x, int len)>();
            int x = minX;

            while (x <= maxX)
            {
                if (isWall(x, y))
                {
                    int start = x;

                    while (x <= maxX && isWall(x, y))
                        x++;

                    current.Add((start, x - start));
                }
                else
                {
                    x++;
                }
            }

            var currentSet = new HashSet<(int x, int len)>(current);

            foreach (var key in active.Keys.ToList())
            {
                if (!currentSet.Contains(key))
                {
                    emit(key.x, key.len, active[key], y);
                    active.Remove(key);
                }
            }

            foreach (var key in current)
            {
                if (!active.ContainsKey(key))
                    active[key] = y;
            }
        }

        foreach (var pair in active)
            emit(pair.Key.x, pair.Key.len, pair.Value, maxY + 1);
    }

    private GameObject MakeTile(string name, Transform parent, Vector2 center, Vector2 size,
                                Sprite sprite, Color color, int order, bool solid)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = center;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = size;

        if (solid)
        {
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = size;

            if (wallLayer >= 0)
                go.layer = wallLayer;
        }

        return go;
    }

    // =====================================================================
    // ROOMS
    // =====================================================================
    private void BuildSafeRoom(Transform parent)
    {
        Marker(parent, "PlayerSpawn", new Vector2(0f, -1.5f));

        // 3 health drops: only the main safe room has them.
        Vector2[] drops = { new Vector2(-3, 3), new Vector2(0, 3), new Vector2(3, 3) };

        for (int i = 0; i < drops.Length; i++)
        {
            GameObject go = Create(healthDropPrefab, "HealthDrop_" + (i + 1), parent, drops[i],
                                   new Vector2(0.6f, 0.6f), new Color(1f, 0.35f, 0.5f));

            EnsureCollider(go, new Vector2(0.8f, 0.8f), true);

            if (healthDropPrefab == null && go.GetComponentInChildren<LifePickup>() == null)
                go.AddComponent<LifePickup>();
        }
    }

    private void BuildMiniSafeRoom(Transform parent)
    {
        // Rest area only: no health drops here.
        Marker(parent, "MiniSafeRoomCentre", MiniSafeRoom.center);
    }

    private void BuildKitchen(Transform parent)
    {
        // Dish counter (dish collection).
        Create(dishCounterPrefab, "DishCounter", parent, new Vector2(30f, 4.8f),
               new Vector2(4f, 1.2f), new Color(0.75f, 0.75f, 0.8f));

        // Static pitfalls.
        SpawnPit(parent, "Pit_Static_1", new Vector2(22.5f, -2f));
        SpawnPit(parent, "Pit_Static_2", new Vector2(25.5f, 2.5f));
        SpawnPit(parent, "Pit_Static_3", new Vector2(28.5f, -1.5f));

        // Moving pitfalls.
        GameObject movingA = SpawnPit(parent, "Pit_Moving_Horizontal", new Vector2(21f, -4f));
        MakeMoving(movingA, parent, 2f, 0.3f, new Vector2(21f, -4f), new Vector2(31f, -4f));

        GameObject movingB = SpawnPit(parent, "Pit_Moving_Vertical", new Vector2(32f, -3f));
        MakeMoving(movingB, parent, 1.8f, 0.3f, new Vector2(32f, -3f), new Vector2(32f, 3f));
    }

    private void BuildTableRoom(Transform parent)
    {
        // 6 tables, 1 seat each.
        Vector2[] tables =
        {
            new Vector2(20f, 37f), new Vector2(33f, 37f),
            new Vector2(20f, 42f), new Vector2(33f, 42f),
            new Vector2(24.5f, 40.5f), new Vector2(28.5f, 40.5f)
        };

        for (int i = 0; i < tables.Length; i++)
        {
            GameObject t = Create(tablePrefab, "Table_" + (i + 1), parent, tables[i],
                                  new Vector2(2f, 2f), new Color(0.45f, 0.3f, 0.18f));

            if (tablePrefab == null)
            {
                GameObject seat = new GameObject("Seat");
                seat.transform.SetParent(t.transform, false);
                seat.transform.localPosition = new Vector3(0f, -1.4f, 0f);
                AddVisual(seat.transform, new Vector2(0.7f, 0.7f), new Color(0.6f, 0.5f, 0.4f), 5);
            }
        }

        // Customer entrance with the queue, visible from inside the room.
        Create(entrancePrefab, "CustomerEntrance_Queue", parent, new Vector2(26f, 47.6f),
               new Vector2(4f, 1.2f), new Color(0.35f, 0.45f, 0.6f));

        // More spike traps than banana skins in this room.
        Vector2[] spikes =
        {
            new Vector2(22.5f, 35f), new Vector2(23.5f, 35f),
            new Vector2(29.5f, 35f), new Vector2(30.5f, 35f),
            new Vector2(22.5f, 39f), new Vector2(30.5f, 39f),
            new Vector2(26.5f, 44.5f), new Vector2(34f, 44f)
        };

        for (int i = 0; i < spikes.Length; i++)
            SpawnSpike(parent, "Spike_Static_" + (i + 1), spikes[i]);

        GameObject m1 = SpawnSpike(parent, "Spike_Moving_Back", new Vector2(18f, 46f));
        MakeMoving(m1, parent, 2.2f, 0.2f, new Vector2(18f, 46f), new Vector2(34f, 46f));

        GameObject m2 = SpawnSpike(parent, "Spike_Moving_Gate", new Vector2(26.5f, 35.5f));
        MakeMoving(m2, parent, 1.8f, 0.2f, new Vector2(26.5f, 35.5f), new Vector2(26.5f, 38.5f));

        SpawnBanana(parent, "Banana_1", new Vector2(18f, 40f));
        SpawnBanana(parent, "Banana_2", new Vector2(34f, 40f));
        SpawnBanana(parent, "Banana_3", new Vector2(26.5f, 43.5f));
    }

    private void BuildSlaughterRoom(Transform parent)
    {
        Vector2 roomCenter = SlaughterRoom.center;

        // Ingredient spawner machine.
        Vector2 machinePos = new Vector2(-33.5f, 0f);
        GameObject machine = Create(spawnerMachinePrefab, "IngredientSpawnerMachine", parent, machinePos,
                                    new Vector2(1.6f, 1.6f), new Color(0.2f, 0.8f, 0.8f));
        EnsureCollider(machine, new Vector2(3.5f, 3.5f), true);

        IngredientSpawnerMachine machineScript = Ensure<IngredientSpawnerMachine>(machine);
        SetRef(machineScript, "ingredientPrefab", GetIngredientComponent());
        SetVector2(machineScript, "areaOffset", roomCenter - machinePos);
        SetVector2(machineScript, "areaSize", new Vector2(SlaughterRoom.width - 4, SlaughterRoom.height - 4));

        // 4 riots.
        Vector2[] riotPositions =
        {
            new Vector2(-31f, 5f), new Vector2(-21f, 5f),
            new Vector2(-31f, -5f), new Vector2(-21f, -5f)
        };

        RiotProjectile projectile = GetProjectileComponent();
        var enemies = new List<UnityEngine.Object>();

        for (int i = 0; i < riotPositions.Length; i++)
        {
            GameObject go = Create(riotEnemyPrefab, "Riot_" + (i + 1), parent, riotPositions[i],
                                   new Vector2(1.1f, 1.1f), new Color(0.3f, 0.75f, 0.3f));

            if (riotEnemyPrefab == null)
            {
                Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                CircleCollider2D col = go.AddComponent<CircleCollider2D>();
                col.radius = 0.5f;
                col.isTrigger = true;
            }

            RiotEnemy enemy = Ensure<RiotEnemy>(go);
            SetFloat(enemy, "chaseSpeed", 2.5f);
            SetFloat(enemy, "returnSpeed", 4f);
            SetFloat(enemy, "stopDistance", 4.5f);
            SetFloat(enemy, "shootInterval", 2.2f + i * 0.3f);
            SetRefIfNull(enemy, "projectilePrefab", projectile);

            enemies.Add(enemy);
        }

        // Room trigger: player enters -> riots chase, player leaves -> riots reset.
        GameObject zone = new GameObject("SlaughterRoomZone");
        zone.transform.SetParent(parent, false);
        zone.transform.position = roomCenter;

        BoxCollider2D zoneBox = zone.AddComponent<BoxCollider2D>();
        zoneBox.isTrigger = true;
        zoneBox.size = new Vector2(SlaughterRoom.width - 1, SlaughterRoom.height - 1);

        SlaughterRoomZone zoneScript = zone.AddComponent<SlaughterRoomZone>();
        SetObjectArray(zoneScript, "enemies", enemies);

        // Hiding spots.
        Vector2[] hiding =
        {
            new Vector2(-29f, 3f), new Vector2(-24f, -3f), new Vector2(-33f, -5.5f),
            new Vector2(-19f, 5.5f), new Vector2(-26f, 6.5f)
        };

        for (int i = 0; i < hiding.Length; i++)
        {
            Create(hidingSpotPrefab, "HidingSpot_" + (i + 1), parent, hiding[i],
                   new Vector2(2f, 1.3f), new Color(0.25f, 0.3f, 0.4f));
        }
    }

    private void BuildPitRoom(Transform parent)
    {
        // Oven fires.
        Vector2[] ovens = { new Vector2(25f, -33f), new Vector2(28f, -35.5f), new Vector2(30.5f, -31.5f) };

        for (int i = 0; i < ovens.Length; i++)
        {
            GameObject go = Create(ovenFirePrefab, "OvenFire_" + (i + 1), parent, ovens[i],
                                   new Vector2(1.8f, 1.8f), new Color(1f, 0.45f, 0.1f));
            EnsureCollider(go, new Vector2(1.6f, 1.6f), true);

            CyclingHazard hazard = Ensure<CyclingHazard>(go);
            SetFloat(hazard, "activeDuration", 1.5f);
            SetFloat(hazard, "inactiveDuration", 2.5f);
            SetFloat(hazard, "startOffset", i * 1.2f);
        }

        // Pop-up clowns.
        Vector2[] clowns = { new Vector2(27f, -30.5f), new Vector2(24.5f, -37f) };

        for (int i = 0; i < clowns.Length; i++)
            SpawnClown(parent, "PopUpClown_" + (i + 1), clowns[i], i * 1f);

        // Escape route: reach the ladder to climb back out.
        GameObject exit = new GameObject("PitRoomExit");
        exit.transform.SetParent(parent, false);
        exit.transform.position = new Vector2(32f, -37.5f);
        AddVisual(exit.transform, new Vector2(1.6f, 1.6f), new Color(0.75f, 0.65f, 0.4f), 5);

        BoxCollider2D exitBox = exit.AddComponent<BoxCollider2D>();
        exitBox.isTrigger = true;
        exitBox.size = new Vector2(2f, 2f);

        PitRoomExit exitScript = exit.AddComponent<PitRoomExit>();
        SetRef(exitScript, "returnPoint", pitReturnPoint);
        SetFloat(exitScript, "minimumStayTime", 4f);
        SetBool(exitScript, "requireInteract", true);
    }

    // =====================================================================
    // CORRIDORS
    // =====================================================================
    private void BuildCorridorWest(Transform parent)
    {
        SpawnSpike(parent, "Spike_Static_1", new Vector2(-8.5f, -0.5f));
        SpawnSpike(parent, "Spike_Static_2", new Vector2(-13f, 1.5f));

        GameObject moving = SpawnSpike(parent, "Spike_Moving", new Vector2(-11.5f, -0.5f));
        MakeMoving(moving, parent, 2.5f, 0.2f, new Vector2(-11.5f, -0.5f), new Vector2(-11.5f, 1.5f));

        SpawnBanana(parent, "Banana_1", new Vector2(-14.5f, 0.5f));
    }

    private void BuildCorridorEast(Transform parent)
    {
        SpawnSpike(parent, "Spike_Static_1", new Vector2(9.5f, -0.5f));
        SpawnSpike(parent, "Spike_Static_2", new Vector2(12f, 1.5f));
        SpawnClown(parent, "PopUpClown_1", new Vector2(15.5f, 0.5f), 0.5f);
        SpawnBanana(parent, "Banana_1", new Vector2(7.5f, 0.5f));
    }

    private void BuildCorridorKitchenMini(Transform parent)
    {
        SpawnBanana(parent, "Banana_1", new Vector2(25.5f, 7.5f));
        SpawnClown(parent, "PopUpClown_1", new Vector2(27.5f, 9.5f), 0f);

        GameObject moving = SpawnSpike(parent, "Spike_Moving", new Vector2(25.5f, 11.5f));
        MakeMoving(moving, parent, 2.2f, 0.2f, new Vector2(25.5f, 11.5f), new Vector2(27.5f, 11.5f));

        SpawnSpike(parent, "Spike_Static_1", new Vector2(26.5f, 14f));
    }

    private void BuildCorridorMiniTable(Transform parent)
    {
        SpawnClown(parent, "PopUpClown_1", new Vector2(26.5f, 24f), 1f);
        SpawnSpike(parent, "Spike_Static_1", new Vector2(25.5f, 25.5f));
        SpawnSpike(parent, "Spike_Static_2", new Vector2(27.5f, 27f));
        SpawnBanana(parent, "Banana_1", new Vector2(26.5f, 29f));

        GameObject moving = SpawnSpike(parent, "Spike_Moving", new Vector2(25.5f, 31f));
        MakeMoving(moving, parent, 2.2f, 0.2f, new Vector2(25.5f, 31f), new Vector2(27.5f, 31f));
    }

    // =====================================================================
    // TRAP SPAWNERS
    // =====================================================================
    private GameObject SpawnSpike(Transform parent, string name, Vector2 pos)
    {
        GameObject go = Create(spikePrefab, name, parent, pos, new Vector2(0.9f, 0.9f), new Color(0.85f, 0.15f, 0.15f));
        EnsureCollider(go, new Vector2(0.9f, 0.9f), true);
        Ensure<SpikeTrap>(go);
        return go;
    }

    private GameObject SpawnPit(Transform parent, string name, Vector2 pos)
    {
        GameObject go = Create(pitfallPrefab, name, parent, pos, new Vector2(1.6f, 1.6f), new Color(0.02f, 0.02f, 0.02f));
        EnsureCollider(go, new Vector2(1.4f, 1.4f), true);

        PitfallTrap trap = Ensure<PitfallTrap>(go);
        SetRef(trap, "pitRoomSpawn", pitRoomSpawn);
        return go;
    }

    private GameObject SpawnBanana(Transform parent, string name, Vector2 pos)
    {
        GameObject go = Create(bananaPrefab, name, parent, pos, new Vector2(0.6f, 0.6f), new Color(1f, 0.9f, 0.2f));
        EnsureCollider(go, new Vector2(0.7f, 0.7f), true);
        Ensure<BananaPeel>(go);
        return go;
    }

    private GameObject SpawnClown(Transform parent, string name, Vector2 pos, float offset)
    {
        GameObject go = Create(clownPrefab, name, parent, pos, new Vector2(1.3f, 1.3f), new Color(0.7f, 0.25f, 0.85f));
        EnsureCollider(go, new Vector2(1.2f, 1.2f), true);

        CyclingHazard hazard = Ensure<CyclingHazard>(go);
        SetFloat(hazard, "activeDuration", 1f);
        SetFloat(hazard, "inactiveDuration", 2f);
        SetFloat(hazard, "startOffset", offset);
        return go;
    }

    // Turns a spike or pit into a moving one that patrols between the points.
    private void MakeMoving(GameObject go, Transform parent, float speed, float wait, params Vector2[] points)
    {
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = go.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        // Waypoints must NOT be children of the mover.
        Transform waypointRoot = Child(parent, go.name + "_Waypoints");
        var waypoints = new List<UnityEngine.Object>();

        for (int i = 0; i < points.Length; i++)
            waypoints.Add(Marker(waypointRoot, "WP_" + (i + 1), points[i]));

        PitfallMover mover = Ensure<PitfallMover>(go);
        SetObjectArray(mover, "waypoints", waypoints);
        SetFloat(mover, "speed", speed);
        SetFloat(mover, "waitAtPoint", wait);
        SetBool(mover, "pingPong", true);

        go.transform.position = points[0];
    }

    // =====================================================================
    // OBJECT HELPERS
    // =====================================================================
    // Instantiates your prefab, or builds a coloured placeholder if the slot is empty.
    private GameObject Create(GameObject prefab, string name, Transform parent, Vector2 pos,
                              Vector2 placeholderSize, Color placeholderColor)
    {
        GameObject go;

        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        }
        else
        {
            go = new GameObject(name);
            go.transform.SetParent(parent, false);
            AddVisual(go.transform, placeholderSize, placeholderColor, 5);
        }

        go.name = name;
        go.transform.position = pos;
        return go;
    }

    private void AddVisual(Transform parent, Vector2 size, Color color, int order)
    {
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(parent, false);
        visual.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = whiteSprite;
        sr.color = color;
        sr.sortingOrder = order;
    }

    // Adds a collider if the object has none. Colliders MUST exist before
    // adding scripts that use [RequireComponent(typeof(Collider2D))].
    private void EnsureCollider(GameObject go, Vector2 size, bool trigger)
    {
        Collider2D existing = go.GetComponentInChildren<Collider2D>();

        if (existing == null)
        {
            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.isTrigger = trigger;
        }
        else if (trigger)
        {
            existing.isTrigger = true;
        }
    }

    private static T Ensure<T>(GameObject go) where T : Component
    {
        T existing = go.GetComponentInChildren<T>();
        return existing != null ? existing : go.AddComponent<T>();
    }

    private static Transform Child(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Transform Marker(Transform parent, string name, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        try
        {
            Texture2D icon = EditorGUIUtility.IconContent("sv_label_3").image as Texture2D;

            if (icon != null)
                EditorGUIUtility.SetIconForObject(go, icon);
        }
        catch (Exception)
        {
            // Icons are only a visual aid.
        }

        return go.transform;
    }

    // =====================================================================
    // PLACEHOLDER PREFAB ASSETS (ingredient + projectile need to be assets)
    // =====================================================================
    private IngredientPickup GetIngredientComponent()
    {
        if (ingredientPrefab != null)
        {
            IngredientPickup own = ingredientPrefab.GetComponent<IngredientPickup>();

            if (own == null)
                Debug.LogWarning("Level Generator: Ingredient prefab has no IngredientPickup component.");

            return own;
        }

        const string path = "Assets/Generated/PlaceholderIngredient.prefab";
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (asset == null)
        {
            GameObject go = new GameObject("PlaceholderIngredient");
            AddVisual(go.transform, new Vector2(0.5f, 0.5f), new Color(0.4f, 1f, 0.4f), 6);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            go.AddComponent<IngredientPickup>();

            asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
        }

        return asset.GetComponent<IngredientPickup>();
    }

    private RiotProjectile GetProjectileComponent()
    {
        if (riotProjectilePrefab != null)
        {
            RiotProjectile own = riotProjectilePrefab.GetComponent<RiotProjectile>();

            if (own == null)
                Debug.LogWarning("Level Generator: Riot Projectile prefab has no RiotProjectile component.");

            return own;
        }

        const string path = "Assets/Generated/PlaceholderRiotProjectile.prefab";
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (asset == null)
        {
            GameObject go = new GameObject("PlaceholderRiotProjectile");
            AddVisual(go.transform, new Vector2(0.4f, 0.4f), new Color(0.5f, 0.4f, 0.3f), 6);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.25f;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            go.AddComponent<RiotProjectile>();

            asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
        }

        return asset.GetComponent<RiotProjectile>();
    }

    // Generates a 1x1 unit white square sprite once, used by all placeholders.
    private static Sprite GetWhiteSprite()
    {
        const string path = "Assets/Generated/GeneratedWhite.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (existing != null)
            return existing;

        string fullPath = Path.Combine(ProjectRoot(), path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[16];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);

        tex.SetPixels32(pixels);
        tex.Apply();

        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 4f;   // 4 pixels = exactly 1 world unit
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;   // needed for tiled drawing
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static string ProjectRoot()
    {
        return Directory.GetParent(Application.dataPath).FullName;
    }

    // =====================================================================
    // SERIALIZED FIELD HELPERS (set private [SerializeField] values)
    // =====================================================================
    private static SerializedProperty Prop(UnityEngine.Object target, string name, out SerializedObject so)
    {
        so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(name);

        if (p == null)
            Debug.LogWarning("Level Generator: field '" + name + "' not found on " + target.GetType().Name);

        return p;
    }

    private static void SetFloat(UnityEngine.Object target, string name, float value)
    {
        SerializedProperty p = Prop(target, name, out SerializedObject so);
        if (p == null) return;
        p.floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(UnityEngine.Object target, string name, bool value)
    {
        SerializedProperty p = Prop(target, name, out SerializedObject so);
        if (p == null) return;
        p.boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector2(UnityEngine.Object target, string name, Vector2 value)
    {
        SerializedProperty p = Prop(target, name, out SerializedObject so);
        if (p == null) return;
        p.vector2Value = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRef(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        SerializedProperty p = Prop(target, name, out SerializedObject so);
        if (p == null) return;
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRefIfNull(UnityEngine.Object target, string name, UnityEngine.Object value)
    {
        SerializedProperty p = Prop(target, name, out SerializedObject so);
        if (p == null || p.objectReferenceValue != null) return;
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(UnityEngine.Object target, string name, List<UnityEngine.Object> values)
    {
        SerializedProperty p = Prop(target, name, out SerializedObject so);
        if (p == null) return;

        p.arraySize = values.Count;

        for (int i = 0; i < values.Count; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
