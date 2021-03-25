using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering
{
    /// <summary>
    /// Internal type definitions for <see cref="AnimationClipUpgrader"/>.
    /// </summary>
    /// <remarks>
    /// This class contains two categories of internal types:
    /// 1. Proxies for UnityObject assets (used for test mocking and facilitating usage without requiring loading all assets of the given type at once).
    /// 2. Asset path wrappers (used for stronger typing and clarity in the API surface).
    /// </remarks>
    static partial class AnimationClipUpgrader
    {
        #region Proxies

        internal interface IAnimationClip
        {
            AnimationClip Clip { get; }
            EditorCurveBinding[] GetCurveBindings();
            void ReplaceBinding(EditorCurveBinding oldBinding, EditorCurveBinding newBinding);
        }

        internal struct AnimationClipProxy : IAnimationClip
        {
            public AnimationClip Clip { get; set; }
            public EditorCurveBinding[] GetCurveBindings() => AnimationUtility.GetCurveBindings(Clip);
            public void ReplaceBinding(EditorCurveBinding oldBinding, EditorCurveBinding newBinding)
            {
                var curve = AnimationUtility.GetEditorCurve(Clip, oldBinding);
                AnimationUtility.SetEditorCurve(Clip, oldBinding, null);
                AnimationUtility.SetEditorCurve(Clip, newBinding, curve);
            }
            public static implicit operator AnimationClip(AnimationClipProxy proxy) => proxy.Clip;
            public static implicit operator AnimationClipProxy(AnimationClip clip) => new AnimationClipProxy { Clip = clip };
            public override string ToString() => Clip.ToString();
        }

        internal interface IMaterial
        {
            string ShaderName{ get; }
        }

        internal struct MaterialProxy : IMaterial
        {
            Material m_Material;
            public string ShaderName => m_Material.shader.name;
            public static implicit operator Material(MaterialProxy proxy) => proxy.m_Material;
            public static implicit operator MaterialProxy(Material material) => new MaterialProxy { m_Material = material };
            public override string ToString() => m_Material.ToString();
        }

        internal interface IRenderer
        {
        }

        internal struct RendererProxy : IRenderer
        {
            Renderer m_Renderer;
            public void GetSharedMaterials(List<IMaterial> materials)
            {
                materials.Clear();
                var m = ListPool<Material>.Get();
                m_Renderer.GetSharedMaterials(m);
                materials.AddRange(m.Select(mm => (MaterialProxy)mm).Cast<IMaterial>());
                ListPool<Material>.Release(m);
            }
            public static implicit operator Renderer(RendererProxy proxy) => proxy.m_Renderer;
            public static implicit operator RendererProxy(Renderer renderer) => new RendererProxy { m_Renderer = renderer };
            public override string ToString() => m_Renderer.ToString();
        }

        #endregion

        #region AssetPath Wrappers

        internal interface IAssetPath
        {
            string Path { get; }
        }

        internal struct ClipPath: IAssetPath
        {
            public string Path { get; set; }
            public static implicit operator string(ClipPath clip) => clip.Path;
            public static implicit operator ClipPath(string path) => new ClipPath { Path = path };
            public static implicit operator ClipPath(AnimationClip clip) => new ClipPath { Path = AssetDatabase.GetAssetPath(clip) };
            public override string ToString() => Path;
        }

        internal struct PrefabPath: IAssetPath
        {
            public string Path { get; set; }
            public static implicit operator string(PrefabPath prefab) => prefab.Path;
            public static implicit operator PrefabPath(string path) => new PrefabPath { Path = path };
            public static implicit operator PrefabPath(GameObject go) => new PrefabPath { Path = AssetDatabase.GetAssetPath(go) };
            public override string ToString() => Path;
        }

        internal struct ScenePath: IAssetPath
        {
            public string Path { get; set; }
            public static implicit operator string(ScenePath scene) => scene.Path;
            public static implicit operator ScenePath(string path) => new ScenePath { Path = path };
            public static implicit operator ScenePath(SceneAsset scene) => new ScenePath { Path = AssetDatabase.GetAssetPath(scene) };
            public override string ToString() => Path;
        }

        #endregion
    }
}
