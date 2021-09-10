﻿using System.Linq;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3.Core;
using T3.Core.Animation;
using T3.Core.Operator;
using T3.Core.Operator.Slots;
using T3.Gui.Interaction;


namespace T3.Gui.InputUi
{
    public class Float3InputUi : InputValueUi<Vector3>
    {
        public override bool IsAnimatable => true;

        public override IInputUi Clone()
        {
            return new Float3InputUi()
                   {
                       Max = Max,
                       Min = Min,
                       _scale = _scale,
                       Clamp = Clamp,
                       InputDefinition = InputDefinition,
                       Parent = Parent,
                       PosOnCanvas = PosOnCanvas,
                       Relevancy = Relevancy,
                       Size = Size
                   };
        }

        protected override InputEditStateFlags DrawEditControl(string name, ref Vector3 float3Value)
        {
            float3Value.CopyTo(_components);
            var inputEditState = VectorValueEdit.Draw(_components, Min, Max, Scale, Clamp);
            float3Value = new Vector3(_components[0], _components[1], _components[2]);

            return inputEditState;
        }

        private float[] _components = new float[3];

        protected override void DrawReadOnlyControl(string name, ref Vector3 float3Value)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Color.Blue.Rgba);
            DrawEditControl(name, ref float3Value);
            ImGui.PopStyleColor();
        }

        protected override string GetSlotValueAsString(ref Vector3 float3Value)
        {
            // This is a stub of value editing. Sadly it's very hard to get
            // under control because of styling issues and because in GraphNodes
            // The op body captures the mouse event first.
            //
            //SingleValueEdit.Draw(ref floatValue,  -Vector2.UnitX);

            return string.Format(T3Ui.FloatNumberFormat, float3Value);
        }

        protected override void DrawAnimatedValue(string name, InputSlot<Vector3> inputSlot, Animator animator)
        {
            double time = EvaluationContext.GlobalTimeForKeyframes;
            var curves = animator.GetCurvesForInput(inputSlot).ToArray();
            if (curves.Length < _components.Length)
            {
                DrawReadOnlyControl(name, ref inputSlot.Value);
                return;
            }

            for (var index = 0; index < _components.Length; index++)
            {
                _components[index] = (float)curves[index].GetSampledValue(time);
            }

            var inputEditState = VectorValueEdit.Draw(_components, Min, Max, Scale, Clamp);
            if (inputEditState == InputEditStateFlags.Nothing)
                return;

            Curve.UpdateCurveValues(curves, time, _components);
        }

        public override void ApplyValueToAnimation(IInputSlot inputSlot, InputValue inputValue, Animator animator) 
        {
            if (inputValue is InputValue<Vector3> float3InputValue)
            {
                Vector3 value = float3InputValue.Value;
                var curves = animator.GetCurvesForInput(inputSlot).ToArray();
                Curve.UpdateCurveValues(curves, EvaluationContext.GlobalTimeForKeyframes, new [] { value.X, value.Y, value.Z});   
            }
        }

        public override void DrawSettings()
        {
            base.DrawSettings();

            ImGui.DragFloat("Min", ref Min);
            ImGui.DragFloat("Max", ref Max);
            ImGui.DragFloat("Scale", ref _scale);
            ImGui.Checkbox("Clamp Range", ref Clamp);
        }

        public override void Write(JsonTextWriter writer)
        {
            base.Write(writer);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (Min != DefaultMin)
                writer.WriteValue("Min", Min);

            if (Max != DefaultMax)
                writer.WriteValue("Max", Max);

            if (_scale != DefaultScale)
                writer.WriteValue("Scale", _scale);
            
            if (Clamp != false)
                writer.WriteValue("Clamp", Clamp);
            // ReSharper enable CompareOfFloatsByEqualityOperator
        }

        public override void Read(JToken inputToken)
        {
            base.Read(inputToken);

            Min = inputToken["Min"]?.Value<float>() ?? DefaultMin;
            Max = inputToken["Max"]?.Value<float>() ?? DefaultMax;
            _scale = inputToken["Scale"]?.Value<float>() ?? DefaultScale;
            Clamp = inputToken["Clamp"]?.Value<bool>() ?? false;
        }

        public float Min = DefaultMin;
        public float Max = DefaultMax;
        private float _scale = DefaultScale;
        public float Scale => FloatInputUi.GetScaleFromRange(_scale, Min, Max);
        public bool Clamp = false;

        private const float DefaultScale = 0.0f;
        private const float DefaultMin = -9999999f;
        private const float DefaultMax = 9999999f;
    }
}