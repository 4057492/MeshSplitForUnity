using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityMeshSimplifier;
using UnityEngine.Rendering;

public class BatchAutoLODsWindowEditor : EditorWindow
{
    #region //name string button size
    // private const string FadeModeFieldName = "fadeMode";
    // private const string AnimateCrossFadingFieldName = "animateCrossFading";
    // private const string AutoCollectRenderersFieldName = "autoCollectRenderers";
    // private const string SimplificationOptionsFieldName = "simplificationOptions";
    // private const string SaveAssetsPathFieldName = "saveAssetsPath";
    //private const string LevelsFieldName = "levels";
    private const string IsGeneratedFieldName = "isGenerated";
    // private const string LevelScreenRelativeHeightFieldName = "screenRelativeTransitionHeight";        
    //private const string LevelFadeTransitionWidthFieldName = "fadeTransitionWidth";
    // private const string LevelQualityFieldName = "quality";
    // private const string LevelCombineMeshesFieldName = "combineMeshes";
    // private const string LevelCombineSubMeshesFieldName = "combineSubMeshes";
    // private const string LevelRenderersFieldName = "renderers";
    // private const string SimplificationOptionsEnableSmartLinkFieldName = "EnableSmartLink";
    // private const string SimplificationOptionsVertexLinkDistanceFieldName = "VertexLinkDistance";
    private const float RemoveLevelButtonSize = 20f;
    // private const float RendererButtonWidth = 60f;
    // private const float RemoveRendererButtonSize = 20f;
    #endregion

    #region //property
    // private SerializedProperty fadeModeProperty = null;
    // private SerializedProperty animateCrossFadingProperty = null;
    // private SerializedProperty autoCollectRenderersProperty = null;
    // private SerializedProperty simplificationOptionsProperty = null;
    // private SerializedProperty saveAssetsPathProperty = null;
    // private SerializedProperty levelsProperty = null;
    // private SerializedProperty isGeneratedProperty = null;

    // private bool overrideSaveAssetsPath = false;
    // private bool[] settingsExpanded = null;
    // private LODGeneratorHelper lodGeneratorHelper = null;
    #endregion

    [MenuItem("Examples/Batch AutoLODs")]
    static void Init()
    {
        BatchAutoLODsWindowEditor window = (BatchAutoLODsWindowEditor)EditorWindow.GetWindow(typeof(BatchAutoLODsWindowEditor));
        window.Show();
    }
    #region // this property
    //private LODGeneratorHelper myHelper = null;

    private LODFadeMode myFadeMode = 0;
    private bool myAnimateCrossFading = false;
    private bool myAutoCollectRenderers = true;
    private bool mySimplificationOptions = false;
    private bool myPreserveBorderEdges = false;
    private bool myPreserveUVSeamEdges = false;
    private bool myPreserveUVFoldoverEdges = false;
    private bool myEnableSmartLink = true;
    private double myVertexLinkDistance = double.Epsilon;
    private int myMaxIterationCount = 100;
    private double myAgressiveness = 7.0;
    private bool myOverrideSaveAssetPath = false;
    private string mySaveAssetsPath = string.Empty;
    //private List<LODLevel> myLevels = null;
    private bool mySettings = false;
    private List<float> myScreenRelativeTransitionHeights = null;
    private List<float> myQualities = null;
    private List<bool> myCombineMeshes = null;
    private List<SkinQuality> mySkinQualities = null;
    private List<ShadowCastingMode> myShadowCastingModes = null;
    private List<bool> myReceiveShadows = null;
    private List<MotionVectorGenerationMode> myMotionVectorGenerationModes = null;
    private List<bool> mySkinnedMotionVectors = null;
    private List<LightProbeUsage> myLightProbeUsages = null;
    private List<ReflectionProbeUsage> myReflectionProbeUsages = null;

    #endregion

