using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class LatestWebGLBuild
{
    public static void Build()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/gamejam.unity");
        OneBitTools.Validate();
        PlayerSettings.productName="Masked Dice — The Masked Tribunal"; PlayerSettings.companyName="DiscElysium";
        PlayerSettings.bundleVersion=DeepSeekClient.BuildVersion;
        PlayerSettings.defaultWebScreenWidth=1280; PlayerSettings.defaultWebScreenHeight=720;
        PlayerSettings.runInBackground=true;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback=true; PlayerSettings.WebGL.dataCaching=true;
        PlayerSettings.WebGL.initialMemorySize=512; PlayerSettings.WebGL.maximumMemorySize=2048;
        PlayerSettings.WebGL.debugSymbolMode=WebGLDebugSymbolMode.Off;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL,ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL,ManagedStrippingLevel.Low);
        AssetDatabase.SaveAssets();
        string output=Path.GetFullPath("../webgl-output/latest"); Directory.CreateDirectory(output);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Scenes/gamejam.unity"},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Latest WebGL build failed: "+report.summary.result);
        Debug.Log("LATEST_WEBGL_BUILD_OK version="+DeepSeekClient.BuildVersion+" bytes="+report.summary.totalSize);
    }
}
