using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

public static class QuestBuild
{
    public const string PackageId="com.eeminionn.bb8.quest";
    public static void Configure()
    {
        Directory.CreateDirectory("Assets/XR");AssetDatabase.Refresh();
        const string path="Assets/XR/Quest General Settings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
        if(!settings){settings=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(settings,path);}
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,settings,true);
        if(!settings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))settings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        var general=settings.SettingsForBuildTarget(BuildTargetGroup.Android);general.InitManagerOnStart=true;
        if(!XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.OpenXR.OpenXRLoader",BuildTargetGroup.Android))throw new Exception("Could not enable OpenXR.");
        FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
        var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        xr.GetFeature<OculusTouchControllerProfile>().enabled=true;
        var quest=xr.GetFeature<MetaQuestFeature>();quest.enabled=true;
        var feature=new SerializedObject(quest);var devices=feature.FindProperty("targetDevices");
        devices.arraySize=1;var device=devices.GetArrayElementAtIndex(0);
        device.FindPropertyRelative("visibleName").stringValue="Quest 2";
        device.FindPropertyRelative("manifestName").stringValue="quest2";
        device.FindPropertyRelative("enabled").boolValue=true;feature.ApplyModifiedPropertiesWithoutUndo();
        xr.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;
        xr.depthSubmissionMode=OpenXRSettings.DepthSubmissionMode.None;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,PackageId);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel36;
        PlayerSettings.Android.forceInternetPermission=true;
        PlayerSettings.Android.applicationEntry=AndroidApplicationEntry.Activity;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan});
        PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.insecureHttpOption=InsecureHttpOption.AlwaysAllowed;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        SetInputHandling(1);
        EditorUtility.SetDirty(xr);EditorUtility.SetDirty(quest);EditorUtility.SetDirty(general);EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log("BB8_QUEST_CONFIGURED");
    }
    public static void RestoreDesktopInput(){SetInputHandling(2);PlayerSettings.colorSpace=ColorSpace.Gamma;AssetDatabase.SaveAssets();}
    static void SetInputHandling(int value)
    {
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        settings.FindProperty("activeInputHandler").intValue=value;settings.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();
    }
    [MenuItem("BB8/Build Quest APK")]
    public static void Build()
    {
        PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel36;
        BB8Setup.OpenDemo();
        EditorUserBuildSettings.buildAppBundle=false;
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{BB8Setup.ScenePath},locationPathName="../BB8-Quest.apk",target=BuildTarget.Android,
            options=BuildOptions.Development
        });
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Quest build failed: "+report.summary.result);
        Debug.Log("BB8_QUEST_BUILD_OK "+report.summary.totalSize);
    }
}
