using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace Unity.Assets.MaterialVariant.Editor
{
    [CustomEditor(typeof(MaterialVariantImporter))]
    public class MaterialVariantEditor : ScriptedImporterEditor
    {
        private UnityEditor.Editor targetEditor = null;

        public override bool showImportedObject => false;

        protected override Type extraDataType => typeof(MaterialVariant);
        protected override void InitializeExtraDataInstance(Object extraTarget, int targetIndex)
            => LoadMaterialVariant((MaterialVariant)extraTarget, ((AssetImporter)targets[targetIndex]).assetPath);
        
        void LoadMaterialVariant(MaterialVariant variantTarget, string assetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(a => a.GetType() == typeof(MaterialVariant));
            if (!assets.Any())
                return;
            var asset = assets.First() as MaterialVariant;

            variantTarget.rootGUID = asset.rootGUID;
            variantTarget.isShader = asset.isShader;
            variantTarget.overrides = asset.overrides;
        }

        static Dictionary<UnityEditor.Editor, MaterialVariant[]> registeredVariants = new Dictionary<UnityEditor.Editor, MaterialVariant[]>();

        public static MaterialVariant[] GetMaterialVariantsFor(MaterialEditor editor)
        {
            if (!registeredVariants.ContainsKey(editor))
                return null;

            return registeredVariants[editor];
        }

        public override void OnEnable()
        {
            base.OnEnable();
            targetEditor = CreateEditor(assetTarget);
            registeredVariants.Add(targetEditor, extraDataTargets.Cast<MaterialVariant>().ToArray());
        }

        public override void OnDisable()
        {
            registeredVariants.Remove(targetEditor);
            DestroyImmediate(targetEditor);
            base.OnDisable();
        }

        protected override void OnHeaderGUI()
        {
            targetEditor.DrawHeader();
        }

        public override void OnInspectorGUI()
        {
            targetEditor.OnInspectorGUI();

            ApplyRevertGUI();
        }

        protected override void Apply()
        {
            base.Apply();

            if (assetTarget != null)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    InternalEditorUtility.SaveToSerializedFileAndForget(new[] { extraDataTargets[i] },
                        (targets[i] as MaterialVariantImporter).assetPath, true);
                    AssetDatabase.ImportAsset((targets[i] as MaterialVariantImporter).assetPath);
                }
            }
        }
    }
}
