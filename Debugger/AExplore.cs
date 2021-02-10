#if DEBUG
namespace ModTools {
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Dummy class to help store values for debuging purposes.
    /// </summary>
    public class AExplore : MonoBehaviour
    {
        public static AExplore Instance;
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

        public AExplore()
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
            var myObject = new GameObject(nameof(AExplore));
            DontDestroyOnLoad(myObject);
            Instance = myObject.AddComponent<AExplore>();
        }

        public static void Release() {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
#endif
}
