using System;
using System.Linq;
using ColossalFramework.UI;
using ModTools.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModTools.Explorer
{
    internal static class GUIButtons
    {
        private static object buffer;

        public static void SetupCommonButtons(ReferenceChain refChain, object value, uint valueIndex, TypeUtil.SmartType smartType = TypeUtil.SmartType.Undefined)
        {
            switch (value)
            {
                case null:
                    if (GUILayout.Button("Watch"))
                    {
                        MainWindow.Instance.Watches.AddWatch(refChain);
                    }

                    return;

                case PrefabInfo prefabInfo:
                    SetupButtonsForPrefab(prefabInfo);
                    break;

                case MilestoneInfo milestoneInfo:
                    if (GUILayout.Button("Unlock"))
                    {
                        var wrapper = new MilestonesWrapper(UnlockManager.instance);
                        wrapper.UnlockMilestone(milestoneInfo.name);
                    }

                    break;

                case NetInfo.Segment segmentInfo:
                    SetupSegmentButtons(refChain, segmentInfo);
                    break;

                case NetInfo.Node nodeInfo:
                    SetupNodeButtons(refChain, nodeInfo);
                    break;

                case BuildingInfo.MeshInfo buildingSumMeshInfo:
                    SetupMeshPreviewButtons(
                        name: null,
                        buildingSumMeshInfo.m_subInfo?.m_mesh,
                        buildingSumMeshInfo.m_subInfo?.m_material,
                        buildingSumMeshInfo.m_subInfo?.m_lodMesh,
                        buildingSumMeshInfo.m_subInfo?.m_lodMaterial);
                    break;

                case VehicleInfo.MeshInfo vehicleSumMeshInfo:
                    SetupMeshPreviewButtons(
                        name: null,
                        vehicleSumMeshInfo.m_subInfo?.m_mesh,
                        vehicleSumMeshInfo.m_subInfo?.m_material,
                        vehicleSumMeshInfo.m_subInfo?.m_lodMesh,
                        vehicleSumMeshInfo.m_subInfo?.m_lodMaterial);
                    break;

                case CitizenInstance instance
                    when valueIndex > 0 &&
                         (instance.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) ==
                         CitizenInstance.Flags.Created:

                    InstanceID citizenInstanceInst = default;
                    citizenInstanceInst.CitizenInstance = (ushort)valueIndex;
                    SetupGotoButton(citizenInstanceInst, instance.GetLastFramePosition());
                    break;

                case Citizen citizen when citizen.m_instance > 0:
                    ref var citizenInstance = ref CitizenManager.instance.m_instances.m_buffer[citizen.m_instance];
                    if ((citizenInstance.m_flags & (CitizenInstance.Flags.Created | CitizenInstance.Flags.Deleted)) ==
                        CitizenInstance.Flags.Created)
                    {
                        InstanceID citizenInstanceInst2 = default;
                        citizenInstanceInst2.CitizenInstance = citizen.m_instance;
                        SetupGotoButton(citizenInstanceInst2, citizenInstance.GetLastFramePosition());
                    }

                    break;

                case Vehicle vehicle when valueIndex > 0 &&
                                          (vehicle.m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Deleted)) ==
                                          Vehicle.Flags.Created:
                    InstanceID vehicleInst = default;
                    vehicleInst.Vehicle = (ushort)valueIndex;
                    SetupGotoButton(vehicleInst, vehicle.GetLastFramePosition());
                    break;

                case VehicleParked parkedVehicle when valueIndex > 0 && (parkedVehicle.m_flags & 3) == 1:
                    InstanceID parkedVehicleInst = default;
                    parkedVehicleInst.ParkedVehicle = (ushort)valueIndex;
                    SetupGotoButton(parkedVehicleInst, parkedVehicle.m_position);
                    break;

                case Building building when valueIndex > 0 &&
                                            (building.m_flags & (Building.Flags.Created | Building.Flags.Deleted)) ==
                                            Building.Flags.Created:
                    InstanceID buildingInst = default;
                    buildingInst.Building = (ushort)valueIndex;
                    SetupGotoButton(buildingInst, building.m_position);
                    break;

                case PropInstance prop when valueIndex > 0 &&
                                            ((PropInstance.Flags)prop.m_flags & (PropInstance.Flags.Created | PropInstance.Flags.Deleted)) ==
                                            PropInstance.Flags.Created:
                    InstanceID propInst = default;
                    propInst.Prop = (ushort)valueIndex;
                    SetupGotoButton(propInst, prop.Position);
                    break;

                case TreeInstance tree when valueIndex > 0 &&
                                            ((TreeInstance.Flags)tree.m_flags & (TreeInstance.Flags.Created | TreeInstance.Flags.Deleted)) ==
                                            TreeInstance.Flags.Created:
                    InstanceID treeInst = default;
                    treeInst.Tree = valueIndex;
                    SetupGotoButton(treeInst, tree.Position);
                    break;

                case NetSegment segment when valueIndex > 0 &&
                                             (segment.m_flags &
                                              (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) ==
                                             NetSegment.Flags.Created:
                    InstanceID segmentInst = default;
                    segmentInst.NetSegment = (ushort)valueIndex;
                    SetupGotoButton(segmentInst, segment.m_middlePosition);
                    break;

                case NetNode node when valueIndex > 0 &&
                                       (node.m_flags & (NetNode.Flags.Created | NetNode.Flags.Deleted)) ==
                                       NetNode.Flags.Created:
                    InstanceID nodeInst = default;
                    nodeInst.NetNode = (ushort)valueIndex;
                    SetupGotoButton(nodeInst, node.m_position);
                    break;

                case NetLane lane when valueIndex > 0 &&
                                       ((NetLane.Flags)lane.m_flags & (NetLane.Flags.Created | NetLane.Flags.Deleted)) ==
                                       NetLane.Flags.Created:
                    InstanceID segmentInst1 = default;
                    segmentInst1.NetSegment = lane.m_segment;
                    var segment1 = NetManager.instance.m_segments.m_buffer[lane.m_segment];
                    SetupGotoButton(segmentInst1, segment1.m_middlePosition);
                    break;

                case Texture texture:
                    SetupTextureButtons(texture);
                    break;

                case UITextureAtlas.SpriteInfo spriteInfo:
                    SetupTextureButtons(spriteInfo.texture);
                    break;

                case UITextureSprite textureSprite:
                    SetupTextureButtons(textureSprite.texture);
                    break;

                case Mesh mesh:
                    SetupMeshButtons(refChain, mesh);
                    break;
            }

            SetupSmartShowButtons(value, smartType);
            if (GUILayout.Button("Watch"))
            {
                MainWindow.Instance.Watches.AddWatch(refChain);
            }

            if (GUILayout.Button("Copy"))
            {
                buffer = value;
            }
        }

        public static void SetupSmartShowButtons(object value, TypeUtil.SmartType smartType)
        {
            if (smartType == TypeUtil.SmartType.Undefined || value == null || value.GetType().IsArray)
            {
                return;
            }

            try
            {
                switch (smartType)
                {
                    case TypeUtil.SmartType.Sprite:
                    {
                        if (!(value is string stringValue) || !UIView.GetAView().defaultAtlas.spriteNames.Contains(stringValue))
                        {
                            return;
                        }

                        if (GUILayout.Button("Show sprite"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForSprite(stringValue));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.ParkedVehicle:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show parked vehicle"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForParkedVehicle(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.Vehicle:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show vehicle"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForVehicle(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.Building:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show building"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForBuilding(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.CitizenUnit:
                    {
                        if (Convert.ToUInt32(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show citizen unit"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForCitizenUnit(Convert.ToUInt32(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.Citizen:
                    {
                        if (Convert.ToUInt32(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show citizen"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForCitizen(Convert.ToUInt32(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.TransportLine:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show transport line"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForTransportLine(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.PathUnit:
                    {
                        if (Convert.ToUInt32(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show path unit"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForPathUnit(Convert.ToUInt32(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.NetNode:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show network node"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForNode(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.NetSegment:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show network segment"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForSegment(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.NetLane:
                    {
                        if (Convert.ToUInt32(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show network lane"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForLane(Convert.ToUInt32(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.ParkDistrict:
                    {
                        if (Convert.ToByte(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show park district"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForPark(Convert.ToByte(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.District:
                    {
                        if (Convert.ToByte(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show district"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForDistrict(Convert.ToByte(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.Tree:
                    {
                        if (Convert.ToUInt32(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show tree"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForTree(Convert.ToUInt32(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.Prop:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show prop"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForProp(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.CitizenInstance:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show citizen instance"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForCitizenInstance(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    case TypeUtil.SmartType.ZoneBlock:
                    {
                        if (Convert.ToUInt16(value) < 1)
                        {
                            return;
                        }

                        if (GUILayout.Button("Show zone block"))
                        {
                            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
                            sceneExplorer.Show(ReferenceChainBuilder.ForZoneBlock(Convert.ToUInt16(value)));
                        }

                        break;
                    }

                    default:
                        return;
                }
            }
            catch
            {
                // suppress
            }
        }

        public static bool SetupPasteButon(Type type, object currentValue, out object paste)
        {
            paste = null;
            if (buffer != null && type.IsInstanceOfType(buffer) && GUILayout.Button("Paste"))
            {
                paste = buffer;
                return true;
            }

            return currentValue != null && GUILayout.Button("Unset");
        }

        public static void SetupJumpButton(object value, ReferenceChain refChain)
        {
            if (!GUILayout.Button(">", GUILayout.ExpandWidth(false)))
            {
                return;
            }

            var sceneExplorer = Object.FindObjectOfType<SceneExplorer>();
            if (sceneExplorer == null)
            {
                return;
            }

            sceneExplorer.Show(ReferenceChainBuilder.Optimize(refChain, value));
        }

        private static void SetupMeshButtons(ReferenceChain refChain, Mesh mesh)
        {
            if (GUILayout.Button("Preview"))
            {
                MeshViewer.CreateMeshViewer(null, mesh, null);
            }

            if (mesh.isReadable && GUILayout.Button("Dump mesh"))
            {
                var outPath = refChain.ToString().Replace(' ', '_');
                DumpUtil.DumpMeshAndTextures(outPath, mesh);
            }
        }

        private static void SetupTextureButtons(Texture texture)
        {
            if (GUILayout.Button("Preview"))
            {
                TextureViewer.CreateTextureViewer(texture);
            }

            if (GUILayout.Button("Dump .png"))
            {
                TextureUtil.DumpTextureToPNG(texture);
            }
        }

        private static void SetupSegmentButtons(ReferenceChain refChain, NetInfo.Segment segmentInfo)
        {
            try
            {
                Debug.Log("SetupSegmentButtons() called");
                if (segmentInfo.m_mesh != null && segmentInfo.m_mesh.isReadable && GUILayout.Button("Dump"))
                {
                    var outPath = refChain.ToString().Replace(' ', '_');
                    DumpUtil.DumpMeshAndTextures(
                        outPath,
                        segmentInfo.m_mesh,
                        segmentInfo.m_material);
                    DumpUtil.DumpMeshAndTextures(
                        outPath + "_lod",
                        segmentInfo.m_lodMesh,
                        segmentInfo.m_lodMaterial);
                }

                SetupMeshPreviewButtons(name: null, segmentInfo.m_mesh, segmentInfo.m_material, segmentInfo.m_lodMesh, segmentInfo.m_lodMaterial);
                Debug.Log("SetupSegmentButtons() successful");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void SetupNodeButtons(ReferenceChain refChain, NetInfo.Node nodeInfo)
        {
            if (nodeInfo.m_mesh != null && nodeInfo.m_mesh.isReadable && GUILayout.Button("Dump"))
            {
                var outPath = refChain.ToString().Replace(' ', '_');
                DumpUtil.DumpMeshAndTextures(
                    outPath,
                    nodeInfo.m_mesh,
                    nodeInfo.m_material);
                DumpUtil.DumpMeshAndTextures(
                    outPath + "_lod",
                    nodeInfo.m_lodMesh,
                    nodeInfo.m_lodMaterial);
            }

            SetupMeshPreviewButtons(name: null, nodeInfo.m_mesh, nodeInfo.m_material, nodeInfo.m_lodMesh, nodeInfo.m_lodMaterial);
        }

        private static void SetupButtonsForPrefab(PrefabInfo prefabInfo)
        {
            switch (prefabInfo)
            {
                case VehicleInfoBase vehicleInfoBase:
                    SetupMeshPreviewButtons(
                        vehicleInfoBase.name,
                        vehicleInfoBase.m_mesh,
                        vehicleInfoBase.m_material,
                        vehicleInfoBase.m_lodMesh,
                        vehicleInfoBase.m_lodMaterial);
                    SetupVehicleFullDumpButton(
                        vehicleInfoBase.name,
                        vehicleInfoBase.m_mesh,
                        vehicleInfoBase.m_material,
                        vehicleInfoBase.m_lodMesh,
                        vehicleInfoBase.m_lodMaterial,
                        vehicleInfoBase is VehicleInfo vehicleInfo ? vehicleInfo.m_subMeshes : null);
                    break;

                case NetInfo netInfo:
                    SetupPlopButton(prefabInfo);
                    if (netInfo.m_segments?.Length > 0)
                    {
                        var previewSegment = Array.Find(netInfo.m_segments, s => s != null);
                        if (previewSegment != null)
                        {
                            SetupMeshPreviewButtons(
                                netInfo.name,
                                previewSegment.m_mesh,
                                previewSegment.m_material,
                                previewSegment.m_lodMesh,
                                previewSegment.m_lodMaterial);
                        }
                    }

                    SetupNetworkFullDumpButton(netInfo.name, netInfo.m_segments, netInfo.m_nodes);
                    break;

                case BuildingInfoBase buildingInfoBase:
                    if (buildingInfoBase is BuildingInfo)
                    {
                        SetupPlopButton(prefabInfo);
                    }

                    SetupMeshPreviewButtons(
                        buildingInfoBase.name,
                        buildingInfoBase.m_mesh,
                        buildingInfoBase.m_material,
                        buildingInfoBase.m_lodMesh,
                        buildingInfoBase.m_lodMaterial);
                    SetupBuildingFullDumpButton(
                        buildingInfoBase.name,
                        buildingInfoBase.m_mesh,
                        buildingInfoBase.m_material,
                        buildingInfoBase.m_lodMesh,
                        buildingInfoBase.m_lodMaterial,
                        buildingInfoBase is BuildingInfo buildingInfo ? buildingInfo.m_subMeshes : buildingInfoBase is BuildingInfoSub buildingInfoSub ? buildingInfoSub.m_subMeshes : null);
                    break;

                case PropInfo propInfo:
                    SetupPlopButton(prefabInfo);
                    SetupMeshPreviewButtons(propInfo.name, propInfo.m_mesh, propInfo.m_material, propInfo.m_lodMesh, propInfo.m_lodMaterial);
                    SetupGenericAssetFullDumpButton(propInfo.name, propInfo.m_mesh, propInfo.m_material, propInfo.m_lodMesh, propInfo.m_lodMaterial);
                    break;

                case TreeInfo treeInfo:
                    SetupPlopButton(prefabInfo);
                    SetupMeshPreviewButtons(treeInfo.name, treeInfo.m_mesh, treeInfo.m_material, null, null);
                    SetupGenericAssetFullDumpButton(treeInfo.name, treeInfo.m_mesh, treeInfo.m_material, null, null);
                    break;

                case CitizenInfo citizenInfo:
                    SetupMeshPreviewButtons(
                        citizenInfo.name,
                        citizenInfo.m_skinRenderer?.sharedMesh,
                        citizenInfo.m_skinRenderer?.material,
                        citizenInfo.m_lodMesh,
                        citizenInfo.m_lodMaterial);
                    SetupGenericAssetFullDumpButton(
                        citizenInfo.name,
                        citizenInfo.m_skinRenderer?.sharedMesh,
                        citizenInfo.m_skinRenderer?.material,
                        citizenInfo.m_lodMesh,
                        citizenInfo.m_lodMaterial);
                    break;
            }
        }

        private static void SetupGenericAssetFullDumpButton(
            string assetName,
            Mesh mesh,
            Material material,
            Mesh lodMesh,
            Material lodMaterial)
        {
            if (!GUILayout.Button("Full dump"))
            {
                return;
            }

            AssetDumpUtil.DumpGenericAsset(assetName, mesh, material, lodMesh, lodMaterial);
        }

        private static void SetupBuildingFullDumpButton(
            string assetName,
            Mesh mesh,
            Material material,
            Mesh lodMesh,
            Material lodMaterial,
            BuildingInfo.MeshInfo[] subMeshes)
        {
            if (!GUILayout.Button("Full dump"))
            {
                return;
            }

            AssetDumpUtil.DumpBuilding(assetName, mesh, material, lodMesh, lodMaterial, subMeshes);
        }

        private static void SetupVehicleFullDumpButton(
            string assetName,
            Mesh mesh,
            Material material,
            Mesh lodMesh,
            Material lodMaterial,
            VehicleInfo.MeshInfo[] subMeshes)
        {
            if (!GUILayout.Button("Full dump"))
            {
                return;
            }

            AssetDumpUtil.DumpVehicle(assetName, mesh, material, lodMesh, lodMaterial, subMeshes);
        }

        private static void SetupNetworkFullDumpButton(string assetName, NetInfo.Segment[] segments, NetInfo.Node[] nodes)
        {
            if (!GUILayout.Button("Full dump"))
            {
                return;
            }

            AssetDumpUtil.DumpNetwork(assetName, segments, nodes);
        }

        private static void SetupMeshPreviewButtons(string name, Mesh mesh, Material material, Mesh lodMesh, Material lodMaterial)
        {
            if (mesh != null && GUILayout.Button("Preview"))
            {
                MeshViewer.CreateMeshViewer(name, mesh, material);
            }

            if (lodMesh != null && GUILayout.Button("Preview LOD"))
            {
                MeshViewer.CreateMeshViewer(name + "_LOD", lodMesh, lodMaterial);
            }
        }

        private static void SetupPlopButton(object @object)
        {
            var info = @object as PrefabInfo;
            if (info != null && GUILayout.Button("Plop"))
            {
                Plopper.StartPlopping(info);
            }
        }

        private static void SetupGotoButton(InstanceID instance, Vector3 position)
        {
            if (GUILayout.Button("Go to"))
            {
                ToolsModifierControl.cameraController.SetTarget(instance, position, true);
            }
        }
    }
}