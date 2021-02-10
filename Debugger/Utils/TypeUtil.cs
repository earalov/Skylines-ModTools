using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModTools.Utils
{
    internal static class TypeUtil
    {
        private static Dictionary<Type, ExtendedMemberInfo[]> typeCache = new Dictionary<Type, ExtendedMemberInfo[]>();

        public enum SmartType
        {
            Building,
            NetNode,
            NetSegment,
            TransportLine,
            District,
            ParkDistrict,
            Vehicle,
            ParkedVehicle,
            Citizen,
            CitizenUnit,
            CitizenInstance,
            NetLane,
            PathUnit,
            Tree,
            Prop,
            Undefined,
            Sprite,
            ZoneBlock,
        }

        public static Type FindTypeByFullName(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetTypes()
                        .FirstOrDefault(t => t.FullName == fullName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // skip
                }
            }

            return null;
        }

        public static bool IsSpecialType(Type t)
        {
            return t.IsPrimitive
                   || t.IsEnum
                   || t == typeof(string)
                   || t == typeof(Vector2)
                   || t == typeof(Vector3)
                   || t == typeof(Vector4)
                   || t == typeof(Quaternion)
                   || t == typeof(Color)
                   || t == typeof(Color32)
                   || t == typeof(Shader);
        }

        public static bool IsBitmaskEnum(Type t) => t.IsDefined(typeof(FlagsAttribute), false);

        public static ExtendedMemberInfo[] GetAllMembers(Type type, bool recursive = false)
            => GetMembersInternal(type, recursive, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        public static void ClearTypeCache() => typeCache = new Dictionary<Type, ExtendedMemberInfo[]>();

        public static bool IsEnumerable(object myProperty)
        {
            return typeof(IEnumerable).IsInstanceOfType(myProperty)
                || typeof(IEnumerable<>).IsInstanceOfType(myProperty);
        }

        public static bool IsCollection(object myProperty)
        {
            return typeof(ICollection).IsAssignableFrom(myProperty.GetType())
                || typeof(ICollection<>).IsAssignableFrom(myProperty.GetType());
        }

        public static bool IsList(object myProperty)
        {
            return typeof(IList).IsAssignableFrom(myProperty.GetType())
                || typeof(IList<>).IsAssignableFrom(myProperty.GetType());
        }

        public static FieldInfo FindField(Type type, string fieldName)
            => Array.Find(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), f => f.Name == fieldName);

        public static SmartType DetectSmartType(string memberName, Type type)
        {
            try
            {
                if (type == typeof(string))
                {
                    if (memberName.IndexOf("sprite", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.Sprite;
                    }
                }
                else if (memberName.IndexOf("count", StringComparison.OrdinalIgnoreCase) < 0 &&
                    memberName.IndexOf("type", StringComparison.OrdinalIgnoreCase) < 0 &&
                    memberName.IndexOf("flags", StringComparison.OrdinalIgnoreCase) < 0 &&
                    memberName.IndexOf("offset", StringComparison.OrdinalIgnoreCase) < 0 &&
                    IsIntegerType(type))
                {
                    if (memberName.IndexOf("parkedVehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.ParkedVehicle;
                    }

                    if (memberName.IndexOf("vehicle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        memberName.IndexOf("cargo", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.Vehicle;
                    }

                    if (memberName.IndexOf("buildingNode", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.NetNode;
                    }

                    if (memberName.IndexOf("building", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.Building;
                    }

                    if (memberName.IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.PathUnit;
                    }

                    if (memberName.IndexOf("unit", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.CitizenUnit;
                    }

                    if (memberName.IndexOf("citizen", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.Citizen;
                    }

                    if (memberName.IndexOf("line", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.TransportLine;
                    }

                    if (memberName.IndexOf("node", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.NetNode;
                    }

                    if (memberName.IndexOf("segment", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.NetSegment;
                    }

                    if (memberName.IndexOf("lane", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.NetLane;
                    }

                    if (memberName.IndexOf("park", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.ParkDistrict;
                    }

                    if (memberName.IndexOf("district", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.District;
                    }

                    if (memberName.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.Tree;
                    }

                    if (memberName.IndexOf("prop", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.Prop;
                    }

                    if (memberName.IndexOf("instance", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.CitizenInstance;
                    }

                    if (memberName.IndexOf("block", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.ZoneBlock;
                    }

                    if (memberName.IndexOf("stop", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return SmartType.NetNode;
                    }
                }
            }
            catch
            {
                // suppress
            }

            return SmartType.Undefined;
        }

        public static SmartType OverrideSmartType(SmartType smartType, string memberName, object value)
        {
            if (value == null || memberName == null)
            {
                return smartType;
            }

            if (memberName != "m_targetBuilding")
            {
                return smartType;
            }

            switch (value)
            {
                case Vehicle vehicle:
                    var vehicleCopy = vehicle;
                    if (vehicle.Info?.m_vehicleAI?.GetTargetID(0, ref vehicleCopy).NetNode > 0)
                    {
                        return SmartType.NetNode;
                    }

                    break;
                case CitizenInstance citizenInstance:
                    var citizenInstanceCopy = citizenInstance;
                    if (citizenInstance.Info?.m_citizenAI?.GetTargetID(0, ref citizenInstanceCopy).NetNode > 0)
                    {
                        return SmartType.NetNode;
                    }

                    break;
            }

            return smartType;
        }

        private static ExtendedMemberInfo[] GetMembersInternal(Type type, bool recursive, BindingFlags bindingFlags)
        {
            if (typeCache.ContainsKey(type))
            {
                return typeCache[type];
            }

            var results = new Dictionary<string, ExtendedMemberInfo>();
            GetMembersInternal2(type, recursive, bindingFlags, results);
            var members = results.Values.ToArray();
            typeCache[type] = members;
            return members;
        }

        private static void GetMembersInternal2(Type type, bool recursive, BindingFlags bindingFlags, Dictionary<string, ExtendedMemberInfo> outResults)
        {
            foreach (var member in type.GetMembers(bindingFlags))
            {
                if (!outResults.ContainsKey(member.Name))
                {
                    outResults.Add(member.Name, new ExtendedMemberInfo(
                        member,
                        member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property ? DetectSmartType(member.Name, GetMemberUnderlyingType(member)) : SmartType.Undefined));
                }
            }

            if (recursive && type.BaseType != null)
            {
                GetMembersInternal2(type.BaseType, true, bindingFlags, outResults);
            }
        }

        private static Type GetMemberUnderlyingType(MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                return ((FieldInfo)member).FieldType;
            }

            if (member.MemberType == MemberTypes.Property)
            {
                return ((PropertyInfo)member).PropertyType;
            }

            throw new ArgumentException("MemberInfo must be if type FieldInfo or PropertyInfo", nameof(member));
        }

        private static bool IsIntegerType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                    return true;
                default:
                    return type.IsArray && IsIntegerType(type.GetElementType());
            }
        }

        public static bool IsNumeric(this Type type)
        {
            return
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

        public static bool IsFloatingPoint(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSigned(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsInteger(this Type type) => type.IsPrimitive && IsIntegerType(type);

        public static bool IsSignedInteger(this Type type) => type.IsSigned() && IsInteger(type);

        public readonly struct ExtendedMemberInfo
        {
            public readonly MemberInfo ReflectionInfo;

            public readonly SmartType DetectedType;

            public ExtendedMemberInfo(MemberInfo reflectionInfo, SmartType detectedType)
            {
                ReflectionInfo = reflectionInfo;
                DetectedType = detectedType;
            }
        }
    }
}