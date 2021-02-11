﻿using System.Collections.Generic;
using ColossalFramework.UI;
using ModTools.Explorer;
using ModTools.Utils;
using UnityEngine;

namespace ModTools
{
    internal class SelectionTool : DefaultTool
    {
        private ushort hoveredSegment;
        private ushort hoveredBuilding;

        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.None;

        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.None;
        }

        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;

        public override TreeInstance.Flags GetTreeIgnoreFlags() => TreeInstance.Flags.None;

        public override PropInstance.Flags GetPropIgnoreFlags() => PropInstance.Flags.None;

        public override Vehicle.Flags GetVehicleIgnoreFlags() => 0;

        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.None;

        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.None;

        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.None;

        public override District.Flags GetDistrictIgnoreFlags() => District.Flags.None;

        public override DistrictPark.Flags GetParkIgnoreFlags() => DistrictPark.Flags.None;

        public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.None;

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (m_hoverInstance.NetNode == 0)
            {
                return;
            }

            RenderManager.instance.OverlayEffect.DrawCircle(
                cameraInfo,
                GetToolColor(false, m_selectErrors != ToolErrors.None),
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position,
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_bounds.size.magnitude,
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position.y - 1f,
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position.y + 1f,
                true,
                true);
        }

        public override void SimulationStep()
        {
            base.SimulationStep();
            if (m_hoverInstance.CitizenInstance > 0 || m_hoverInstance.Vehicle > 0 ||
                m_hoverInstance.ParkedVehicle > 0 ||
                m_hoverInstance.District > 0 || m_hoverInstance.Park > 0 || m_hoverInstance.TransportLine > 0 ||
                m_hoverInstance.Prop > 0 || m_hoverInstance.Tree > 0)
            {
                this.hoveredSegment = m_hoverInstance.NetSegment;
                hoveredBuilding = m_hoverInstance.Building;
                return;
            }

            if (!RayCastSegmentAndNode(out var hoveredSegment, out var hoveredNode))
            {
                return;
            }

            var segments = new Dictionary<ushort, SegmentAndNode>();

            if (hoveredSegment != 0)
            {
                var segment = NetManager.instance.m_segments.m_buffer[hoveredSegment];
                var startNode = NetManager.instance.m_nodes.m_buffer[segment.m_startNode];
                var endNode = NetManager.instance.m_nodes.m_buffer[segment.m_endNode];
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (startNode.CountSegments() > 0)
                {
                    var bounds = startNode.m_bounds;
                    if (hoveredNode != 0)
                    {
                        bounds.extents /= 2f;
                    }

                    if (bounds.IntersectRay(mouseRay))
                    {
                        hoveredNode = segment.m_startNode;
                    }
                }

                if (hoveredSegment != 0 && endNode.CountSegments() > 0)
                {
                    var bounds = endNode.m_bounds;
                    if (hoveredNode != 0)
                    {
                        bounds.extents /= 2f;
                    }

                    if (bounds.IntersectRay(mouseRay))
                    {
                        hoveredNode = segment.m_endNode;
                    }
                }

                if (hoveredSegment != 0 && !segments.ContainsKey(hoveredSegment))
                {
                    segments.Clear();
                    SetSegments(hoveredSegment, segments);
                }
            }

            hoveredBuilding = m_hoverInstance.Building;

            if (hoveredNode > 0)
            {
                m_hoverInstance.NetNode = hoveredNode;
            }
            else if (hoveredSegment > 0)
            {
                m_hoverInstance.NetSegment = hoveredSegment;
            }

            this.hoveredSegment = hoveredSegment > 0 ? hoveredSegment : m_hoverInstance.NetSegment;
        }

        protected override void OnToolGUI(Event e)
        {
            DrawLabel();
            if (m_toolController.IsInsideUI || e.type != EventType.MouseDown)
            {
                base.OnToolGUI(e);
                return;
            }

            if (m_hoverInstance.IsEmpty)
            {
                return;
            }

            var sceneExplorer = FindObjectOfType<SceneExplorer>();
            if (e.button == 0)
            {
                if (m_hoverInstance.NetNode > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForNode(m_hoverInstance.NetNode));
                }
                else if (m_hoverInstance.NetSegment > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForSegment(m_hoverInstance.NetSegment));
                }
                else if (m_hoverInstance.Tree > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForTree(m_hoverInstance.Tree));
                }
                else if (m_hoverInstance.Prop > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForProp(m_hoverInstance.Prop));
                }
                else if (m_hoverInstance.CitizenInstance > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForCitizenInstance(m_hoverInstance.CitizenInstance));
                }
                else if (m_hoverInstance.Building > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForBuilding(m_hoverInstance.Building));
                }
                else if (m_hoverInstance.Vehicle > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForVehicle(m_hoverInstance.Vehicle));
                }
                else if (m_hoverInstance.ParkedVehicle > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForParkedVehicle(m_hoverInstance.ParkedVehicle));
                }
                else if (m_hoverInstance.District > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForDistrict(m_hoverInstance.District));
                }
                else if (m_hoverInstance.Park > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForPark(m_hoverInstance.Park));
                }
                else if (m_hoverInstance.TransportLine > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForTransportLine(m_hoverInstance.TransportLine));
                }
            }
            else if (e.button == 1)
            {
                if (m_hoverInstance.CitizenInstance > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForCitizen(m_hoverInstance.GetCitizenId()));
                }
                else if (m_hoverInstance.NetNode > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForSegment(hoveredSegment));
                }
                else if (m_hoverInstance.NetSegment > 0)
                {
                    sceneExplorer.Show(ReferenceChainBuilder.ForBuilding(hoveredBuilding));
                }
            }
        }

        protected override bool CheckNode(ushort node, ref ToolErrors errors) => true;

        protected override bool CheckSegment(ushort segment, ref ToolErrors errors) => true;

        protected override bool CheckBuilding(ushort building, ref ToolErrors errors) => true;

        protected override bool CheckProp(ushort prop, ref ToolErrors errors) => true;

        protected override bool CheckTree(uint tree, ref ToolErrors errors) => true;

        protected override bool CheckVehicle(ushort vehicle, ref ToolErrors errors) => true;

        protected override bool CheckParkedVehicle(ushort parkedVehicle, ref ToolErrors errors) => true;

        protected override bool CheckCitizen(ushort citizenInstance, ref ToolErrors errors) => true;

        protected override bool CheckDisaster(ushort disaster, ref ToolErrors errors) => true;

        private static void SetSegments(ushort segmentId, IDictionary<ushort, SegmentAndNode> segments)
        {
            var segment = NetManager.instance.m_segments.m_buffer[segmentId];
            var seg = new SegmentAndNode()
            {
                SegmentId = segmentId,
                TargetNode = segment.m_endNode,
            };

            segments[segmentId] = seg;

            var infoIndex = segment.m_infoIndex;
            var node = NetManager.instance.m_nodes.m_buffer[segment.m_startNode];
            if (node.CountSegments() == 2)
            {
                SetSegments(node.m_segment0 == segmentId ? node.m_segment1 : node.m_segment0, infoIndex, ref seg, segments);
            }

            node = NetManager.instance.m_nodes.m_buffer[segment.m_endNode];
            if (node.CountSegments() == 2)
            {
                SetSegments(node.m_segment0 == segmentId ? node.m_segment1 : node.m_segment0, infoIndex, ref seg, segments);
            }
        }

        private static void SetSegments(ushort segmentId, ushort infoIndex, ref SegmentAndNode previousSeg, IDictionary<ushort, SegmentAndNode> segments)
        {
            var segment = NetManager.instance.m_segments.m_buffer[segmentId];

            if (segment.m_infoIndex != infoIndex || segments.ContainsKey(segmentId))
            {
                return;
            }

            var seg = default(SegmentAndNode);
            seg.SegmentId = segmentId;

            var previousSegment = NetManager.instance.m_segments.m_buffer[previousSeg.SegmentId];
            ushort nextNode;
            if (segment.m_startNode == previousSegment.m_endNode ||
                segment.m_startNode == previousSegment.m_startNode)
            {
                nextNode = segment.m_endNode;
                seg.TargetNode = segment.m_startNode == previousSeg.TargetNode
                    ? segment.m_endNode
                    : segment.m_startNode;
            }
            else
            {
                nextNode = segment.m_startNode;
                seg.TargetNode = segment.m_endNode == previousSeg.TargetNode
                    ? segment.m_startNode
                    : segment.m_endNode;
            }

            segments[segmentId] = seg;

            var node = NetManager.instance.m_nodes.m_buffer[nextNode];
            if (node.CountSegments() == 2)
            {
                SetSegments(node.m_segment0 == segmentId ? node.m_segment1 : node.m_segment0, infoIndex, ref seg, segments);
            }
        }

        private static bool RayCastSegmentAndNode(out ushort netSegment, out ushort netNode)
        {
            if (RayCastSegmentAndNode(out var output))
            {
                netSegment = output.m_netSegment;
                netNode = output.m_netNode;
                return true;
            }

            netSegment = 0;
            netNode = 0;
            return false;
        }

        private static bool RayCastSegmentAndNode(out RaycastOutput output)
        {
            var input = new RaycastInput(Camera.main.ScreenPointToRay(Input.mousePosition), Camera.main.farClipPlane)
            {
                m_netService = { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels },
                m_ignoreSegmentFlags = NetSegment.Flags.None,
                m_ignoreNodeFlags = NetNode.Flags.None,
                m_ignoreTerrain = true,
            };

            return RayCast(input, out output);
        }

        private void DrawLabel()
        {
            var hoverInstance1 = m_hoverInstance;
            var text = (string)null;

            if (hoverInstance1.NetNode != 0)
            {
                text = $"[Click LMB to show node in SceneExplorer]\n[Click RMB to show segment in SceneExplorer]\nNode ID: {hoverInstance1.NetNode}\nSegment ID: {hoveredSegment}\nAsset: {hoverInstance1.GetNetworkAssetName()}";
            }
            else if (hoverInstance1.NetSegment != 0)
            {
                text = $"[Click LMB to show segment in SceneExplorer]\n[Click RMB to show building in SceneExplorer]\nSegment ID: {hoverInstance1.NetSegment}\nBuilding ID: {hoveredBuilding}\nAsset: {hoverInstance1.GetNetworkAssetName()}";
            }
            else if (hoverInstance1.Building != 0)
            {
                text = $"[Click LMB to show building in SceneExplorer]\nBuilding ID: {hoverInstance1.Building}\nAsset: {hoverInstance1.GetBuildingAssetName()}";
            }
            else if (hoverInstance1.Vehicle != 0)
            {
                text = $"[Click LMB to show vehicle in SceneExplorer]\nVehicle ID: {hoverInstance1.Vehicle}\nAsset: {hoverInstance1.GetVehicleAssetName()}";
            }
            else if (hoverInstance1.ParkedVehicle != 0)
            {
                text = $"[Click LMB to show parked vehicle in SceneExplorer]\nParked Vehicle ID: {hoverInstance1.ParkedVehicle}\nAsset: {hoverInstance1.GetVehicleAssetName()}";
            }
            else if (hoverInstance1.CitizenInstance != 0)
            {
                text = $"[Click LMB to show citizen instance in SceneExplorer]\n[Click RMB to show citizen in SceneExplorer]\nCitizen instance ID: {hoverInstance1.CitizenInstance}\nCitizen ID: {hoverInstance1.GetCitizenId()}\nAsset: {hoverInstance1.GetCitizenAssetName()}";
            }
            else if (hoverInstance1.Prop != 0)
            {
                text = $"[Click LMB to show prop in SceneExplorer]\nProp ID: {hoverInstance1.Prop}\nAsset: {hoverInstance1.GetPropAssetName()}";
            }
            else if (hoverInstance1.Tree != 0)
            {
                text = $"[Click LMB to show tree in SceneExplorer]\nTree ID: {hoverInstance1.Tree}\nAsset: {hoverInstance1.GetTreeAssetName()}";
            }
            else if (hoverInstance1.Park != 0)
            {
                text = $"[Click LMB to show park in SceneExplorer]\nPark ID: {hoverInstance1.Park}\nName: {hoverInstance1.GetParkName()}";
            }
            else if (hoverInstance1.District != 0)
            {
                text = $"[Click LMB to show district in SceneExplorer]\nDistrict ID: {hoverInstance1.District}\nName: {hoverInstance1.GetDistrictName()}";
            }
            else if (hoverInstance1.TransportLine != 0)
            {
                text = $"[Click LMB to show transport line in SceneExplorer]\nTransport Line ID: {hoverInstance1.TransportLine}\nName: {hoverInstance1.GetLineName()}";
            }

            if (text == null)
            {
                return;
            }

            var screenPoint = Input.mousePosition;
            screenPoint.y = Screen.height - screenPoint.y;
            var color = GUI.color;
            GUI.color = Color.white;
            DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
            GUI.color = color;
        }

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();
            if (UIView.library.Get("PauseMenu")?.isVisible == true)
            {
                UIView.library.Hide("PauseMenu");
                ToolsModifierControl.SetTool<DefaultTool>();
            }

            if (Input.GetMouseButtonDown(1))
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }

        private struct SegmentAndNode
        {
            public ushort SegmentId;
            public ushort TargetNode;
        }
    }
}