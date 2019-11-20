using System;

namespace UnityEngine.Rendering.HighDefinition
{
    [Serializable]
    public sealed class GlobalLightingQualitySettings
    {
        static int s_QualitySettingCount = Enum.GetNames(typeof(ScalableSettingLevelParameter.Level)).Length;

        public GlobalLightingQualitySettings()
        {
            /* Ambient Occlusion */
            AOStepCount[(int)ScalableSettingLevelParameter.Level.Low] = 4;
            AOStepCount[(int)ScalableSettingLevelParameter.Level.Medium] = 6;
            AOStepCount[(int)ScalableSettingLevelParameter.Level.High] = 20;

            AOFullRes[(int)ScalableSettingLevelParameter.Level.Low] = false;
            AOFullRes[(int)ScalableSettingLevelParameter.Level.Medium] = false;
            AOFullRes[(int)ScalableSettingLevelParameter.Level.High] = true;

            AOMaximumRadiusPixels[(int)ScalableSettingLevelParameter.Level.Low] = 24;
            AOMaximumRadiusPixels[(int)ScalableSettingLevelParameter.Level.Medium] = 40;
            AOMaximumRadiusPixels[(int)ScalableSettingLevelParameter.Level.High] = 80;

            /* Contact Shadow */
            ContactShadowSampleCount[(int)ScalableSettingLevelParameter.Level.Low] = 4;
            ContactShadowSampleCount[(int)ScalableSettingLevelParameter.Level.Medium] = 8;
            ContactShadowSampleCount[(int)ScalableSettingLevelParameter.Level.High] = 16;

        }

        /// <summary>Default GlobalPostProcessingQualitySettings</summary>
        public static readonly GlobalLightingQualitySettings @default = new GlobalLightingQualitySettings();

        // SSAO
        public int[] AOStepCount = new int[s_QualitySettingCount];
        public bool[] AOFullRes = new bool[s_QualitySettingCount];
        public int[] AOMaximumRadiusPixels = new int[s_QualitySettingCount];

        // Contact Shadows
        public int[] ContactShadowSampleCount = new int[s_QualitySettingCount];

        // TODO: Add SSR

        // TODO: Volumetric fog quality

        // TODO: Shadows. This needs to be discussed further as there is an idiosyncracy here as we have different level of quality settings,
        //some for resolution per light (4 levels) some per volume (which are 3 levels everywhere). This needs to be discussed more.


    }
}
