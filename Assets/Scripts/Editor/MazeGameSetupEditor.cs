using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using StarterAssets;

public class MazeGameSetupEditor : MonoBehaviour
{
    [MenuItem("Maze Game/Build & Setup Entire Game")]
    public static void BuildEntireGame()
    {
        Debug.Log("Building Entire Maze Explorer Game (All Scenes & Menus)...");

        // Common Materials
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        Material brickMat = CreateOrLoadMaterial("Assets/Materials/Mat_BrickWall.mat", litShader, "Assets/Textures/Brick_Albedo.png", "Assets/Textures/Brick_Normal.png", new Vector2(2f, 2f));
        Material wallCapMat = CreateOrLoadMaterial("Assets/Materials/Mat_StoneTrim.mat", litShader, "Assets/Textures/Stone_Trim.png", null, new Vector2(4f, 1f));
        Material groundMat = CreateOrLoadMaterial("Assets/Materials/Mat_GroundAsh.mat", litShader, "Assets/Textures/Ground_Ash_Albedo.png", null, new Vector2(16f, 16f));
        Material barkMat = CreateOrLoadMaterial("Assets/Materials/Mat_TreeBark.mat", litShader, "Assets/Textures/Tree_Bark.png", null, new Vector2(1f, 2f));
        Material leafMat = CreateOrLoadMaterial("Assets/Materials/Mat_TreeLeaves.mat", litShader, "Assets/Textures/Tree_Leaves.png", null, new Vector2(2f, 2f));
        Material rockMat = CreateOrLoadMaterial("Assets/Materials/Mat_Rock.mat", litShader, "Assets/Textures/Rock_Albedo.png", null, new Vector2(1f, 1f));
        Material crateMat = CreateOrLoadMaterial("Assets/Materials/Mat_Crate.mat", litShader, "Assets/Textures/Crate_Albedo.png", null, Vector2.one);
        Material mountainMat = CreateOrLoadMaterial("Assets/Materials/Mat_Mountains.mat", litShader, "Assets/Textures/Mountain_Backdrop.png", null, Vector2.one);
        Material playerJacketMat = CreateOrLoadMaterial("Assets/Materials/Mat_PlayerJacket.mat", litShader, "Assets/Textures/Player_Jacket.png", null, Vector2.one);

        // Exit Frame & Glow Materials
        Material exitFrameMat = new Material(litShader);
        exitFrameMat.color = new Color(0.12f, 0.15f, 0.18f);
        AssetDatabase.CreateAsset(exitFrameMat, "Assets/Materials/Mat_ExitFrame.mat");

        Material exitGlowMat = new Material(litShader);
        exitGlowMat.color = new Color(0.2f, 1.0f, 0.4f, 0.9f);
        if (exitGlowMat.HasProperty("_EmissionColor"))
        {
            exitGlowMat.EnableKeyword("_EMISSION");
            exitGlowMat.SetColor("_EmissionColor", new Color(0.2f, 1.0f, 0.4f) * 2.8f);
        }
        AssetDatabase.CreateAsset(exitGlowMat, "Assets/Materials/Mat_ExitGlow.mat");

        Material exitSignMat = CreateOrLoadMaterial("Assets/Materials/Mat_ExitSign.mat", litShader, "Assets/Textures/Exit_Sign.png", null, Vector2.one);
        if (exitSignMat.HasProperty("_EmissionColor"))
        {
            exitSignMat.EnableKeyword("_EMISSION");
            exitSignMat.SetColor("_EmissionColor", new Color(0.1f, 0.95f, 0.35f) * 1.8f);
        }

        // Build Scene 1: Main Menu
        BuildMainMenuScene(litShader, brickMat, groundMat, barkMat, leafMat, rockMat, exitFrameMat, exitGlowMat, exitSignMat, mountainMat);

        // Build Gameplay Scenes: Level 1, Level 2, Level 3
        BuildLevelScene(1, "The Training Grounds", litShader, brickMat, wallCapMat, groundMat, barkMat, leafMat, rockMat, crateMat, exitFrameMat, exitGlowMat, exitSignMat, mountainMat, playerJacketMat);
        BuildLevelScene(2, "The Forest Labyrinth", litShader, brickMat, wallCapMat, groundMat, barkMat, leafMat, rockMat, crateMat, exitFrameMat, exitGlowMat, exitSignMat, mountainMat, playerJacketMat);
        BuildLevelScene(3, "The Citadel Maze", litShader, brickMat, wallCapMat, groundMat, barkMat, leafMat, rockMat, crateMat, exitFrameMat, exitGlowMat, exitSignMat, mountainMat, playerJacketMat);

        // Build Victory Scene
        BuildVictoryScene(litShader);

        // Setup EditorBuildSettings with all scenes
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level3.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/VictoryScene.unity", true)
        };
        EditorBuildSettings.scenes = buildScenes;
        AssetDatabase.SaveAssets();

