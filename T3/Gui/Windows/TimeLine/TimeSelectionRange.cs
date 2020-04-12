﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using T3.Core.Animation;
using T3.Core.Logging;
using T3.Gui.Interaction.Snapping;
using UiHelpers;

namespace T3.Gui.Windows.TimeLine
{
    /// <summary>
    /// A graphic representation that allows to move and scale multiple selected timeline elements
    /// </summary>
    public class TimeSelectionRange : IValueSnapAttractor
    {
        public TimeSelectionRange(TimeLineCanvas timeLineCanvas, ValueSnapHandler snapHandler)
        {
            _timeLineCanvas = timeLineCanvas;
            _snapHandler = snapHandler;
        }

        public void Draw(ImDrawListPtr drawlist)
        {
            _selectionTimeRange = _timeLineCanvas.GetSelectionTimeRange();
            if (!_selectionTimeRange.IsValid || _selectionTimeRange.Duration <= 0)
                return;


            var contentRegionMin = ImGui.GetWindowContentRegionMin() + ImGui.GetWindowPos();
            var contentRegionMax = ImGui.GetWindowContentRegionMax() + ImGui.GetWindowPos();
            ImGui.PushStyleColor(ImGuiCol.Button, TimeRangeMarkerColor.Rgba);
            // Range start
            {
                var xRangeStartOnScreen = _timeLineCanvas.TransformX(_selectionTimeRange.Start);
                var rangeStartPos = new Vector2(xRangeStartOnScreen, contentRegionMin.Y);
                // Shade outside
                drawlist.AddRectFilled(
                                       new Vector2(0, 0),
                                       new Vector2(xRangeStartOnScreen, TimeRangeShadowSize.Y),
                                       TimeRangeOutsideColor);

                // Shadow
                drawlist.AddRectFilled(
                                       rangeStartPos - new Vector2(TimeRangeShadowSize.X - 1, 0),
                                       rangeStartPos + new Vector2(0, TimeRangeShadowSize.Y),
                                       TimeRangeShadowColor);

                // Line
                drawlist.AddRectFilled(rangeStartPos, rangeStartPos + new Vector2(1, 9999), TimeRangeShadowColor);
                
                ImGui.SetCursorScreenPos(rangeStartPos +  new Vector2(-TimeRangeHandleSize.X-5,0));
                ImGui.Button("##SelectionStartPos", TimeRangeHandleSize);

                HandleDrag(_selectionTimeRange.Start, _selectionTimeRange.End);
            }

            // Range end
            {
                var xRangeEndOnScreen = _timeLineCanvas.TransformX(_selectionTimeRange.End);
                var rangeEndPos = new Vector2(xRangeEndOnScreen, contentRegionMin.Y);

                // Shade outside
                //var windowMaxX =  ImGui.GetContentRegionAvail().X + _timeLineCanvas.WindowPos.X;
                if (xRangeEndOnScreen < contentRegionMax.X)
                    drawlist.AddRectFilled(
                                           rangeEndPos,
                                           new Vector2(contentRegionMax.X, TimeRangeShadowSize.Y),
                                           TimeRangeOutsideColor);

                // Shadow
                drawlist.AddRectFilled(
                                       rangeEndPos,
                                       rangeEndPos + TimeRangeShadowSize,
                                       TimeRangeShadowColor);

                // Line
                drawlist.AddRectFilled(rangeEndPos, rangeEndPos + new Vector2(1, 9999), TimeRangeShadowColor);


                ImGui.SetCursorScreenPos(rangeEndPos + new Vector2(5,0));
                ImGui.Button("##SelectionEndPos", TimeRangeHandleSize);
                HandleDrag(_selectionTimeRange.End, _selectionTimeRange.Start);
            }

            ImGui.PopStyleColor();
        }

        private void HandleDrag(double originalU, double origin)
        {
            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(0))
            {
                var u = _timeLineCanvas.InverseTransformX(ImGui.GetIO().MousePos.X);

                if (!_isDragging)
                {
                    _timeLineCanvas.StartDragCommand();
                    _lastDragU = originalU;
                    _isDragging = true;
                }

                _snapHandler.CheckForSnapping(ref u, new List<IValueSnapAttractor> { this });
                var dScale = (u - origin) / (_lastDragU - origin);
                _timeLineCanvas.UpdateDragStretchCommand(scaleU: dScale, scaleV: 1, originU: origin, originV: 0);
                _lastDragU = u;
            }
            else if (_isDragging)
            {
                _isDragging = false;
                _timeLineCanvas.CompleteDragCommand();
            }
        }

        private bool _isDragging;
        private double _lastDragU;

        private static void SetCursorToBottom(float xInScreen, float paddingFromBottom)
        {
            var max = ImGui.GetWindowContentRegionMax() + ImGui.GetWindowPos();
            var p = new Vector2(xInScreen, max.Y - paddingFromBottom);
            ImGui.SetCursorScreenPos(p);
        }

        #region implement snapping interface -----------------------------------
        SnapResult IValueSnapAttractor.CheckForSnap(double targetTime)
        {
            const float snapDistance = 4;
            var snapThresholdOnCanvas = _timeLineCanvas.InverseTransformDirection(new Vector2(snapDistance, 0)).X;
            SnapResult bestSnapResult = null;

            KeyframeOperations.CheckForBetterSnapping(targetTime, _selectionTimeRange.Start, snapThresholdOnCanvas, ref bestSnapResult);
            KeyframeOperations.CheckForBetterSnapping(targetTime, _selectionTimeRange.End, snapThresholdOnCanvas, ref bestSnapResult);
            return bestSnapResult;
        }
        #endregion

        private static readonly Vector2 TimeRangeHandleSize = new Vector2(5, 1000);
        private static readonly Vector2 TimeRangeShadowSize = new Vector2(5, 9999);
        private static readonly Color TimeRangeShadowColor = new Color(0, 0, 0, 0.4f);
        private static readonly Color TimeRangeOutsideColor = new Color(0.0f, 0.0f, 0.0f, 0.1f);
        private static readonly Color TimeRangeMarkerColor = new Color(1f, 1, 1f, 0.3f);
        private readonly TimeLineCanvas _timeLineCanvas;
        private readonly ValueSnapHandler _snapHandler;
        private TimeRange _selectionTimeRange;
    }
}