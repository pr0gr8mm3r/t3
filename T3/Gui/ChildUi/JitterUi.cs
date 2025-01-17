﻿using ImGuiNET;
using T3.Core.Operator;
using T3.Gui.ChildUi.Animators;
using T3.Operators.Types.Id_3b0eb327_6ad8_424f_bca7_ccbfa2c9a986;
using UiHelpers;

namespace T3.Gui.ChildUi
{
    public static class JitterUi
    {
        public static SymbolChildUi.CustomUiResult DrawChildUi(Instance instance, ImDrawListPtr drawList, ImRect screenRect)
        {
            if (!(instance is Jitter jitter))
                return SymbolChildUi.CustomUiResult.None;
            
            if (RateEditLabel.Draw(ref jitter.Rate.TypedInputValue.Value, 
                               screenRect, drawList, nameof(jitter)))
            {
                jitter.Rate.Input.IsDefault = false;
                jitter.Rate.DirtyFlag.Invalidate();
            }
            var label = $"±{jitter.JumpDistance.TypedInputValue.Value:0.0}";
            
            if (MicroGraph.Draw(ref jitter.JumpDistance.TypedInputValue.Value, 
                                ref jitter.Blending.TypedInputValue.Value, 
                                jitter.Fragment, 
                                screenRect, drawList, label))
            {
                jitter.Blending.Input.IsDefault = false;
                jitter.Blending.DirtyFlag.Invalidate();
                jitter.JumpDistance.Input.IsDefault = false;
                jitter.JumpDistance.DirtyFlag.Invalidate();
            }
            return SymbolChildUi.CustomUiResult.Rendered | SymbolChildUi.CustomUiResult.PreventInputLabels;
        }
    }
}