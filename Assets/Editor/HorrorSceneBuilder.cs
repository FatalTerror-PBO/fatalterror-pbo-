using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using TMPro;
using UISlider = UnityEngine.UI.Slider;
using UIImage = UnityEngine.UI.Image;
using UIButton = UnityEngine.UI.Button;
using UICanvasScaler = UnityEngine.UI.CanvasScaler;
using UIGraphicRaycaster = UnityEngine.UI.GraphicRaycaster;
using HorrorGame.Audio;
using HorrorGame.Core;
using HorrorGame.Effects;
using HorrorGame.Enemies;
using HorrorGame.Interactables;
using HorrorGame.Items;
using HorrorGame.Player;
using HorrorGame.UI;

/// <summary>
/// Membangun scene demo horor otomatis: rumah sakit sederhana (dari primitif), pemain, musuh,
/// item, pintu, UI, suara, dan NavMesh. Menu: Horror > ...
/// </summary>
public static class HorrorSceneBuilder
{
    private const string Root = "Assets/HorrorDemo";
    private const string AudioDir = Root + "/Audio";
    private const string MatDir = Root + "/Materials";
    private const string SceneDir = Root + "/Scenes";

    private const float WallH = 3f;
    private const float WallT = 0.3f;
    private const float DoorW = 1.6f;
    private const float DoorH = 2.5f;

    private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

    // ------------------------------------------------------------------ MENU

    [MenuItem("Horror/0. Set Input Handling to Both")]
    public static void MenuSetInput()
    {
        if (EnsureInputBoth())
            EditorUtility.DisplayDialog("Input", "Active Input Handling diubah ke Both.\nRestart Unity Editor agar berlaku.", "OK");
        else
            EditorUtility.DisplayDialog("Input", "Sudah Both (atau tidak bisa diubah otomatis).\nCek: Edit > Project Settings > Player > Active Input Handling.", "OK");
    }

    [MenuItem("Horror/1. Build Complete Demo Scene")]
    public static void BuildDemoScene()
    {
        if (!CheckTmp()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        bool inputChanged = EnsureInputBoth();

        EnsureFolders();
        materials.Clear();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        SetupAtmosphere();

        Transform level = new GameObject("Level").transform;
        BuildLevelGeometry(level);
        BuildProps(level);

        bool navOk = BakeNavMesh();

        BuildCeiling(level);
        BuildDoors(level);
        BuildLights(level);

        GameObject player = BuildPlayer(new Vector3(-13f, 1.1f, 0f));
        BuildManagersAndUi(player, CreateSpawnPoints());
        BuildItems();
        BuildEnemies();

        string scenePath = SceneDir + "/HorrorDemo.unity";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), scenePath);
        AddToBuildSettings(scenePath);
        AssetDatabase.SaveAssets();

