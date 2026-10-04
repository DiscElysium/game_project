using System;
using System.IO;
using LLMUnity;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class WebGLBuild
{
    public static void Build()
    {
        string output = Environment.GetEnvironmentVariable("MASKED_DICE_WEBGL_OUTPUT") ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../../webgl-output/site"));
        EditorSceneManager.OpenScene("Assets/Scenes/gamejam.unity");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        foreach (var texturePath in new[] { "Assets/BrowserArt/texture/bayer_m8.png", "Assets/BrowserArt/texture/blue_noise_128.png" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        foreach (var path in new[] { "Assets/Shader Graphs_1bit shader.mat", "Assets/Shader Graphs_1bit shader 1.mat", "Assets/Shader Graphs_1bit shader 2.mat" })
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var texture = material.GetTexture("_Dither_texture");
            if (!texture) throw new Exception("Missing dither texture in " + path);
            EditorUtility.SetDirty(material);
            Debug.Log("WEBGL_BAYER=" + path + "; texture=" + AssetDatabase.GetAssetPath(material.GetTexture("_Dither_texture")));
        }
        var ui = UnityEngine.Object.FindObjectOfType<PlayerUIController>(true);
        var manager = UnityEngine.Object.FindObjectOfType<LiarGameManager>(true);
        if (manager && manager.statusText) manager.statusText.text = "CLICK THE MASK TO START";
        if (ui && ui.btnStartGame)
        {
            var label = ui.btnStartGame.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (label) label.text = "START GAME";
            var tmp = ui.btnStartGame.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (tmp) tmp.text = "START GAME";
        }
        foreach (var model in UnityEngine.Object.FindObjectsOfType<LLM>(true))
            UnityEngine.Object.DestroyImmediate(model.gameObject);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/webgl.unity");

        PlayerSettings.companyName = "DiscElysium";
        PlayerSettings.productName = "Masked Liar's Dice";
        PlayerSettings.defaultWebScreenWidth = 1600;
        PlayerSettings.defaultWebScreenHeight = 900;
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.initialMemorySize = 256;
        PlayerSettings.WebGL.maximumMemorySize = 2048;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);
        AssetDatabase.SaveAssets();

        Directory.CreateDirectory(output);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/webgl.unity" },
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Debug.Log("WEBGL_BUILD_RESULT=" + report.summary.result + "; bytes=" + report.summary.totalSize);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("WebGL build failed: " + report.summary.result);
    }
}
