using System;
using System.IO;
using System.Threading;

namespace BackupToMail
{
    public partial class MailFile
    {
        public void DataValueGet(int SegmentNo, ref int[][] DataVals, int DataOffset, int DataPos)
        {
            int DataValueSizeValuesDataOffset = (int)DataValueSizeValues + DataOffset;
            byte[] SegmentData = null;
            if (ParamDataFile != null)
            {
                if (IsDummyFile)
                {
                    long SegmentOffset = SegmentNo;
                    SegmentOffset = SegmentOffset * SegmentSize + DataValueByteOffset;
                    long SegmentSize_ = DummyFileSize - SegmentOffset;
                    if (SegmentSize_ > 0)
                    {
                        Monitor.Enter(DataF_);
                        SegmentData = RandomSequence_.GenSeq(SegmentOffset, SegmentSize_);
                        Monitor.Exit(DataF_);
                        for (long i = SegmentSize_; i < DataValueSizeBytes; i++)
                        {
                            SegmentData[i] = 0;
                        }
                    }
                    else
                    {
                        for (long i = 0; i < DataValueSizeBytes; i++)
                        {
                            SegmentData[i] = 0;
                        }
                    }
                }
                else
                {
                    long SegmentOffset = SegmentNo;
                    if (ParamDigestMode)
                    {
                        throw new Exception("Data value cannot be read from digest file");
                    }
                    else
                    {
                        SegmentOffset = SegmentOffset * SegmentSize + DataValueByteOffset;
                        long SegmentSize_ = Math.Min(DataValueSizeBytes, ParamValueFStream.Length - SegmentOffset);
                        if (SegmentSize_ > 0)
                        {
                            SegmentData = new byte[DataValueSizeBytes];
                            Monitor.Enter(DataF_);
                            ParamValueFStream.Seek(SegmentOffset, SeekOrigin.Begin);
                            ParamValueFStream.Read(SegmentData, 0, (int)SegmentSize_);
                            Monitor.Exit(DataF_);
                            for (long i = SegmentSize_; i < DataValueSizeBytes; i++)
                            {
                                SegmentData[i] = 0;
                            }
                        }
                    }
                }

                if (SegmentData != null)
                {
                    int i_ = 0;
                    switch (DataValueNumOfBits)
                    {
                        case 2:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = ((int)SegmentData[i_]) >> 6;
                                    DataVals[i + 1][DataPos] = (((int)SegmentData[i_]) >> 4) & 3;
                                    DataVals[i + 2][DataPos] = (((int)SegmentData[i_]) >> 2) & 3;
                                    DataVals[i + 3][DataPos] = ((int)SegmentData[i_]) & 3;
                                    i_ += 1;
                                }
                            }
                            break;
                        case 3:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = ((int)SegmentData[i_ + 0]) >> 5;
                                    DataVals[i + 1][DataPos] = (((int)SegmentData[i_ + 0]) >> 2) & 7;
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 0]) & 3) << 1) + (((int)SegmentData[i_ + 1]) >> 7);
                                    DataVals[i + 3][DataPos] = (((int)SegmentData[i_ + 1]) >> 4) & 7;
                                    DataVals[i + 4][DataPos] = (((int)SegmentData[i_ + 1]) >> 1) & 7;
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 1]) & 1) << 2) + (((int)SegmentData[i_ + 2]) >> 6);
                                    DataVals[i + 6][DataPos] = (((int)SegmentData[i_ + 2]) >> 3) & 7;
                                    DataVals[i + 7][DataPos] = ((int)SegmentData[i_ + 2]) & 7;
                                    i_ += 3;
                                }
                            }
                            break;
                        case 4:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                {
                                    DataVals[i + 0][DataPos] = ((int)SegmentData[i_]) >> 4;
                                    DataVals[i + 1][DataPos] = ((int)SegmentData[i_]) & 15;
                                    i_ += 1;
                                }
                            }
                            break;
                        case 5:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = ((int)SegmentData[i_ + 0]) >> 3;
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 0]) & 7) << 2) + (((int)SegmentData[i_ + 1]) >> 6);
                                    DataVals[i + 2][DataPos] = (((int)SegmentData[i_ + 1]) >> 1) & 31;
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 1]) & 1) << 4) + (((int)SegmentData[i_ + 2]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 2]) & 15) << 1) + (((int)SegmentData[i_ + 3]) >> 7);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 3]) >> 2) & 31);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 3]) & 3) << 3) + (((int)SegmentData[i_ + 4]) >> 5);
                                    DataVals[i + 7][DataPos] = ((int)SegmentData[i_ + 4]) & 31;
                                    i_ += 5;
                                }
                            }
                            break;
                        case 6:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = ((int)SegmentData[i_ + 0]) >> 2;
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 0]) & 3) << 4) + (((int)SegmentData[i_ + 1]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 1]) & 15) << 2) + (((int)SegmentData[i_ + 2]) >> 6);
                                    DataVals[i + 3][DataPos] = ((int)SegmentData[i_ + 2]) & 63;
                                    i_ += 3;
                                }
                            }
                            break;
                        case 7:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = ((int)SegmentData[i_ + 0]) >> 1;
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 0]) & 1) << 6) + (((int)SegmentData[i_ + 1]) >> 2);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 1]) & 3) << 5) + (((int)SegmentData[i_ + 2]) >> 3);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 2]) & 7) << 4) + (((int)SegmentData[i_ + 3]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 3]) & 15) << 3) + (((int)SegmentData[i_ + 4]) >> 5);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 4]) & 31) << 2) + (((int)SegmentData[i_ + 5]) >> 6);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 5]) & 63) << 1) + (((int)SegmentData[i_ + 6]) >> 7);
                                    DataVals[i + 7][DataPos] = ((int)SegmentData[i_ + 6]) & 127;
                                    i_ += 7;
                                }
                            }
                            break;
                        case 8:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                                {
                                    DataVals[i][DataPos] = ((int)SegmentData[i_ + 0]);
                                    i_ += 1;
                                }
                            }
                            break;
                        case 9:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 1) + (((int)SegmentData[i_ + 1]) >> 7);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 127) << 2) + (((int)SegmentData[i_ + 2]) >> 6);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 2]) & 63) << 3) + (((int)SegmentData[i_ + 3]) >> 5);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 3]) & 31) << 4) + (((int)SegmentData[i_ + 4]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 4]) & 15) << 5) + (((int)SegmentData[i_ + 5]) >> 3);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 5]) & 7) << 6) + (((int)SegmentData[i_ + 6]) >> 2);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 6]) & 3) << 7) + (((int)SegmentData[i_ + 7]) >> 1);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 7]) & 1) << 8) + ((int)SegmentData[i_ + 8]);
                                    i_ += 9;
                                }
                            }
                            break;
                        case 10:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 2) + (((int)SegmentData[i_ + 1]) >> 6);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 63) << 4) + (((int)SegmentData[i_ + 2]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 2]) & 15) << 6) + (((int)SegmentData[i_ + 3]) >> 2);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 3]) & 3) << 8) + ((int)SegmentData[i_ + 4]);
                                    i_ += 5;
                                }
                            }
                            break;
                        case 11:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 3) + (((int)SegmentData[i_ + 1]) >> 5);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 31) << 6) + (((int)SegmentData[i_ + 2]) >> 2);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 2]) & 3) << 9) + (((int)SegmentData[i_ + 3]) << 1) + (((int)SegmentData[i_ + 4]) >> 7);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 4]) & 127) << 4) + (((int)SegmentData[i_ + 5]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 5]) & 15) << 7) + (((int)SegmentData[i_ + 6]) >> 1);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 6]) & 1) << 10) + (((int)SegmentData[i_ + 7]) << 2) + (((int)SegmentData[i_ + 8]) >> 6);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 8]) & 63) << 5) + (((int)SegmentData[i_ + 9]) >> 3);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 9]) & 7) << 8) + ((int)SegmentData[i_ + 10]);
                                    i_ += 11;
                                }
                            }
                            break;
                        case 12:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 4) + (((int)SegmentData[i_ + 1]) >> 4);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 15) << 8) + ((int)SegmentData[i_ + 2]);
                                    i_ += 3;
                                }
                            }
                            break;
                        case 13:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 5) + (((int)SegmentData[i_ + 1]) >> 3);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 7) << 10) + (((int)SegmentData[i_ + 2]) << 2) + (((int)SegmentData[i_ + 3]) >> 6);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 3]) & 63) << 7) + (((int)SegmentData[i_ + 4]) >> 1);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 4]) & 1) << 12) + (((int)SegmentData[i_ + 5]) << 4) + (((int)SegmentData[i_ + 6]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 6]) & 15) << 9) + (((int)SegmentData[i_ + 7]) << 1) + (((int)SegmentData[i_ + 8]) >> 7);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 8]) & 127) << 6) + (((int)SegmentData[i_ + 9]) >> 2);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 9]) & 3) << 11) + (((int)SegmentData[i_ + 10]) << 3) + (((int)SegmentData[i_ + 11]) >> 5);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 11]) & 31) << 8) + ((int)SegmentData[i_ + 12]);
                                    i_ += 13;
                                }
                            }
                            break;
                        case 14:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 6) + (((int)SegmentData[i_ + 1]) >> 2);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 3) << 12) + (((int)SegmentData[i_ + 2]) << 4) + (((int)SegmentData[i_ + 3]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 3]) & 15) << 10) + (((int)SegmentData[i_ + 4]) << 2) + (((int)SegmentData[i_ + 5]) >> 6);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 5]) & 63) << 8) + ((int)SegmentData[i_ + 6]);
                                    i_ += 7;
                                }
                            }
                            break;
                        case 15:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 7) + (((int)SegmentData[i_ + 1]) >> 1);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 1]) & 1) << 14) + (((int)SegmentData[i_ + 2]) << 6) + (((int)SegmentData[i_ + 3]) >> 2);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 3]) & 3) << 13) + (((int)SegmentData[i_ + 4]) << 5) + (((int)SegmentData[i_ + 5]) >> 3);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 5]) & 7) << 12) + (((int)SegmentData[i_ + 6]) << 4) + (((int)SegmentData[i_ + 7]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 7]) & 15) << 11) + (((int)SegmentData[i_ + 8]) << 3) + (((int)SegmentData[i_ + 9]) >> 5);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 9]) & 31) << 10) + (((int)SegmentData[i_ + 10]) << 2) + (((int)SegmentData[i_ + 11]) >> 6);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 11]) & 63) << 9) + (((int)SegmentData[i_ + 12]) << 1) + (((int)SegmentData[i_ + 13]) >> 7);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 13]) & 127) << 8) + ((int)SegmentData[i_ + 14]);
                                    i_ += 15;
                                }
                            }
                            break;
                        case 16:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                                {
                                    DataVals[i][DataPos] = ((((int)SegmentData[i_ + 0]) << 8) + ((int)SegmentData[i_ + 1]));
                                    i_ += 2;
                                }
                            }
                            break;
                        case 17:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 9) + (((int)SegmentData[i_ + 1]) << 1) + (((int)SegmentData[i_ + 2]) >> 7);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 127) << 10) + (((int)SegmentData[i_ + 3]) << 2) + (((int)SegmentData[i_ + 4]) >> 6);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 4]) & 63) << 11) + (((int)SegmentData[i_ + 5]) << 3) + (((int)SegmentData[i_ + 6]) >> 5);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 6]) & 31) << 12) + (((int)SegmentData[i_ + 7]) << 4) + (((int)SegmentData[i_ + 8]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 8]) & 15) << 13) + (((int)SegmentData[i_ + 9]) << 5) + (((int)SegmentData[i_ + 10]) >> 3);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 10]) & 7) << 14) + (((int)SegmentData[i_ + 11]) << 6) + (((int)SegmentData[i_ + 12]) >> 2);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 12]) & 3) << 15) + (((int)SegmentData[i_ + 13]) << 7) + (((int)SegmentData[i_ + 14]) >> 1);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 14]) & 1) << 16) + (((int)SegmentData[i_ + 15]) << 8) + ((int)SegmentData[i_ + 16]);
                                    i_ += 17;
                                }
                            }
                            break;
                        case 18:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 10) + (((int)SegmentData[i_ + 1]) << 2) + (((int)SegmentData[i_ + 2]) >> 6);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 63) << 12) + (((int)SegmentData[i_ + 3]) << 4) + (((int)SegmentData[i_ + 4]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 4]) & 15) << 14) + (((int)SegmentData[i_ + 5]) << 6) + (((int)SegmentData[i_ + 6]) >> 2);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 6]) & 3) << 16) + (((int)SegmentData[i_ + 7]) << 8) + ((int)SegmentData[i_ + 8]);
                                    i_ += 9;
                                }
                            }
                            break;
                        case 19:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 11) + (((int)SegmentData[i_ + 1]) << 3) + (((int)SegmentData[i_ + 2]) >> 5);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 31) << 14) + (((int)SegmentData[i_ + 3]) << 6) + (((int)SegmentData[i_ + 4]) >> 2);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 4]) & 3) << 17) + (((int)SegmentData[i_ + 5]) << 9) + (((int)SegmentData[i_ + 6]) << 1) + (((int)SegmentData[i_ + 7]) >> 7);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 7]) & 127) << 12) + (((int)SegmentData[i_ + 8]) << 4) + (((int)SegmentData[i_ + 9]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 9]) & 15) << 15) + (((int)SegmentData[i_ + 10]) << 7) + (((int)SegmentData[i_ + 11]) >> 1);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 11]) & 1) << 18) + (((int)SegmentData[i_ + 12]) << 10) + (((int)SegmentData[i_ + 13]) << 2) + (((int)SegmentData[i_ + 14]) >> 6);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 14]) & 63) << 13) + (((int)SegmentData[i_ + 15]) << 5) + (((int)SegmentData[i_ + 16]) >> 3);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 16]) & 7) << 16) + (((int)SegmentData[i_ + 17]) << 8) + ((int)SegmentData[i_ + 18]);
                                    i_ += 19;
                                }
                            }
                            break;
                        case 20:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 12) + (((int)SegmentData[i_ + 1]) << 4) + (((int)SegmentData[i_ + 2]) >> 4);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 15) << 16) + (((int)SegmentData[i_ + 3]) << 8) + ((int)SegmentData[i_ + 4]);
                                    i_ += 5;
                                }
                            }
                            break;
                        case 21:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 13) + (((int)SegmentData[i_ + 1]) << 5) + (((int)SegmentData[i_ + 2]) >> 3);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 7) << 18) + (((int)SegmentData[i_ + 3]) << 10) + (((int)SegmentData[i_ + 4]) << 2) + (((int)SegmentData[i_ + 5]) >> 6);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 5]) & 63) << 15) + (((int)SegmentData[i_ + 6]) << 7) + (((int)SegmentData[i_ + 7]) >> 1);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 7]) & 1) << 20) + (((int)SegmentData[i_ + 8]) << 12) + (((int)SegmentData[i_ + 9]) << 4) + (((int)SegmentData[i_ + 10]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 10]) & 15) << 17) + (((int)SegmentData[i_ + 11]) << 9) + (((int)SegmentData[i_ + 12]) << 1) + (((int)SegmentData[i_ + 13]) >> 7);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 13]) & 127) << 14) + (((int)SegmentData[i_ + 14]) << 6) + (((int)SegmentData[i_ + 15]) >> 2);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 15]) & 3) << 19) + (((int)SegmentData[i_ + 16]) << 11) + (((int)SegmentData[i_ + 17]) << 3) + (((int)SegmentData[i_ + 18]) >> 5);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 18]) & 31) << 16) + (((int)SegmentData[i_ + 19]) << 8) + ((int)SegmentData[i_ + 20]);
                                    i_ += 21;
                                }
                            }
                            break;
                        case 22:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 14) + (((int)SegmentData[i_ + 1]) << 6) + (((int)SegmentData[i_ + 2]) >> 2);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 3) << 20) + (((int)SegmentData[i_ + 3]) << 12) + (((int)SegmentData[i_ + 4]) << 4) + (((int)SegmentData[i_ + 5]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 5]) & 15) << 18) + (((int)SegmentData[i_ + 6]) << 10) + (((int)SegmentData[i_ + 7]) << 2) + (((int)SegmentData[i_ + 8]) >> 6);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 8]) & 63) << 16) + (((int)SegmentData[i_ + 9]) << 8) + ((int)SegmentData[i_ + 10]);
                                    i_ += 11;
                                }
                            }
                            break;
                        case 23:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 15) + (((int)SegmentData[i_ + 1]) << 7) + (((int)SegmentData[i_ + 2]) >> 1);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 2]) & 1) << 22) + (((int)SegmentData[i_ + 3]) << 14) + (((int)SegmentData[i_ + 4]) << 6) + (((int)SegmentData[i_ + 5]) >> 2);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 5]) & 3) << 21) + (((int)SegmentData[i_ + 6]) << 13) + (((int)SegmentData[i_ + 7]) << 5) + (((int)SegmentData[i_ + 8]) >> 3);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 8]) & 7) << 20) + (((int)SegmentData[i_ + 9]) << 12) + (((int)SegmentData[i_ + 10]) << 4) + (((int)SegmentData[i_ + 11]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 11]) & 15) << 19) + (((int)SegmentData[i_ + 12]) << 11) + (((int)SegmentData[i_ + 13]) << 3) + (((int)SegmentData[i_ + 14]) >> 5);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 14]) & 31) << 18) + (((int)SegmentData[i_ + 15]) << 10) + (((int)SegmentData[i_ + 16]) << 2) + (((int)SegmentData[i_ + 17]) >> 6);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 17]) & 63) << 17) + (((int)SegmentData[i_ + 18]) << 9) + (((int)SegmentData[i_ + 19]) << 1) + (((int)SegmentData[i_ + 20]) >> 7);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 20]) & 127) << 16) + (((int)SegmentData[i_ + 21]) << 8) + ((int)SegmentData[i_ + 22]);
                                    i_ += 23;
                                }
                            }
                            break;
                        case 24:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                                {
                                    DataVals[i][DataPos] = ((((int)SegmentData[i_ + 0]) << 16) + (((int)SegmentData[i_ + 1]) << 8) + ((int)SegmentData[i_ + 2]));
                                    i_ += 3;
                                }
                            }
                            break;
                        case 25:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 17) + (((int)SegmentData[i_ + 1]) << 9) + (((int)SegmentData[i_ + 2]) << 1) + (((int)SegmentData[i_ + 3]) >> 7);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 3]) & 127) << 18) + (((int)SegmentData[i_ + 4]) << 10) + (((int)SegmentData[i_ + 5]) << 2) + (((int)SegmentData[i_ + 6]) >> 6);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 6]) & 63) << 19) + (((int)SegmentData[i_ + 7]) << 11) + (((int)SegmentData[i_ + 8]) << 3) + (((int)SegmentData[i_ + 9]) >> 5);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 9]) & 31) << 20) + (((int)SegmentData[i_ + 10]) << 12) + (((int)SegmentData[i_ + 11]) << 4) + (((int)SegmentData[i_ + 12]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 12]) & 15) << 21) + (((int)SegmentData[i_ + 13]) << 13) + (((int)SegmentData[i_ + 14]) << 5) + (((int)SegmentData[i_ + 15]) >> 3);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 15]) & 7) << 22) + (((int)SegmentData[i_ + 16]) << 14) + (((int)SegmentData[i_ + 17]) << 6) + (((int)SegmentData[i_ + 18]) >> 2);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 18]) & 3) << 23) + (((int)SegmentData[i_ + 19]) << 15) + (((int)SegmentData[i_ + 20]) << 7) + (((int)SegmentData[i_ + 21]) >> 1);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 21]) & 1) << 24) + (((int)SegmentData[i_ + 22]) << 16) + (((int)SegmentData[i_ + 23]) << 8) + ((int)SegmentData[i_ + 24]);
                                    i_ += 25;
                                }
                            }
                            break;
                        case 26:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 18) + (((int)SegmentData[i_ + 1]) << 10) + (((int)SegmentData[i_ + 2]) << 2) + (((int)SegmentData[i_ + 3]) >> 6);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 3]) & 63) << 20) + (((int)SegmentData[i_ + 4]) << 12) + (((int)SegmentData[i_ + 5]) << 4) + (((int)SegmentData[i_ + 6]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 6]) & 15) << 22) + (((int)SegmentData[i_ + 7]) << 14) + (((int)SegmentData[i_ + 8]) << 6) + (((int)SegmentData[i_ + 9]) >> 2);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 9]) & 3) << 24) + (((int)SegmentData[i_ + 10]) << 16) + (((int)SegmentData[i_ + 11]) << 8) + ((int)SegmentData[i_ + 12]);
                                    i_ += 13;
                                }
                            }
                            break;
                        case 27:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 19) + (((int)SegmentData[i_ + 1]) << 11) + (((int)SegmentData[i_ + 2]) << 3) + (((int)SegmentData[i_ + 3]) >> 5);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 3]) & 31) << 22) + (((int)SegmentData[i_ + 4]) << 14) + (((int)SegmentData[i_ + 5]) << 6) + (((int)SegmentData[i_ + 6]) >> 2);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 6]) & 3) << 25) + (((int)SegmentData[i_ + 7]) << 17) + (((int)SegmentData[i_ + 8]) << 9) + (((int)SegmentData[i_ + 9]) << 1) + (((int)SegmentData[i_ + 10]) >> 7);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 10]) & 127) << 20) + (((int)SegmentData[i_ + 11]) << 12) + (((int)SegmentData[i_ + 12]) << 4) + (((int)SegmentData[i_ + 13]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 13]) & 15) << 23) + (((int)SegmentData[i_ + 14]) << 15) + (((int)SegmentData[i_ + 15]) << 7) + (((int)SegmentData[i_ + 16]) >> 1);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 16]) & 1) << 26) + (((int)SegmentData[i_ + 17]) << 18) + (((int)SegmentData[i_ + 18]) << 10) + (((int)SegmentData[i_ + 19]) << 2) + (((int)SegmentData[i_ + 20]) >> 6);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 20]) & 63) << 21) + (((int)SegmentData[i_ + 21]) << 13) + (((int)SegmentData[i_ + 22]) << 5) + (((int)SegmentData[i_ + 23]) >> 3);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 23]) & 7) << 24) + (((int)SegmentData[i_ + 24]) << 16) + (((int)SegmentData[i_ + 25]) << 8) + ((int)SegmentData[i_ + 26]);
                                    i_ += 27;
                                }
                            }
                            break;
                        case 28:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 2)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 20) + (((int)SegmentData[i_ + 1]) << 12) + (((int)SegmentData[i_ + 2]) << 4) + (((int)SegmentData[i_ + 3]) >> 4);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 3]) & 15) << 24) + (((int)SegmentData[i_ + 4]) << 16) + (((int)SegmentData[i_ + 5]) << 8) + ((int)SegmentData[i_ + 6]);
                                    i_ += 7;
                                }
                            }
                            break;
                        case 29:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 8)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 21) + (((int)SegmentData[i_ + 1]) << 13) + (((int)SegmentData[i_ + 2]) << 5) + (((int)SegmentData[i_ + 3]) >> 3);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 3]) & 7) << 26) + (((int)SegmentData[i_ + 4]) << 18) + (((int)SegmentData[i_ + 5]) << 10) + (((int)SegmentData[i_ + 6]) << 2) + (((int)SegmentData[i_ + 7]) >> 6);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 7]) & 63) << 23) + (((int)SegmentData[i_ + 8]) << 15) + (((int)SegmentData[i_ + 9]) << 7) + (((int)SegmentData[i_ + 10]) >> 1);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 10]) & 1) << 28) + (((int)SegmentData[i_ + 11]) << 20) + (((int)SegmentData[i_ + 12]) << 12) + (((int)SegmentData[i_ + 13]) << 4) + (((int)SegmentData[i_ + 14]) >> 4);
                                    DataVals[i + 4][DataPos] = ((((int)SegmentData[i_ + 14]) & 15) << 25) + (((int)SegmentData[i_ + 15]) << 17) + (((int)SegmentData[i_ + 16]) << 9) + (((int)SegmentData[i_ + 17]) << 1) + (((int)SegmentData[i_ + 18]) >> 7);
                                    DataVals[i + 5][DataPos] = ((((int)SegmentData[i_ + 18]) & 127) << 22) + (((int)SegmentData[i_ + 19]) << 14) + (((int)SegmentData[i_ + 20]) << 6) + (((int)SegmentData[i_ + 21]) >> 2);
                                    DataVals[i + 6][DataPos] = ((((int)SegmentData[i_ + 21]) & 3) << 27) + (((int)SegmentData[i_ + 22]) << 19) + (((int)SegmentData[i_ + 23]) << 11) + (((int)SegmentData[i_ + 24]) << 3) + (((int)SegmentData[i_ + 25]) >> 5);
                                    DataVals[i + 7][DataPos] = ((((int)SegmentData[i_ + 25]) & 31) << 24) + (((int)SegmentData[i_ + 26]) << 16) + (((int)SegmentData[i_ + 27]) << 8) + ((int)SegmentData[i_ + 28]);
                                    i_ += 29;
                                }
                            }
                            break;
                        case 30:
                            {
                                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i += 4)
                                {
                                    DataVals[i + 0][DataPos] = (((int)SegmentData[i_ + 0]) << 22) + (((int)SegmentData[i_ + 1]) << 14) + (((int)SegmentData[i_ + 2]) << 6) + (((int)SegmentData[i_ + 3]) >> 2);
                                    DataVals[i + 1][DataPos] = ((((int)SegmentData[i_ + 3]) & 3) << 28) + (((int)SegmentData[i_ + 4]) << 20) + (((int)SegmentData[i_ + 5]) << 12) + (((int)SegmentData[i_ + 6]) << 4) + (((int)SegmentData[i_ + 7]) >> 4);
                                    DataVals[i + 2][DataPos] = ((((int)SegmentData[i_ + 7]) & 15) << 26) + (((int)SegmentData[i_ + 8]) << 18) + (((int)SegmentData[i_ + 9]) << 10) + (((int)SegmentData[i_ + 10]) << 2) + (((int)SegmentData[i_ + 11]) >> 6);
                                    DataVals[i + 3][DataPos] = ((((int)SegmentData[i_ + 11]) & 63) << 24) + (((int)SegmentData[i_ + 12]) << 16) + (((int)SegmentData[i_ + 13]) << 8) + ((int)SegmentData[i_ + 14]);
                                    i_ += 15;
                                }
                            }
                            break;
                    }



                }
                else
                {
                    for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                    {
                        DataVals[i][DataPos] = 0;
                    }
                }
            }
            else
            {
                for (int i = DataOffset; i < DataValueSizeValuesDataOffset; i++)
                {
                    DataVals[i][DataPos] = 0;
                }
            }
        }
    }
}