    #region //content
    private static readonly GUIContent fadeModeContent = new GUIContent("Fade Mode", "The fade mode used by the created LOD group.");
    private static readonly GUIContent animateCrossFadingContent = new GUIContent("Animate Cross Fading", "If the cross-fading should be animated by time.");
    private static readonly GUIContent autoCollectRenderersContent = new GUIContent("Auto Collect Renderers", "If the renderers under this game object and any children should be automatically collected.");
    private static readonly GUIContent simplificationOptionsContent = new GUIContent("Simplification Options", "The simplification options.");
    private static readonly GUIContent PreserveBorderEdgesContent = new GUIContent("Preserve Border Edges", "If the border edges should be preserved.");
    private static readonly GUIContent PreserveUVSeamEdgesContent = new GUIContent("Preserve UV Seam Edges", "If the UV seam edges should be preserved.");
    private static readonly GUIContent PreserveUVFoldoverEdgesContent = new GUIContent("Preserve UV Foldover Edges", "If the UV foldover edges should be preserved.");
    private static readonly GUIContent EnableSmartLinkContent = new GUIContent("Enable Smart Link", "If a feature for smarter vertex linking should be enabled, reducing artifacts at the cost of slower simplification.");
    private static readonly GUIContent VertexLinkDistanceContent = new GUIContent("Vertex Link Distance", "The maximum distance between two vertices in order to link them.");
    private static readonly GUIContent MaxIterationCountContent = new GUIContent("Max Iteration Count", "The maximum squared distance between two vertices in order to link them.");
    private static readonly GUIContent AgressivenessContent = new GUIContent("Agressiveness", "The agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.");
    private static readonly GUIContent screenRelativeTransitionHeightContent = new GUIContent("Screen Relative Transition Height Content", "The screen relative height to use for the transition.");
    private static readonly GUIContent qualityContent = new GUIContent("Quality", "The desired quality for this level.");
    //private static readonly GUIContent createLevelButtonContent = new GUIContent("Create Level", "Creates a new LOD level.");
    private static readonly GUIContent deleteLevelButtonContent = new GUIContent("X", "Deletes this LOD level.");
    private static readonly GUIContent generateLODButtonContent = new GUIContent("Batch Generate LODs", "Generates the LOD levels.");
    //private static readonly GUIContent destroyLODButtonContent = new GUIContent("Destroy LODs", "Destroys the LOD levels.");
    private static readonly GUIContent settingsContent = new GUIContent("Settings", "The settings for the LOD level.");
    private static readonly GUIContent combineMeshesContent = new GUIContent("Combine Meshes", "If all renderers and meshes under this level should be combined into one, where possible.");
    private static readonly GUIContent skinQualityContent = new GUIContent("Skin Quality", "The skin quality to use for renderers on this level.");
    private static readonly GUIContent shadowCastingModeContent = new GUIContent("Shadow Casting Mode", "The shadow casting mode for renderers on this level.");
    private static readonly GUIContent receiveShadowsContent = new GUIContent("Receive Shadows", "If renderers on this level should receive shadows.");
    private static readonly GUIContent motionVectorGenerationModeContent = new GUIContent("Motion Vector Generation Mode", "The motion vector generation mode for renderers on this level.");
    private static readonly GUIContent skinnedMotionVectorsContent = new GUIContent("Skinned Motion Vectors", "If renderers on this level should use skinned motion vectors.");
    private static readonly GUIContent lightProbeUsageContent = new GUIContent("Light Probe Usage", "The light probe usage for renderers on this level.");
    private static readonly GUIContent reflectionProbeUsageContent = new GUIContent("Reflection Probe Usage", "The reflection probe usage for renderers on this level.");
    //private static readonly GUIContent renderersHeaderContent = new GUIContent("Renderers:", "The renderers used for this LOD level.");
    //private static readonly GUIContent removeRendererButtonContent = new GUIContent("X", "Removes this renderer.");
    //private static readonly GUIContent addRendererButtonContent = new GUIContent("Add", "Adds a renderer to this LOD level.");
    private static readonly GUIContent overrideSaveAssetsPathContent = new GUIContent("Override Save Assets Path", "If you want to override the path where the generated assets are saved.");
    private static readonly GUIContent saveAssetsPathContent = new GUIContent("Save Assets Path", "The path within the project to save the generated assets. Leave this empty to use the default path.");
    private static readonly Color removeColor = new Color(1f, 0.6f, 0.6f, 1f);
    #endregion
    
