#if DEBUG
namespace ModTools 
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Dummy class to help store values for debuging purposes.
    /// </summary>
    public class Test : MonoBehaviour
    {
        [Flags]
        public enum TestFlags : long
        {
            None = 0,
            A = 1,
            B = 2,
            C = 4,
            D = -8000000000000000L,
        }

        public static Test Instance;
        public List<NetInfo.Node> Nodes = new List<NetInfo.Node>();
        public ICollection<NetInfo.Node> Nodes2;
        public IEnumerable<NetInfo.Node> Nodes3;
        public NetInfo.Node[] Nodes4 = new NetInfo.Node[100];

        public float f = 1.1f;
        public int i = -1;
        public uint u = 1;
        public string s = "string";
        public char c = 'A';
        public Vector3 v;

        public NetNode.Flags NodeFlags;
        public TextAnchor Anchor;
        public TestFlags testFlags;

        public NetInfo.Segment segmentInfo = new NetInfo.Segment();

        public float ScreenWidth => Screen.width;
        public float ScreenHeight => Screen.height;
        public string Scene => SceneManager.GetActiveScene().name;

        public Test()
        {
            for (int i = 0; i < 100; ++i)
            {
                Nodes.Add(new NetInfo.Node());
            }
            Nodes2 = Nodes;
            Nodes3 = Nodes.AsEnumerable();
            Nodes4 = Nodes.ToArray();
        }

        public static void Create() {
            var myObject = new GameObject(nameof(Test));
            DontDestroyOnLoad(myObject);
            Instance = myObject.AddComponent<Test>();
        }

        public static void Release() {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}
#endif
