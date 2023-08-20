using System;
using System.Collections.Generic;

namespace RaceModeTimer
{
    public abstract class AddressValue<T>
    {
        public List<int> Offsets;
        public T Value;
        public T PreviousValue;
        public Action<T, T> ValueUpdated;

        public void UpdateValue()
        {
            int bytesRead = 0;
            int i;
            List<byte[]> bufferList = new List<byte[]>();
            for (i = 0; i < Offsets.Count; i++)
            {
                int pointer = i == 0 ? 0 : BitConverter.ToInt32(bufferList[i - 1], 0);
                bufferList.Add(new byte[8]);
                MemorySource.ReadProcessMemory((int)MemorySource.NppProcessHandle, pointer + Offsets[i], bufferList[i], bufferList[i].Length, ref bytesRead);
            }

            T value = ConvertToType(bufferList[i - 1]);
            PreviousValue = Value;
            Value = value;
            ValueUpdated?.Invoke(Value, PreviousValue);
        }

        protected abstract T ConvertToType(byte[] buffer);
    }

    public class DoubleAddressValue : AddressValue<double>
    {
        protected override double ConvertToType(byte[] buffer)
        {
            return BitConverter.ToDouble(buffer, 0);
        }
    }

    public class IntAddressValue : AddressValue<int>
    {
        protected override int ConvertToType(byte[] buffer)
        {
            return BitConverter.ToInt32(buffer, 0);
        }
    }

    public class BoolAddressValue : AddressValue<bool>
    {
        protected override bool ConvertToType(byte[] buffer)
        {
            return BitConverter.ToBoolean(buffer, 0);
        }
    }
}
