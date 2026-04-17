using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class ProjectSetup : EditorWindow
{
    [MenuItem("Assassin/1. Full Project Setup (Run This First)")]
    public static void FullSetup()
    {
        SetupTagsAndLayers();
        CreateMaterials();
        CreatePlayerPrefab();
        CreateEnemyPrefab();
        CreateMainMenuScene();
        CreateLevelScene("Level1", 4);
        CreateLevelScene("Level2", 6);
        SetupBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Open MainMenu so Play starts from the right scene
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        Debug.Log("=== FULL PROJECT SETUP COMPLETE ===");
        EditorUtility.DisplayDialog("Setup Complete",
            "All scenes, prefabs, tags, layers, materials, and UI have been created.\n\n" +
            "Press Play to start from the Main Menu!",
            "OK");
    }

    // ==================== TAGS & LAYERS ====================

    [MenuItem("Assassin/Setup Tags and Layers")]
    public static void SetupTagsAndLayers()
    {
        // Add tags
        AddTag("Player");
        AddTag("Enemy");
        AddTag("Wall");

        // Add layers
        AddLayer(8, "Player");
        AddLayer(9, "Enemy");
        AddLayer(10, "Wall");
        AddLayer(11, "Obstacle");
        AddLayer(12, "Minimap");

        // Set up collision matrix — enemies don't collide with each other
        Physics2D.IgnoreLayerCollision(9, 9, true);

        Debug.Log("Tags and Layers configured.");
    }

    static void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName) return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    static void AddLayer(int index, string layerName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        if (layers.GetArrayElementAtIndex(index).stringValue == "")
        {
            layers.GetArrayElementAtIndex(index).stringValue = layerName;
            tagManager.ApplyModifiedProperties();
        }
    }

    // ==================== MATERIALS ====================

    [MenuItem("Assassin/Create Materials")]
    public static void CreateMaterials()
    {
        // Vision cone material
        Shader visionShader = Shader.Find("Custom/VisionCone");
        if (visionShader == null) visionShader = Shader.Find("Sprites/Default");

        Material visionMat = new Material(visionShader);
        visionMat.color = new Color(1f, 0.2f, 0.4f, 0.25f);
        SaveAsset(visionMat, "Assets/Materials/VisionConeMaterial.mat");

        // Player material
        Material playerMat = new Material(Shader.Find("Sprites/Default"));
        playerMat.color = new Color(0f, 1f, 0.53f, 1f); // #00FF88
        SaveAsset(playerMat, "Assets/Materials/PlayerMaterial.mat");

        // Enemy material
        Material enemyMat = new Material(Shader.Find("Sprites/Default"));
        enemyMat.color = new Color(1f, 0.2f, 0.4f, 1f); // #FF3366
        SaveAsset(enemyMat, "Assets/Materials/EnemyMaterial.mat");

        // Wall material
        Material wallMat = new Material(Shader.Find("Sprites/Default"));
        wallMat.color = new Color(0.165f, 0.176f, 0.2f, 1f); // #2A2D33
        SaveAsset(wallMat, "Assets/Materials/WallMaterial.mat");

        // Obstacle material
        Material obstacleMat = new Material(Shader.Find("Sprites/Default"));
        obstacleMat.color = new Color(0.42f, 0.314f, 0.208f, 1f); // #6B5035
        SaveAsset(obstacleMat, "Assets/Materials/ObstacleMaterial.mat");

        // Background material
        Material bgMat = new Material(Shader.Find("Sprites/Default"));
        bgMat.color = new Color(0.051f, 0.059f, 0.071f, 1f); // #0D0F12
        SaveAsset(bgMat, "Assets/Materials/BackgroundMaterial.mat");

        Debug.Log("Materials created.");
    }

    static void SaveAsset(Object asset, string path)
    {
        Object existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    // ==================== SPRITES ====================

    static Sprite CreateCircleSprite(int size = 64)
    {
        string path = $"Assets/Art/Circle_{size}.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = size / 2f;
        float radius = size / 2f - 1f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                    tex.SetPixel(x, y, Color.white);
                else if (dist <= radius + 1f)
                    tex.SetPixel(x, y, new Color(1, 1, 1, radius + 1f - dist));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Configure as sprite
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = size;
        importer.filterMode = FilterMode.Bilinear;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite CreateSquareSprite(int size = 32)
    {
        string path = $"Assets/Art/Square_{size}.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = size;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite CreateHeartSprite()
    {
        // Delete old heart so we regenerate
        string path = "Assets/Art/Heart.png";
        if (System.IO.File.Exists(path))
            AssetDatabase.DeleteAsset(path);

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        // Heart built from geometry: two circles on top + triangle on bottom
        // All in normalized 0..1 space
        float leftCX = 0.3f, leftCY = 0.62f, radius = 0.22f;
        float rightCX = 0.7f, rightCY = 0.62f;
        float tipX = 0.5f, tipY = 0.15f;

        for (int px = 0; px < size; px++)
        {
            for (int py = 0; py < size; py++)
            {
                float x = px / (float)size;
                float y = py / (float)size;

                bool inside = false;

                // Left circle
                float dxL = x - leftCX;
                float dyL = y - leftCY;
                if (dxL * dxL + dyL * dyL <= radius * radius) inside = true;

                // Right circle
                float dxR = x - rightCX;
                float dyR = y - rightCY;
                if (dxR * dxR + dyR * dyR <= radius * radius) inside = true;

                // Lower body: bounded by two diagonal lines from outer circle edges to tip
                // Left edge: from (leftCX - radius * 0.7, leftCY - radius * 0.3) to tip
                // Right edge: from (rightCX + radius * 0.7, rightCY - radius * 0.3) to tip
                if (y <= leftCY && y >= tipY)
                {
                    float t = (y - tipY) / (leftCY - tipY); // 0 at tip, 1 at circles
                    float leftBound = Mathf.Lerp(tipX, leftCX - radius, t);
                    float rightBound = Mathf.Lerp(tipX, rightCX + radius, t);
                    if (x >= leftBound && x <= rightBound) inside = true;
                }

                // Top connection between the two circles
                if (y >= leftCY - radius * 0.2f && y <= leftCY + radius && x >= leftCX && x <= rightCX)
                {
                    // Fill the valley between the two bumps with a smooth curve
                    float midDip = leftCY + radius * 0.55f;
                    float xt = (x - leftCX) / (rightCX - leftCX); // 0..1 across
                    float curveY = midDip + (radius * 0.45f) * Mathf.Sin(xt * Mathf.PI);
                    if (y <= curveY) inside = true;
                }

                if (inside)
                {
                    // Compute distance to nearest edge for anti-aliasing
                    // Simple SDF approximation
                    float dLeft = Mathf.Sqrt(dxL * dxL + dyL * dyL) - radius;
                    float dRight = Mathf.Sqrt(dxR * dxR + dyR * dyR) - radius;
                    float minDist = Mathf.Min(dLeft, dRight);

                    float edgeWidth = 1.5f / size;
                    float alpha = Mathf.Clamp01(-minDist / edgeWidth + 0.5f);
                    alpha = Mathf.Max(alpha, 0.85f); // fill interior solidly

                    // Subtle highlight in upper-left for a 3D feel
                    float highlight = 0f;
                    float hlDist = Mathf.Sqrt((x - 0.35f) * (x - 0.35f) + (y - 0.7f) * (y - 0.7f));
                    if (hlDist < 0.12f)
                        highlight = (1f - hlDist / 0.12f) * 0.3f;

                    tex.SetPixel(px, py, new Color(1f + highlight, 1f + highlight, 1f + highlight, alpha));
                }
                else
                {
                    tex.SetPixel(px, py, Color.clear);
                }
            }
        }
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = size;
        importer.filterMode = FilterMode.Bilinear;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite CreateBrokenHeartSprite()
    {
        string path = "Assets/Art/HeartBroken.png";
        if (System.IO.File.Exists(path))
            AssetDatabase.DeleteAsset(path);

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        // Same heart geometry as CreateHeartSprite
        float leftCX = 0.3f, leftCY = 0.62f, radius = 0.22f;
        float rightCX = 0.7f, rightCY = 0.62f;
        float tipX = 0.5f, tipY = 0.15f;

        // Jagged crack points (x offsets from center at various y levels)
        // y normalized 0..1, x offset from 0.5
        float[] crackY = { 0.15f, 0.25f, 0.35f, 0.42f, 0.50f, 0.58f, 0.65f, 0.72f, 0.80f };
        float[] crackX = { 0.00f, 0.04f, -0.03f, 0.05f, -0.02f, 0.04f, -0.04f, 0.03f, 0.00f };

        for (int px = 0; px < size; px++)
        {
            for (int py = 0; py < size; py++)
            {
                float x = px / (float)size;
                float y = py / (float)size;

                bool inside = false;

                float dxL = x - leftCX;
                float dyL = y - leftCY;
                if (dxL * dxL + dyL * dyL <= radius * radius) inside = true;

                float dxR = x - rightCX;
                float dyR = y - rightCY;
                if (dxR * dxR + dyR * dyR <= radius * radius) inside = true;

                if (y <= leftCY && y >= tipY)
                {
                    float t = (y - tipY) / (leftCY - tipY);
                    float leftBound = Mathf.Lerp(tipX, leftCX - radius, t);
                    float rightBound = Mathf.Lerp(tipX, rightCX + radius, t);
                    if (x >= leftBound && x <= rightBound) inside = true;
                }

                if (y >= leftCY - radius * 0.2f && y <= leftCY + radius && x >= leftCX && x <= rightCX)
                {
                    float midDip = leftCY + radius * 0.55f;
                    float xt = (x - leftCX) / (rightCX - leftCX);
                    float curveY = midDip + (radius * 0.45f) * Mathf.Sin(xt * Mathf.PI);
                    if (y <= curveY) inside = true;
                }

                if (inside)
                {
                    // Compute crack center x at this y
                    float crackCenterX = 0.5f;
                    for (int c = 0; c < crackY.Length - 1; c++)
                    {
                        if (y >= crackY[c] && y <= crackY[c + 1])
                        {
                            float ct = (y - crackY[c]) / (crackY[c + 1] - crackY[c]);
                            crackCenterX = 0.5f + Mathf.Lerp(crackX[c], crackX[c + 1], ct);
                            break;
                        }
                    }

                    // Gap width for the crack
                    float gapWidth = 0.025f;
                    float distFromCrack = Mathf.Abs(x - crackCenterX);

                    if (distFromCrack < gapWidth)
                    {
                        // Inside the crack — transparent
                        tex.SetPixel(px, py, Color.clear);
                    }
                    else
                    {
                        // Shift halves apart slightly
                        float shift = (x < crackCenterX) ? -0.015f : 0.015f;

                        // Darken near crack edges
                        float edgeDarken = 1f;
                        if (distFromCrack < gapWidth + 0.03f)
                            edgeDarken = 0.6f + 0.4f * ((distFromCrack - gapWidth) / 0.03f);

                        tex.SetPixel(px, py, new Color(edgeDarken, edgeDarken, edgeDarken, 1f));
                    }
                }
                else
                {
                    tex.SetPixel(px, py, Color.clear);
                }
            }
        }
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = size;
        importer.filterMode = FilterMode.Bilinear;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ==================== PLAYER PREFAB ====================

    [MenuItem("Assassin/Create Player Prefab")]
    public static void CreatePlayerPrefab()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = 8; // Player layer

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0f, 1f, 0.53f, 1f);
        sr.sortingOrder = 10;

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        // Scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerStats>();
        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        combat.enemyLayer = 1 << 9; // Enemy layer
        combat.wallLayer = (1 << 10) | (1 << 11); // Wall + Obstacle

        // Direction indicator (small triangle showing forward)
        GameObject indicator = new GameObject("DirectionIndicator");
        indicator.transform.SetParent(player.transform);
        indicator.transform.localPosition = new Vector3(0, 0.6f, 0);
        SpriteRenderer indSr = indicator.AddComponent<SpriteRenderer>();
        indSr.sprite = CreateCircleSprite(8);
        indSr.color = new Color(0f, 1f, 0.53f, 0.8f);
        indSr.sortingOrder = 11;
        indicator.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // Save prefab
        string path = "Assets/Prefabs/Player.prefab";
        PrefabUtility.SaveAsPrefabAsset(player, path);
        Object.DestroyImmediate(player);
        Debug.Log("Player prefab created.");
    }

    // ==================== ENEMY PREFAB ====================

    [MenuItem("Assassin/Create Enemy Prefab")]
    public static void CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";
        enemy.layer = 9; // Enemy layer

        // Sprite
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.2f, 0.4f, 1f);
        sr.sortingOrder = 10;

        // Physics
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        // Scripts
        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.obstacleMask = (1 << 10) | (1 << 11); // Wall + Obstacle

        VisionCone vc = enemy.AddComponent<VisionCone>();
        vc.visionRange = 5f;
        vc.visionAngle = 27f;
        vc.obstacleLayer = (1 << 10) | (1 << 11); // Wall + Obstacle

        // Vision cone material reference
        Material visionMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VisionConeMaterial.mat");
        if (visionMat != null)
            vc.coneMaterial = visionMat;

        enemy.AddComponent<EnemyLabel>();

        // Save prefab
        string path = "Assets/Prefabs/Enemy.prefab";
        PrefabUtility.SaveAsPrefabAsset(enemy, path);
        Object.DestroyImmediate(enemy);
        Debug.Log("Enemy prefab created.");
    }

    // ==================== MAIN MENU SCENE ====================

    [MenuItem("Assassin/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera.main.backgroundColor = new Color(0.03f, 0.035f, 0.05f, 1f);
        Camera.main.orthographic = true;

        GameObject canvasObj = CreateCanvas("MainMenuCanvas");

        // ---- BACKGROUND VIGNETTE ----
        GameObject vignette = new GameObject("Vignette");
        vignette.transform.SetParent(canvasObj.transform);
        RectTransform vigRect = vignette.AddComponent<RectTransform>();
        vigRect.anchorMin = Vector2.zero;
        vigRect.anchorMax = Vector2.one;
        vigRect.offsetMin = Vector2.zero;
        vigRect.offsetMax = Vector2.zero;
        Image vigImg = vignette.AddComponent<Image>();
        vigImg.color = new Color(0f, 0f, 0f, 0.3f);

        // ---- TOP DECORATION: thin green line ----
        CreateDecorLine(canvasObj.transform, "TopLine", new Vector2(0, 270), new Vector2(620, 2f),
            new Color(0f, 1f, 0.53f, 0.3f));

        // ---- TITLE: ROGUE ASSASSIN on one line ----
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 200);
        titleRect.sizeDelta = new Vector2(1100, 130);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "ROGUE ASSASSIN";
        titleTmp.fontSize = 90;
        titleTmp.color = new Color(0f, 1f, 0.53f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.characterSpacing = 12f;
        titleTmp.enableAutoSizing = false;

        // Title glow (shadow behind title)
        Shadow titleGlow = titleObj.AddComponent<Shadow>();
        titleGlow.effectColor = new Color(0f, 1f, 0.53f, 0.25f);
        titleGlow.effectDistance = new Vector2(0, -3);

        // ---- LINE BELOW TITLE ----
        CreateDecorLine(canvasObj.transform, "TitleUnderline", new Vector2(0, 140), new Vector2(500, 2f),
            new Color(0f, 1f, 0.53f, 0.4f));

        // ---- TAGLINE ----
        GameObject tagObj = new GameObject("TaglineText");
        tagObj.transform.SetParent(canvasObj.transform);
        RectTransform tagRect = tagObj.AddComponent<RectTransform>();
        tagRect.anchoredPosition = new Vector2(0, 105);
        tagRect.sizeDelta = new Vector2(700, 35);
        TextMeshProUGUI tagTmp = tagObj.AddComponent<TextMeshProUGUI>();
        tagTmp.text = "ONE BLADE.  NO MERCY.  NO TRACE.";
        tagTmp.fontSize = 18;
        tagTmp.color = new Color(0.55f, 0.62f, 0.7f, 0.7f);
        tagTmp.alignment = TextAlignmentOptions.Center;
        tagTmp.fontStyle = FontStyles.Italic;
        tagTmp.characterSpacing = 6f;

        // ---- MENU UI SCRIPT ----
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();

        // ---- PLAY BUTTON ----
        GameObject playBtnObj = new GameObject("PlayButton");
        playBtnObj.transform.SetParent(canvasObj.transform);
        RectTransform playRect = playBtnObj.AddComponent<RectTransform>();
        playRect.anchoredPosition = new Vector2(0, -10);
        playRect.sizeDelta = new Vector2(400, 68);

        Image playBg = playBtnObj.AddComponent<Image>();
        playBg.color = new Color(0f, 1f, 0.53f, 0.1f);

        Outline playOutline = playBtnObj.AddComponent<Outline>();
        playOutline.effectColor = new Color(0f, 1f, 0.53f, 0.5f);
        playOutline.effectDistance = new Vector2(1, 1);

        Button playBtn = playBtnObj.AddComponent<Button>();
        ColorBlock playCb = playBtn.colors;
        playCb.normalColor = Color.white;
        playCb.highlightedColor = new Color(0f, 1f, 0.53f, 0.25f);
        playCb.pressedColor = new Color(0f, 1f, 0.53f, 0.5f);
        playBtn.colors = playCb;
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, menuUI.OnPlayButton);

        GameObject playTextObj = new GameObject("Text");
        playTextObj.transform.SetParent(playBtnObj.transform);
        RectTransform ptRect = playTextObj.AddComponent<RectTransform>();
        ptRect.anchorMin = Vector2.zero;
        ptRect.anchorMax = Vector2.one;
        ptRect.offsetMin = Vector2.zero;
        ptRect.offsetMax = Vector2.zero;
        TextMeshProUGUI playTmp = playTextObj.AddComponent<TextMeshProUGUI>();
        playTmp.text = "INITIATE MISSION";
        playTmp.fontSize = 25;
        playTmp.color = new Color(0f, 1f, 0.53f);
        playTmp.alignment = TextAlignmentOptions.Center;
        playTmp.fontStyle = FontStyles.Bold;
        playTmp.characterSpacing = 5f;

        // ---- AMMO TIP ----
        GameObject tipObj = new GameObject("AmmoTip");
        tipObj.transform.SetParent(canvasObj.transform);
        RectTransform tipRect = tipObj.AddComponent<RectTransform>();
        tipRect.anchoredPosition = new Vector2(0, -95);
        tipRect.sizeDelta = new Vector2(600, 30);
        TextMeshProUGUI tipTmp = tipObj.AddComponent<TextMeshProUGUI>();
        tipTmp.text = "KNIFE KILL  =  +1 AMMO";
        tipTmp.fontSize = 18;
        tipTmp.color = new Color(1f, 0.6f, 0f, 0.6f);
        tipTmp.alignment = TextAlignmentOptions.Center;
        tipTmp.fontStyle = FontStyles.Bold;
        tipTmp.characterSpacing = 5f;

        // ---- BOTTOM DECORATIONS ----
        CreateDecorLine(canvasObj.transform, "BottomLine", new Vector2(0, -190), new Vector2(380, 1.5f),
            new Color(0f, 1f, 0.53f, 0.15f));

        // Controls hint
        GameObject controlsObj = new GameObject("ControlsHint");
        controlsObj.transform.SetParent(canvasObj.transform);
        RectTransform ctrlRect = controlsObj.AddComponent<RectTransform>();
        ctrlRect.anchoredPosition = new Vector2(0, -225);
        ctrlRect.sizeDelta = new Vector2(750, 45);
        TextMeshProUGUI ctrlTmp = controlsObj.AddComponent<TextMeshProUGUI>();
        ctrlTmp.text = "ARROWS  move    SPACE  knife    S  shoot    R  restart";
        ctrlTmp.fontSize = 14;
        ctrlTmp.color = new Color(0.45f, 0.5f, 0.58f, 0.4f);
        ctrlTmp.alignment = TextAlignmentOptions.Center;
        ctrlTmp.characterSpacing = 3f;

        // Version
        GameObject verObj = new GameObject("VersionText");
        verObj.transform.SetParent(canvasObj.transform);
        RectTransform verRect = verObj.AddComponent<RectTransform>();
        verRect.anchoredPosition = new Vector2(0, -270);
        verRect.sizeDelta = new Vector2(350, 25);
        TextMeshProUGUI verTmp = verObj.AddComponent<TextMeshProUGUI>();
        verTmp.text = "v1.0  //  CLASSIFIED";
        verTmp.fontSize = 11;
        verTmp.color = new Color(0.4f, 0.45f, 0.5f, 0.25f);
        verTmp.alignment = TextAlignmentOptions.Center;
        verTmp.characterSpacing = 3f;

        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("MainMenu scene created.");
    }

    static void CreateDecorLine(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent);
        RectTransform rect = line.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        Image img = line.AddComponent<Image>();
        img.color = color;
    }

    // ==================== LEVEL SCENES ====================

    public static void CreateLevelScene(string levelName, int enemyCount)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera setup
        Camera cam = Camera.main;
        cam.backgroundColor = new Color(0.051f, 0.059f, 0.071f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 10f;
        cam.gameObject.AddComponent<ScreenShake>();
        cam.gameObject.AddComponent<CameraFollow>();

        // GameManager + Managers
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        gmObj.AddComponent<PauseManager>();
        gmObj.AddComponent<AudioManager>();

        // Load prefabs
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");

        // Spawn player
        if (playerPrefab != null)
        {
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(-12f, -7f, 0f);
        }

        // Create level geometry
        CreateLevelGeometry(levelName);

        // Spawn enemies with waypoints
        if (enemyPrefab != null)
        {
            SpawnEnemiesForLevel(enemyPrefab, levelName, enemyCount);
        }

        // Create HUD Canvas
        CreateHUDCanvas();

        // Create Minimap
        CreateMinimapCamera();

        string scenePath = $"Assets/Scenes/{levelName}.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"{levelName} scene created with {enemyCount} enemies.");
    }

    // ==================== LEVEL GEOMETRY ====================

    static void CreateLevelGeometry(string levelName)
    {
        GameObject levelParent = new GameObject("Level");

        // Background
        GameObject bg = CreateWall(levelParent.transform, "Background",
            Vector3.zero, new Vector2(30f, 20f),
            new Color(0.051f, 0.059f, 0.071f), -10);

        // Border walls
        float wallThickness = 0.5f;
        CreateWallCollider(levelParent.transform, "WallTop", new Vector3(0, 9.5f), new Vector2(30f, wallThickness));
        CreateWallCollider(levelParent.transform, "WallBottom", new Vector3(0, -9.5f), new Vector2(30f, wallThickness));
        CreateWallCollider(levelParent.transform, "WallLeft", new Vector3(-14.5f, 0), new Vector2(wallThickness, 20f));
        CreateWallCollider(levelParent.transform, "WallRight", new Vector3(14.5f, 0), new Vector2(wallThickness, 20f));

        switch (levelName)
        {
            case "Level1":
                CreateLevel1Layout(levelParent.transform);
                break;
            case "Level2":
                CreateLevel2Layout(levelParent.transform);
                break;
        }
    }

    static void CreateLevel1Layout(Transform parent)
    {
        // Interior walls — L-shapes and barriers creating paths
        CreateObstacle(parent, "Wall1", new Vector3(-4f, 4f), new Vector2(0.5f, 6f));
        CreateObstacle(parent, "Wall2", new Vector3(-4f, 7f), new Vector2(5f, 0.5f));
        CreateObstacle(parent, "Wall3", new Vector3(3f, -1f), new Vector2(0.5f, 7f));
        CreateObstacle(parent, "Wall4", new Vector3(8f, 3f), new Vector2(4f, 0.5f));
        CreateObstacle(parent, "Wall5", new Vector3(-8f, -3f), new Vector2(3f, 0.5f));

        // Cover boxes — spread across the map for sneaking
        CreateObstacle(parent, "Cover1", new Vector3(-10f, 5f), new Vector2(2f, 2f));
        CreateObstacle(parent, "Cover2", new Vector3(-6f, -6f), new Vector2(2f, 1.8f));
        CreateObstacle(parent, "Cover3", new Vector3(0f, 2f), new Vector2(1.8f, 2f));
        CreateObstacle(parent, "Cover4", new Vector3(7f, -4f), new Vector2(2.2f, 2f));
        CreateObstacle(parent, "Cover5", new Vector3(11f, 0f), new Vector2(2f, 2f));
        CreateObstacle(parent, "Cover6", new Vector3(-1f, -4f), new Vector2(2f, 1.8f));
        CreateObstacle(parent, "Cover7", new Vector3(6f, 6f), new Vector2(1.8f, 2.2f));
        CreateObstacle(parent, "Cover8", new Vector3(-8f, 2f), new Vector2(1.8f, 1.8f));
    }

    static void CreateLevel2Layout(Transform parent)
    {
        // Corridor walls — create narrow passages
        CreateObstacle(parent, "Wall1", new Vector3(-5f, 3f), new Vector2(0.5f, 6f));
        CreateObstacle(parent, "Wall2", new Vector3(4f, 3f), new Vector2(0.5f, 4f));
        CreateObstacle(parent, "Wall3", new Vector3(-3f, 0f), new Vector2(6f, 0.5f));
        CreateObstacle(parent, "Wall4", new Vector3(7f, 0f), new Vector2(4f, 0.5f));
        CreateObstacle(parent, "Wall5", new Vector3(0f, -6f), new Vector2(0.5f, 4f));
        CreateObstacle(parent, "Wall6", new Vector3(-10f, 1f), new Vector2(0.5f, 5f));
        CreateObstacle(parent, "Wall7", new Vector3(11f, 1f), new Vector2(0.5f, 4f));

        // Cover boxes — scattered throughout corridors
        CreateObstacle(parent, "Cover1", new Vector3(-8f, 0f), new Vector2(2f, 2f));
        CreateObstacle(parent, "Cover2", new Vector3(-1f, 3f), new Vector2(1.8f, 1.8f));
        CreateObstacle(parent, "Cover3", new Vector3(6f, -2f), new Vector2(1.8f, 2.2f));
        CreateObstacle(parent, "Cover4", new Vector3(2f, -7f), new Vector2(2.2f, 1.8f));
        CreateObstacle(parent, "Cover5", new Vector3(-6f, -7f), new Vector2(1.8f, 1.8f));
        CreateObstacle(parent, "Cover6", new Vector3(9f, 2f), new Vector2(2f, 1.8f));
        CreateObstacle(parent, "Cover7", new Vector3(-3f, -3f), new Vector2(1.8f, 1.8f));
        CreateObstacle(parent, "Cover8", new Vector3(5f, 7f), new Vector2(1.8f, 2f));
    }

    static void CreateRoom(Transform parent, string name, Vector3 pos, Vector2 size)
    {
        GameObject room = new GameObject(name);
        room.transform.SetParent(parent);
        room.transform.position = pos;

        SpriteRenderer sr = room.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.165f, 0.176f, 0.2f, 0.4f); // dark gray semi-transparent
        sr.sortingOrder = -5;
        room.transform.localScale = new Vector3(size.x, size.y, 1f);

        // Room border (visual only — using 4 thin walls)
        float borderWidth = 0.08f;
        Color borderColor = new Color(0f, 1f, 0.53f, 0.15f);

        CreateBorderLine(room.transform, "BorderTop", new Vector3(0, 0.5f), new Vector2(1f, borderWidth / size.y), borderColor);
        CreateBorderLine(room.transform, "BorderBottom", new Vector3(0, -0.5f), new Vector2(1f, borderWidth / size.y), borderColor);
        CreateBorderLine(room.transform, "BorderLeft", new Vector3(-0.5f, 0), new Vector2(borderWidth / size.x, 1f), borderColor);
        CreateBorderLine(room.transform, "BorderRight", new Vector3(0.5f, 0), new Vector2(borderWidth / size.x, 1f), borderColor);
    }

    static void CreateBorderLine(Transform parent, string name, Vector3 localPos, Vector2 localScale, Color color)
    {
        GameObject border = new GameObject(name);
        border.transform.SetParent(parent);
        border.transform.localPosition = localPos;
        border.transform.localScale = new Vector3(localScale.x, localScale.y, 1f);

        SpriteRenderer sr = border.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = -4;
    }

    static GameObject CreateWall(Transform parent, string name, Vector3 pos, Vector2 size, Color color, int sortOrder = 0)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = color;
        sr.sortingOrder = sortOrder;

        return wall;
    }

    static void CreateWallCollider(Transform parent, string name, Vector3 pos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.layer = 10; // Wall layer
        wall.tag = "Wall";

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.165f, 0.176f, 0.2f, 0.8f);
        sr.sortingOrder = -1;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    static void CreateObstacle(Transform parent, string name, Vector3 pos, Vector2 size)
    {
        GameObject obs = new GameObject(name);
        obs.transform.SetParent(parent);
        obs.transform.position = pos;
        obs.transform.localScale = new Vector3(size.x, size.y, 1f);
        obs.layer = 11; // Obstacle layer

        SpriteRenderer sr = obs.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.42f, 0.314f, 0.208f, 0.8f); // brown
        sr.sortingOrder = 5;

        BoxCollider2D col = obs.AddComponent<BoxCollider2D>();
        // Collider size is 1x1 since we use localScale for size
    }

    // ==================== ENEMY SPAWNING ====================

    static void SpawnEnemiesForLevel(GameObject enemyPrefab, string levelName, int count)
    {
        string[] designations = { "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", "FOXTROT" };

        // Define enemy positions and patrol waypoints per level
        Vector3[][] positions;
        Vector3[][][] waypoints;

        switch (levelName)
        {
            case "Level1":
                positions = new Vector3[][] {
                    new Vector3[] { new Vector3(5f, 3f) },
                    new Vector3[] { new Vector3(-3f, 8f) },
                    new Vector3[] { new Vector3(-6f, -3f) },
                    new Vector3[] { new Vector3(8f, -2f) }
                };
                waypoints = new Vector3[][][] {
                    new Vector3[][] { new Vector3[] { new Vector3(5f, 3f), new Vector3(5f, -3f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(-3f, 8f), new Vector3(-8f, 8f), new Vector3(-8f, 5f), new Vector3(-3f, 5f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(-6f, -3f), new Vector3(-6f, -6f), new Vector3(-2f, -6f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(8f, -2f), new Vector3(8f, -5f), new Vector3(4f, -5f), new Vector3(4f, -2f) } }
                };
                break;
            case "Level2":
                positions = new Vector3[][] {
                    new Vector3[] { new Vector3(-6f, 5f) },
                    new Vector3[] { new Vector3(2f, 5f) },
                    new Vector3[] { new Vector3(7f, -1f) },
                    new Vector3[] { new Vector3(-8f, -3f) },
                    new Vector3[] { new Vector3(0f, -5f) },
                    new Vector3[] { new Vector3(10f, 4f) }
                };
                waypoints = new Vector3[][][] {
                    new Vector3[][] { new Vector3[] { new Vector3(-6f, 5f), new Vector3(-6f, 3f), new Vector3(-8f, 3f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(2f, 5f), new Vector3(2f, 3f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(7f, -1f), new Vector3(7f, -4f), new Vector3(9f, -4f), new Vector3(9f, 1f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(-8f, -3f), new Vector3(-5f, -3f), new Vector3(-5f, -6f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(0f, -5f), new Vector3(3f, -5f), new Vector3(3f, -2f), new Vector3(0f, -2f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(10f, 4f), new Vector3(10f, 1f), new Vector3(7f, 1f) } }
                };
                break;
            default: // Level3
                positions = new Vector3[][] {
                    new Vector3[] { new Vector3(-8f, 6f) },
                    new Vector3[] { new Vector3(-3f, -4f) },
                    new Vector3[] { new Vector3(3f, 3f) },
                    new Vector3[] { new Vector3(8f, -2f) }
                };
                waypoints = new Vector3[][][] {
                    new Vector3[][] { new Vector3[] { new Vector3(-8f, 6f), new Vector3(-8f, 3f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(-3f, -4f), new Vector3(-3f, -7f), new Vector3(-7f, -7f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(3f, 3f), new Vector3(3f, 7f), new Vector3(7f, 7f) } },
                    new Vector3[][] { new Vector3[] { new Vector3(8f, -2f), new Vector3(8f, -5f), new Vector3(11f, -5f), new Vector3(11f, 1f) } }
                };
                break;
        }

        for (int i = 0; i < count && i < positions.Length; i++)
        {
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            enemy.transform.position = positions[i][0];
            enemy.name = $"Enemy_{designations[i]}";

            // Set designation label
            EnemyLabel label = enemy.GetComponent<EnemyLabel>();
            if (label != null) label.designation = $"TARGET_{designations[i]}";

            // Create waypoints
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null && i < waypoints.Length)
            {
                GameObject wpParent = new GameObject($"Waypoints_{designations[i]}");
                Transform[] wps = new Transform[waypoints[i][0].Length];

                for (int w = 0; w < waypoints[i][0].Length; w++)
                {
                    GameObject wp = new GameObject($"WP_{w}");
                    wp.transform.SetParent(wpParent.transform);
                    wp.transform.position = waypoints[i][0][w];
                    wps[w] = wp.transform;
                }

                ec.waypoints = wps;
            }
        }
    }

    // ==================== HUD CANVAS ====================

    static void CreateHUDCanvas()
    {
        GameObject canvasObj = CreateCanvas("HUDCanvas");
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();

        // Add UIManager
        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // ---- TOP LEFT: KILL COUNTER ----
        GameObject topLeftGroup = CreateUIGroup(canvasObj.transform, "TopLeftGroup",
            TextAnchor.UpperLeft, new Vector2(25, -20));

        // Kill counter background panel
        GameObject killBg = new GameObject("KillCounterBg");
        killBg.transform.SetParent(topLeftGroup.transform);
        RectTransform killBgRect = killBg.AddComponent<RectTransform>();
        killBgRect.anchoredPosition = new Vector2(80, -5);
        killBgRect.sizeDelta = new Vector2(200, 45);
        Image killBgImg = killBg.AddComponent<Image>();
        killBgImg.color = new Color(0f, 1f, 0.53f, 0.08f);
        Outline killBgOutline = killBg.AddComponent<Outline>();
        killBgOutline.effectColor = new Color(0f, 1f, 0.53f, 0.25f);
        killBgOutline.effectDistance = new Vector2(1, 1);

        TextMeshProUGUI killCounter = CreateTextElement(killBg.transform, "KillCounterText",
            "KILLS: 0 / 7", 26, new Color(0f, 1f, 0.53f), TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(190, 40));
        uiManager.killCounterText = killCounter;

        // ---- TOP RIGHT: AMMO COUNTER ----
        GameObject topRightGroup = CreateUIGroup(canvasObj.transform, "TopRightGroup",
            TextAnchor.UpperRight, new Vector2(-25, -20));

        GameObject ammoBg = new GameObject("AmmoBg");
        ammoBg.transform.SetParent(topRightGroup.transform);
        RectTransform ammoBgRect = ammoBg.AddComponent<RectTransform>();
        ammoBgRect.anchoredPosition = new Vector2(-60, -5);
        ammoBgRect.sizeDelta = new Vector2(160, 45);
        Image ammoBgImg = ammoBg.AddComponent<Image>();
        ammoBgImg.color = new Color(1f, 0.6f, 0f, 0.08f);
        Outline ammoBgOutline = ammoBg.AddComponent<Outline>();
        ammoBgOutline.effectColor = new Color(1f, 0.6f, 0f, 0.25f);
        ammoBgOutline.effectDistance = new Vector2(1, 1);

        TextMeshProUGUI ammoText = CreateTextElement(ammoBg.transform, "AmmoText",
            "AMMO: 0", 26, new Color(1f, 0.6f, 0f), TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(150, 40));
        uiManager.ammoText = ammoText;

        CreateTextElement(topRightGroup.transform, "ShootHint",
            "PRESS [S] TO SHOOT", 9, new Color(1f, 0.6f, 0f, 0.4f), TextAlignmentOptions.Right,
            new Vector2(-60, -35), new Vector2(160, 15));

        // ---- RIGHT SIDE: COORDINATES ----
        GameObject rightGroup = CreateUIGroup(canvasObj.transform, "RightGroup",
            TextAnchor.UpperRight, new Vector2(-20, -80));

        TextMeshProUGUI latText = CreateTextElement(rightGroup.transform, "LatText",
            "LAT: 52.5200", 11, new Color(0f, 1f, 0.53f, 0.7f), TextAlignmentOptions.Right,
            new Vector2(0, 0), new Vector2(180, 20));
        uiManager.latText = latText;

        TextMeshProUGUI longText = CreateTextElement(rightGroup.transform, "LongText",
            "LONG: 13.4050", 11, new Color(0f, 1f, 0.53f, 0.7f), TextAlignmentOptions.Right,
            new Vector2(0, -20), new Vector2(180, 20));
        uiManager.longText = longText;

        // ---- RIGHT SIDE: DETECTION ALERT ----
        GameObject alertPanel = new GameObject("AlertPanel");
        alertPanel.transform.SetParent(rightGroup.transform);
        RectTransform alertRect = alertPanel.AddComponent<RectTransform>();
        alertRect.anchoredPosition = new Vector2(0, -50);
        alertRect.sizeDelta = new Vector2(200, 25);

        Image alertBg = alertPanel.AddComponent<Image>();
        alertBg.color = new Color(1f, 0.2f, 0.4f, 0.3f);

        TextMeshProUGUI alertText = CreateTextElement(alertPanel.transform, "AlertText",
            "BOGIE_DETECTED_3", 11, new Color(1f, 0.2f, 0.4f), TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(200, 25));

        uiManager.alertPanel = alertPanel;
        uiManager.alertText = alertText;

        // ---- BOTTOM LEFT: LIVES ----
        GameObject bottomLeftGroup = CreateUIGroup(canvasObj.transform, "BottomLeftGroup",
            TextAnchor.LowerLeft, new Vector2(20, 15));

        Sprite heartFull = CreateHeartSprite();
        Sprite heartBroken = CreateBrokenHeartSprite();
        Image[] fullHearts = new Image[3];
        Image[] brokenHearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject heartSlot = new GameObject($"HeartSlot_{i}");
            heartSlot.transform.SetParent(bottomLeftGroup.transform);
            RectTransform slotRect = heartSlot.AddComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(25 + i * 65, 30);
            slotRect.sizeDelta = new Vector2(58, 58);

            // Broken heart (behind)
            GameObject brokenObj = new GameObject("Broken");
            brokenObj.transform.SetParent(heartSlot.transform);
            RectTransform bRect = brokenObj.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            Image brokenImg = brokenObj.AddComponent<Image>();
            brokenImg.sprite = heartBroken;
            brokenImg.color = new Color(0.4f, 0.1f, 0.12f, 0.7f);
            brokenImg.preserveAspect = true;
            brokenObj.SetActive(false);
            brokenHearts[i] = brokenImg;

            // Full heart (in front)
            GameObject fullObj = new GameObject("Full");
            fullObj.transform.SetParent(heartSlot.transform);
            RectTransform fRect = fullObj.AddComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;
            Image fullImg = fullObj.AddComponent<Image>();
            fullImg.sprite = heartFull;
            fullImg.color = new Color(0.9f, 0.1f, 0.15f, 1f);
            fullImg.preserveAspect = true;
            fullHearts[i] = fullImg;
        }
        uiManager.fullHeartImages = fullHearts;
        uiManager.brokenHeartImages = brokenHearts;

        CreateTextElement(bottomLeftGroup.transform, "RestartHint",
            "PRESS [R] TO RESTART", 9, new Color(0.67f, 0.73f, 0.8f, 0.3f),
            TextAlignmentOptions.Left, new Vector2(20, -5), new Vector2(200, 15));

        // ---- WIN SCREEN OVERLAY ----
        GameObject winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(canvasObj.transform);
        RectTransform winRect = winPanel.AddComponent<RectTransform>();
        winRect.anchorMin = Vector2.zero;
        winRect.anchorMax = Vector2.one;
        winRect.offsetMin = Vector2.zero;
        winRect.offsetMax = Vector2.zero;

        Image winBg = winPanel.AddComponent<Image>();
        winBg.color = new Color(0.051f, 0.059f, 0.071f, 0.9f);

        // "LEVEL X CLEARED" text
        TextMeshProUGUI winText = CreateTextElement(winPanel.transform, "WinText",
            "LEVEL 1 CLEARED", 42, new Color(0f, 1f, 0.53f),
            TextAlignmentOptions.Center, new Vector2(0, 50), new Vector2(600, 60));

        // Next level button
        GameObject nextBtn = new GameObject("NextLevelButton");
        nextBtn.transform.SetParent(winPanel.transform);
        RectTransform btnRect = nextBtn.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -40);
        btnRect.sizeDelta = new Vector2(300, 55);

        Image btnImg = nextBtn.AddComponent<Image>();
        btnImg.color = new Color(0f, 1f, 0.53f, 0.15f);

        Outline btnOutline = nextBtn.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0f, 1f, 0.53f, 0.6f);
        btnOutline.effectDistance = new Vector2(1, 1);

        Button btnComp = nextBtn.AddComponent<Button>();
        ColorBlock cb = btnComp.colors;
        cb.highlightedColor = new Color(0f, 1f, 0.53f, 0.3f);
        cb.pressedColor = new Color(0f, 1f, 0.53f, 0.5f);
        btnComp.colors = cb;

        // Button text
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(nextBtn.transform);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "START LEVEL 2";
        btnText.fontSize = 22;
        btnText.color = new Color(0f, 1f, 0.53f);
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontStyle = FontStyles.Bold;

        WinScreen winScreen = canvasObj.AddComponent<WinScreen>();
        winScreen.winPanel = winPanel;
        winScreen.winText = winText;
        winScreen.nextLevelButton = btnComp;
        winScreen.buttonText = btnText;

        // ---- GAME OVER SCREEN OVERLAY ----
        GameObject goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(canvasObj.transform);
        RectTransform goRect = goPanel.AddComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.offsetMin = Vector2.zero;
        goRect.offsetMax = Vector2.zero;

        Image goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0.05f, 0.02f, 0.02f, 0.92f);

        // "MISSION FAILED" title
        TextMeshProUGUI goTitle = CreateTextElement(goPanel.transform, "GameOverTitle",
            "MISSION FAILED", 62, new Color(1f, 0.15f, 0.25f),
            TextAlignmentOptions.Center, new Vector2(0, 100), new Vector2(800, 80));

        // Decorative line
        CreateDecorLine(goPanel.transform, "GOLine", new Vector2(0, 55), new Vector2(450, 2.5f),
            new Color(1f, 0.15f, 0.25f, 0.4f));

        // Sassy message
        TextMeshProUGUI goSass = CreateTextElement(goPanel.transform, "SassText",
            "MAYBE STEALTH ISN'T YOUR THING.", 20, new Color(0.7f, 0.5f, 0.5f, 0.8f),
            TextAlignmentOptions.Center, new Vector2(0, 20), new Vector2(700, 35));

        // Restart button
        GameObject restartBtn = new GameObject("RestartButton");
        restartBtn.transform.SetParent(goPanel.transform);
        RectTransform restartRect = restartBtn.AddComponent<RectTransform>();
        restartRect.anchoredPosition = new Vector2(0, -55);
        restartRect.sizeDelta = new Vector2(380, 65);
        Image restartBg = restartBtn.AddComponent<Image>();
        restartBg.color = new Color(0f, 1f, 0.53f, 0.1f);
        Outline restartOutline = restartBtn.AddComponent<Outline>();
        restartOutline.effectColor = new Color(0f, 1f, 0.53f, 0.5f);
        restartOutline.effectDistance = new Vector2(1, 1);
        Button restartBtnComp = restartBtn.AddComponent<Button>();
        ColorBlock restartCb = restartBtnComp.colors;
        restartCb.highlightedColor = new Color(0f, 1f, 0.53f, 0.25f);
        restartCb.pressedColor = new Color(0f, 1f, 0.53f, 0.5f);
        restartBtnComp.colors = restartCb;

        GameObject restartTextObj = new GameObject("Text");
        restartTextObj.transform.SetParent(restartBtn.transform);
        RectTransform rtRect = restartTextObj.AddComponent<RectTransform>();
        rtRect.anchorMin = Vector2.zero;
        rtRect.anchorMax = Vector2.one;
        rtRect.offsetMin = Vector2.zero;
        rtRect.offsetMax = Vector2.zero;
        TextMeshProUGUI restartTmp = restartTextObj.AddComponent<TextMeshProUGUI>();
        restartTmp.text = "RESTART LEVEL";
        restartTmp.fontSize = 26;
        restartTmp.color = new Color(0f, 1f, 0.53f);
        restartTmp.alignment = TextAlignmentOptions.Center;
        restartTmp.fontStyle = FontStyles.Bold;
        restartTmp.characterSpacing = 5f;

        // Main menu button
        GameObject menuBtn = new GameObject("MainMenuButton");
        menuBtn.transform.SetParent(goPanel.transform);
        RectTransform menuRect = menuBtn.AddComponent<RectTransform>();
        menuRect.anchoredPosition = new Vector2(0, -140);
        menuRect.sizeDelta = new Vector2(270, 50);
        Image menuBg = menuBtn.AddComponent<Image>();
        menuBg.color = new Color(1f, 0.15f, 0.25f, 0.06f);
        Outline menuOutline = menuBtn.AddComponent<Outline>();
        menuOutline.effectColor = new Color(1f, 0.15f, 0.25f, 0.3f);
        menuOutline.effectDistance = new Vector2(1, 1);
        Button menuBtnComp = menuBtn.AddComponent<Button>();
        ColorBlock menuCb = menuBtnComp.colors;
        menuCb.highlightedColor = new Color(1f, 0.15f, 0.25f, 0.2f);
        menuCb.pressedColor = new Color(1f, 0.15f, 0.25f, 0.4f);
        menuBtnComp.colors = menuCb;

        GameObject menuTextObj = new GameObject("Text");
        menuTextObj.transform.SetParent(menuBtn.transform);
        RectTransform mtRect = menuTextObj.AddComponent<RectTransform>();
        mtRect.anchorMin = Vector2.zero;
        mtRect.anchorMax = Vector2.one;
        mtRect.offsetMin = Vector2.zero;
        mtRect.offsetMax = Vector2.zero;
        TextMeshProUGUI menuTmp = menuTextObj.AddComponent<TextMeshProUGUI>();
        menuTmp.text = "MAIN MENU";
        menuTmp.fontSize = 20;
        menuTmp.color = new Color(1f, 0.15f, 0.25f, 0.7f);
        menuTmp.alignment = TextAlignmentOptions.Center;
        menuTmp.characterSpacing = 4f;

        GameOverScreen goScreen = canvasObj.AddComponent<GameOverScreen>();
        goScreen.gameOverPanel = goPanel;
        goScreen.titleText = goTitle;
        goScreen.sassText = goSass;
        goScreen.restartButton = restartBtnComp;
        goScreen.mainMenuButton = menuBtnComp;
    }

    // ==================== MINIMAP ====================

    static void CreateMinimapCamera()
    {
        // Minimap camera
        GameObject minimapCamObj = new GameObject("MinimapCamera");
        Camera minimapCam = minimapCamObj.AddComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = 15f;
        minimapCam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.depth = 1;
        minimapCamObj.transform.position = new Vector3(0, 0, -10);

        // Minimap render texture setup — will need to be created at runtime
        // For now, set the viewport rect for the minimap camera
        minimapCam.rect = new Rect(0.78f, 0.02f, 0.2f, 0.25f);

        MinimapController mc = minimapCamObj.AddComponent<MinimapController>();
        mc.minimapCamera = minimapCam;

        // Minimap border/label (on the HUD canvas)
        GameObject hudCanvas = GameObject.Find("HUDCanvas");
        if (hudCanvas != null)
        {
            GameObject minimapGroup = CreateUIGroup(hudCanvas.transform, "MinimapGroup",
                TextAnchor.LowerRight, new Vector2(-15, 15));

            CreateTextElement(minimapGroup.transform, "MinimapLabel",
                "SECTOR_07_ALPHA", 9, new Color(0f, 1f, 0.53f, 0.6f),
                TextAlignmentOptions.Left, new Vector2(-80, 175), new Vector2(150, 15));

            // Compass directions
            CreateTextElement(minimapGroup.transform, "CompassS",
                "S", 9, new Color(0f, 1f, 0.53f, 0.4f),
                TextAlignmentOptions.Center, new Vector2(0, -10), new Vector2(30, 15));
            CreateTextElement(minimapGroup.transform, "CompassSW",
                "S W", 8, new Color(0.67f, 0.73f, 0.8f, 0.3f),
                TextAlignmentOptions.Center, new Vector2(-65, -10), new Vector2(30, 15));
            CreateTextElement(minimapGroup.transform, "CompassSE",
                "S E", 8, new Color(0.67f, 0.73f, 0.8f, 0.3f),
                TextAlignmentOptions.Center, new Vector2(65, -10), new Vector2(30, 15));
        }
    }

    // ==================== UI HELPERS ====================

    static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Event system
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    static GameObject CreateUIGroup(Transform parent, string name, TextAnchor anchor, Vector2 offset)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        RectTransform rect = group.AddComponent<RectTransform>();

        // Set anchors based on TextAnchor
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                break;
            case TextAnchor.UpperRight:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                break;
            case TextAnchor.LowerLeft:
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                break;
            case TextAnchor.LowerRight:
                rect.anchorMin = new Vector2(1, 0);
                rect.anchorMax = new Vector2(1, 0);
                rect.pivot = new Vector2(1, 0);
                break;
            case TextAnchor.MiddleLeft:
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0, 0.5f);
                break;
        }

        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(300, 200);

        return group;
    }

    static TextMeshProUGUI CreateTextElement(Transform parent, string name, string text,
        int fontSize, Color color, TextAlignmentOptions alignment, Vector2 pos, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableAutoSizing = false;

        return tmp;
    }

    static void CreateText(Transform parent, string name, string text, Vector2 pos, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(600, 80);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }

    static GameObject CreateButton(Transform parent, string name, string text, Vector2 pos, Color textColor, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(300, 50);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0.8f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 1f, 0.53f, 0.2f);
        cb.pressedColor = new Color(0f, 1f, 0.53f, 0.4f);
        btn.colors = cb;

        // Border
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = textColor;
        outline.effectDistance = new Vector2(1, 1);

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return btnObj;
    }

    static GameObject CreateBarBackground(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject bg = new GameObject(name);
        bg.transform.SetParent(parent);
        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = bg.AddComponent<Image>();
        img.color = color;

        return bg;
    }

    static GameObject CreateBarFill(Transform parent, string name, Color color)
    {
        GameObject fill = new GameObject(name);
        fill.transform.SetParent(parent);
        RectTransform rect = fill.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0, 0.5f);

        Image img = fill.AddComponent<Image>();
        img.color = color;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillAmount = 1f;

        return fill;
    }

    // ==================== BUILD SETTINGS ====================

    [MenuItem("Assassin/Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        string[] scenePaths = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity"
        };

        foreach (string path in scenePaths)
        {
            if (File.Exists(path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build settings configured with " + scenes.Count + " scenes.");
    }
}
