using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    using CED = CoreEditorDrawer<SerializedHDCamera>;

    static partial class HDCameraUI
    {
        partial class Rendering
        {
            public static readonly CED.IDrawer Drawer = CED.FoldoutGroup(
                Styles.header,
                Expandable.Rendering,
                k_ExpandedState,
                FoldoutOption.Indent,
                CED.Group(
                    Drawer_Rendering_Antialiasing
                    ),
                AntialiasingModeDrawer(
                    HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing,
                    Drawer_Rendering_Antialiasing_SMAA),
                AntialiasingModeDrawer(
                    HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing,
                    Drawer_Rendering_Antialiasing_TAA),
                CED.Group(
                    Drawer_Rendering_StopNaNs,
                    Drawer_Rendering_Dithering,
                    Drawer_Rendering_CullingMask,
                    Drawer_Rendering_OcclusionCulling,
                    Drawer_Rendering_ExposureTarget,
                    Drawer_Rendering_RenderingPath,
                    Drawer_Rendering_CameraWarnings
                    ),
                CED.Conditional(
                    (serialized, owner) => !serialized.passThrough.boolValue && serialized.customRenderingSettings.boolValue,
                    (serialized, owner) => FrameSettingsUI.Inspector().Draw(serialized.frameSettings, owner)
                )
            );

#if ENABLE_NVIDIA_MODULE
            static void DrawDLSSControl(SerializedHDCamera p, Editor owner, out bool showAntialiasContentAsFallback, out bool doGlobalIndent)
            {
                showAntialiasContentAsFallback = false;
                doGlobalIndent = false;

                if (!p.allowDynamicResolution.boolValue)
                    return;

                bool isDLSSEnabledInQualityAsset = HDRenderPipeline.currentAsset.currentPlatformRenderPipelineSettings.dynamicResolutionSettings.enableDLSS;

                EditorGUILayout.PropertyField(p.allowDeepLearningSuperSampling, Styles.DLSSAllow);
                if (!isDLSSEnabledInQualityAsset && p.allowDeepLearningSuperSampling.boolValue)
                {
                    EditorGUILayout.HelpBox(Styles.DLSSNotEnabledInQualityAsset, MessageType.Info);
                }

                if (p.allowDeepLearningSuperSampling.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(p.deepLearningSuperSamplingUseCameraQualitySettings, Styles.DLSSUseCameraQualitySettings);
                    if (p.deepLearningSuperSamplingUseCameraQualitySettings.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        var dlssRect = EditorGUILayout.GetControlRect();
                        EditorGUI.BeginProperty(dlssRect, HDRenderPipelineUI.Styles.DLSSQualitySettingContent, p.deepLearningSuperSamplingQuality);
                        {
                            EditorGUI.BeginChangeCheck();
                            int selectedVal = EditorGUI.Popup(dlssRect, HDRenderPipelineUI.Styles.DLSSQualitySettingContent, p.deepLearningSuperSamplingQuality.intValue, HDRenderPipelineUI.Styles.DLSSPerfQualityNames);
                            if (EditorGUI.EndChangeCheck())
                                p.deepLearningSuperSamplingQuality.intValue = selectedVal;
                        }
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.PropertyField(p.deepLearningSuperSamplingUseCameraAttributeSettings, Styles.DLSSUseCameraAttributes);
                    if (p.deepLearningSuperSamplingUseCameraAttributeSettings.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(p.deepLearningSuperSamplingUseOptimalSettings, HDRenderPipelineUI.Styles.DLSSUseOptimalSettingsContent);
                        using (new EditorGUI.DisabledScope(p.deepLearningSuperSamplingUseOptimalSettings.boolValue))
                        {
                            EditorGUILayout.PropertyField(p.deepLearningSuperSamplingSharpening, HDRenderPipelineUI.Styles.DLSSSharpnessContent);
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                bool isDLSSEnabled = isDLSSEnabledInQualityAsset && p.allowDeepLearningSuperSampling.boolValue;
                doGlobalIndent = isDLSSEnabled;
                if (isDLSSEnabled)
                {
                    showAntialiasContentAsFallback = true;
                    bool featureDetected = HDDynamicResolutionPlatformCapabilities.DLSSDetected;

                    //write here support string for dlss upscaler
                    EditorGUILayout.HelpBox(
                        featureDetected ? Styles.DLSSFeatureDetectedMsg : Styles.DLSSFeatureNotDetectedMsg,
                        featureDetected ? MessageType.Info : MessageType.Warning);
                }
            }

#endif

            static void Drawer_Rendering_Antialiasing(SerializedHDCamera p, Editor owner)
            {
                bool doGlobalIndent = false;
                bool showAntialiasContentAsFallback = false;

#if ENABLE_NVIDIA_MODULE
                DrawDLSSControl(p, owner, out showAntialiasContentAsFallback, out doGlobalIndent);
#endif

                if (doGlobalIndent)
                    EditorGUI.indentLevel++;

                Rect antiAliasingRect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(antiAliasingRect, Styles.antialiasing, p.antialiasing);
                {
                    EditorGUI.BeginChangeCheck();
                    int selectedValue = (int)(HDAdditionalCameraData.AntialiasingMode)EditorGUI.EnumPopup(antiAliasingRect, showAntialiasContentAsFallback ? Styles.antialiasingContentFallback : Styles.antialiasing, (HDAdditionalCameraData.AntialiasingMode)p.antialiasing.intValue);

                    if (EditorGUI.EndChangeCheck())
                        p.antialiasing.intValue = selectedValue;
                }

                if (doGlobalIndent)
                    EditorGUI.indentLevel--;
            }

            static CED.IDrawer AntialiasingModeDrawer(HDAdditionalCameraData.AntialiasingMode antialiasingMode, CED.ActionDrawer antialiasingDrawer)
            {
                return CED.Conditional(
                    (serialized, owner) => serialized.antialiasing.intValue == (int)antialiasingMode,
                    CED.Group(
                        GroupOption.Indent,
                        antialiasingDrawer
                    )
                );
            }

            static void Drawer_Rendering_Antialiasing_SMAA(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.SMAAQuality, Styles.SMAAQualityPresetContent);
            }

            static void Drawer_Rendering_Antialiasing_TAA(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.taaQualityLevel, Styles.TAAQualityLevel);
                EditorGUILayout.PropertyField(p.taaSharpenStrength, Styles.TAASharpen);

                if (p.taaQualityLevel.intValue > (int)HDAdditionalCameraData.TAAQualityLevel.Low)
                {
                    EditorGUILayout.PropertyField(p.taaHistorySharpening, Styles.TAAHistorySharpening);
                    EditorGUILayout.PropertyField(p.taaAntiFlicker, Styles.TAAAntiFlicker);
                }

                if (p.taaQualityLevel.intValue == (int)HDAdditionalCameraData.TAAQualityLevel.High)
                {
                    EditorGUILayout.PropertyField(p.taaMotionVectorRejection, Styles.TAAMotionVectorRejection);
                    EditorGUILayout.PropertyField(p.taaAntiRinging, Styles.TAAAntiRinging);
                }
            }

            static void Drawer_Rendering_RenderingPath(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.passThrough, Styles.fullScreenPassthrough);
                using (new EditorGUI.DisabledScope(p.passThrough.boolValue))
                    EditorGUILayout.PropertyField(p.customRenderingSettings, Styles.renderingPath);
            }

            static void Drawer_Rendering_Dithering(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.dithering, Styles.dithering);
            }

            static void Drawer_Rendering_StopNaNs(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.stopNaNs, Styles.stopNaNs);
            }

            static void Drawer_Rendering_CullingMask(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.baseCameraSettings.cullingMask, Styles.cullingMask);
            }

            static void Drawer_Rendering_OcclusionCulling(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.baseCameraSettings.occlusionCulling, Styles.occlusionCulling);
            }

            static void Drawer_Rendering_ExposureTarget(SerializedHDCamera p, Editor owner)
            {
                EditorGUILayout.PropertyField(p.exposureTarget, Styles.exposureTarget);
            }

            static readonly MethodInfo k_Camera_GetCameraBufferWarnings = typeof(Camera).GetMethod("GetCameraBufferWarnings", BindingFlags.Instance | BindingFlags.NonPublic);
            static string[] GetCameraBufferWarnings(Camera camera)
            {
                return (string[])k_Camera_GetCameraBufferWarnings.Invoke(camera, null);
            }

            static void Drawer_Rendering_CameraWarnings(SerializedHDCamera p, Editor owner)
            {
                foreach (Camera camera in p.serializedObject.targetObjects)
                {
                    var warnings = GetCameraBufferWarnings(camera);
                    if (warnings.Length > 0)
                        EditorGUILayout.HelpBox(string.Join("\n\n", warnings), MessageType.Warning, true);
                }
            }
        }
    }
}