        // Also copy Level 1 to MazeGameScene.unity for direct legacy link
        AssetDatabase.CopyAsset("Assets/Scenes/Level1.unity", "Assets/Scenes/MazeGameScene.unity");

        Debug.Log("Entire Maze Explorer Game Successfully Built!");
    }

    private static void BuildMainMenuScene(Shader shader, Material brick, Material ground, Material bark, Material leaf, Material rock, Material frame, Material glow, Material sign, Material mountains)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        SetupLightingAndFog();

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        camObj.transform.position = new Vector3(0f, 6.5f, -14f);
        camObj.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
        camObj.AddComponent<AudioListener>();

        // Audio Source
        AudioSource audio = camObj.AddComponent<AudioSource>();
        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BackgroundMusic.wav");
        if (bgm != null)
        {
            audio.clip = bgm;
            audio.loop = true;
            audio.playOnAwake = true;
            audio.volume = 0.65f;
            audio.Play();
        }

        // Showcase Backdrop Environment
        GameObject envObj = new GameObject("ShowcaseEnvironment");
        MazeEnvironmentBuilder builder = envObj.AddComponent<MazeEnvironmentBuilder>();
        builder.BrickWallMaterial = brick;
        builder.GroundMaterial = ground;
        builder.TreeBarkMaterial = bark;
        builder.TreeLeavesMaterial = leaf;
        builder.RockMaterial = rock;
        builder.ExitFrameMaterial = frame;
        builder.ExitGlowMaterial = glow;
        builder.ExitSignMaterial = sign;
        builder.MountainBackdropMaterial = mountains;
        builder.BuildMaze(1);

        // UI Canvas
        GameObject canvasObj = CreateCanvas();
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Title Panel
        GameObject titlePanel = CreateUIPanel(canvasObj.transform, "TitlePanel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(760, 140), new Color(0.08f, 0.12f, 0.18f, 0.92f));
        CreateUIText(titlePanel.transform, "Title", "MAZE EXPLORER", defaultFont, 36, FontStyle.Bold, new Color(0.3f, 0.95f, 0.55f), new Vector2(0, 24), new Vector2(720, 48), TextAnchor.MiddleCenter);
        CreateUIText(titlePanel.transform, "Subtitle", "Find your way through the maze. Avoid obstacles, survive, and reach the goal!", defaultFont, 16, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), new Vector2(0, -22), new Vector2(720, 40), TextAnchor.MiddleCenter);

        // Level Select Buttons
        GameObject lvl1BtnObj = CreateUIButton(canvasObj.transform, "Level1Btn", "▶ Play Level 1: The Training Grounds", defaultFont, 18, new Color(0.15f, 0.6f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(440, 52));
        GameObject lvl2BtnObj = CreateUIButton(canvasObj.transform, "Level2Btn", "▶ Play Level 2: The Forest Labyrinth", defaultFont, 18, new Color(0.18f, 0.45f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(0, -25), new Vector2(440, 52));
        GameObject lvl3BtnObj = CreateUIButton(canvasObj.transform, "Level3Btn", "▶ Play Level 3: The Citadel Maze", defaultFont, 18, new Color(0.55f, 0.25f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(0, -90), new Vector2(440, 52));

        // Instructions Button
        GameObject howToBtnObj = CreateUIButton(canvasObj.transform, "HowToPlayBtn", "📖 How to Play & Game Rules", defaultFont, 16, new Color(0.2f, 0.25f, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0, -155), new Vector2(440, 46));

        // Audio Button
        GameObject audioBtnObj = CreateUIButton(canvasObj.transform, "AudioBtn", "♫ Music: ON", defaultFont, 14, new Color(0.12f, 0.16f, 0.22f), new Vector2(1f, 0f), new Vector2(-25, 25), new Vector2(180, 42));

        // Instructions Modal (Popup)
        GameObject modal = CreateUIPanel(canvasObj.transform, "InstructionsModal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 460), new Color(0.06f, 0.09f, 0.14f, 0.98f));
        CreateUIText(modal.transform, "ModalTitle", "CORE GAME RULES (FROM PRESENTATION)", defaultFont, 22, FontStyle.Bold, new Color(0.3f, 0.95f, 0.55f), new Vector2(0, 185), new Vector2(620, 36), TextAnchor.MiddleCenter);
        string guideText = "1. START / SPAWN: You spawn at the entrance facing the open corridor.\n\n" +
                           "2. CONTROLS (KEYBOARD ONLY):\n" +
                           "   ▲ / W : Move Forward   |   ▼ / S : Move Backward\n" +
                           "   ◄ / A : Turn Left      |   ► / D : Turn Right\n" +
                           "   ▲▲ Double-Tap Up (or Space) : JUMP over low obstacle boxes!\n" +
                           "   SHIFT : Sprint fast\n\n" +
                           "3. AVOID DEADLY OBSTACLES:\n" +
                           "   ⚠️ If you touch a TREE or ROCK, the level restarts immediately!\n\n" +
                           "4. PASSING A LEVEL:\n" +
                           "   Reach the green glowing EXIT portal at the end of the maze!";
        CreateUIText(modal.transform, "ModalBody", guideText, defaultFont, 15, FontStyle.Normal, Color.white, new Vector2(0, 20), new Vector2(600, 280), TextAnchor.UpperLeft);
        GameObject closeBtn = CreateUIButton(modal.transform, "CloseBtn", "Got It! Let's Play", defaultFont, 16, new Color(0.18f, 0.65f, 0.35f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(240, 46));
        modal.SetActive(false);

        // MainMenuUI Component
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();
        menuUI.Level1Button = lvl1BtnObj.GetComponent<Button>();
        menuUI.Level2Button = lvl2BtnObj.GetComponent<Button>();
        menuUI.Level3Button = lvl3BtnObj.GetComponent<Button>();
        menuUI.HowToPlayButton = howToBtnObj.GetComponent<Button>();
        menuUI.CloseInstructionsButton = closeBtn.GetComponent<Button>();
        menuUI.InstructionsPanel = modal;
        menuUI.MusicToggleButton = audioBtnObj.GetComponent<Button>();
        menuUI.MusicStatusText = audioBtnObj.GetComponentInChildren<Text>();
        menuUI.MenuAudioSource = audio;

        // GameManager instance
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenuScene.unity");
    }

    private static void BuildLevelScene(int level, string levelName, Shader shader, Material brick, Material cap, Material ground, Material bark, Material leaf, Material rock, Material crate, Material frame, Material glow, Material sign, Material mountains, Material jacket)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        SetupLightingAndFog();

        // 1. Maze Environment
        GameObject mazeObj = new GameObject("MazeEnvironment");
        MazeEnvironmentBuilder builder = mazeObj.AddComponent<MazeEnvironmentBuilder>();
        builder.BrickWallMaterial = brick;
        builder.WallCapMaterial = cap;
        builder.GroundMaterial = ground;
        builder.TreeBarkMaterial = bark;
        builder.TreeLeavesMaterial = leaf;
        builder.RockMaterial = rock;
        builder.CrateMaterial = crate;
        builder.ExitFrameMaterial = frame;
        builder.ExitGlowMaterial = glow;
        builder.ExitSignMaterial = sign;
        builder.MountainBackdropMaterial = mountains;
        builder.BuildMaze(level);

        // 2. Player Character (Spawn facing East down the hallway!)
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = builder.PlayerSpawnPoint;
        player.transform.rotation = builder.PlayerSpawnRotation;

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        ThirdPersonController controller = player.AddComponent<ThirdPersonController>();
        controller.MoveSpeed = 3.4f;
        controller.SprintSpeed = 6.2f;
        controller.TurnSpeed = 160.0f;
        controller.JumpHeight = 1.35f;
        controller.Gravity = -22.0f;
        int obsLayer = LayerMask.NameToLayer("Obstacle");
        if (obsLayer != -1) controller.ObstacleLayers = (1 << obsLayer);

        // Background Music
        AudioSource audioSource = player.AddComponent<AudioSource>();
        AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BackgroundMusic.wav");
        if (bgmClip != null)
        {
            audioSource.clip = bgmClip;
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.volume = 0.65f;
        }
        controller.BackgroundMusic = audioSource;

        // Player Visual Explorer Character
        BuildExplorerVisuals(player.transform, shader, jacket);

        // 3. Camera with ThirdPersonCamera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        camObj.AddComponent<AudioListener>();

        ThirdPersonCamera tpCam = camObj.AddComponent<ThirdPersonCamera>();
        tpCam.Target = player.transform;
        tpCam.TargetHeight = 1.0f;
        tpCam.Distance = 3.2f;
        tpCam.Height = 3.4f;
        tpCam.LookAheadDistance = 3.5f;

        // 4. In-Game Canvas & HUD
        GameObject canvasObj = CreateCanvas();
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // Top-Right Objective Banner (Slide 3)
        GameObject objCard = CreateUIPanel(canvasObj.transform, "ObjectivePanel", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-25, -25), new Vector2(370, 110), new Color(0.08f, 0.12f, 0.18f, 0.94f));
        uiMgr.ObjectivePanel = objCard;

        Sprite objSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Icon_Objective.png");
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(objCard.transform);
        Image iconImg = iconObj.AddComponent<Image>();
        if (objSprite != null) iconImg.sprite = objSprite;
        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.5f);
        iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(15, 6);
        iconRT.sizeDelta = new Vector2(48, 48);
        uiMgr.ObjectiveIcon = iconImg;

        CreateUIText(objCard.transform, "Title", "Objective", defaultFont, 18, FontStyle.Bold, Color.white, new Vector2(72, -15), new Vector2(280, 24), TextAnchor.UpperLeft);
        CreateUIText(objCard.transform, "Subtitle", "Come out from the boundaries or walls.\nFind the EXIT to pass the level!", defaultFont, 13, FontStyle.Normal, new Color(0.82f, 0.9f, 0.98f), new Vector2(72, -40), new Vector2(280, 40), TextAnchor.UpperLeft);
        GameObject distTxt = CreateUIText(objCard.transform, "ExitDist", "Exit Portal: ~30m away (Follow green beacon)", defaultFont, 12, FontStyle.Bold, new Color(0.4f, 0.95f, 0.6f), new Vector2(72, -82), new Vector2(280, 22), TextAnchor.UpperLeft);
        uiMgr.ExitDistanceText = distTxt.GetComponent<Text>();

        // Top-Left Level & Timer HUD
        GameObject timerCard = CreateUIPanel(canvasObj.transform, "TimerPanel", new Vector2(0, 1), new Vector2(0, 1), new Vector2(25, -25), new Vector2(260, 68), new Color(0.08f, 0.12f, 0.18f, 0.90f));
        CreateUIText(timerCard.transform, "LvlName", "LEVEL " + level + ": " + levelName.ToUpper(), defaultFont, 13, FontStyle.Bold, new Color(0.35f, 0.9f, 0.55f), new Vector2(15, -10), new Vector2(230, 22), TextAnchor.UpperLeft);
        GameObject timerTxt = CreateUIText(timerCard.transform, "Timer", "⏱ 00:00", defaultFont, 16, FontStyle.Bold, Color.white, new Vector2(15, -34), new Vector2(230, 26), TextAnchor.UpperLeft);
        uiMgr.TimerText = timerTxt.GetComponent<Text>();

        // Bottom-Left Controls HUD
        GameObject controlsCard = CreateUIPanel(canvasObj.transform, "ControlsPanel", new Vector2(0, 0), new Vector2(0, 0), new Vector2(25, 25), new Vector2(340, 125), new Color(0.08f, 0.12f, 0.18f, 0.92f));
        CreateUIText(controlsCard.transform, "CtrlTitle", "CONTROLS (KEYBOARD ONLY)", defaultFont, 13, FontStyle.Bold, new Color(0.4f, 0.85f, 1f), new Vector2(15, -12), new Vector2(310, 20), TextAnchor.UpperLeft);
        string ctrlInfo = "▲ / [W] Forward   |   ▼ / [S] Backward\n" +
                          "◄ / [A] Turn Left |   ► / [D] Turn Right\n" +
                          "▲▲ Double-Tap Up or [SPACE] : JUMP over boxes!\n" +
                          "[SHIFT] Sprint    |   [P] Pause";
        CreateUIText(controlsCard.transform, "CtrlDesc", ctrlInfo, defaultFont, 12, FontStyle.Normal, Color.white, new Vector2(15, -34), new Vector2(310, 85), TextAnchor.UpperLeft);

        // Bottom-Right Audio Status
        GameObject audioCard = CreateUIPanel(canvasObj.transform, "AudioPanel", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-25, 25), new Vector2(210, 44), new Color(0.08f, 0.12f, 0.18f, 0.88f));
        Button audioBtn = audioCard.AddComponent<Button>();
        uiMgr.AudioMuteButton = audioBtn;
        GameObject audioTxt = CreateUIText(audioCard.transform, "AudioTxt", "♫ Music: PLAYING [M]", defaultFont, 14, FontStyle.Bold, new Color(0.3f, 0.95f, 0.5f), Vector2.zero, new Vector2(200, 38), TextAnchor.MiddleCenter);
        uiMgr.AudioStatusText = audioTxt.GetComponent<Text>();

        // Collision Alert Banner (Slide 4)
        GameObject collCard = CreateUIPanel(canvasObj.transform, "CollisionPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 160), new Color(0.65f, 0.08f, 0.08f, 0.94f));
        GameObject collTxt = CreateUIText(collCard.transform, "CollTxt", "⚠️ OBSTACLE COLLISION!\nHit a Tree or Rock obstacle!\nRestarting level...", defaultFont, 22, FontStyle.Bold, Color.yellow, Vector2.zero, new Vector2(490, 140), TextAnchor.MiddleCenter);
        uiMgr.CollisionPanel = collCard;
        uiMgr.CollisionText = collTxt.GetComponent<Text>();
        collCard.SetActive(false);

        // Pause Menu
        GameObject pauseCard = CreateUIPanel(canvasObj.transform, "PausePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 320), new Color(0.06f, 0.09f, 0.14f, 0.97f));
        CreateUIText(pauseCard.transform, "PTitle", "GAME PAUSED", defaultFont, 24, FontStyle.Bold, Color.white, new Vector2(0, 110), new Vector2(360, 40), TextAnchor.MiddleCenter);
        GameObject resBtn = CreateUIButton(pauseCard.transform, "ResumeBtn", "Resume Game", defaultFont, 16, new Color(0.18f, 0.65f, 0.35f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(280, 46));
        GameObject pRestBtn = CreateUIButton(pauseCard.transform, "RestartBtn", "Restart Level [R]", defaultFont, 16, new Color(0.2f, 0.45f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(280, 46));
        GameObject pMenuBtn = CreateUIButton(pauseCard.transform, "MenuBtn", "Main Menu", defaultFont, 16, new Color(0.3f, 0.35f, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(280, 46));
        uiMgr.PausePanel = pauseCard;
        uiMgr.ResumeButton = resBtn.GetComponent<Button>();
        uiMgr.PauseRestartButton = pRestBtn.GetComponent<Button>();
        uiMgr.PauseMainMenuButton = pMenuBtn.GetComponent<Button>();
        pauseCard.SetActive(false);

        // Level Complete Modal (Slide 1 & Slide 3)
        GameObject vicCard = CreateUIPanel(canvasObj.transform, "LevelCompletePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 410), new Color(0.06f, 0.10f, 0.16f, 0.98f));
        uiMgr.LevelCompletePanel = vicCard;

        Sprite trophySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Icon_Trophy.png");
        GameObject trophyObj = new GameObject("Trophy");
        trophyObj.transform.SetParent(vicCard.transform);
        Image trImg = trophyObj.AddComponent<Image>();
        if (trophySprite != null) trImg.sprite = trophySprite;
        RectTransform trRT = trophyObj.GetComponent<RectTransform>();
        trRT.anchoredPosition = new Vector2(0, 120);
        trRT.sizeDelta = new Vector2(90, 90);

        GameObject vicTitle = CreateUIText(vicCard.transform, "VicTitle", "🎉 LEVEL " + level + " PASSED! 🎉", defaultFont, 26, FontStyle.Bold, new Color(0.3f, 1.0f, 0.55f), new Vector2(0, 48), new Vector2(520, 38), TextAnchor.MiddleCenter);
        uiMgr.VictoryTitleText = vicTitle.GetComponent<Text>();

        GameObject vicPassed = CreateUIText(vicCard.transform, "VicPassed", "✓ SUCCESS: You escaped the boundaries of Level " + level + "!", defaultFont, 16, FontStyle.Bold, new Color(0.95f, 0.82f, 0.25f), new Vector2(0, 12), new Vector2(500, 28), TextAnchor.MiddleCenter);
        uiMgr.VictoryLevelPassedText = vicPassed.GetComponent<Text>();

        GameObject vicStats = CreateUIText(vicCard.transform, "VicStats", "Completion Time: 00:00\nStatus: Objective Achieved!\nNext Stage Unlocked!", defaultFont, 15, FontStyle.Normal, Color.white, new Vector2(0, -38), new Vector2(480, 65), TextAnchor.MiddleCenter);
        uiMgr.VictoryStatsText = vicStats.GetComponent<Text>();

        string nextBtnLabel = level < 3 ? "Proceed to Next Level ▶" : "Finish Game (Master Trophy) 🏆";
        GameObject nextBtn = CreateUIButton(vicCard.transform, "NextBtn", nextBtnLabel, defaultFont, 17, new Color(0.18f, 0.65f, 0.35f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(300, 48));
        GameObject repBtn = CreateUIButton(vicCard.transform, "ReplayBtn", "Replay Level ⟳", defaultFont, 15, new Color(0.2f, 0.45f, 0.7f), new Vector2(0.5f, 0f), new Vector2(0, 46), new Vector2(300, 42));
        uiMgr.NextLevelButton = nextBtn.GetComponent<Button>();
        uiMgr.PlayAgainButton = repBtn.GetComponent<Button>();
        vicCard.SetActive(false);

        // GameManager instance
        if (FindAnyObjectByType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            GameManager gmComp = gm.AddComponent<GameManager>();
            gmComp.CurrentLevelNumber = level;
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level" + level + ".unity");
    }

    private static void BuildVictoryScene(Shader shader)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupLightingAndFog();

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        camObj.AddComponent<AudioListener>();

        GameObject canvasObj = CreateCanvas();
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject panel = CreateUIPanel(canvasObj.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 460), new Color(0.06f, 0.10f, 0.16f, 0.98f));

        Sprite trophySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Icon_Trophy.png");
        GameObject trophy = new GameObject("Trophy");
        trophy.transform.SetParent(panel.transform);
        Image trImg = trophy.AddComponent<Image>();
        if (trophySprite != null) trImg.sprite = trophySprite;
        RectTransform trRT = trophy.GetComponent<RectTransform>();
        trRT.anchoredPosition = new Vector2(0, 115);
        trRT.sizeDelta = new Vector2(115, 115);

        CreateUIText(panel.transform, "VicHeading", "🏆 MASTER EXPLORER! 🏆", defaultFont, 28, FontStyle.Bold, new Color(0.95f, 0.8f, 0.2f), new Vector2(0, 20), new Vector2(620, 44), TextAnchor.MiddleCenter);
        CreateUIText(panel.transform, "VicMsg", "Congratulations!\nYou successfully conquered all 3 levels of Maze Explorer!\n\nYou navigated through the winding brick walls,\njumped over the hurdle boxes, avoided all tree & rock hazards, and escaped!", defaultFont, 16, FontStyle.Normal, Color.white, new Vector2(0, -65), new Vector2(620, 105), TextAnchor.MiddleCenter);

        GameObject againBtn = CreateUIButton(panel.transform, "AgainBtn", "Play Again from Level 1", defaultFont, 18, new Color(0.18f, 0.65f, 0.35f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(320, 50));
        againBtn.GetComponent<Button>().onClick.AddListener(() => {
            if (GameManager.Instance != null) GameManager.Instance.LoadLevel(1);
            else UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
        });

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/VictoryScene.unity");
    }

    private static void BuildExplorerVisuals(Transform playerParent, Shader shader, Material jacketMat)
    {
        GameObject visuals = new GameObject("Visuals");
        visuals.transform.SetParent(playerParent);
        visuals.transform.localPosition = Vector3.zero;

        Material skinMat = new Material(shader);
        skinMat.color = new Color(0.92f, 0.76f, 0.62f);
        Material pantsMat = new Material(shader);
        pantsMat.color = new Color(0.18f, 0.20f, 0.24f);
        Material packMat = new Material(shader);
        packMat.color = new Color(0.55f, 0.35f, 0.20f);
        Material capMat = new Material(shader);
        capMat.color = new Color(0.85f, 0.35f, 0.15f);

        // Torso
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(visuals.transform);
        torso.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        torso.transform.localScale = new Vector3(0.58f, 0.68f, 0.36f);
        torso.GetComponent<Renderer>().sharedMaterial = jacketMat;
        DestroyImmediate(torso.GetComponent<Collider>());

        // Backpack on back
        GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backpack.name = "Backpack";
        backpack.transform.SetParent(visuals.transform);
        backpack.transform.localPosition = new Vector3(0f, 1.08f, -0.25f);
        backpack.transform.localScale = new Vector3(0.44f, 0.52f, 0.26f);
        backpack.GetComponent<Renderer>().sharedMaterial = packMat;
        DestroyImmediate(backpack.GetComponent<Collider>());

        // Sleeping bag rolled on top of backpack
        GameObject bedroll = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bedroll.name = "Bedroll";
        bedroll.transform.SetParent(backpack.transform);
        bedroll.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        bedroll.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        bedroll.transform.localScale = new Vector3(0.26f, 0.36f, 0.26f);
        bedroll.GetComponent<Renderer>().sharedMaterial = capMat;
        DestroyImmediate(bedroll.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(visuals.transform);
        head.transform.localPosition = new Vector3(0f, 1.55f, 0f);
        head.transform.localScale = new Vector3(0.38f, 0.42f, 0.38f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        DestroyImmediate(head.GetComponent<Collider>());

        // Explorer Cap
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "Cap";
        cap.transform.SetParent(head.transform);
        cap.transform.localPosition = new Vector3(0f, 0.38f, 0.05f);
        cap.transform.localScale = new Vector3(0.95f, 0.18f, 1.15f);
        cap.GetComponent<Renderer>().sharedMaterial = capMat;
        DestroyImmediate(cap.GetComponent<Collider>());

        // Left Arm
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.name = "LeftArm";
        leftArm.transform.SetParent(visuals.transform);
        leftArm.transform.localPosition = new Vector3(-0.40f, 1.05f, 0f);
        leftArm.transform.localScale = new Vector3(0.18f, 0.34f, 0.18f);
        leftArm.GetComponent<Renderer>().sharedMaterial = jacketMat;
        DestroyImmediate(leftArm.GetComponent<Collider>());

        // Right Arm
        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.name = "RightArm";
        rightArm.transform.SetParent(visuals.transform);
        rightArm.transform.localPosition = new Vector3(0.40f, 1.05f, 0f);
        rightArm.transform.localScale = new Vector3(0.18f, 0.34f, 0.18f);
        rightArm.GetComponent<Renderer>().sharedMaterial = jacketMat;
        DestroyImmediate(rightArm.GetComponent<Collider>());

        // Left Leg
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(visuals.transform);
        leftLeg.transform.localPosition = new Vector3(-0.16f, 0.45f, 0f);
        leftLeg.transform.localScale = new Vector3(0.20f, 0.44f, 0.20f);
        leftLeg.GetComponent<Renderer>().sharedMaterial = pantsMat;
        DestroyImmediate(leftLeg.GetComponent<Collider>());

        // Right Leg
        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(visuals.transform);
        rightLeg.transform.localPosition = new Vector3(0.16f, 0.45f, 0f);
        rightLeg.transform.localScale = new Vector3(0.20f, 0.44f, 0.20f);
        rightLeg.GetComponent<Renderer>().sharedMaterial = pantsMat;
        DestroyImmediate(rightLeg.GetComponent<Collider>());

        PlayerVisuals anim = visuals.AddComponent<PlayerVisuals>();
        anim.Head = head.transform;
        anim.Backpack = backpack.transform;
        anim.LeftArm = leftArm.transform;
        anim.RightArm = rightArm.transform;
        anim.LeftLeg = leftLeg.transform;
        anim.RightLeg = rightLeg.transform;
    }

    private static void SetupLightingAndFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.010f;
        RenderSettings.fogColor = new Color(0.70f, 0.76f, 0.84f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.52f, 0.58f);

        GameObject dirLightObj = new GameObject("Directional Light");
        Light dirLight = dirLightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1.0f, 0.97f, 0.92f);
        dirLight.intensity = 1.30f;
        dirLight.shadows = LightShadows.Soft;
        dirLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        return canvasObj;
    }

    private static Material CreateOrLoadMaterial(string path, Shader shader, string albedoTexPath, string normalTexPath, Vector2 tiling)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        if (!string.IsNullOrEmpty(albedoTexPath))
        {
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoTexPath);
            if (albedo != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
                else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", albedo);
            }
        }

        if (!string.IsNullOrEmpty(normalTexPath))
        {
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalTexPath);
            if (normal != null && mat.HasProperty("_BumpMap"))
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }
        }

        if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", tiling);
        else if (mat.HasProperty("_MainTex")) mat.SetTextureScale("_MainTex", tiling);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return panel;
    }

    private static GameObject CreateUIButton(Transform parent, string name, string text, Font font, int fontSize, Color color, Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = color;
        Button btn = btnObj.AddComponent<Button>();
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        CreateUIText(btnObj.transform, "BtnText", text, font, fontSize, FontStyle.Bold, Color.white, Vector2.zero, sizeDelta - new Vector2(20, 10), TextAnchor.MiddleCenter);
        return btnObj;
    }

    private static GameObject CreateUIText(Transform parent, string name, string text, Font font, int fontSize, FontStyle style, Color color, Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor alignment)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.text = text;
        txt.font = font;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.color = color;
        txt.alignment = alignment;
        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return txtObj;
    }
}
