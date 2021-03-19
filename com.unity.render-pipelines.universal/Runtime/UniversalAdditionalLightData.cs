using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.LWRP
{
    [Obsolete("LWRP -> Universal (UnityUpgradable) -> UnityEngine.Rendering.Universal.UniversalAdditionalLightData", true)]
    public class LWRPAdditionalLightData
    {
    }
}


namespace UnityEngine.Rendering.Universal
{
    /// <summary>Light Layers.</summary>
    public enum LightLayerEnum
    {
        /// <summary>The light will no affect any object.</summary>
        Nothing = 0,   // Custom name for "Nothing" option
        /// <summary>Light Layer 0.</summary>
        LightLayerDefault = 1 << 0,
        /// <summary>Light Layer 1.</summary>
        LightLayer1 = 1 << 1,
        /// <summary>Light Layer 2.</summary>
        LightLayer2 = 1 << 2,
        /// <summary>Light Layer 3.</summary>
        LightLayer3 = 1 << 3,
        /// <summary>Light Layer 4.</summary>
        LightLayer4 = 1 << 4,
        /// <summary>Light Layer 5.</summary>
        LightLayer5 = 1 << 5,
        /// <summary>Light Layer 6.</summary>
        LightLayer6 = 1 << 6,
        /// <summary>Light Layer 7.</summary>
        LightLayer7 = 1 << 7,
        /// <summary>Everything.</summary>
        Everything = 0xFF, // Custom name for "Everything" option
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class UniversalAdditionalLightData : MonoBehaviour
    {
        [Tooltip("Controls if light Shadow Bias parameters use pipeline settings.")]
        [SerializeField] bool m_UsePipelineSettings = true;

        public bool usePipelineSettings
        {
            get { return m_UsePipelineSettings; }
            set { m_UsePipelineSettings = value; }
        }

        public static readonly int AdditionalLightsShadowResolutionTierCustom    = -1;
        public static readonly int AdditionalLightsShadowResolutionTierLow       =  0;
        public static readonly int AdditionalLightsShadowResolutionTierMedium    =  1;
        public static readonly int AdditionalLightsShadowResolutionTierHigh      =  2;
        public static readonly int AdditionalLightsShadowDefaultResolutionTier   = AdditionalLightsShadowResolutionTierLow;
        public static readonly int AdditionalLightsShadowDefaultCustomResolution = 128;

        [Tooltip("Controls if light shadow resolution uses pipeline settings.")]
        [SerializeField] int m_AdditionalLightsShadowResolutionTier   = AdditionalLightsShadowDefaultResolutionTier;

        public int additionalLightsShadowResolutionTier
        {
            get { return m_AdditionalLightsShadowResolutionTier; }
        }

        // The layer(s) this light belongs too.
        [SerializeField] LightLayerEnum m_LightLayersMask = LightLayerEnum.LightLayerDefault;

        public LightLayerEnum lightLayersMask
        {
            get { return m_LightLayersMask; }
            set { m_LightLayersMask = value; }
        }

        [SerializeField] bool m_LinkLightLayers = true;

        // if enabled, shadowLayersMask use the same settings as lightLayersMask.
        public bool linkLightLayers
        {
            get { return m_LinkLightLayers; }
            set { m_LinkLightLayers = value; }
        }

        // The layer(s) used for shadow casting.
        [SerializeField] LightLayerEnum m_ShadowLayersMask = LightLayerEnum.LightLayerDefault;

        public LightLayerEnum shadowLayersMask
        {
            get { return m_ShadowLayersMask; }
            set { m_ShadowLayersMask = value; }
        }
    }
}
