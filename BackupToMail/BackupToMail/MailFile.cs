﻿/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-06-07
 * Time: 15:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace BackupToMail
{
    /// <summary>
    /// One file consisting of data file and map file 
    /// </summary>
    public class MailFile
    {
        public const string DummyFileSign = "*";
        public const int DigestSize = 32;

        /// <summary>
        /// Dummy map contents used when map file name is null
        /// </summary>
        List<byte> MapDummy = new List<byte>();

        /// <summary>
        /// Mutex for data file
        /// </summary>
        object DataF_ = new object();
        
        /// <summary>
        /// Mutex for map file
        /// </summary>
        object MapF_ = new object();
        
        /// <summary>
        /// Segment size if known
        /// </summary>
        long SegmentSizeL = 0;
        
        /// <summary>
        /// Segment size if known
        /// </summary>
        int SegmentSize = 0;
        
        /// <summary>
        /// Segment count if known
        /// </summary>
        int SegmentCount = 0;
        
        /// <summary>
        /// Blank array used for extend data file if necessary
        /// </summary>
        byte[] Dummy;

        /// <summary>
        /// One-element array used to read or write byte from/to map file
        /// </summary>
        byte[] MapValue;
        
        /// <summary>
        /// Number of particular values
        /// </summary>
        int[] MapStats;
        
        public MailFile()
        {
            MapValue = new byte[1];
            MapStats = new int[3];
        }

        /// <summary>
        /// Data file is the digest file
        /// </summary>
        bool ParamDigestMode = false;

        /// <summary>
        /// Data file opened as read-only
        /// </summary>
        bool ParamDataRead = false;
        
        /// <summary>
        /// Data file name
        /// </summary>
        string ParamDataFile = "";
        
        /// <summary>
        /// Map file name
        /// </summary>
        string ParamMapFile = "";


        bool IsDummyFile = false;
        long DummyFileSize = 0;
        long FileSize__ = 0;

        public long GetDataSizeSeg()
        {
            return SegmentSizeL * (long)SegmentCount;
        }

        public long GetDataSize()
        {
            if (IsDummyFile)
            {
                return DummyFileSize;
            }
            else
            {
                return FileSize__;
            }
        }

        RandomSequence RandomSequence_;

        /// <summary>
        /// Open data file before operation
        /// </summary>
        /// <returns></returns>
        FileStream DataOpen()
        {
            if (ParamDataRead)
            {
                return new FileStream(ParamDataFile, FileMode.Open, FileAccess.Read);
            }
            else
            {
                if (File.Exists(ParamDataFile))
                {
                    return new FileStream(ParamDataFile, FileMode.Open, FileAccess.Write);
                }
                else
                {
                    return new FileStream(ParamDataFile, FileMode.Create, FileAccess.Write);
                }
            }
        }
        
        /// <summary>
        /// Open map file before operation
        /// </summary>
        /// <returns></returns>
        FileStream MapOpen()
        {
            if (File.Exists(ParamMapFile))
            {
                return new FileStream(ParamMapFile, FileMode.Open, FileAccess.ReadWrite);
            }
            else
            {
                return new FileStream(ParamMapFile, FileMode.Create, FileAccess.ReadWrite);
            }
        }

        public string OpenError = "";

        public long DigestFileSize = 0;
        public int DigestSegmentSize = 0;

        /// <summary>
        /// Open data/map file object with accessibility test before action 
        /// </summary>
        /// <param name="DigestMode"></param>
        /// <param name="DataRead"></param>
        /// <param name="DataFile"></param>
        /// <param name="MapFile"></param>
        /// <returns></returns>
        public bool Open(bool DigestMode, bool DataRead, string DataFile, string MapFile)
        {
            MapDummy.Clear();
            OpenError = "";
            ParamDigestMode = DigestMode;
            ParamDataRead = DataRead;
            ParamDataFile = DataFile;
            ParamMapFile = MapFile;
            SegmentSizeL = 0;
            SegmentSize = 0;
            SegmentCount = 0;
            DigestFileSize = 0;
            DigestSegmentSize = 0;
            try
            {
                if (DataFile != null)
                {
                    IsDummyFile = DataFile.StartsWith(DummyFileSign, StringComparison.InvariantCulture);
                    if (IsDummyFile)
                    {
                        RandomSequence_ = RandomSequence.CreateRS(DataFile.Substring(1), MailSegment.RandomCacheStep);
                        if (RandomSequence_ == null)
                        {
                            throw new Exception(RandomSequence.ErrorMsg);
                        }
                        DummyFileSize = RandomSequence.DummyFileSize;
                    }
                    else
                    {
                        FileStream Temp = DataOpen();
                        if (ParamDigestMode)
                        {
                            if (ParamDataRead)
                            {
                                if (Temp.Length < 32)
                                {
                                    if (ParamDataRead)
                                    {
                                        Temp.Close();
                                        OpenError = "Incorrect digest file";
                                        return false;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < DigestSize; i++)
                                        {
                                            Temp.WriteByte((byte)'_');
                                        }
                                        Temp.Seek(0, SeekOrigin.Begin);
                                    }
                                }
                                else
                                {
                                    byte[] Buf1 = new byte[16];
                                    byte[] Buf2 = new byte[16];
                                    Temp.Seek(0, SeekOrigin.Begin);
                                    Temp.Read(Buf1, 0, 16);
                                    Temp.Read(Buf2, 0, 16);
                                    Temp.Seek(0, SeekOrigin.Begin);
                                    try
                                    {
                                        DigestFileSize = long.Parse(MailSegment.BinToStr(Buf1), NumberStyles.HexNumber);
                                        DigestSegmentSize = int.Parse(MailSegment.BinToStr(Buf2), NumberStyles.HexNumber);
                                    }
                                    catch
                                    {
                                        Temp.Close();
                                        OpenError = "Incorrect digest file";
                                        return false;
                                    }
                                }
                            }
                        }
                        Temp.Close();
                    }
                }
            }
            catch (Exception e)
            {
                OpenError = e.Message;
                return false;
            }
            try
            {
                if (MapFile != null)
                {
                    FileStream Temp = MapOpen();
                    Temp.Close();
                }
            }
            catch (Exception e)
            {
                OpenError = e.Message;
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Calculate number of segments based on data file length and segment size
        /// </summary>
        /// <returns></returns>
        public int CalcSegmentCount()
        {
            if (SegmentSizeL == 0)
            {
                SegmentCount = 0;
                return 0;
            }
            Monitor.Enter(DataF_);
            long FileSize;
            if (IsDummyFile)
            {
                FileSize = DummyFileSize;
            }
            else
            {
                FileStream DataS = DataOpen();
                if (ParamDigestMode)
                {
                    byte[] Buf = new byte[16];
                    DataS.Read(Buf, 0, 16);
                    FileSize = long.Parse(MailSegment.BinToStr(Buf), NumberStyles.HexNumber);
                }
                else
                {
                    FileSize = DataS.Length;
                }
                DataS.Close();
            }
            FileSize__ = FileSize;
            SegmentCount = (int)(FileSize / SegmentSizeL);
            if ((FileSize % SegmentSizeL) > 0)
            {
                SegmentCount++;
            }
            Monitor.Exit(DataF_);
            return SegmentCount;
        }
        
        /// <summary>
        /// Get number of segments
        /// </summary>
        /// <returns></returns>
        public int GetSegmentCount()
        {
            return SegmentCount;
        }
        
        /// <summary>
        /// Set number of segments arbitrary
        /// </summary>
        /// <param name="SegmentCount_"></param>
        public void SetSegmentCount(int SegmentCount_)
        {
            SegmentCount = SegmentCount_;
        }
        
        /// <summary>
        /// Set segment size
        /// </summary>
        /// <param name="SegmentSize_"></param>
        public void SetSegmentSize(int SegmentSize_)
        {
            SegmentSizeL = SegmentSize_;
            SegmentSize = SegmentSize_;
            if (ParamDigestMode)
            {
                Dummy = new byte[DigestSize];
                for (int i = 0; i < DigestSize; i++)
                {
                    Dummy[i] = (byte)'_';
                }
            }
            else
            {
                Dummy = new byte[SegmentSize];
                for (int i = 0; i < SegmentSize; i++)
                {
                    Dummy[i] = 0;
                }
            }
        }
        
        /// <summary>
        /// Change every occurence of certain value to another value, change from 0 is not possible
        /// </summary>
        /// <param name="ValFrom"></param>
        /// <param name="ValTo"></param>
        public void MapChange(byte ValFrom, byte ValTo)
        {
            Monitor.Enter(MapF_);
            if (ParamMapFile != null)
            {
                FileStream MapS = MapOpen();
                MapS.Seek(0, SeekOrigin.Begin);
                int MapL = (int)MapS.Length;
                byte[] MapValues = new byte[MapL];
                MapS.Read(MapValues, 0, MapL);
                bool Changed = false;
                for (int i = 0; i < MapL; i++)
                {
                    if (MapValues[i] == (ValFrom + 48))
                    {
                        Changed = true;
                        MapValues[i] = (byte)(ValTo + 48);
                    }
                }
                if (Changed)
                {
                    MapS.Seek(0, SeekOrigin.Begin);
                    MapS.Write(MapValues, 0, MapL);
                }
                MapS.Close();
            }
            else
            {
                for (int i = 0; i < MapDummy.Count; i++)
                {
                    if (MapDummy[i] == ValFrom)
                    {
                        MapDummy[i] = ValTo;
                    }
                }
            }
            Monitor.Exit(MapF_);
        }

        /// <summary>
        /// Calculate statistics of map, it counts number of occurences of every value
        /// </summary>
        public void MapCalcStats()
        {
            Monitor.Enter(MapF_);
            MapStats[0] = 0;
            MapStats[1] = 0;
            MapStats[2] = 0;
            if (ParamMapFile != null)
            {
                FileStream MapS = MapOpen();
                MapS.Seek(0, SeekOrigin.Begin);
                int MapL = (int)MapS.Length;
                if (MapL > SegmentCount)
                {
                    MapL = SegmentCount;
                }
                byte[] MapValues = new byte[MapL];
                MapS.Read(MapValues, 0, MapL);
                for (int i = 0; i < MapL; i++)
                {
                    if ((MapValues[i] >= 48) && (MapValues[i] <= 50))
                    {
                        MapStats[(MapValues[i] - 48)]++;
                    }
                    else
                    {
                        MapStats[0]++;
                    }
                }
                if (SegmentCount > MapL)
                {
                    MapStats[0] = MapStats[0] + (SegmentCount - MapL);
                }
                MapS.Close();
            }
            else
            {
                int MapL = MapDummy.Count;
                if (MapL > SegmentCount)
                {
                    MapL = SegmentCount;
                }
                for (int i = 0; i < MapL; i++)
                {
                    if ((MapDummy[i] == 0) || (MapDummy[i] == 1) || (MapDummy[i] == 2))
                    {
                        MapStats[MapDummy[i]]++;
                    }
                    else
                    {
                        MapStats[0]++;
                    }
                }
                if (SegmentCount > MapL)
                {
                    MapStats[0] = MapStats[0] + (SegmentCount - MapL);
                }
            }
            Monitor.Exit(MapF_);
        }
        
        /// <summary>
        /// Get number of occurences of certain value, calculated by CalcMapStats()
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public int MapCount(byte Value)
        {
            return MapStats[Value];
        }
        
        /// <summary>
        /// Get map value of segment
        /// </summary>
        /// <param name="SegmentNo"></param>
        /// <returns></returns>
        public byte MapGet(int SegmentNo)
        {
            Monitor.Enter(MapF_);
            if (ParamMapFile != null)
            {
                if (File.Exists(ParamMapFile))
                {
                    FileStream MapS = MapOpen();
                    if (MapS.Length <= SegmentNo)
                    {
                        MapS.Close();
                        Monitor.Exit(MapF_);
                        return 0;
                    }
                    else
                    {
                        MapS.Seek(SegmentNo, SeekOrigin.Begin);
                        MapS.Read(MapValue, 0, 1);
                        byte Val = 0;
                        if ((MapValue[0] >= 48) && (MapValue[0] <= 50))
                        {
                            Val = (byte)(MapValue[0] - 48);
                        }
                        MapS.Close();
                        Monitor.Exit(MapF_);
                        return Val;
                    }
                }
                else
                {
                    Monitor.Exit(MapF_);
                    return 0;
                }
            }
            else
            {
                if (MapDummy.Count > SegmentNo)
                {
                    byte X = MapDummy[SegmentNo];
                    Monitor.Exit(MapF_);
                    return X;
                }
                else
                {
                    Monitor.Exit(MapF_);
                    return 0;
                }
            }
        }

        /// <summary>
        /// Set map value of segment
        /// </summary>
        /// <param name="SegmentNo"></param>
        /// <param name="SegmentValue"></param>
        public void MapSet(int SegmentNo, byte SegmentValue)
        {
            Monitor.Enter(MapF_);
            if (ParamMapFile != null)
            {
                FileStream MapS = MapOpen();
                if (MapS.Length < SegmentNo)
                {
                    MapValue[0] = 48;
                    MapS.Seek(0, SeekOrigin.End);
                    while (MapS.Length < SegmentNo)
                    {
                        MapS.Write(MapValue, 0, 1);
                    }
                }
                MapValue[0] = (byte)(SegmentValue + 48);
                MapS.Seek(SegmentNo, SeekOrigin.Begin);
                MapS.Write(MapValue, 0, 1);
                MapS.Close();
            }
            else
            {
                while (MapDummy.Count <= SegmentNo)
                {
                    MapDummy.Add(0);
                }
                MapDummy[SegmentNo] = SegmentValue;
            }
            Monitor.Exit(MapF_);
        }
        
        /// <summary>
        /// Read segment from data file
        /// </summary>
        /// <param name="SegmentNo"></param>
        /// <returns></returns>
        public byte[] DataGet(int SegmentNo)
        {
            Monitor.Enter(DataF_);
            byte[] SegmentData;
            if (IsDummyFile)
            {
                long SegmentOffset = SegmentNo;
                SegmentOffset = SegmentOffset * SegmentSize;
                long SegmentSize_ = DummyFileSize - SegmentOffset;
                if (SegmentSize_ > SegmentSize)
                {
                    SegmentSize_ = SegmentSize;
                }
                SegmentData = RandomSequence_.GenSeq(SegmentOffset, SegmentSize_);
                if (ParamDigestMode)
                {
                    SegmentData = MailSegment.StrToBin(MailSegment.Digest(SegmentData));
                }
            }
            else
            {
                FileStream DataS = DataOpen();
                long SegmentOffset = SegmentNo;
                if (ParamDigestMode)
                {
                    SegmentOffset = (SegmentOffset + 1) * DigestSize;
                    SegmentData = new byte[DigestSize];
                    for (int i = 0; i < DigestSize; i++)
                    {
                        SegmentData[i] = (byte)'_';
                    }
                    if ((SegmentOffset + DigestSize) <= DataS.Length)
                    {
                        DataS.Seek(SegmentOffset, SeekOrigin.Begin);
                        DataS.Read(SegmentData, 0, DigestSize);
                    }
                }
                else
                {
                    SegmentOffset = SegmentOffset * SegmentSize;
                    long SegmentSize_ = DataS.Length - SegmentOffset;
                    if (SegmentSize_ > SegmentSize)
                    {
                        SegmentSize_ = SegmentSize;
                    }
                    SegmentData = new byte[SegmentSize_];
                    DataS.Seek(SegmentOffset, SeekOrigin.Begin);
                    DataS.Read(SegmentData, 0, (int)SegmentSize_);
                }
                DataS.Close();
            }
            Monitor.Exit(DataF_);
            return SegmentData;
        }
        
        /// <summary>
        /// Write segment to data file
        /// </summary>
        /// <param name="SegmentNo"></param>
        /// <param name="SegmentData"></param>
        /// <param name="SegmentDataLength"></param>
        public void DataSet(int SegmentNo, byte[] SegmentData, int SegmentDataLength)
        {
            Monitor.Enter(DataF_);
            if (!IsDummyFile)
            {
                FileStream DataS = DataOpen();
                long SegmentOffset = SegmentNo;
                if (ParamDigestMode)
                {
                    SegmentOffset = (SegmentOffset + 1) * DigestSize;
                    long FileSize___ = DataS.Length;
                    if (DataS.Length < SegmentOffset)
                    {
                        DataS.Seek(0, SeekOrigin.End);
                        while (DataS.Length < SegmentOffset)
                        {
                            DataS.Write(Dummy, 0, DigestSize);
                        }
                    }
                    DataS.Seek(SegmentOffset, SeekOrigin.Begin);
                    DataS.Write(MailSegment.DigestBin(SegmentData), 0, DigestSize);

                    if (FileSize___ != DataS.Length)
                    {
                        SegmentOffset = SegmentNo;
                        FileSize__ = (SegmentOffset * SegmentSizeL) + SegmentData.LongLength;
                        DataS.Seek(0, SeekOrigin.Begin);
                        SegmentData = MailSegment.StrToBin(FileSize__.ToString("X").PadLeft(16, '0'));
                        DataS.Write(SegmentData, 0, 16);
                        SegmentData = MailSegment.StrToBin(SegmentSize.ToString("X").PadLeft(16, '0'));
                        DataS.Write(SegmentData, 0, 16);
                    }
                }
                else
                {
                    SegmentOffset = SegmentOffset * SegmentSize;
                    if (DataS.Length < SegmentOffset)
                    {
                        DataS.Seek(0, SeekOrigin.End);
                        while (DataS.Length < SegmentOffset)
                        {
                            DataS.Write(Dummy, 0, SegmentSize);
                        }
                    }
                    if (SegmentCount <= SegmentNo)
                    {
                        SegmentCount = SegmentNo + 1;
                    }
                    DataS.Seek(SegmentOffset, SeekOrigin.Begin);
                    DataS.Write(SegmentData, 0, SegmentDataLength);
                    FileSize__ = DataS.Length;
                }
                DataS.Close();
            }
            Monitor.Exit(DataF_);
        }
        
        /// <summary>
        /// Close data/map file object after action
        /// </summary>
        public void Close()
        {
            ParamDataRead = false;
            ParamDataFile = "";
            ParamMapFile = "";
        }
    }
}
