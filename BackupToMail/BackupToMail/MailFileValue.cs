using System;
using System.IO;

namespace BackupToMail
{
    public partial class MailFile
    {
        long DataValueNumOfBits = 0;
        long DataValueSizeValues = 0;
        long DataValueSizeBytes = 0;
        long DataValueByteOffset = 0;

        /// <summary>
        /// Calculate data value parameters
        /// </summary>
        /// <returns>The value parameters calculate.</returns>
        /// <param name="BitCount">Bit count.</param>
        /// <param name="PoolSize">Pool size.</param>
        public long DataValueParamsCalc(int BitCount, long PoolSize)
        {
            DataValueNumOfBits = BitCount;
            DataValueSizeValues = PoolSize;
            DataValueSizeBytes = DataValueNumOfBits * DataValueSizeValues / 8;
            if (((DataValueNumOfBits * DataValueSizeValues) % 8) == 0)
            {
                return DataValueSizeBytes;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Set the value offet inside one segment
        /// </summary>
        /// <param name="ValOffset">Value offset.</param>
        public void DataValueParamsOffset(long ValOffset)
        {
            DataValueByteOffset = ValOffset;

            if ((SegmentSize - DataValueByteOffset) < DataValueSizeBytes)
            {
                DataValueByteOffset = SegmentSize - DataValueSizeBytes;
            }
        }

        FileStream ParamValueFStream = null;

        public void DataValueFileOpen()
        {
            if (ParamDataFile != null)
            {
                if (!IsDummyFile)
                {
                    ParamValueFStream = DataOpenRW(false);
                }
            }
        }

        public void DataValueFileClose()
        {
            if (ParamValueFStream != null)
            {
                ParamValueFStream.Close();
                ParamValueFStream = null;
            }
        }
    }
}
