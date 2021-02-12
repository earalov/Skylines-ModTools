namespace FbxUtil
{
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using ColossalFramework.UI;
    using UnityEngine;

    internal static class FbxConverter
    {
        public static void ExportAsciiFbx(this Mesh mesh, Stream stream)
        {
            using (var sw = new StreamWriter(stream))
                sw.Write(mesh.ToAsciiFBX());
        }

        public static void ExportAsciiFbx(this Mesh mesh, string path) =>
            File.WriteAllText(path, mesh.ToAsciiFBX());

        public static string ToAsciiFBX(this Mesh mesh)
        {
            GameObject go = new GameObject();
            go.name = mesh.name;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            string data = UnityFBXExporter.FBXExporter.MeshToString(go, "");
            GameObject.Destroy(go);
            return data;
        }

        public static Process ConvertBinary(string source, string target)
        {
            string modPath = ModTools.Utils.FileUtil.PlugingPath;
            string converter = "FbxFormatConverter.exe";
            source = '"' + source + '"';
            target = '"' + target + '"';
            return Execute(modPath, converter, $"-c {source} -o {target} -binary");
        }

        static Process Execute(string dir, string exeFile, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = dir,
                FileName = exeFile,
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process process = new Process { StartInfo = startInfo };
            process.Start();
            return process;
        }
    }
}