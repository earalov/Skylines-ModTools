using System;
using System.Reflection;
using ColossalFramework.UI;
using ICities;
using ModTools.Utils;
using UnityEngine;

namespace ModTools
{
    public class MeshViewer : GUIWindow
    {
        private Mesh previewMesh = null;
        private Material previewMaterial;
        private String assetName = null;

        private RenderTexture targetRT;

        private float m_Distance = 4f;
        private Vector2 m_PreviewDir = new Vector2(120f, -20f);
        private Bounds bounds;
        private Vector2 lastLeftMousePos = Vector2.zero;
        private Vector2 lastRightMousePos = Vector2.zero;

        private Camera meshViewerCamera;

        private Material material;

        private Light light;

        private bool useOriginalShader = false;

        private MeshViewer()
            : base("Mesh Viewer", new Rect(512, 128, 512, 512), skin)
        {
            onDraw = DrawWindow;
            onClose = () =>
            {
                SetCitizenInfoObjects(true);
                Destroy(this);
            };
            onUnityDestroy = () =>
            {
                Destroy(meshViewerCamera);
                Destroy(targetRT);
            };

            try
            {
                light = GameObject.Find("Directional Light").GetComponent<Light>();
            }
            catch (Exception)
            {
                light = null;
            }

            meshViewerCamera = gameObject.AddComponent<Camera>();
            meshViewerCamera.transform.position = new Vector3(-10000.0f, -10000.0f, -10000.0f);
            meshViewerCamera.fieldOfView = 30f;
            meshViewerCamera.backgroundColor = Color.grey;
            meshViewerCamera.nearClipPlane = 1.0f;
            meshViewerCamera.farClipPlane = 1000.0f;
            meshViewerCamera.enabled = false;
            meshViewerCamera.allowHDR = true;

            targetRT = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            meshViewerCamera.targetTexture = targetRT;
        }

        public static MeshViewer CreateMeshViewer(String assetName, Mesh mesh, Material material, bool calculateBounds = true)
        {
            var go = new GameObject("MeshViewer");
            go.transform.parent = ModTools.Instance.transform;
            var meshViewer = go.AddComponent<MeshViewer>();
            meshViewer.assetName = assetName;
            meshViewer.previewMesh = mesh;
            meshViewer.material = material;

            meshViewer.previewMaterial = new Material(Shader.Find("Diffuse"));
            if (material != null)
            {
                meshViewer.previewMaterial.mainTexture = material.mainTexture;
            }
            meshViewer.Setup(calculateBounds);
            meshViewer.visible = true;
            meshViewer.SetCitizenInfoObjects(false);
            return meshViewer;
        }

        void Update()
        {
            if (previewMesh == null)
            {
                return;
            }

            float intensity = 0.0f;
            Color color = Color.black;
            bool enabled = false;

            if (light != null)
            {
                intensity = light.intensity;
                color = light.color;
                enabled = light.enabled;
                light.intensity = 2.0f;
                light.color = Color.white;
                light.enabled = true;
            }

            float magnitude = bounds.extents.magnitude;
            float num1 = magnitude + 16f;
            float num2 = magnitude * this.m_Distance;
            this.meshViewerCamera.transform.position = -Vector3.forward * num2;
            this.meshViewerCamera.transform.rotation = Quaternion.identity;
            this.meshViewerCamera.nearClipPlane = Mathf.Max(num2 - num1 * 1.5f, 0.01f);
            this.meshViewerCamera.farClipPlane = num2 + num1 * 1.5f;
            Quaternion q = Quaternion.Euler(this.m_PreviewDir.y, 0.0f, 0.0f) *
                           Quaternion.Euler(0.0f, this.m_PreviewDir.x, 0.0f);
            var trs = Matrix4x4.TRS(q * -bounds.center, q, Vector3.one);

            var material1 = (useOriginalShader && material != null) ? material : previewMaterial;
            Graphics.DrawMesh(previewMesh, trs, material1, 0, meshViewerCamera, 0, null, false, false);
            meshViewerCamera.RenderWithShader(material1.shader, "");

            if (light != null)
            {
                light.intensity = intensity;
                light.color = color;
                light.enabled = enabled;
            }
        }

