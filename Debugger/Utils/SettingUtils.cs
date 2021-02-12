namespace ModTools.Utils
{
    using ColossalFramework.UI;
    using ICities;
    using System;

    public static class SettingUtils
    {
        public static UISlider AddSlider2(
            this UIHelperBase helper,
            string text,
            float min, float max, float step, float defaultValue,
            Func<float, string> onValueChanged)
        {
            UISlider slider = null;
            slider = helper.AddSlider(
                text, min, max, step, defaultValue,
                Func) as UISlider;

            void Func(float value)
            {
                string result = onValueChanged?.Invoke(value);
                if (slider)
                {
                    slider.parent.Find<UILabel>("Label").text = $"{text}: {result}";
                    slider.tooltip = result;
                    slider.RefreshTooltip();
                }
            }

            Func(defaultValue);

            return slider;
        }
    }
}