        string msg = "Scene dibuat: " + scenePath + "\n\nTekan Play. Kontrol: WASD, Shift, F, E, Esc.";
        if (!navOk) msg += "\n\nPERINGATAN: NavMesh gagal dibake otomatis. Pasang package AI Navigation, lalu buat objek kosong, tambah NavMeshSurface, klik Bake.";
        if (inputChanged) msg += "\n\nPenting: Active Input Handling baru diubah ke Both. RESTART Unity dulu sebelum Play.";
        EditorUtility.DisplayDialog("Horror Demo", msg, "OK");
    }

    [MenuItem("Horror/2. Add Player + Managers + UI to Current Scene")]
    public static void AddCoreToCurrentScene()
    {
        if (!CheckTmp()) return;

        bool inputChanged = EnsureInputBoth();
        EnsureFolders();
        materials.Clear();

        Vector3 pos = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.up;
        pos.y += 1.1f;

        GameObject player = BuildPlayer(pos);
        BuildManagersAndUi(player, new List<Transform>());

        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        string msg = "Player, Managers, dan UI ditambahkan di posisi pivot Scene view.\n\nUntuk map sendiri: bake NavMesh (NavMeshSurface) lalu taruh musuh/item manual.";
        if (inputChanged) msg += "\n\nPenting: restart Unity karena Input Handling baru diubah.";
        EditorUtility.DisplayDialog("Horror", msg, "OK");
    }

    // ------------------------------------------------------------ PREPARATION

    private static bool CheckTmp()
    {
        if (TMP_Settings.instance != null) return true;

        if (EditorUtility.DisplayDialog("TextMeshPro",
                "TMP Essential Resources belum diimpor.\nImpor sekarang, lalu jalankan menu Horror lagi.", "Impor", "Batal"))
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        }
        return false;
    }

    private static bool EnsureInputBoth()
    {
        try
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return false;

            SerializedObject so = new SerializedObject(assets[0]);
            SerializedProperty p = so.FindProperty("activeInputHandler");
            if (p == null || p.intValue == 2) return false;

            p.intValue = 2;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Gagal mengubah Input Handling otomatis: " + e.Message);
            return false;
        }
    }

    private static void EnsureFolders()
    {
        foreach (string dir in new[] { Root, MatDir, SceneDir, Root + "/Prefabs" })
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string parent = System.IO.Path.GetDirectoryName(dir).Replace('\\', '/');
                AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(dir));
            }
        }
    }

    private static void SetupAtmosphere()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.07f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.07f;
        RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.03f);
    }

    private static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene s in list) if (s.path == path) return;
        list.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    // ------------------------------------------------------------- MATERIALS

    private static Material Mat(string name, Color color, float emission = 0f)
    {
        if (materials.TryGetValue(name, out Material cached)) return cached;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material m = new Material(shader);
        m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.15f);

        if (emission > 0f)
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            m.SetColor("_EmissionColor", color * emission);
        }

        string path = MatDir + "/" + name + ".mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);

        materials[name] = m;
        return m;
    }

    private static Material Floor => Mat("Floor", new Color(0.16f, 0.17f, 0.17f));
    private static Material Wall => Mat("Wall", new Color(0.42f, 0.45f, 0.42f));
    private static Material Ceil => Mat("Ceiling", new Color(0.25f, 0.26f, 0.25f));
    private static Material Metal => Mat("Metal", new Color(0.30f, 0.32f, 0.34f));
    private static Material Wood => Mat("Wood", new Color(0.30f, 0.20f, 0.12f));
    private static Material Sheet => Mat("Sheet", new Color(0.68f, 0.68f, 0.62f));
    private static Material Blood => Mat("Blood", new Color(0.30f, 0.02f, 0.02f));
    private static Material DoorMat => Mat("Door", new Color(0.24f, 0.17f, 0.12f));

    // ------------------------------------------------------------ PRIMITIVES

    private static GameObject Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static GameObject Empty(string name, Vector3 pos, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        return go;
    }

    // -------------------------------------------------------------- LEVEL

    private static void BuildLevelGeometry(Transform level)
    {
        Box("Floor", new Vector3(0f, -0.15f, 0f), new Vector3(30.6f, 0.3f, 20.6f), Floor, level);

        // Dinding luar
        WallAlongX(level, 10f, -15.15f, 15.15f, new float[0]);
        WallAlongX(level, -10f, -15.15f, 15.15f, new float[0]);
        WallAlongZ(level, -15f, -10f, 10f, new float[0]);
        WallAlongZ(level, 15f, -10f, 10f, new[] { 0f });   // celah pintu keluar

        // Dinding dalam: koridor (z -1.5..1.5), 3 ruangan di utara dan selatan
        float[] doorXs = { -10f, 0f, 10f };
        WallAlongX(level, 1.5f, -15f, 15f, doorXs);
        WallAlongX(level, -1.5f, -15f, 15f, doorXs);

        WallAlongZ(level, -5f, 1.5f, 10f, new float[0]);
        WallAlongZ(level, 5f, 1.5f, 10f, new float[0]);
        WallAlongZ(level, -5f, -10f, -1.5f, new float[0]);
        WallAlongZ(level, 5f, -10f, -1.5f, new float[0]);
    }

    private static void WallAlongX(Transform parent, float z, float x0, float x1, float[] gaps)
    {
        System.Array.Sort(gaps);
        float cur = x0;
        foreach (float g in gaps)
        {
            float a = g - DoorW * 0.5f;
            if (a > cur) WallSegX(parent, z, cur, a);
            cur = g + DoorW * 0.5f;
            Box("Lintel", new Vector3(g, (DoorH + WallH) * 0.5f, z), new Vector3(DoorW, WallH - DoorH, WallT), Wall, parent);
        }
        if (x1 > cur) WallSegX(parent, z, cur, x1);
    }

    private static void WallSegX(Transform parent, float z, float a, float b)
    {
        Box("Wall", new Vector3((a + b) * 0.5f, WallH * 0.5f, z), new Vector3(b - a, WallH, WallT), Wall, parent);
    }

    private static void WallAlongZ(Transform parent, float x, float z0, float z1, float[] gaps)
    {
        System.Array.Sort(gaps);
        float cur = z0;
        foreach (float g in gaps)
        {
            float a = g - DoorW * 0.5f;
            if (a > cur) WallSegZ(parent, x, cur, a);
            cur = g + DoorW * 0.5f;
            Box("Lintel", new Vector3(x, (DoorH + WallH) * 0.5f, g), new Vector3(WallT, WallH - DoorH, DoorW), Wall, parent);
        }
        if (z1 > cur) WallSegZ(parent, x, cur, z1);
    }

    private static void WallSegZ(Transform parent, float x, float a, float b)
    {
        Box("Wall", new Vector3(x, WallH * 0.5f, (a + b) * 0.5f), new Vector3(WallT, WallH, b - a), Wall, parent);
    }

    private static void BuildCeiling(Transform level)
    {
        Box("Ceiling", new Vector3(0f, WallH + 0.15f, 0f), new Vector3(30.6f, 0.3f, 20.6f), Ceil, level);
    }

    // -------------------------------------------------------------- PROPS

    private static void BuildProps(Transform level)
    {
        Transform props = new GameObject("Props").transform;
        props.SetParent(level, false);

        // Tempat tidur
        Bed(props, new Vector3(-13.2f, 0f, 4.5f), 90f);
        Bed(props, new Vector3(-13.2f, 0f, 7.5f), 90f);
        Bed(props, new Vector3(13.2f, 0f, 4.5f), 90f);
        Bed(props, new Vector3(13.2f, 0f, 7.5f), 90f);
        Bed(props, new Vector3(-3.5f, 0f, -6.5f), 0f);
        Bed(props, new Vector3(13.2f, 0f, -6f), 90f);

        // Meja (item di atasnya)
        foreach (Vector3 p in TablePositions) Table(props, p);

        // Lemari
        Cabinet(props, new Vector3(-6f, 0f, 6f));
        Cabinet(props, new Vector3(4.3f, 0f, 5f));
        Cabinet(props, new Vector3(-14.3f, 0f, -4f));
        Cabinet(props, new Vector3(6f, 0f, -5f));

        // Peti
        Crate(props, new Vector3(3f, 0f, 8.5f), 15f);
        Crate(props, new Vector3(-7f, 0f, -8.5f), 40f);
        Crate(props, new Vector3(-6.4f, 0f, -7.7f), 5f);
        Crate(props, new Vector3(-13.5f, 0f, 1f), 20f);
        Crate(props, new Vector3(7f, 0f, -8.6f), 60f);
        Crate(props, new Vector3(13f, 0f, 1f), 10f);

        // Noda darah di lantai
        Box("Blood", new Vector3(-2f, 0.01f, 5f), new Vector3(1.6f, 0.02f, 1.1f), Blood, props);
        Box("Blood", new Vector3(11f, 0.01f, -4f), new Vector3(1.2f, 0.02f, 1.8f), Blood, props);
        Box("Blood", new Vector3(-9f, 0.01f, -5f), new Vector3(1.8f, 0.02f, 0.9f), Blood, props);
        Box("Blood", new Vector3(6f, 0.01f, 0.3f), new Vector3(0.9f, 0.02f, 0.9f), Blood, props);
    }

    private static readonly Vector3[] TablePositions =
    {
        new Vector3(-8f, 0f, 9f),      // A  : baterai
        new Vector3(-3f, 0f, 8.7f),    // B  : medkit
        new Vector3(9f, 0f, 8.7f),     // C  : catatan
        new Vector3(-12f, 0f, -8.5f),  // D  : kunci
        new Vector3(2.5f, 0f, -8.5f),  // E  : baterai
        new Vector3(11f, 0f, -8.5f),   // F  : catatan
    };

    private static void Bed(Transform parent, Vector3 pos, float rotY)
    {
        GameObject root = Empty("Bed", pos, parent);
        Quaternion rot = Quaternion.Euler(0f, rotY, 0f);
        root.transform.rotation = rot;

        GameObject frame = Box("Frame", pos + rot * new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.4f, 2.0f), Metal, root.transform);
        GameObject mattress = Box("Mattress", pos + rot * new Vector3(0f, 0.58f, 0f), new Vector3(0.8f, 0.16f, 1.9f), Sheet, root.transform);
        GameObject pillow = Box("Pillow", pos + rot * new Vector3(0f, 0.72f, -0.75f), new Vector3(0.55f, 0.12f, 0.35f), Sheet, root.transform);
        frame.transform.rotation = rot;
        mattress.transform.rotation = rot;
        pillow.transform.rotation = rot;
    }

    private static void Table(Transform parent, Vector3 pos)
    {
        GameObject root = Empty("Table", pos, parent);
        Box("Top", pos + new Vector3(0f, 0.8f, 0f), new Vector3(1.2f, 0.08f, 0.7f), Wood, root.transform);
        foreach (Vector2 o in new[] { new Vector2(-0.52f, -0.28f), new Vector2(0.52f, -0.28f), new Vector2(-0.52f, 0.28f), new Vector2(0.52f, 0.28f) })
            Box("Leg", pos + new Vector3(o.x, 0.38f, o.y), new Vector3(0.07f, 0.76f, 0.07f), Wood, root.transform);
    }

    private static void Cabinet(Transform parent, Vector3 pos)
    {
        Box("Cabinet", pos + new Vector3(0f, 0.95f, 0f), new Vector3(0.9f, 1.9f, 0.5f), Metal, parent);
    }

    private static void Crate(Transform parent, Vector3 pos, float rotY)
    {
        GameObject c = Box("Crate", pos + new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.8f, 0.8f), Wood, parent);
        c.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    // -------------------------------------------------------------- NAVMESH

    private static bool BakeNavMesh()
    {
        try
        {
            System.Type t = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (t == null)
            {
                Debug.LogWarning("Package AI Navigation tidak ditemukan. Pasang lewat Package Manager.");
                return false;
            }

            Physics.SyncTransforms();
            GameObject go = new GameObject("NavMesh");
            Component surface = go.AddComponent(t);
            t.GetMethod("BuildNavMesh").Invoke(surface, null);

            Object data = t.GetProperty("navMeshData").GetValue(surface, null) as Object;
            if (data == null) return false;

            string path = SceneDir + "/HorrorDemo_NavMesh.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(data, path);
            EditorUtility.SetDirty(surface);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Bake NavMesh gagal: " + e.Message);
            return false;
        }
    }

    // --------------------------------------------------------------- DOORS

    private static void BuildDoors(Transform level)
    {
        Transform doors = new GameObject("Doors").transform;
        doors.SetParent(level, false);

        foreach (float x in new[] { -10f, 0f, 10f })
        {
            RegularDoor(doors, new Vector3(x, 0f, 1.5f), -90f);   // membuka ke ruangan utara
            RegularDoor(doors, new Vector3(x, 0f, -1.5f), 90f);   // membuka ke ruangan selatan
        }

        // Pintu keluar (dinding timur), butuh kunci
        GameObject exit = Empty("ExitDoor", new Vector3(15f, 0f, -DoorW * 0.5f), doors);
        exit.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        GameObject leaf = Box("Leaf", Vector3.zero, new Vector3(DoorW, DoorH, 0.15f), Mat("ExitDoor", new Color(0.12f, 0.2f, 0.12f)), exit.transform);
        leaf.transform.localPosition = new Vector3(DoorW * 0.5f, DoorH * 0.5f, 0f);
        leaf.transform.localRotation = Quaternion.identity;

        Door door = exit.AddComponent<Door>();
        SetEnum(door, "requiredItem", (int)ItemType.Key);
        SetBool(door, "isExit", true);
        SetString(door, "prompt", "Tekan E untuk membuka pintu keluar");

        GameObject sign = Box("ExitSign", new Vector3(14.7f, 2.72f, 0f), new Vector3(0.1f, 0.2f, 0.9f), Mat("ExitSign", new Color(0.1f, 1f, 0.2f), 2.5f), doors);
        Object.DestroyImmediate(sign.GetComponent<Collider>());

        GameObject lightGo = Empty("ExitLight", new Vector3(13.6f, 2.3f, 0f), doors);
        Light l = lightGo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(0.2f, 1f, 0.3f); l.range = 6f; l.intensity = 3f;
    }

    private static void RegularDoor(Transform parent, Vector3 gapCenter, float openAngle)
    {
        GameObject hinge = Empty("Door", gapCenter + new Vector3(-DoorW * 0.5f, 0f, 0f), parent);
        GameObject leaf = Box("Leaf", Vector3.zero, new Vector3(DoorW, DoorH, 0.12f), DoorMat, hinge.transform);
        leaf.transform.localPosition = new Vector3(DoorW * 0.5f, DoorH * 0.5f, 0f);
        leaf.transform.localRotation = Quaternion.identity;

        Door door = hinge.AddComponent<Door>();
        SetFloat(door, "openAngle", openAngle);
        SetString(door, "prompt", "Tekan E untuk membuka pintu");
    }

    // -------------------------------------------------------------- LIGHTS

    private static void BuildLights(Transform level)
    {
        Transform lights = new GameObject("Lights").transform;
        lights.SetParent(level, false);

        // posisi, warna, intensitas, berkedip?
        Lamp(lights, new Vector3(-10f, 2.9f, 5.75f), new Color(1f, 0.75f, 0.45f), 4f, false);
        Lamp(lights, new Vector3(0f, 2.9f, 5.75f), new Color(0.7f, 0.9f, 1f), 4f, true);
        Lamp(lights, new Vector3(10f, 2.9f, 5.75f), new Color(0.6f, 1f, 0.7f), 3.5f, false);
        Lamp(lights, new Vector3(-10f, 2.9f, -5.75f), new Color(1f, 0.55f, 0.35f), 3.5f, true);
        Lamp(lights, new Vector3(0f, 2.9f, -5.75f), new Color(0.8f, 0.85f, 1f), 3f, false);
        Lamp(lights, new Vector3(10f, 2.9f, -5.75f), new Color(1f, 0.4f, 0.3f), 3f, true);

        // Koridor
        Lamp(lights, new Vector3(-12f, 2.9f, 0f), new Color(0.9f, 0.95f, 1f), 3f, false);
        Lamp(lights, new Vector3(-4f, 2.9f, 0f), new Color(0.9f, 0.95f, 1f), 3f, true);
        Lamp(lights, new Vector3(5f, 2.9f, 0f), new Color(0.9f, 0.95f, 1f), 3f, false);
    }

    private static void Lamp(Transform parent, Vector3 pos, Color color, float intensity, bool flicker)
    {
        GameObject bulb = Box("Lamp", pos + new Vector3(0f, 0.07f, 0f), new Vector3(0.6f, 0.06f, 0.25f), Mat("Bulb", Color.white, 2f), parent);
        Object.DestroyImmediate(bulb.GetComponent<Collider>());

        GameObject go = Empty("LampLight", pos, parent);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.range = 9f;
        l.intensity = intensity;
        l.shadows = LightShadows.None;

        if (flicker) go.AddComponent<LightFlicker>();
    }

    // -------------------------------------------------------------- PLAYER

    private static GameObject BuildPlayer(Vector3 pos)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = pos;

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.4f; cc.center = Vector3.zero; cc.stepOffset = 0.3f;

        PlayerController pc = player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
        player.AddComponent<Inventory>();

        GameObject camRoot = Empty("CameraRoot", pos + new Vector3(0f, 0.7f, 0f), player.transform);

        GameObject camGo = Empty("Main Camera", camRoot.transform.position, camRoot.transform);
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cam.fieldOfView = 70f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.AddComponent<AudioListener>();

        GameObject flashGo = Empty("Flashlight", camGo.transform.position + new Vector3(0.15f, -0.1f, 0.2f), camGo.transform);
        Light spot = flashGo.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.range = 25f;
        spot.spotAngle = 60f;
        spot.innerSpotAngle = 30f;
        spot.intensity = 30f;
        spot.color = new Color(1f, 0.96f, 0.82f);
        spot.shadows = LightShadows.Soft;
        Flashlight flashlight = flashGo.AddComponent<Flashlight>();

        SetRef(pc, "cameraRoot", camRoot.transform);
        SetRef(interaction, "playerCamera", cam);
        SetRef(flashlight, "lightSource", spot);

        return player;
    }

    // ------------------------------------------------------ MANAGERS + UI

    private static List<Transform> CreateSpawnPoints()
    {
        Transform parent = new GameObject("ScareSpawnPoints").transform;
        List<Transform> list = new List<Transform>();
        foreach (Vector3 p in new[]
        {
            new Vector3(-10f, 0f, 8f), new Vector3(10f, 0f, 3f), new Vector3(0f, 0f, -4f),
            new Vector3(-8f, 0f, -3f), new Vector3(12f, 0f, -8f), new Vector3(1f, 0f, 3f)
        })
            list.Add(Empty("Spawn", p, parent).transform);
        return list;
    }

    private static AudioClip Clip(string file)
    {
        AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + "/" + file + ".wav");
        if (c == null) Debug.LogWarning("File suara tidak ditemukan: " + AudioDir + "/" + file + ".wav");
        return c;
    }

    private static void BuildManagersAndUi(GameObject player, List<Transform> spawnPoints)
    {
        GameObject mgr = new GameObject("Managers");
        mgr.AddComponent<GameManager>();

        // AudioManager + AudioSource
        GameObject audioGo = new GameObject("AudioManager");
        audioGo.transform.SetParent(mgr.transform, false);
        AudioSource sfx = audioGo.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;
        AudioManager audio = audioGo.AddComponent<AudioManager>();
        SetRef(audio, "sfxSource", sfx);

        string[] ids = { "pickup", "door", "locked", "hurt", "attack", "scare", "ghost_spot" };
        SerializedObject aso = new SerializedObject(audio);
        SerializedProperty list = aso.FindProperty("sounds");
        list.arraySize = ids.Length;
        for (int i = 0; i < ids.Length; i++)
        {
            SerializedProperty e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("id").stringValue = ids[i];
            e.FindPropertyRelative("clip").objectReferenceValue = Clip(ids[i]);
        }
        aso.ApplyModifiedPropertiesWithoutUndo();

        // Suara latar
        GameObject amb = new GameObject("AmbientLoop");
        amb.transform.SetParent(mgr.transform, false);
        AudioSource ambSrc = amb.AddComponent<AudioSource>();
        ambSrc.clip = Clip("ambient_loop");
        ambSrc.loop = true;
        ambSrc.playOnAwake = true;
        ambSrc.volume = 0.45f;
        ambSrc.spatialBlend = 0f;

        // ScareManager + prefab
        GameObject scareGo = new GameObject("ScareManager");
        scareGo.transform.SetParent(mgr.transform, false);
        ScareManager scare = scareGo.AddComponent<ScareManager>();
        SetRef(scare, "scarePrefab", CreateScarePrefab());
        SetList(scare, "spawnPoints", spawnPoints);

        BuildUi(player.GetComponent<Inventory>());
    }

    private static ScareEffect CreateScarePrefab()
    {
        GameObject root = new GameObject("ScareEffect");
        root.AddComponent<ScareEffect>();

        Material dark = Mat("ScareBody", new Color(0.03f, 0.03f, 0.04f));
        Material eye = Mat("ScareEye", new Color(1f, 0.05f, 0.05f), 4f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        body.transform.localScale = new Vector3(0.6f, 1.2f, 0.6f);
        body.GetComponent<Renderer>().sharedMaterial = dark;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        foreach (float x in new[] { -0.12f, 0.12f })
        {
            GameObject e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            e.name = "Eye";
            e.transform.SetParent(root.transform, false);
            e.transform.localPosition = new Vector3(x, 1.9f, 0.25f);
            e.transform.localScale = Vector3.one * 0.09f;
            e.GetComponent<Renderer>().sharedMaterial = eye;
            Object.DestroyImmediate(e.GetComponent<Collider>());
        }

        string path = Root + "/Prefabs/ScareEffect.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ScareEffect>();
    }

    // ---------------------------------------------------------------- UI

    private static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, string text, float size,
        TextAlignmentOptions align, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 sizeDelta, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;

        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    private static UISlider MakeBar(Transform parent, string name, string label, float y, Color color)
    {
        RectTransform root = NewRect(name, parent);
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.zero; root.pivot = Vector2.zero;
        root.anchoredPosition = new Vector2(40f, y);
        root.sizeDelta = new Vector2(320f, 18f);

        RectTransform bg = NewRect("Background", root);
        Stretch(bg);
        bg.gameObject.AddComponent<UIImage>().color = new Color(0f, 0f, 0f, 0.6f);

        RectTransform fillArea = NewRect("Fill Area", root);
        Stretch(fillArea);
        fillArea.offsetMin = new Vector2(2f, 2f);
        fillArea.offsetMax = new Vector2(-2f, -2f);

        RectTransform fill = NewRect("Fill", fillArea);
        fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
        fill.gameObject.AddComponent<UIImage>().color = color;

        UISlider s = root.gameObject.AddComponent<UISlider>();
        s.fillRect = fill;
        s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        s.minValue = 0f; s.maxValue = 1f; s.value = 1f;
        s.interactable = false;
        s.transition = UnityEngine.UI.Selectable.Transition.None;

        MakeText(parent, name + "Label", label, 22f, TextAlignmentOptions.Left,
            Vector2.zero, new Vector2(0f, 0.5f), new Vector2(375f, y + 9f), new Vector2(200f, 30f), new Color(1f, 1f, 1f, 0.8f));
        return s;
    }

    private static GameObject MakePanel(Transform parent, string name, string title, Color titleColor, HUDController hudForButtons,
        bool restart, bool quit)
    {
        RectTransform panel = NewRect(name, parent);
        Stretch(panel);
        panel.gameObject.AddComponent<UIImage>().color = new Color(0f, 0f, 0f, 0.82f);

        MakeText(panel, "Title", title, 80f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(1400f, 140f), titleColor);

        float y = 0f;
        if (restart) { MakeButton(panel, "Restart", "MAIN LAGI", new Vector2(0f, y), hudForButtons, true); y -= 90f; }
        if (quit) MakeButton(panel, "Quit", "KELUAR", new Vector2(0f, y), hudForButtons, false);

        return panel.gameObject;
    }

    private static void MakeButton(Transform parent, string name, string label, Vector2 pos, HUDController hud, bool isRestart)
    {
        RectTransform rt = NewRect(name + "Button", parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(340f, 70f);

        UIImage img = rt.gameObject.AddComponent<UIImage>();
        img.color = new Color(0.35f, 0.06f, 0.06f, 1f);
        UIButton btn = rt.gameObject.AddComponent<UIButton>();
        btn.targetGraphic = img;

        TextMeshProUGUI t = MakeText(rt, "Label", label, 34f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(340f, 70f), Color.white);
        t.raycastTarget = false;

        if (isRestart) UnityEventTools.AddVoidPersistentListener(btn.onClick, hud.OnRestartClicked);
        else UnityEventTools.AddVoidPersistentListener(btn.onClick, hud.OnQuitClicked);
    }

    private static void BuildUi(Inventory inventory)
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        GameObject canvasGo = new GameObject("HUD Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UICanvasScaler scaler = canvasGo.AddComponent<UICanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<UIGraphicRaycaster>();

        HUDController hud = canvasGo.AddComponent<HUDController>();
        Transform c = canvasGo.transform;

        // Crosshair
        RectTransform dot = NewRect("Crosshair", c);
        dot.anchorMin = dot.anchorMax = new Vector2(0.5f, 0.5f);
        dot.sizeDelta = new Vector2(6f, 6f);
        UIImage dotImg = dot.gameObject.AddComponent<UIImage>();
        dotImg.color = new Color(1f, 1f, 1f, 0.5f);
        dotImg.raycastTarget = false;

        UISlider health = MakeBar(c, "HealthBar", "NYAWA", 40f, new Color(0.8f, 0.1f, 0.1f));
        UISlider battery = MakeBar(c, "BatteryBar", "BATERAI", 70f, new Color(0.95f, 0.8f, 0.15f));
        UISlider stamina = MakeBar(c, "StaminaBar", "STAMINA", 100f, new Color(0.3f, 0.8f, 0.3f));

        TextMeshProUGUI prompt = MakeText(c, "PromptText", "", 36f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(1200f, 60f), Color.white);
        TextMeshProUGUI message = MakeText(c, "MessageText", "", 40f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(1500f, 160f), new Color(1f, 0.9f, 0.6f));
        TextMeshProUGUI inv = MakeText(c, "InventoryText", "", 30f, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(500f, 300f), new Color(1f, 1f, 1f, 0.85f));
        MakeText(c, "HelpText", "WASD jalan | Shift lari | F senter | E interaksi | Esc jeda", 22f, TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 25f), new Vector2(900f, 40f), new Color(1f, 1f, 1f, 0.45f));

        GameObject pause = MakePanel(c, "PausePanel", "DIJEDA", Color.white, hud, false, true);
        GameObject win = MakePanel(c, "WinPanel", "KAMU BERHASIL KABUR", new Color(0.4f, 1f, 0.5f), hud, true, true);
        GameObject over = MakePanel(c, "GameOverPanel", "KAMU MATI", new Color(0.9f, 0.1f, 0.1f), hud, true, true);

        SetRef(hud, "healthBar", health);
        SetRef(hud, "batteryBar", battery);
        SetRef(hud, "staminaBar", stamina);
        SetRef(hud, "promptText", prompt);
        SetRef(hud, "messageText", message);
        SetRef(hud, "inventoryText", inv);
        SetRef(hud, "pausePanel", pause);
        SetRef(hud, "winPanel", win);
        SetRef(hud, "gameOverPanel", over);
        SetRef(hud, "inventory", inventory);

        pause.SetActive(false);
        win.SetActive(false);
        over.SetActive(false);
    }

    // --------------------------------------------------------------- ITEMS

    private static void BuildItems()
    {
        Transform items = new GameObject("Items").transform;

        Pickup<KeyPickup>(items, "Kunci", new Vector3(-12f, 0.88f, -8.5f), ItemType.Key, new Vector3(0.08f, 0.04f, 0.3f),
            Mat("KeyMat", new Color(1f, 0.85f, 0.1f), 1.5f), "Tekan E untuk mengambil kunci");
        Pickup<BatteryPickup>(items, "Baterai A", new Vector3(-8f, 0.89f, 9f), ItemType.Battery, new Vector3(0.12f, 0.12f, 0.22f),
            Mat("BatteryMat", new Color(0.2f, 0.9f, 0.3f), 1.2f), "Tekan E untuk mengambil baterai");
        Pickup<BatteryPickup>(items, "Baterai B", new Vector3(2.5f, 0.89f, -8.5f), ItemType.Battery, new Vector3(0.12f, 0.12f, 0.22f),
            Mat("BatteryMat", new Color(0.2f, 0.9f, 0.3f), 1.2f), "Tekan E untuk mengambil baterai");
        Pickup<MedkitPickup>(items, "Medkit", new Vector3(-3f, 0.93f, 8.7f), ItemType.Medkit, new Vector3(0.35f, 0.18f, 0.25f),
            Mat("MedkitMat", new Color(0.95f, 0.95f, 0.95f), 0.6f), "Tekan E untuk mengambil medkit");

        GameObject n1 = Pickup<NotePickup>(items, "Catatan 1", new Vector3(9f, 0.86f, 8.7f), ItemType.Note, new Vector3(0.28f, 0.01f, 0.2f),
            Mat("NoteMat", new Color(0.9f, 0.85f, 0.65f), 0.5f), "Tekan E untuk membaca catatan");
        SetString(n1.GetComponent<NotePickup>(), "noteText", "Dia hanya bergerak saat kamu tidak melihatnya. Jangan berkedip.");

        GameObject n2 = Pickup<NotePickup>(items, "Catatan 2", new Vector3(11f, 0.86f, -8.5f), ItemType.Note, new Vector3(0.28f, 0.01f, 0.2f),
            Mat("NoteMat", new Color(0.9f, 0.85f, 0.65f), 0.5f), "Tekan E untuk membaca catatan");
        SetString(n2.GetComponent<NotePickup>(), "noteText", "Kunci pintu keluar kutinggalkan di ruangan paling barat daya... gelap di sana.");
    }

    private static GameObject Pickup<T>(Transform parent, string name, Vector3 pos, ItemType type, Vector3 scale, Material mat, string prompt)
        where T : PickupBase
    {
        GameObject go = Box(name, pos, scale, mat, parent);
        T p = go.AddComponent<T>();
        SetEnum(p, "itemType", (int)type);
        SetString(p, "prompt", prompt);
        return go;
    }

    // ------------------------------------------------------------- ENEMIES

    private static void BuildEnemies()
    {
        Transform enemies = new GameObject("Enemies").transform;

        // Hantu patroli
        Material ghostMat = Mat("GhostMat", new Color(0.65f, 0.8f, 0.85f), 0.8f);
        GameObject ghost = Enemy("Ghost", new Vector3(6f, 1f, 0f), ghostMat, enemies);
        GhostEnemy ghostComp = ghost.AddComponent<GhostEnemy>();

        Transform wp = new GameObject("GhostWaypoints").transform;
        List<Transform> points = new List<Transform>();
        foreach (Vector3 p in new[]
        {
            new Vector3(-8f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 6.5f), new Vector3(0f, 0f, 0f),
            new Vector3(8f, 0f, 0f), new Vector3(10f, 0f, 6f), new Vector3(8f, 0f, 0f), new Vector3(10f, 0f, -6f), new Vector3(8f, 0f, 0f)
        })
            points.Add(Empty("WP", p, wp).transform);
        SetList(ghostComp, "waypoints", points);

        // Stalker (hanya bergerak saat tidak dilihat)
        Material stalkerMat = Mat("StalkerMat", new Color(0.05f, 0.02f, 0.02f), 0f);
        GameObject stalker = Enemy("Stalker", new Vector3(10f, 1f, 6f), stalkerMat, enemies);
        StalkerEnemy stalkerComp = stalker.AddComponent<StalkerEnemy>();
        SetFloat(stalkerComp, "stalkSpeed", 3f);
    }

    private static GameObject Enemy(string name, Vector3 pos, Material mat, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        go.GetComponent<Renderer>().sharedMaterial = mat;

        NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
        agent.baseOffset = 1f;
        agent.radius = 0.5f;
        agent.height = 2f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 0.8f;

        // titik merah kecil sebagai "mata"
        Material eye = Mat("ScareEye", new Color(1f, 0.05f, 0.05f), 4f);
        foreach (float x in new[] { -0.15f, 0.15f })
        {
            GameObject e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            e.name = "Eye";
            e.transform.SetParent(go.transform, false);
            e.transform.localPosition = new Vector3(x, 0.55f, 0.42f);
            e.transform.localScale = Vector3.one * 0.12f;
            e.GetComponent<Renderer>().sharedMaterial = eye;
            Object.DestroyImmediate(e.GetComponent<Collider>());
        }
        return go;
    }

    // -------------------------------------------- SERIALIZED FIELD HELPERS

    private static SerializedProperty Prop(SerializedObject so, string field)
    {
        SerializedProperty p = so.FindProperty(field);
        if (p == null) Debug.LogWarning("Field '" + field + "' tidak ditemukan pada " + so.targetObject.GetType().Name);
        return p;
    }

    private static void SetRef(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string field, float value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) return;
        p.floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Object target, string field, bool value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(Object target, string field, int index)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.enumValueIndex = index;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetList(Object target, string field, List<Transform> values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = Prop(so, field);
        if (p == null) return;
        p.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
