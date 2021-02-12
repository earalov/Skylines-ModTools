namespace ModTools.UI
{
    using System;
    using UnityEngine;


    public static class UIScaler
    {
        public const float DEFAULT_WIDTH = 1920f;

        public const float DEFAULT_HEIGHT = 1080f;

        private static ModConfiguration Config => MainWindow.Instance.Config;

        public static float MaxWidth
        {
            get
            {
                float ret =
                    Config.ScaleToResolution ?
                    DEFAULT_WIDTH :
                    Screen.width;
                return ret / MainWindow.Instance.Config.UIScale;
            }
        }

        public static float MaxHeight
        {
            get
            {
                float ret =
                    Config.ScaleToResolution ?
                    DEFAULT_HEIGHT :
                    Screen.height;
                return ret / MainWindow.Instance.Config.UIScale;
            }
        }

        public static float UIScale
        {
            get
            {
                var w = Screen.width * (1 / MaxWidth);
                var h = Screen.height * (1 / MaxHeight);
                return Mathf.Min(w, h);
            }
        }

        public static Matrix4x4 ScaleMatrix => Matrix4x4.Scale(Vector3.one * UIScaler.UIScale);

        public static Vector2 MousePosition
        {
            get
            {
                var mouse = Input.mousePosition;
                mouse.y = Screen.height - mouse.y;
                return mouse / UIScaler.UIScale;
            }
        }
    }
}
