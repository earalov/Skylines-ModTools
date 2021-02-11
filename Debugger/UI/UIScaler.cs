namespace ModTools.UI
{
    using System;
    using UnityEngine;


    public static class UIScaler
    {
        public const float GUI_WIDTH = 1920f;
        public const float GUI_HEIGHT = 1080f;

        public static float UIScale
        {
            get
            {
                var w = Screen.width * (1 / GUI_WIDTH);
                var h = Screen.height * (1 / GUI_HEIGHT);
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

        //public static Rect Scale2UI(this Rect rect) =>
        //    rect.Scale(ScaleFactor);

        //public static Rect ScaleBack2UI(this Rect rect) =>
        //    rect.Scale(1/ScaleFactor);

        //public static Rect Scale(this Rect rect, float scale)
        //{
        //    rect.x *= scale;
        //    rect.y *= scale;
        //    rect.width *= scale;
        //    rect.height *= scale;
        //    return rect;
        //}
    }
}
