using System;
using System.IO;
using System.Threading;

namespace BackupToMail
{
    public partial class MailFile
    {
        public void DataValueSet(int SegmentNo, ref int[][] DataVals, int DataOffset, int DataPos)
        {
            int DataValueSizeValuesDataOffset = (int)DataValueSizeValues + DataOffset;
            if ((!IsDummyFile) && (ParamDataFile != null))
            {
                long SegmentOffset = SegmentNo;
                if (ParamDigestMode)
                {
                    throw new Exception("Data value cannot be written to digest file");
                }
                else
                {
                    Monitor.Enter(DataF_);
                    SegmentOffset = SegmentOffset * SegmentSize + DataValueByteOffset;
                    long SegmentSize_ = Math.Min(DataValueSizeBytes, ParamValueFStream.Length - SegmentOffset);
                    if (SegmentSize_ > 0)
                    {
                        byte[] SegmentData = new byte[DataValueSizeBytes];

                        int i_ = 0;
                        switch (DataValueNumOfBits)
                        {
                            case 2:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_] = (byte)((DataVals[i + 0][DataPos] << 6) + (DataVals[i + 1][DataPos] << 4) + (DataVals[i + 2][DataPos] << 2) + (DataVals[i + 3][DataPos]));
                                        i_ += 1;
                                    }
                                }
                                break;
                            case 3:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)((DataVals[i + 0][DataPos] << 5) + (DataVals[i + 1][DataPos] << 2) + (DataVals[i + 2][DataPos] >> 1));
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 2][DataPos] & 1) << 7) + (DataVals[i + 3][DataPos] << 4) + (DataVals[i + 4][DataPos] << 1) + (DataVals[i + 5][DataPos] >> 2));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] << 3) + (DataVals[i + 7][DataPos]));
                                        i_ += 3;
                                    }
                                }
                                break;
                            case 4:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                    {
                                        SegmentData[i_] = (byte)((DataVals[i + 0][DataPos] << 4) + DataVals[i + 1][DataPos]);
                                        i_ += 1;
                                    }
                                }
                                break;
                            case 5:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)((DataVals[i + 0][DataPos] << 3) + (DataVals[i + 1][DataPos] >> 2));
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] << 1) + (DataVals[i + 3][DataPos] >> 4));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 1));
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 4][DataPos] & 1) << 7) + (DataVals[i + 5][DataPos] << 2) + (DataVals[i + 6][DataPos] >> 3));
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 6][DataPos] & 7) << 5) + (DataVals[i + 7][DataPos]));
                                        i_ += 5;
                                    }
                                }
                                break;
                            case 6:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)((DataVals[i + 0][DataPos] << 2) + (DataVals[i + 1][DataPos] >> 4));
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 2));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 2][DataPos] & 3) << 6) + (DataVals[i + 3][DataPos]));
                                        i_ += 3;
                                    }
                                }
                                break;
                            case 7:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)((DataVals[i + 0][DataPos] << 1) + (DataVals[i + 1][DataPos] >> 6));
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 1][DataPos] & 63) << 2) + (DataVals[i + 2][DataPos] >> 5));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 2][DataPos] & 31) << 3) + (DataVals[i + 3][DataPos] >> 4));
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 3));
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 4][DataPos] & 7) << 5) + (DataVals[i + 5][DataPos] >> 2));
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] >> 1));
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 6][DataPos] & 1) << 7) + (DataVals[i + 7][DataPos]));
                                        i_ += 7;
                                    }
                                }
                                break;
                            case 8:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i][DataPos]);
                                        i_ += 1;
                                    }
                                }
                                break;
                            case 9:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 1);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] & 1) << 7) + (DataVals[i + 1][DataPos] >> 2));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] >> 3));
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 2][DataPos] & 7) << 5) + (DataVals[i + 3][DataPos] >> 4));
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 5));
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 4][DataPos] & 31) << 3) + (DataVals[i + 5][DataPos] >> 6));
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 5][DataPos] & 63) << 2) + (DataVals[i + 6][DataPos] >> 7));
                                        SegmentData[i_ + 7] = (byte)(((DataVals[i + 6][DataPos] & 127) << 1) + (DataVals[i + 7][DataPos] >> 8));
                                        SegmentData[i_ + 8] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 9;
                                    }
                                }
                                break;
                            case 10:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 2);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] & 3) << 6) + (DataVals[i + 1][DataPos] >> 4));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 6));
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 2][DataPos] & 63) << 2) + (DataVals[i + 3][DataPos] >> 8));
                                        SegmentData[i_ + 4] = (byte)(DataVals[i + 3][DataPos] & 255);
                                        i_ += 5;
                                    }
                                }
                                break;
                            case 11:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 3);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] & 7) << 5) + (DataVals[i + 1][DataPos] >> 6));
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 1][DataPos] & 63) << 2) + (DataVals[i + 2][DataPos] >> 9));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 2][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 2][DataPos] & 1) << 7) + (DataVals[i + 3][DataPos] >> 4));
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 7));
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 4][DataPos] & 127) << 1) + (DataVals[i + 5][DataPos] >> 10));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 5][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 8] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] >> 5));
                                        SegmentData[i_ + 9] = (byte)(((DataVals[i + 6][DataPos] & 31) << 3) + (DataVals[i + 7][DataPos] >> 8));
                                        SegmentData[i_ + 10] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 11;
                                    }
                                }
                                break;
                            case 12:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 4);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] << 4) & 255) + (DataVals[i + 1][DataPos] >> 8));
                                        SegmentData[i_ + 2] = (byte)(DataVals[i + 1][DataPos] & 255);
                                        i_ += 3;
                                    }
                                }
                                break;
                            case 13:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 5);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] & 31) << 3) + (DataVals[i + 1][DataPos] >> 10));
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 1][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] >> 7));
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 2][DataPos] & 127) << 1) + (DataVals[i + 3][DataPos] >> 12));
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 9));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 4][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 8] = (byte)(((DataVals[i + 4][DataPos] & 1) << 7) + (DataVals[i + 5][DataPos] >> 6));
                                        SegmentData[i_ + 9] = (byte)(((DataVals[i + 5][DataPos] & 63) << 2) + (DataVals[i + 6][DataPos] >> 11));
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 6][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 11] = (byte)(((DataVals[i + 6][DataPos] & 7) << 5) + (DataVals[i + 7][DataPos] >> 8));
                                        SegmentData[i_ + 12] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 13;
                                    }
                                }
                                break;
                            case 14:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 6);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] & 63) << 2) + (DataVals[i + 1][DataPos] >> 12));
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 1][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 10));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 2][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 2][DataPos] & 3) << 6) + (DataVals[i + 3][DataPos] >> 8));
                                        SegmentData[i_ + 6] = (byte)(DataVals[i + 3][DataPos] & 255);
                                        i_ += 7;
                                    }
                                }
                                break;
                            case 15:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 7);
                                        SegmentData[i_ + 1] = (byte)(((DataVals[i + 0][DataPos] & 127) << 1) + (DataVals[i + 1][DataPos] >> 14));
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 1][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 1][DataPos] & 63) << 2) + (DataVals[i + 2][DataPos] >> 13));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 2][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 2][DataPos] & 31) << 3) + (DataVals[i + 3][DataPos] >> 12));
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 7] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 11));
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 4][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 9] = (byte)(((DataVals[i + 4][DataPos] & 7) << 5) + (DataVals[i + 5][DataPos] >> 10));
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 5][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 11] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] >> 9));
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 6][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 13] = (byte)(((DataVals[i + 6][DataPos] & 1) << 7) + (DataVals[i + 7][DataPos] >> 8));
                                        SegmentData[i_ + 14] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 15;
                                    }
                                }
                                break;
                            case 16:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i][DataPos] >> 8);
                                        SegmentData[i_ + 1] = (byte)(DataVals[i][DataPos] & 255);
                                        i_ += 2;
                                    }
                                }
                                break;
                            case 17:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 9);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 1) << 7) + (DataVals[i + 1][DataPos] >> 10));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] >> 11));
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 2][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 2][DataPos] & 7) << 5) + (DataVals[i + 3][DataPos] >> 12));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 8] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 13));
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 4][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 10] = (byte)(((DataVals[i + 4][DataPos] & 31) << 3) + (DataVals[i + 5][DataPos] >> 14));
                                        SegmentData[i_ + 11] = (byte)((DataVals[i + 5][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 12] = (byte)(((DataVals[i + 5][DataPos] & 63) << 2) + (DataVals[i + 6][DataPos] >> 15));
                                        SegmentData[i_ + 13] = (byte)((DataVals[i + 6][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 14] = (byte)(((DataVals[i + 6][DataPos] & 127) << 1) + (DataVals[i + 7][DataPos] >> 16));
                                        SegmentData[i_ + 15] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 16] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 17;
                                    }
                                }
                                break;
                            case 18:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 10);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 3) << 6) + (DataVals[i + 1][DataPos] >> 12));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 14));
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 2][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 2][DataPos] & 63) << 2) + (DataVals[i + 3][DataPos] >> 16));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 3][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 8] = (byte)(DataVals[i + 3][DataPos] & 255);
                                        i_ += 9;
                                    }
                                }
                                break;
                            case 19:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 11);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 7) << 5) + (DataVals[i + 1][DataPos] >> 14));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 4] = (byte)(((DataVals[i + 1][DataPos] & 63) << 2) + (DataVals[i + 2][DataPos] >> 17));
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 2][DataPos] >> 9) & 255);
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 2][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 7] = (byte)(((DataVals[i + 2][DataPos] & 1) << 7) + (DataVals[i + 3][DataPos] >> 12));
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 9] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 15));
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 4][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 11] = (byte)(((DataVals[i + 4][DataPos] & 127) << 1) + (DataVals[i + 5][DataPos] >> 18));
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 5][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 13] = (byte)((DataVals[i + 5][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 14] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] >> 13));
                                        SegmentData[i_ + 15] = (byte)((DataVals[i + 6][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 16] = (byte)(((DataVals[i + 6][DataPos] & 31) << 3) + (DataVals[i + 7][DataPos] >> 16));
                                        SegmentData[i_ + 17] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 18] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 19;
                                    }
                                }
                                break;
                            case 20:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 12);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 15) << 4) + ((DataVals[i + 1][DataPos] >> 16) & 15));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos]) & 255);
                                        i_ += 5;
                                    }
                                }
                                break;
                            case 21:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 13);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 31) << 3) + (DataVals[i + 1][DataPos] >> 18));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] >> 15));
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 2][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 7] = (byte)(((DataVals[i + 2][DataPos] & 127) << 1) + (DataVals[i + 3][DataPos] >> 20));
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 3][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 10] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 17));
                                        SegmentData[i_ + 11] = (byte)((DataVals[i + 4][DataPos] >> 9) & 255);
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 4][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 13] = (byte)(((DataVals[i + 4][DataPos] & 1) << 7) + (DataVals[i + 5][DataPos] >> 14));
                                        SegmentData[i_ + 14] = (byte)((DataVals[i + 5][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 15] = (byte)(((DataVals[i + 5][DataPos] & 63) << 2) + (DataVals[i + 6][DataPos] >> 19));
                                        SegmentData[i_ + 16] = (byte)((DataVals[i + 6][DataPos] >> 11) & 255);
                                        SegmentData[i_ + 17] = (byte)((DataVals[i + 6][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 18] = (byte)(((DataVals[i + 6][DataPos] & 7) << 5) + (DataVals[i + 7][DataPos] >> 16));
                                        SegmentData[i_ + 19] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 20] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 21;
                                    }
                                }
                                break;
                            case 22:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 14);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 63) << 2) + (DataVals[i + 1][DataPos] >> 20));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 18));
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 2][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 2][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 8] = (byte)(((DataVals[i + 2][DataPos] & 3) << 6) + (DataVals[i + 3][DataPos] >> 16));
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 3][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 10] = (byte)(DataVals[i + 3][DataPos] & 255);
                                        i_ += 11;
                                    }
                                }
                                break;
                            case 23:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 15);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 2] = (byte)(((DataVals[i + 0][DataPos] & 127) << 1) + (DataVals[i + 1][DataPos] >> 22));
                                        SegmentData[i_ + 3] = (byte)((DataVals[i + 1][DataPos] >> 14) & 255);
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 5] = (byte)(((DataVals[i + 1][DataPos] & 63) << 2) + (DataVals[i + 2][DataPos] >> 21));
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 2][DataPos] >> 13) & 255);
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 2][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 8] = (byte)(((DataVals[i + 2][DataPos] & 31) << 3) + (DataVals[i + 3][DataPos] >> 20));
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 3][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 11] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 19));
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 4][DataPos] >> 11) & 255);
                                        SegmentData[i_ + 13] = (byte)((DataVals[i + 4][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 14] = (byte)(((DataVals[i + 4][DataPos] & 7) << 5) + (DataVals[i + 5][DataPos] >> 18));
                                        SegmentData[i_ + 15] = (byte)((DataVals[i + 5][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 16] = (byte)((DataVals[i + 5][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 17] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] >> 17));
                                        SegmentData[i_ + 18] = (byte)((DataVals[i + 6][DataPos] >> 9) & 255);
                                        SegmentData[i_ + 19] = (byte)((DataVals[i + 6][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 20] = (byte)(((DataVals[i + 6][DataPos] & 1) << 7) + (DataVals[i + 7][DataPos] >> 16));
                                        SegmentData[i_ + 21] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 22] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 23;
                                    }
                                }
                                break;
                            case 24:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i][DataPos] >> 16);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 2] = (byte)(DataVals[i][DataPos] & 255);
                                        i_ += 3;
                                    }
                                }
                                break;
                            case 25:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 17);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 9) & 255);
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 0][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 0][DataPos] & 1) << 7) + (DataVals[i + 1][DataPos] >> 18));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 1][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] >> 19));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 2][DataPos] >> 11) & 255);
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 2][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 9] = (byte)(((DataVals[i + 2][DataPos] & 7) << 5) + (DataVals[i + 3][DataPos] >> 20));
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 3][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 11] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 12] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 21));
                                        SegmentData[i_ + 13] = (byte)((DataVals[i + 4][DataPos] >> 13) & 255);
                                        SegmentData[i_ + 14] = (byte)((DataVals[i + 4][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 15] = (byte)(((DataVals[i + 4][DataPos] & 31) << 3) + (DataVals[i + 5][DataPos] >> 22));
                                        SegmentData[i_ + 16] = (byte)((DataVals[i + 5][DataPos] >> 14) & 255);
                                        SegmentData[i_ + 17] = (byte)((DataVals[i + 5][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 18] = (byte)(((DataVals[i + 5][DataPos] & 63) << 2) + (DataVals[i + 6][DataPos] >> 23));
                                        SegmentData[i_ + 19] = (byte)((DataVals[i + 6][DataPos] >> 15) & 255);
                                        SegmentData[i_ + 20] = (byte)((DataVals[i + 6][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 21] = (byte)(((DataVals[i + 6][DataPos] & 127) << 1) + (DataVals[i + 7][DataPos] >> 24));
                                        SegmentData[i_ + 22] = (byte)((DataVals[i + 7][DataPos] >> 16) & 255);
                                        SegmentData[i_ + 23] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 24] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 25;
                                    }
                                }
                                break;
                            case 26:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 18);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 0][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 0][DataPos] & 3) << 6) + (DataVals[i + 1][DataPos] >> 20));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 1][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 22));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 2][DataPos] >> 14) & 255);
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 2][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 9] = (byte)(((DataVals[i + 2][DataPos] & 63) << 2) + (DataVals[i + 3][DataPos] >> 24));
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 3][DataPos] >> 16) & 255);
                                        SegmentData[i_ + 11] = (byte)((DataVals[i + 3][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 12] = (byte)(DataVals[i + 3][DataPos] & 255);
                                        i_ += 13;
                                    }
                                }
                                break;
                            case 27:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 19);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 11) & 255);
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 0][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 0][DataPos] & 7) << 5) + (DataVals[i + 1][DataPos] >> 22));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 14) & 255);
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 1][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 6] = (byte)(((DataVals[i + 1][DataPos] & 63) << 2) + (DataVals[i + 2][DataPos] >> 25));
                                        SegmentData[i_ + 7] = (byte)((DataVals[i + 2][DataPos] >> 17) & 255);
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 2][DataPos] >> 9) & 255);
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 2][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 10] = (byte)(((DataVals[i + 2][DataPos] & 1) << 7) + (DataVals[i + 3][DataPos] >> 20));
                                        SegmentData[i_ + 11] = (byte)((DataVals[i + 3][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 13] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 23));
                                        SegmentData[i_ + 14] = (byte)((DataVals[i + 4][DataPos] >> 15) & 255);
                                        SegmentData[i_ + 15] = (byte)((DataVals[i + 4][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 16] = (byte)(((DataVals[i + 4][DataPos] & 127) << 1) + (DataVals[i + 5][DataPos] >> 26));
                                        SegmentData[i_ + 17] = (byte)((DataVals[i + 5][DataPos] >> 18) & 255);
                                        SegmentData[i_ + 18] = (byte)((DataVals[i + 5][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 19] = (byte)((DataVals[i + 5][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 20] = (byte)(((DataVals[i + 5][DataPos] & 3) << 6) + (DataVals[i + 6][DataPos] >> 21));
                                        SegmentData[i_ + 21] = (byte)((DataVals[i + 6][DataPos] >> 13) & 255);
                                        SegmentData[i_ + 22] = (byte)((DataVals[i + 6][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 23] = (byte)(((DataVals[i + 6][DataPos] & 31) << 3) + (DataVals[i + 7][DataPos] >> 24));
                                        SegmentData[i_ + 24] = (byte)((DataVals[i + 7][DataPos] >> 16) & 255);
                                        SegmentData[i_ + 25] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 26] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 27;
                                    }
                                }
                                break;
                            case 28:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 20);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 0][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 0][DataPos] & 15) << 4) + ((DataVals[i + 1][DataPos] >> 24) & 15));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 16) & 255);
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 1][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 1][DataPos]) & 255);
                                        i_ += 7;
                                    }
                                }
                                break;
                            case 29:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 21);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 13) & 255);
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 0][DataPos] >> 5) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 0][DataPos] & 31) << 3) + (DataVals[i + 1][DataPos] >> 26));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 18) & 255);
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 1][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 1][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 7] = (byte)(((DataVals[i + 1][DataPos] & 3) << 6) + (DataVals[i + 2][DataPos] >> 23));
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 2][DataPos] >> 15) & 255);
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 2][DataPos] >> 7) & 255);
                                        SegmentData[i_ + 10] = (byte)(((DataVals[i + 2][DataPos] & 127) << 1) + (DataVals[i + 3][DataPos] >> 28));
                                        SegmentData[i_ + 11] = (byte)((DataVals[i + 3][DataPos] >> 20) & 255);
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 3][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 13] = (byte)((DataVals[i + 3][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 14] = (byte)(((DataVals[i + 3][DataPos] & 15) << 4) + (DataVals[i + 4][DataPos] >> 25));
                                        SegmentData[i_ + 15] = (byte)((DataVals[i + 4][DataPos] >> 17) & 255);
                                        SegmentData[i_ + 16] = (byte)((DataVals[i + 4][DataPos] >> 9) & 255);
                                        SegmentData[i_ + 17] = (byte)((DataVals[i + 4][DataPos] >> 1) & 255);
                                        SegmentData[i_ + 18] = (byte)(((DataVals[i + 4][DataPos] & 1) << 7) + (DataVals[i + 5][DataPos] >> 22));
                                        SegmentData[i_ + 19] = (byte)((DataVals[i + 5][DataPos] >> 14) & 255);
                                        SegmentData[i_ + 20] = (byte)((DataVals[i + 5][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 21] = (byte)(((DataVals[i + 5][DataPos] & 63) << 2) + (DataVals[i + 6][DataPos] >> 27));
                                        SegmentData[i_ + 22] = (byte)((DataVals[i + 6][DataPos] >> 19) & 255);
                                        SegmentData[i_ + 23] = (byte)((DataVals[i + 6][DataPos] >> 11) & 255);
                                        SegmentData[i_ + 24] = (byte)((DataVals[i + 6][DataPos] >> 3) & 255);
                                        SegmentData[i_ + 25] = (byte)(((DataVals[i + 6][DataPos] & 7) << 5) + (DataVals[i + 7][DataPos] >> 24));
                                        SegmentData[i_ + 26] = (byte)((DataVals[i + 7][DataPos] >> 16) & 255);
                                        SegmentData[i_ + 27] = (byte)((DataVals[i + 7][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 28] = (byte)(DataVals[i + 7][DataPos] & 255);
                                        i_ += 29;
                                    }
                                }
                                break;
                            case 30:
                                {
                                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                    {
                                        SegmentData[i_ + 0] = (byte)(DataVals[i + 0][DataPos] >> 22);
                                        SegmentData[i_ + 1] = (byte)((DataVals[i + 0][DataPos] >> 14) & 255);
                                        SegmentData[i_ + 2] = (byte)((DataVals[i + 0][DataPos] >> 6) & 255);
                                        SegmentData[i_ + 3] = (byte)(((DataVals[i + 0][DataPos] & 63) << 2) + (DataVals[i + 1][DataPos] >> 28));
                                        SegmentData[i_ + 4] = (byte)((DataVals[i + 1][DataPos] >> 20) & 255);
                                        SegmentData[i_ + 5] = (byte)((DataVals[i + 1][DataPos] >> 12) & 255);
                                        SegmentData[i_ + 6] = (byte)((DataVals[i + 1][DataPos] >> 4) & 255);
                                        SegmentData[i_ + 7] = (byte)(((DataVals[i + 1][DataPos] & 15) << 4) + (DataVals[i + 2][DataPos] >> 26));
                                        SegmentData[i_ + 8] = (byte)((DataVals[i + 2][DataPos] >> 18) & 255);
                                        SegmentData[i_ + 9] = (byte)((DataVals[i + 2][DataPos] >> 10) & 255);
                                        SegmentData[i_ + 10] = (byte)((DataVals[i + 2][DataPos] >> 2) & 255);
                                        SegmentData[i_ + 11] = (byte)(((DataVals[i + 2][DataPos] & 3) << 6) + (DataVals[i + 3][DataPos] >> 24));
                                        SegmentData[i_ + 12] = (byte)((DataVals[i + 3][DataPos] >> 16) & 255);
                                        SegmentData[i_ + 13] = (byte)((DataVals[i + 3][DataPos] >> 8) & 255);
                                        SegmentData[i_ + 14] = (byte)(DataVals[i + 3][DataPos] & 255);
                                        i_ += 15;
                                    }
                                }
                                break;
                        }

                        ParamValueFStream.Seek(SegmentOffset, SeekOrigin.Begin);
                        ParamValueFStream.Write(SegmentData, 0, (int)SegmentSize_);
                    }
                    Monitor.Exit(DataF_);
                }
            }
        }
    }
}
