using System.Collections.Generic;

namespace T3.Core.Operator.Slots
{
    public class MultiInputSlot<T> : InputSlot<T>, IMultiInputSlot
    {
        public List<Slot<T>> CollectedInputs { get; } = new List<Slot<T>>(10);

        public MultiInputSlot(InputValue<T> typedInputValue) : base(typedInputValue)
        {
            IsMultiInput = true;
        }

        public MultiInputSlot()
        {
            IsMultiInput = true;
        }

        public List<Slot<T>> GetCollectedTypedInputs()
        {
            CollectedInputs.Clear();

            foreach (var slot in InputConnection)
            {
                if (slot.IsMultiInput && slot.IsConnected)
                {
                    var multiInput = (MultiInputSlot<T>)slot;
                    CollectedInputs.AddRange(multiInput.GetCollectedTypedInputs());
                }
                else
                {
                    CollectedInputs.Add(slot);
                }
            }

            return CollectedInputs;
        }

        public IEnumerable<ISlot> GetCollectedInputs()
        {
            return GetCollectedTypedInputs();
        }
    }
}