        void DrawWindow()
        {
            if (previewMesh != null)
            {
                title = $"Previewing \"{assetName ?? previewMesh.name}\"";

                GUILayout.BeginHorizontal();

                if (material != null)
                {
                    useOriginalShader = GUILayout.Toggle(useOriginalShader, "Original Shader");
                    if (previewMesh.isReadable)
                    {
                        if (GUILayout.Button("Dump mesh+textures", GUILayout.Width(160)))
                        {
                            DumpUtil.DumpMeshAndTextures(assetName ?? previewMesh.name, previewMesh, material);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Dump textures", GUILayout.Width(160)))
                        {
                            DumpUtil.DumpTextures(assetName, material);
                        }
                    }
                }
                else
                {
                    useOriginalShader = false;
                    if (previewMesh.isReadable)
                    {
                        if (GUILayout.Button("Dump mesh", GUILayout.Width(160)))
                        {
                            DumpUtil.DumpMeshAndTextures($"{previewMesh.name}", previewMesh);
                        }
                    }
                }
                if (previewMesh.isReadable)
                {
                    GUILayout.Label($"Triangles: {previewMesh.triangles.Length / 3}");
                }
                else
                {
                    var oldColor = GUI.color;
                    GUI.color = Color.yellow;
                    GUILayout.Label("Mesh insn't readable!");
                    GUI.color = oldColor;
                }
                if (material?.mainTexture != null)
                {
                    GUILayout.Label($"Texture size: {material.mainTexture.width}x{material.mainTexture.height}");
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0 || Event.current.button == 2)
                    {
                        lastLeftMousePos = Event.current.mousePosition;
                    }
                    else
                    {
                        lastRightMousePos = Event.current.mousePosition;
                    }
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    var pos = Event.current.mousePosition;
                    if (Event.current.button == 0 || Event.current.button == 2)
                    {
                        if (lastLeftMousePos != Vector2.zero)
                        {
                            Vector2 moveDelta = (pos - lastLeftMousePos) * 2.0f;
                            this.m_PreviewDir -= moveDelta / Mathf.Min(this.targetRT.width, this.targetRT.height) *
                                                 UIView.GetAView().ratio * 140f;
                            this.m_PreviewDir.y = Mathf.Clamp(this.m_PreviewDir.y, -90f, 90f);
                        }
                        lastLeftMousePos = pos;
                    }
                    else
                    {
                        if (lastRightMousePos != Vector2.zero)
                        {
                            Vector2 moveDelta1 = pos - lastRightMousePos;
                            this.m_Distance += (float) ((double) moveDelta1.y / (double) this.targetRT.height *
                                                        (double) UIView.GetAView().ratio * 40.0);
                            float num1 = 6f;
                            float magnitude = bounds.extents.magnitude;
                            float num2 = magnitude + 16f;
                            this.m_Distance = Mathf.Min(this.m_Distance, 4f, num1 * (num2 / magnitude));
                        }
                        lastRightMousePos = pos;
                    }
                }

                GUI.DrawTexture(new Rect(0.0f, 64.0f, rect.width, rect.height - 64.0f), targetRT,
                    ScaleMode.StretchToFill, false);
            }
            else
            {
                title = "Mesh Viewer";
                GUILayout.Label("Use the Scene Explorer to select a Mesh for preview");
            }
        }

        public void Setup(bool calculateBounds)
        {
            if (!((UnityEngine.Object) this.previewMesh != (UnityEngine.Object) null))
                return;
            if (calculateBounds && previewMesh.isReadable)
            {
                this.bounds = new Bounds(Vector3.zero, Vector3.zero);
                foreach (Vector3 vertex in this.previewMesh.vertices)
                    this.bounds.Encapsulate(vertex);
            }
            else
            {
                this.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(3, 3, 3));
            }
            this.m_Distance = 4f;
        }

        public void SetCitizenInfoObjects(bool enabled)
        {
            CitizenInfo[] info = Resources.FindObjectsOfTypeAll<CitizenInfo>();
            foreach (CitizenInfo i in info)
            {
                i.gameObject.SetActive(enabled);
            }
        }
    }
}