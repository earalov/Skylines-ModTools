using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModTools.Utils
{
    internal static class ShaderUtil
    {
        private static readonly Dictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>();
        private static readonly Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
        private static string[] shaders;

        static ShaderUtil()
        {
            var shaderUtilType = TypeUtil.FindTypeByFullName("ColossalFramework.Packaging.ShaderUtil");
            if (shaderUtilType == null)
            {
                Debug.LogError("ModTools failed to find type ColossalFramework.Packaging.ShaderUtil!");
                return;
            }

            foreach (var field in shaderUtilType.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Static))
            {
                Fields[field.Name] = field;
            }

            foreach (var method in shaderUtilType.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                Methods[method.Name] = method;
            }
        }

        public static void ClearShaderCache() => shaders = null;

        public static string[] GetShaders()
        {
            return shaders ??= Resources.FindObjectsOfTypeAll<Shader>()
                .Select(shader => shader.name)
                .OrderBy(name => name)
                .ToArray();
        }

        public static IEnumerable<string> GetTextureProperties() => (string[])Fields["textureProps"].GetValue(null);

        public static IEnumerable<string> GetColorProperties() => (string[])Fields["colorProps"].GetValue(null);

        public static IEnumerable<string> GetVectorProperties() => (string[])Fields["vectorProps"].GetValue(null);

        public static IEnumerable<string> GetFloatProperties() => (string[])Fields["floatProps"].GetValue(null);

        public static int CountBoundProperties(this Material material) => Call<int>("CountBoundProperties", material);

        private static T Call<T>(string name, params object[] parameters) => (T)Methods[name].Invoke(null, parameters);

        public static object GetProperty(Material material, string propName)
        {
            try
            {
                if (GetTextureProperties().Contains(propName))
                {
                    return material.GetTexture(propName);
                }
                else if (GetColorProperties().Contains(propName))
                {
                    return material.GetColor(propName);
                }
                else if (GetVectorProperties().Contains(propName))
                {
                    return material.GetVector(propName);
                }
                else if (GetFloatProperties().Contains(propName))
                {
                    return material.GetFloat(propName);
                }
                
                Logger.Error($"unknown property \"{propName}\"");
            }
            catch (Exception ex)
            {
                Logger.Error($"There was an error while trying to get property \"{propName}\" - {ex.Message}");
            }

            return null;
        }

    }
}