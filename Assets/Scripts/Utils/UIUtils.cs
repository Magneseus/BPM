using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class UIUtils {

        static Texture2D _whiteTexture;
        private static float cameraSpeed = 0.75f;
        private static int scrollRectangleSize = 40;
        public static Rect ScreenRecDown = new Rect(0, 0, Screen.width, scrollRectangleSize);
        public static Rect ScreenRecUp = new Rect(0, Screen.height - scrollRectangleSize, Screen.width, scrollRectangleSize);
        public static Rect ScreenRecLeft = new Rect(0, 0, scrollRectangleSize, Screen.height);
        public static Rect ScreenRecRight = new Rect(Screen.width - scrollRectangleSize, 0, scrollRectangleSize, Screen.height);

        public enum CommandType
        {
            Move,
            Attack
        }

        public static Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }

                return _whiteTexture;
            }
        }

        public static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }

        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }

        public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
        {
            var v1 = camera.ScreenToViewportPoint(screenPosition1);
            var v2 = camera.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = camera.nearClipPlane;
            max.z = camera.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        public static void ScrollCamera(Transform cameraTransform)
        {
            if (ScreenRecDown.Contains(Input.mousePosition))
                cameraTransform.Translate(0, 0, -cameraSpeed, Space.World);

            if (ScreenRecUp.Contains(Input.mousePosition))
                cameraTransform.Translate(0, 0, cameraSpeed, Space.World);

            if (ScreenRecLeft.Contains(Input.mousePosition))
                cameraTransform.Translate(-cameraSpeed, 0, 0, Space.World);

            if (ScreenRecRight.Contains(Input.mousePosition))
                cameraTransform.Translate(cameraSpeed, 0, 0, Space.World);
        }

        public static void UpdatePlayerCommand(CommandType command)
        {
            if (Input.GetKeyDown(KeyCode.A))
                command = CommandType.Attack;
            if (Input.GetKeyDown(KeyCode.M))
                command = CommandType.Move;
        }
    }
}
