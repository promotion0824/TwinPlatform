using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Expressions;

namespace WillowExpressions.Test
{
    internal class TemporalMock : ITemporalObject
    {
        public double AverageResult;
        public UnitValue AveragePeriod;
        public UnitValue AverageFrom;

        public IConvertible All(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible Any(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible Average(UnitValue startPeriod, UnitValue endPeriod)
        {
            AveragePeriod = startPeriod;
            AverageFrom = endPeriod;
            return AverageResult;
        }

        public IConvertible Count(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible Delta(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible DeltaLastAndPrevious()
        {
            throw new NotImplementedException();
        }

        public IConvertible DeltaTimeLastAndPrevious(Unit unitOfMeasure)
        {
            throw new NotImplementedException();
        }

        public IConvertible Forecast(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public (bool ok, TimeSpan buffer) IsInRange(UnitValue startPeriod, UnitValue endPeriod)
        {
            return (true, TimeSpan.Zero);
        }

        public IConvertible Max(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible Min(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible Slope(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible StandardDeviation(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public IConvertible Sum(UnitValue startPeriod, UnitValue endPeriod)
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}