    void OnEnable()//initialize list
    {
        myScreenRelativeTransitionHeights = new List<float>();
        myQualities = new List<float>();
        myCombineMeshes = new List<bool>();
        mySkinQualities = new List<SkinQuality>();
        myShadowCastingModes = new List<ShadowCastingMode>();
        myReceiveShadows = new List<bool>();
        myMotionVectorGenerationModes = new List<MotionVectorGenerationMode>();
        mySkinnedMotionVectors = new List<bool>();
        myLightProbeUsages = new List<LightProbeUsage>();
        myReflectionProbeUsages = new List<ReflectionProbeUsage>();
        myScreenRelativeTransitionHeights.Add(0.5f);
        myScreenRelativeTransitionHeights.Add(0.17f);
        myScreenRelativeTransitionHeights.Add(0.02f);
        myQualities.Add(1f);
        myQualities.Add(0.65f);
        myQualities.Add(0.4225f);
        myCombineMeshes.Add(false);
        myCombineMeshes.Add(false);
        myCombineMeshes.Add(false);
        mySkinQualities.Add(SkinQuality.Auto);
        mySkinQualities.Add(SkinQuality.Auto);
        mySkinQualities.Add(SkinQuality.Auto);
        myShadowCastingModes.Add(ShadowCastingMode.On);
        myShadowCastingModes.Add(ShadowCastingMode.On);
        myShadowCastingModes.Add(ShadowCastingMode.On);
        myReceiveShadows.Add(true);
        myReceiveShadows.Add(true);
        myReceiveShadows.Add(true);
        myMotionVectorGenerationModes.Add(MotionVectorGenerationMode.Object);
        myMotionVectorGenerationModes.Add(MotionVectorGenerationMode.Object);
        myMotionVectorGenerationModes.Add(MotionVectorGenerationMode.Object);
        mySkinnedMotionVectors.Add(true);
        mySkinnedMotionVectors.Add(true);
        mySkinnedMotionVectors.Add(true);
        myLightProbeUsages.Add(LightProbeUsage.BlendProbes);
        myLightProbeUsages.Add(LightProbeUsage.BlendProbes);
        myLightProbeUsages.Add(LightProbeUsage.BlendProbes);
        myReflectionProbeUsages.Add(ReflectionProbeUsage.BlendProbes);
        myReflectionProbeUsages.Add(ReflectionProbeUsage.BlendProbes);
        myReflectionProbeUsages.Add(ReflectionProbeUsage.BlendProbes);
    }
    void OnGUI()
    {
        DrawLODHelperProperty();
        if(Selection.gameObjects.Length > 0)
        {
            if (Selection.gameObjects[0].transform.childCount>0)
            {
                if (GUILayout.Button(generateLODButtonContent))
                {
                    BatchGenerateLOD();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This GameObject has no child.", MessageType.Error, true);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please select the GameObject to operate.", MessageType.Info, true);
        }
    }

    void DrawLODHelperProperty()
    {
        myFadeMode = (LODFadeMode)EditorGUILayout.EnumPopup(fadeModeContent, myFadeMode);
        if (myFadeMode != 0)
            myAnimateCrossFading = EditorGUILayout.Toggle(animateCrossFadingContent, myAnimateCrossFading);
        myAutoCollectRenderers = EditorGUILayout.Toggle(autoCollectRenderersContent, myAutoCollectRenderers);
        mySimplificationOptions = EditorGUILayout.Foldout(mySimplificationOptions, simplificationOptionsContent);
        if (mySimplificationOptions)
        {
            ++EditorGUI.indentLevel;
            myPreserveBorderEdges = EditorGUILayout.Toggle(PreserveBorderEdgesContent, myPreserveBorderEdges);
            myPreserveUVSeamEdges = EditorGUILayout.Toggle(PreserveUVSeamEdgesContent, myPreserveUVSeamEdges);
            myPreserveUVFoldoverEdges = EditorGUILayout.Toggle(PreserveUVFoldoverEdgesContent, myPreserveUVFoldoverEdges);
            myEnableSmartLink = EditorGUILayout.Toggle(EnableSmartLinkContent, myEnableSmartLink);
            myVertexLinkDistance = EditorGUILayout.DoubleField(VertexLinkDistanceContent, myVertexLinkDistance);
            myMaxIterationCount = EditorGUILayout.IntField(MaxIterationCountContent, myMaxIterationCount);
            myAgressiveness = EditorGUILayout.DoubleField(AgressivenessContent, myAgressiveness);
            --EditorGUI.indentLevel;
        }
        myOverrideSaveAssetPath = EditorGUILayout.Toggle(overrideSaveAssetsPathContent, myOverrideSaveAssetPath);
        if(myOverrideSaveAssetPath)
            mySaveAssetsPath = EditorGUILayout.TextField(saveAssetsPathContent, mySaveAssetsPath);
        else
            mySaveAssetsPath = string.Empty;
        for (int index = 0; index < myQualities.Count; index++)
        { 
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(string.Format("Level {0}", index + 1), EditorStyles.boldLabel);
            var previousBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = removeColor;
            if (GUILayout.Button(deleteLevelButtonContent, GUILayout.Width(RemoveLevelButtonSize)))
            {
                myScreenRelativeTransitionHeights.RemoveAt(index);
                myQualities.RemoveAt(index);
                break;
            }
            GUI.backgroundColor = previousBackgroundColor;
            EditorGUILayout.EndHorizontal();
            ++EditorGUI.indentLevel;
            myScreenRelativeTransitionHeights[index] = EditorGUILayout.Slider(screenRelativeTransitionHeightContent, myScreenRelativeTransitionHeights[index], 0, 1f);
            myQualities[index] = EditorGUILayout.Slider(qualityContent, myQualities[index], 0, 1f);
            mySettings = EditorGUILayout.Foldout(mySettings, settingsContent);
            if(mySettings)
            {
                ++EditorGUI.indentLevel;
                myCombineMeshes[index] = EditorGUILayout.Toggle(combineMeshesContent, myCombineMeshes[index]);
                mySkinQualities[index] = (SkinQuality)EditorGUILayout.EnumPopup(skinQualityContent, mySkinQualities[index]);
                myShadowCastingModes[index] = (ShadowCastingMode)EditorGUILayout.EnumPopup(shadowCastingModeContent, myShadowCastingModes[index]);
                myReceiveShadows[index] = EditorGUILayout.Toggle(receiveShadowsContent, myReceiveShadows[index]);
                myMotionVectorGenerationModes[index] = (MotionVectorGenerationMode)EditorGUILayout.EnumPopup(motionVectorGenerationModeContent, myMotionVectorGenerationModes[index]);
                mySkinnedMotionVectors[index] = EditorGUILayout.Toggle(skinnedMotionVectorsContent, mySkinnedMotionVectors[index]);
                myLightProbeUsages[index] = (LightProbeUsage)EditorGUILayout.EnumPopup(lightProbeUsageContent, myLightProbeUsages[index]);
                myReflectionProbeUsages[index] = (ReflectionProbeUsage)EditorGUILayout.EnumPopup(reflectionProbeUsageContent, myReflectionProbeUsages[index]);
                --EditorGUI.indentLevel;
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();
        }
        if(GUILayout.Button("Create Level"))
        {
            myScreenRelativeTransitionHeights.Add(myScreenRelativeTransitionHeights[myScreenRelativeTransitionHeights.Count - 1] * 0.5f);
            myQualities.Add(myQualities[myQualities.Count - 1] * 0.65f);
        }

    }

    void BatchGenerateLOD()
    {
        Debug.Log("Begin Generating.");
        GameObject SubMeshHolder = Selection.gameObjects[0];
        LODGeneratorHelper myHelper = null;
        List<LODLevel> myLODLevels = new List<LODLevel>();
        //SerializedProperty levelsProperty = null;
        //SerializedProperty LevelProperty = null;
        for (int i = 0; i < myQualities.Count; i++) 
        {
            myLODLevels.Add(new LODLevel(myScreenRelativeTransitionHeights[i], myQualities[i]));
        }
        for (int index = 0; index < SubMeshHolder.transform.childCount; index++)
        {
            if (!EditorUtility.DisplayCancelableProgressBar("Generating LODs", "Child index: " + index, (float)index / SubMeshHolder.transform.childCount))
            {
                myHelper = SubMeshHolder.transform.GetChild(index).gameObject.AddComponent<LODGeneratorHelper>();
                myHelper.FadeMode = myFadeMode;
                myHelper.AnimateCrossFading = myAnimateCrossFading;
                myHelper.AutoCollectRenderers = myAutoCollectRenderers;
                myHelper.SimplificationOptions = new SimplificationOptions() { 
                    PreserveBorderEdges = myPreserveBorderEdges, 
                    PreserveUVSeamEdges = myPreserveUVSeamEdges, 
                    PreserveUVFoldoverEdges = myPreserveUVFoldoverEdges, 
                    EnableSmartLink = myEnableSmartLink, 
                    VertexLinkDistance = myVertexLinkDistance, 
                    MaxIterationCount = myMaxIterationCount, 
                    Agressiveness = myAgressiveness 
                    };
                myHelper.SaveAssetsPath = mySaveAssetsPath;
                myHelper.Levels = myLODLevels.ToArray();
                //levelsProperty = new SerializedObject(myHelper).FindProperty(LevelsFieldName);
                //LevelProperty = levelsProperty.GetArrayElementAtIndex(index);
                try
                {
                    var lodGroup = LODGenerator.GenerateLODs(myHelper);
                    if (lodGroup != null)
                    {
                        using (var serializedObject = new SerializedObject(myHelper))
                        {
                            var isGeneratedProperty = serializedObject.FindProperty(IsGeneratedFieldName);
                            serializedObject.UpdateIfRequiredOrScript();
                            isGeneratedProperty.boolValue = true;
                            serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    EditorUtility.DisplayDialog("Failed to generate LODs!", ex.Message, "OK");
                    break;
                }
                finally
                {
                    //EditorUtility.ClearProgressBar();
                }
            }
            else
            {
                Debug.LogWarning("Generate cancle.");
                break;
            }
        }
        EditorUtility.ClearProgressBar();
        Debug.Log("End Generating.");

    }


    void OnInspectorUpdate()
    {
        Repaint();
    }
}
