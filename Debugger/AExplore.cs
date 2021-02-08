#if DEBUG
namespace ModTools {
    using UnityEngine;
    using System.Collections.Generic;

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

        public static void Create() {
            var myObject = new GameObject(nameof(AExplore));
            DontDestroyOnLoad(myObject);
            Instance = myObject.AddComponent<AExplore>();
            for (int i = 0; i < 100; ++i) {
                Instance.Nodes.Add(new NetInfo.Node());
            }
        }

        public static void Release() {
            Destroy(Instance);
            Instance = null;
        }
    }
#endif
}
