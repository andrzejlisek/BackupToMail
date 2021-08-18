using System;
using System.Collections.Generic;
using System.Threading;

namespace BackupToMail
{
    public class CodeReedSolomon
    {
        public CodeReedSolomon()
        {
        }

        int ReedSolomonFileThreads = 1;
        int ReedSolomonComputeThreads = 1;
        int ReedSolomonValuesPerThread = 10000;


        int WorkPoolSizeI_Units;
        int WorkPoolSegPerUnit;
        int WorkPoolSizeI;
        long WorkPoolSizeL;
        int[][] WorkPoolValueSerie;
        int[][] WorkPoolValueSerie_;
        STH1123.ReedSolomon.ReedSolomonEncoder[] WorkPoolRSE;
        STH1123.ReedSolomon.ReedSolomonDecoder[] WorkPoolRSD;
        Thread[] WorkPoolThread;
        int WorkPoolDataSegments;
        int WorkPoolCodeSegments;
        int WorkPoolDataUnits;
        int WorkPoolCodeUnits;
        int[][] WorkPoolErasureArray;

        Thread[] WorkFileThread;
        int[] WorkFileData1;
        int[] WorkFileData2;
        int[] WorkFileCode1;
        int[] WorkFileCode2;
        MailFile WorkPool_MF;
        MailFile WorkPool_RS;

        long WorkPoolDecodeValsTrueNotChanged = 0;
        long WorkPoolDecodeValsTrueChangedData = 0;
        long WorkPoolDecodeValsTrueChangedCode = 0;
        long WorkPoolDecodeValsTrueChangedDataCode = 0;
        long WorkPoolDecodeValsFalse = 0;
        byte[][] WorkPoolSegmentMap;
        byte[][] WorkPoolSegmentMod;
        byte[][] WorkPoolSegmentMod_;
        bool WorkPoolAllowOutsideMap;
        bool WorkPoolAllowModify;

        void ThreadDataReadCodeClear(int ThrNum)
        {
            for (int i_ = 0; i_ < WorkPoolSegPerUnit; i_++)
            {
                for (int ii = WorkFileData1[ThrNum]; ii < WorkFileData2[ThrNum]; ii++)
                {
                    WorkPool_MF.DataValueGet(ii * WorkPoolSegPerUnit + i_, ref WorkPoolValueSerie, WorkPoolSizeI * i_, ii);
                }
            }
            for (int ii = WorkFileCode1[ThrNum]; ii < WorkFileCode2[ThrNum]; ii++)
            {
                for (int i_ = 0; i_ < WorkPoolSizeI_Units; i_++)
                {
                    WorkPoolValueSerie[i_][ii + WorkPoolDataUnits] = 0;
                }
            }
        }

        void ThreadDataReadCodeRead(int ThrNum)
        {
            for (int i_ = 0; i_ < WorkPoolSegPerUnit; i_++)
            {
                for (int ii = WorkFileData1[ThrNum]; ii < WorkFileData2[ThrNum]; ii++)
                {
                    WorkPool_MF.DataValueGet(ii * WorkPoolSegPerUnit + i_, ref WorkPoolValueSerie, WorkPoolSizeI * i_, ii);
                }
                for (int ii = WorkFileCode1[ThrNum]; ii < WorkFileCode2[ThrNum]; ii++)
                {
                    WorkPool_RS.DataValueGet(ii * WorkPoolSegPerUnit + i_, ref WorkPoolValueSerie, WorkPoolSizeI * i_, ii + WorkPoolDataUnits);
                }
            }
            for (int ii = WorkFileData1[ThrNum]; ii < WorkFileData2[ThrNum]; ii++)
            {
                for (int i_ = 0; i_ < WorkPoolSizeI_Units; i_++)
                {
                    WorkPoolValueSerie_[i_][ii] = WorkPoolValueSerie[i_][ii];
                }
            }
            for (int ii = WorkFileCode1[ThrNum]; ii < WorkFileCode2[ThrNum]; ii++)
            {
                for (int i_ = 0; i_ < WorkPoolSizeI_Units; i_++)
                {
                    WorkPoolValueSerie_[i_][ii + WorkPoolDataUnits] = WorkPoolValueSerie[i_][ii + WorkPoolDataUnits];
                }
            }
        }

        void ThreadCodeWrite(int ThrNum)
        {
            for (int i_ = 0; i_ < WorkPoolSegPerUnit; i_++)
            {
                for (int ii = WorkFileCode1[ThrNum]; ii < WorkFileCode2[ThrNum]; ii++)
                {
                    WorkPool_RS.DataValueSet(ii * WorkPoolSegPerUnit + i_, ref WorkPoolValueSerie, WorkPoolSizeI * i_, ii + WorkPoolDataUnits);
                }
            }
        }

        void ThreadDataWriteCodeWrite(int ThrNum)
        {
            for (int i_ = 0; i_ < WorkPoolSegPerUnit; i_++)
            {
                for (int ii = WorkFileData1[ThrNum]; ii < WorkFileData2[ThrNum]; ii++)
                {
                    if (WorkPoolSegmentMod_[i_][ii] == 1)
                    {
                        WorkPool_MF.DataValueSet(ii * WorkPoolSegPerUnit + i_, ref WorkPoolValueSerie, WorkPoolSizeI * i_, ii);
                    }
                }
                for (int ii = WorkFileCode1[ThrNum]; ii < WorkFileCode2[ThrNum]; ii++)
                {
                    if (WorkPoolSegmentMod_[i_][ii + WorkPoolDataUnits] == 1)
                    {
                        WorkPool_RS.DataValueSet(ii * WorkPoolSegPerUnit + i_, ref WorkPoolValueSerie, WorkPoolSizeI * i_, ii + WorkPoolDataUnits);
                    }
                }
            }
        }


        void PrepareFileThreads(MailFile WorkPool_MF_, MailFile WorkPool_RS_)
        {
            WorkPool_MF = WorkPool_MF_;
            WorkPool_RS = WorkPool_RS_;
            WorkFileThread = new Thread[ReedSolomonFileThreads];
            int DataFileSize_Thr = WorkPoolDataUnits / ReedSolomonFileThreads;
            if ((WorkPoolDataUnits % ReedSolomonFileThreads) > 0)
            {
                DataFileSize_Thr++;
            }
            int CodeFileSize_Thr = WorkPoolCodeUnits / ReedSolomonFileThreads;
            if ((WorkPoolCodeUnits % ReedSolomonFileThreads) > 0)
            {
                CodeFileSize_Thr++;
            }
            WorkFileData1 = new int[ReedSolomonFileThreads];
            WorkFileData2 = new int[ReedSolomonFileThreads];
            WorkFileCode1 = new int[ReedSolomonFileThreads];
            WorkFileCode2 = new int[ReedSolomonFileThreads];
            for (int i = 0; i < ReedSolomonFileThreads; i++)
            {
                WorkFileData1[i] = i * DataFileSize_Thr;
                WorkFileData2[i] = (i + 1) * DataFileSize_Thr;
                if (WorkFileData2[i] > WorkPoolDataUnits)
                {
                    WorkFileData2[i] = WorkPoolDataUnits;
                }
                WorkFileCode1[i] = i * CodeFileSize_Thr;
                WorkFileCode2[i] = (i + 1) * CodeFileSize_Thr;
                if (WorkFileCode2[i] > WorkPoolCodeUnits)
                {
                    WorkFileCode2[i] = WorkPoolCodeUnits;
                }
            }
        }

        void WorkPoolEncode(int ThrNum)
        {
            int PoolOffset = ThrNum * ReedSolomonValuesPerThread;
            for (int i_ = 0; i_ < WorkPoolSegPerUnit; i_++)
            {
                for (int i = 0; i < ReedSolomonValuesPerThread; i++)
                {
                    WorkPoolRSE[PoolOffset + i].Encode(WorkPoolValueSerie[PoolOffset + i], WorkPoolCodeUnits);
                }
                PoolOffset += WorkPoolSizeI;
            }
        }

        object WorkPoolMutex = new object();

        void WorkPoolDecode(int ThrNum)
        {
            int PoolOffset = ThrNum * ReedSolomonValuesPerThread;
            for (int i_ = 0; i_ < WorkPoolSegPerUnit; i_++)
            {
                for (int i = 0; i < ReedSolomonValuesPerThread; i++)
                {
                    if (WorkPoolRSD[PoolOffset + i].Decode(WorkPoolValueSerie[PoolOffset + i], WorkPoolCodeUnits, WorkPoolErasureArray[i_]))
                    {
                        bool ChangedData = false;
                        bool ChangedCode = false;
                        for (int ii = 0; ii < WorkPoolDataUnits; ii++)
                        {
                            if (WorkPoolValueSerie[PoolOffset + i][ii] != WorkPoolValueSerie_[PoolOffset + i][ii])
                            {
                                ChangedData = true;
                                Monitor.Enter(WorkPoolMutex);
                                if (WorkPoolSegmentMod[i_][ii] != 3)
                                {
                                    if (WorkPoolAllowModify && (WorkPoolAllowOutsideMap || (WorkPoolSegmentMap[i_][ii] == 0)))
                                    {
                                        WorkPoolSegmentMod[i_][ii] = 1;
                                        WorkPoolSegmentMod_[i_][ii] = 1;
                                    }
                                    else
                                    {
                                        WorkPoolSegmentMod[i_][ii] = 2;
                                    }
                                }
                                Monitor.Exit(WorkPoolMutex);
                            }
                        }
                        for (int ii = 0; ii < WorkPoolCodeUnits; ii++)
                        {
                            if (WorkPoolValueSerie[PoolOffset + i][ii + WorkPoolDataUnits] != WorkPoolValueSerie_[PoolOffset + i][ii + WorkPoolDataUnits])
                            {
                                ChangedCode = true;
                                Monitor.Enter(WorkPoolMutex);
                                if (WorkPoolSegmentMod[i_][ii + WorkPoolDataUnits] != 3)
                                {
                                    if (WorkPoolAllowModify && (WorkPoolAllowOutsideMap || (WorkPoolSegmentMap[i_][ii + WorkPoolDataUnits] == 0)))
                                    {
                                        WorkPoolSegmentMod[i_][ii + WorkPoolDataUnits] = 1;
                                        WorkPoolSegmentMod_[i_][ii + WorkPoolDataUnits] = 1;
                                    }
                                    else
                                    {
                                        WorkPoolSegmentMod[i_][ii + WorkPoolDataUnits] = 2;
                                    }
                                }
                                Monitor.Exit(WorkPoolMutex);
                            }
                        }

                        Monitor.Enter(WorkPoolMutex);
                        if (ChangedData)
                        {
                            if (ChangedCode)
                            {
                                WorkPoolDecodeValsTrueChangedDataCode++;
                            }
                            else
                            {
                                WorkPoolDecodeValsTrueChangedData++;
                            }
                        }
                        else
                        {
                            if (ChangedCode)
                            {
                                WorkPoolDecodeValsTrueChangedCode++;
                            }
                            else
                            {
                                WorkPoolDecodeValsTrueNotChanged++;
                            }
                        }
                        Monitor.Exit(WorkPoolMutex);
                    }
                    else
                    {
                        Monitor.Enter(WorkPoolMutex);
                        WorkPoolDecodeValsFalse++;
                        Monitor.Exit(WorkPoolMutex);
                    }
                }
                PoolOffset += WorkPoolSizeI;
            }
        }

        public int PolynomialNumber = 0;
        public int NumberOfBits = 0;

        public void SetPolynomialNumber(int PolynomialNumber_)
        {
            PolynomialNumber = PolynomialNumber_;
            if (PolynomialNumber < 4)
            {
                PolynomialNumber = 0;
            }
            switch (PolynomialNumber)
            {
                case 4: PolynomialNumber = 4 + 2 + 1; break;
                case 8: PolynomialNumber = 8 + 2 + 1; break;
                case 16: PolynomialNumber = 16 + 2 + 1; break;
                case 32: PolynomialNumber = 32 + 4 + 1; break;
                case 64: PolynomialNumber = 64 + 2 + 1; break;
                case 128: PolynomialNumber = 128 + 2 + 1; break;
                case 256: PolynomialNumber = 256 + 16 + 8 + 4 + 1; break;
                case 512: PolynomialNumber = 512 + 16 + 1; break;
                case 1024: PolynomialNumber = 1024 + 8 + 1; break;
                case 2048: PolynomialNumber = 2048 + 4 + 1; break;
                case 4096: PolynomialNumber = 4096 + 64 + 16 + 2 + 1; break;
                case 8192: PolynomialNumber = 8192 + 16 + 8 + 2 + 1; break;
                case 16384: PolynomialNumber = 16384 + 32 + 8 + 2 + 1; break;
                case 32768: PolynomialNumber = 32768 + 2 + 1; break;
                case 65536: PolynomialNumber = 65536 + 32 + 8 + 4 + 1; break;
                case 131072: PolynomialNumber = 131072 + 8 + 1; break;
                case 262144: PolynomialNumber = 262144 + 32 + 4 + 2 + 1; break;
                case 524288: PolynomialNumber = 524288 + 32 + 4 + 2 + 1; break;
                case 1048576: PolynomialNumber = 1048576 + 8 + 1; break;
                case 2097152: PolynomialNumber = 2097152 + 4 + 1; break;
                case 4194304: PolynomialNumber = 4194304 + 2 + 1; break;
                case 8388608: PolynomialNumber = 8388608 + 32 + 1; break;
                case 16777216: PolynomialNumber = 16777216 + 16 + 8 + 2 + 1; break;
                case 33554432: PolynomialNumber = 33554432 + 8 + 1; break;
                case 67108864: PolynomialNumber = 67108864 + 64 + 4 + 2 + 1; break;
                case 134217728: PolynomialNumber = 134217728 + 32 + 4 + 2 + 1; break;
                case 268435456: PolynomialNumber = 268435456 + 8 + 1; break;
                case 536870912: PolynomialNumber = 536870912 + 4 + 1; break;
                case 1073741824: PolynomialNumber = 1073741824 + 64 + 16 + 2 + 1; break;
            }

            if ((PolynomialNumber >= 4) && (PolynomialNumber < 8)) { NumberOfBits = 2; }
            if ((PolynomialNumber >= 8) && (PolynomialNumber < 16)) { NumberOfBits = 3; }
            if ((PolynomialNumber >= 16) && (PolynomialNumber < 32)) { NumberOfBits = 4; }
            if ((PolynomialNumber >= 32) && (PolynomialNumber < 64)) { NumberOfBits = 5; }
            if ((PolynomialNumber >= 64) && (PolynomialNumber < 128)) { NumberOfBits = 6; }
            if ((PolynomialNumber >= 128) && (PolynomialNumber < 256)) { NumberOfBits = 7; }
            if ((PolynomialNumber >= 256) && (PolynomialNumber < 512)) { NumberOfBits = 8; }
            if ((PolynomialNumber >= 512) && (PolynomialNumber < 1024)) { NumberOfBits = 9; }
            if ((PolynomialNumber >= 1024) && (PolynomialNumber < 2048)) { NumberOfBits = 10; }
            if ((PolynomialNumber >= 2048) && (PolynomialNumber < 4096)) { NumberOfBits = 11; }
            if ((PolynomialNumber >= 4096) && (PolynomialNumber < 8192)) { NumberOfBits = 12; }
            if ((PolynomialNumber >= 8192) && (PolynomialNumber < 16384)) { NumberOfBits = 13; }
            if ((PolynomialNumber >= 16384) && (PolynomialNumber < 32768)) { NumberOfBits = 14; }
            if ((PolynomialNumber >= 32768) && (PolynomialNumber < 65536)) { NumberOfBits = 15; }
            if ((PolynomialNumber >= 65536) && (PolynomialNumber < 131072)) { NumberOfBits = 16; }
            if ((PolynomialNumber >= 131072) && (PolynomialNumber < 262144)) { NumberOfBits = 17; }
            if ((PolynomialNumber >= 262144) && (PolynomialNumber < 524288)) { NumberOfBits = 18; }
            if ((PolynomialNumber >= 524288) && (PolynomialNumber < 1048576)) { NumberOfBits = 19; }
            if ((PolynomialNumber >= 1048576) && (PolynomialNumber < 2097152)) { NumberOfBits = 20; }
            if ((PolynomialNumber >= 2097152) && (PolynomialNumber < 4194304)) { NumberOfBits = 21; }
            if ((PolynomialNumber >= 4194304) && (PolynomialNumber < 8388608)) { NumberOfBits = 22; }
            if ((PolynomialNumber >= 8388608) && (PolynomialNumber < 16777216)) { NumberOfBits = 23; }
            if ((PolynomialNumber >= 16777216) && (PolynomialNumber < 33554432)) { NumberOfBits = 24; }
            if ((PolynomialNumber >= 33554432) && (PolynomialNumber < 67108864)) { NumberOfBits = 25; }
            if ((PolynomialNumber >= 67108864) && (PolynomialNumber < 134217728)) { NumberOfBits = 26; }
            if ((PolynomialNumber >= 134217728) && (PolynomialNumber < 268435456)) { NumberOfBits = 27; }
            if ((PolynomialNumber >= 268435456) && (PolynomialNumber < 536870912)) { NumberOfBits = 28; }
            if ((PolynomialNumber >= 536870912) && (PolynomialNumber < 1073741824)) { NumberOfBits = 29; }
            if ((PolynomialNumber >= 1073741824)) { NumberOfBits = 30; }
        }

        public static int BinToNum(string Bin)
        {
            int Num = 0;
            int T = 1;
            int I = Bin.Length - 1;
            while (I >= 0)
            {
                if (Bin[I] == '1')
                {
                    Num = Num + T;
                }
                T = T << 1;
                I--;
            }
            return Num;
        }

        public static string NumToBin(long Num, int Bits)
        {
            return NumToBin((int)Num, Bits);
        }

        public static string NumToBin(int Num, int Bits)
        {
            string Bin = "";
            int T = 1073741824;
            while (T > 0)
            {
                if (Num >= T) { Bin += "1"; Num = Num - T; } else { Bin += "0"; }
                T = T >> 1;
            }
            if (Bin.Substring(0, Bin.Length - Bits).Contains("1"))
            {
                return ("".PadLeft(Bits, '!'));
            }
            return Bin.Substring(Bin.Length - Bits);
        }

        private void SimulateClear(string Prefix1, string Prefix2, string FileName, string MapName, int SegmentSize, int SizeMode)
        {
            if ((FileName != null) && (MapName != null))
            {
                if ((FileName != "") && (MapName != ""))
                {
                    MailSegment.Console_WriteLine("Processing " + Prefix2 + " file");
                    MailFile MF = new MailFile();
                    if (MF.Open(false, false, FileName, MapName))
                    {
                        Stopwatch_ SWProgress = new Stopwatch_();
                        long SWWorkTime = 0;
                        SWProgress.Reset();

                        MF.SetSegmentSize(SegmentSize);
                        int XSegments = MF.CalcSegmentCount();
                        int LastGoodSegment = 0;
                        for (int i = 0; i < XSegments; i++)
                        {
                            if (MF.MapGet(i) == 0)
                            {
                                MF.DataSet(i, null, SegmentSize);
                            }
                            else
                            {
                                LastGoodSegment = i;
                            }

                            if (SWProgress.ProgressTriggeringValue(ref SWWorkTime))
                            {
                                MailSegment.Console_WriteLine(Prefix1 + " file progress: " + (i + 1) + "/" + XSegments + " (" + (i * 100 / XSegments) + "%)");
                            }
                        }
                        MailSegment.Console_WriteLine(Prefix1 + " file progress: " + XSegments + "/" + XSegments + " (100%)");
                        MailSegment.Console_WriteLine("");

                        if (LastGoodSegment < (XSegments - 1))
                        {
                            if (SizeMode == 1)
                            {
                                MailSegment.Console_WriteLine(Prefix1 + " file resizing started");
                                long NewSize = ((long)XSegments - 1L) * (long)SegmentSize + 1L;
                                MF.ResizeData(NewSize);
                                MailSegment.Console_WriteLine(Prefix1 + " file resizing finished");
                                MailSegment.Console_WriteLine("");
                            }
                            if (SizeMode == 2)
                            {
                                MailSegment.Console_WriteLine(Prefix1 + " file resizing started");
                                long NewSize = ((long)LastGoodSegment + 1L) * (long)SegmentSize;
                                MF.ResizeData(NewSize);
                                MailSegment.Console_WriteLine(Prefix1 + " file resizing finished");
                                MailSegment.Console_WriteLine("");
                            }
                        }

                        MailSegment.Console_WriteLine(Prefix1 + " file processed");
                    }
                    else
                    {
                        MailSegment.Console_WriteLine(Prefix1 + " file open error: " + MF.OpenError);
                    }
                    return;
                }
            }
            MailSegment.Console_WriteLine(Prefix1 + " file not specified");
        }

        private void ResizeFile(string Prefix1, string Prefix2, string FileName, string MapName)
        {
            if ((FileName != null) && (MapName != null))
            {
                if ((FileName != "") && (MapName != ""))
                {
                    long FileSize = long.Parse(MapName);
                    if (FileSize > 0)
                    {
                        MailFile MF = new MailFile();
                        if (MF.Open(false, false, FileName, null))
                        {
                            MailSegment.Console_WriteLine(Prefix1 + " file resizing started");
                            MF.ResizeData(FileSize);
                            MailSegment.Console_WriteLine(Prefix1 + " file resizing finished");
                        }
                        else
                        {
                            MailSegment.Console_WriteLine(Prefix1 + " file open error: " + MF.OpenError);
                        }
                        return;
                    }
                    else
                    {
                        MailSegment.Console_WriteLine(Prefix1 + " - invalid desired file size");
                        return;
                    }
                }
            }
            MailSegment.Console_WriteLine(Prefix1 + " file not specified");
        }

        public void Proc(int Mode, string DataFile, string DataMapFile, string CodeFile, string CodeMapFile, int CodeUnits, int SegPerUnit, int SegmentSize, bool PromptConfirm)
        {
            ReedSolomonFileThreads = MailSegment.ReedSolomonFileThreads;
            ReedSolomonComputeThreads = MailSegment.ReedSolomonComputeThreads;
            ReedSolomonValuesPerThread = MailSegment.ReedSolomonValuesPerThread;


            // Resize data and map files
            if (Mode == 8)
            {
                ResizeFile("Data", "data", DataFile, DataMapFile);
                MailSegment.Console_WriteLine("");
                ResizeFile("Code", "code", CodeFile, CodeMapFile);
                MailSegment.Console_WriteLine("");
            }

            // Clear some segments according the map file - simulate incomplete download
            if (Mode == 9)
            {
                SimulateClear("Data", "data", DataFile, DataMapFile, SegmentSize, SegPerUnit);
                MailSegment.Console_WriteLine("");
                SimulateClear("Code", "code", CodeFile, CodeMapFile, SegmentSize, SegPerUnit);
                MailSegment.Console_WriteLine("");
            }

            // Creating RS code or recovering data file
            if ((Mode >= 0) && (Mode <= 7))
            {
                MailFile MF = new MailFile();
                MailFile RS = new MailFile();

                if (MF.Open(false, (Mode == 0) || (Mode == 7), DataFile, DataMapFile))
                {
                    if (RS.Open(false, (Mode == 7), CodeFile, CodeMapFile))
                    {
                        Stopwatch_ TSW = new Stopwatch_();
                        MailSegment.LogReset();


                        // Set desired segment size for both data and code files
                        MF.SetSegmentSize(SegmentSize);
                        RS.SetSegmentSize(SegmentSize);

                        int DataSegments = MF.CalcSegmentCount();
                        int DataSegmentsReal = DataSegments;
                        int CodeSegments = CodeUnits * SegPerUnit;
                        if ((Mode >= 1) && (Mode <= 7))
                        {
                            CodeSegments = RS.CalcSegmentCount();
                            if ((CodeSegments % SegPerUnit) != 0)
                            {
                                CodeSegments = CodeSegments / SegPerUnit;
                                CodeSegments = CodeSegments * SegPerUnit;
                                CodeSegments++;
                            }
                        }
                        int DataUnits = DataSegments / SegPerUnit;
                        if ((DataSegments % SegPerUnit) > 0)
                        {
                            DataUnits++;
                            DataSegments = SegPerUnit * DataUnits;
                        }

                        long SegmentSizeL = SegmentSize;
                        SegmentSizeL = SegmentSizeL * 8;

                        int TotalSegments = DataSegments + CodeSegments;
                        int TotalUnits = DataUnits + CodeUnits;

                        MailSegment.ConsoleLineToLog = true;
                        MailSegment.ConsoleLineToLogSum = true;
                        MailSegment.Console_WriteLine("");
                        MailSegment.Console_WriteLine("Real data segments: " + DataSegmentsReal);
                        MailSegment.Console_WriteLine("Data segments: " + DataSegments);
                        MailSegment.Console_WriteLine("Code segments: " + CodeSegments);
                        MailSegment.Console_WriteLine("All segments: " + TotalSegments);
                        MailSegment.Console_WriteLine("Data units: " + DataUnits);
                        MailSegment.Console_WriteLine("Code units: " + CodeUnits);
                        MailSegment.Console_WriteLine("All units: " + TotalUnits);

                        bool MessageSizeFound = true;

                        if (PolynomialNumber < 4)
                        {
                            int PolyBase = 4;
                            NumberOfBits = 2;

                            while (MessageSizeFound && ((TotalUnits > (PolyBase - 1)) || ((SegmentSizeL % (long)NumberOfBits) != 0)))
                            {
                                if (PolyBase >= 1073741824)
                                {
                                    MessageSizeFound = false;
                                }
                                else
                                {
                                    PolyBase = PolyBase << 1;
                                    SetPolynomialNumber(PolyBase);
                                }
                            }
                        }

                        if (MessageSizeFound)
                        {
                            MailSegment.Console_WriteLine("Bits per value: " + NumberOfBits);
                            MailSegment.Console_WriteLine("Primitive polynomial: " + PolynomialNumber);
                        }
                        MailSegment.ConsoleLineToLogSum = false;
                        MailSegment.ConsoleLineToLog = false;

                        if (MessageSizeFound && ((SegmentSizeL % (long)NumberOfBits) == 0) && (TotalUnits <= ((1 << NumberOfBits) - 1)))
                        {
                            long ValuesPerSegment = (long)SegmentSize * 8L / (long)NumberOfBits;

                            MailSegment.ConsoleLineToLog = true;
                            MailSegment.ConsoleLineToLogSum = true;
                            MailSegment.Console_WriteLine("Values per segment: " + ValuesPerSegment);

                            WorkPoolSizeI = ReedSolomonComputeThreads * ReedSolomonValuesPerThread;
                            WorkPoolSizeL = ReedSolomonComputeThreads * ReedSolomonValuesPerThread;
                            WorkPoolSegPerUnit = SegPerUnit;
                            WorkPoolSizeI_Units = WorkPoolSizeI * SegPerUnit;
                            long PoolSizeBytes = MF.DataValueParamsCalc(NumberOfBits, WorkPoolSizeL);
                            RS.DataValueParamsCalc(NumberOfBits, WorkPoolSizeL);
                            MailSegment.Console_WriteLine("Bits per segment in work pool: " + (WorkPoolSizeL * (long)NumberOfBits));
                            MailSegment.Console_WriteLine("Values per segment in work pool: " + WorkPoolSizeL);
                            if ((PoolSizeBytes > 0) && (PoolSizeBytes <= ((long)SegmentSize)))
                            {
                                MailSegment.Console_WriteLine("Bytes per segment in work pool: " + PoolSizeBytes);

                                // Create work pool
                                WorkPoolRSE = new STH1123.ReedSolomon.ReedSolomonEncoder[WorkPoolSizeI_Units];
                                WorkPoolRSD = new STH1123.ReedSolomon.ReedSolomonDecoder[WorkPoolSizeI_Units];
                                WorkPoolValueSerie = new int[WorkPoolSizeI_Units][];
                                WorkPoolValueSerie_ = new int[WorkPoolSizeI_Units][];
                                WorkPoolThread = new Thread[ReedSolomonComputeThreads];
                                WorkPoolDataSegments = DataSegments;
                                WorkPoolCodeSegments = CodeSegments;
                                WorkPoolDataUnits = DataUnits;
                                WorkPoolCodeUnits = CodeUnits;
                                WorkPoolErasureArray = new int[SegPerUnit][];
                                for (int i = 0; i < WorkPoolSizeI_Units; i++)
                                {
                                    WorkPoolValueSerie[i] = new int[TotalUnits];
                                    WorkPoolValueSerie_[i] = new int[TotalUnits];
                                }

                                MailSegment.ConsoleLineToLogSum = false;
                                MailSegment.ConsoleLineToLog = false;
                                MailSegment.Console_WriteLine("");

                                if (Program.PromptConfirm(PromptConfirm))
                                {

                                    Stopwatch_ SWProgress = new Stopwatch_();
                                    long SWWorkTime = 0;
                                    SWProgress.Reset();

                                    // Prepare blank code file
                                    if (Mode == 0)
                                    {
                                        MailSegment.Console_WriteLine("Preparing code file");

                                        // Write blank code segments
                                        byte[] BlankSegment = new byte[SegmentSize];
                                        for (int i = 0; i < SegmentSize; i++)
                                        {
                                            BlankSegment[i] = 0;
                                        }
                                        for (int i = 0; i < CodeSegments; i++)
                                        {
                                            RS.DataSet(i, BlankSegment, SegmentSize);
                                        }

                                        MailSegment.Console_WriteLine("Code file prepared");
                                    }

                                    MF.DataValueFileOpen();
                                    RS.DataValueFileOpen();

                                    // Create Galois field for RS code
                                    STH1123.ReedSolomon.GenericGF GGF = new STH1123.ReedSolomon.GenericGF(PolynomialNumber, 1 << NumberOfBits, 1);

                                    // Create code
                                    if (Mode == 0)
                                    {
                                        for (int i = 0; i < WorkPoolSizeI_Units; i++)
                                        {
                                            WorkPoolRSE[i] = new STH1123.ReedSolomon.ReedSolomonEncoder(GGF);
                                        }

                                        bool PrintLastProgress = true;
                                        MailSegment.Log();
                                        MailSegment.Log("Time stamp", "Processed values since previous entry", "Totally processed values", "All values");

                                        long WorkPoolCount = ValuesPerSegment / WorkPoolSizeL;
                                        long FilePointer = 0;

                                        PrepareFileThreads(MF, RS);
                                        if ((ValuesPerSegment % WorkPoolSizeL) > 0)
                                        {
                                            WorkPoolCount++;
                                        }

                                        MailSegment.Console_WriteLine("Code generation started");
                                        for (long i = 0; i < WorkPoolCount; i++)
                                        {
                                            MF.DataValueParamsOffset(FilePointer);
                                            RS.DataValueParamsOffset(FilePointer);

                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                int i__ = i_;
                                                WorkFileThread[i_] = new Thread(() => ThreadDataReadCodeClear(i__));
                                                WorkFileThread[i_].Start();
                                            }
                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                WorkFileThread[i_].Join();
                                            }

                                            for (int i_ = 0; i_ < ReedSolomonComputeThreads; i_++)
                                            {
                                                int i__ = i_;
                                                WorkPoolThread[i_] = new Thread(() => WorkPoolEncode(i__));
                                                WorkPoolThread[i_].Start();
                                            }
                                            for (int i_ = 0; i_ < ReedSolomonComputeThreads; i_++)
                                            {
                                                WorkPoolThread[i_].Join();
                                            }

                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                int i__ = i_;
                                                WorkFileThread[i_] = new Thread(() => ThreadCodeWrite(i__));
                                                WorkFileThread[i_].Start();
                                            }
                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                WorkFileThread[i_].Join();
                                            }

                                            if (SWProgress.ProgressTriggeringValue(ref SWWorkTime))
                                            {
                                                long I__ = ((i + 1) * WorkPoolSizeL);
                                                MailSegment.Console_WriteLine("Code generation progress: " + I__ + "/" + ValuesPerSegment + " (" + (i * WorkPoolSizeL * 100L / ValuesPerSegment) + "%)");
                                                MailSegment.Log(TSW.Elapsed().ToString(), MailSegment.LogDiffB(I__).ToString(), I__.ToString(), ValuesPerSegment.ToString());
                                                if ((I__ + 1) >= ValuesPerSegment)
                                                {
                                                    PrintLastProgress = false;
                                                }
                                            }

                                            FilePointer += PoolSizeBytes;
                                        }
                                        if (PrintLastProgress)
                                        {
                                            MailSegment.Console_WriteLine("Code generation progress: " + ValuesPerSegment + "/" + ValuesPerSegment + " (100%)");
                                            MailSegment.Log(TSW.Elapsed().ToString(), MailSegment.LogDiffB(ValuesPerSegment).ToString(), ValuesPerSegment.ToString(), ValuesPerSegment.ToString());
                                        }


                                        MailSegment.ConsoleLineToLog = true;
                                        MailSegment.ConsoleLineToLogSum = true;
                                        MailSegment.Console_WriteLine("");
                                        MailSegment.Console_WriteLine("Code file created");
                                        MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                                        MailSegment.ConsoleLineToLogSum = false;
                                        MailSegment.ConsoleLineToLog = false;
                                    }

                                    // Check data and code
                                    if ((Mode >= 1) && (Mode <= 6))
                                    {
                                        bool PrintLastProgress = true;
                                        WorkPoolDecodeValsTrueNotChanged = 0;
                                        WorkPoolDecodeValsTrueChangedData = 0;
                                        WorkPoolDecodeValsTrueChangedCode = 0;
                                        WorkPoolDecodeValsTrueChangedDataCode = 0;
                                        WorkPoolDecodeValsFalse = 0;
                                        WorkPoolSegmentMap = new byte[SegPerUnit][];
                                        WorkPoolSegmentMod = new byte[SegPerUnit][];
                                        WorkPoolSegmentMod_ = new byte[SegPerUnit][];
                                        for (int i = 0; i < SegPerUnit; i++)
                                        {
                                            WorkPoolErasureArray[i] = null;
                                            WorkPoolSegmentMap[i] = new byte[TotalUnits];
                                            WorkPoolSegmentMod[i] = new byte[TotalUnits];
                                            WorkPoolSegmentMod_[i] = new byte[TotalUnits];
                                        }
                                        WorkPoolAllowOutsideMap = ((Mode == 5) || (Mode == 6)) ? true : false;
                                        WorkPoolAllowModify = (Mode >= 3) ? true : false;

                                        // Read map to buffer
                                        for (int i = 0; i < DataSegmentsReal; i++)
                                        {
                                            WorkPoolSegmentMap[i % SegPerUnit][i / SegPerUnit] = MF.MapGet(i);
                                            WorkPoolSegmentMod[i % SegPerUnit][i / SegPerUnit] = 0;
                                        }
                                        for (int i = DataSegments; i < (DataSegments + CodeSegments); i++)
                                        {
                                            WorkPoolSegmentMap[i % SegPerUnit][i / SegPerUnit] = RS.MapGet(i - DataSegments);
                                            WorkPoolSegmentMod[i % SegPerUnit][i / SegPerUnit] = 0;
                                        }

                                        // Additional dummy data segments are always treated as surviving
                                        for (int i = DataSegmentsReal; i < DataSegments; i++)
                                        {
                                            WorkPoolSegmentMap[i % SegPerUnit][i / SegPerUnit] = 2;
                                            WorkPoolSegmentMod[i % SegPerUnit][i / SegPerUnit] = 3;
                                        }

                                        // Create erasure array
                                        if ((Mode == 2) || (Mode == 4) || (Mode == 6))
                                        {
                                            for (int i_ = 0; i_ < SegPerUnit; i_++)
                                            {
                                                List<int> ErasureArray_ = new List<int>();
                                                for (int i = 0; i < TotalUnits; i++)
                                                {
                                                    if (WorkPoolSegmentMap[i_][i] == 0)
                                                    {
                                                        ErasureArray_.Add(i);
                                                    }
                                                }
                                                WorkPoolErasureArray[i_] = ErasureArray_.ToArray();
                                            }
                                        }

                                        for (int i = 0; i < WorkPoolSizeI_Units; i++)
                                        {
                                            WorkPoolRSD[i] = new STH1123.ReedSolomon.ReedSolomonDecoder(GGF);
                                        }

                                        MailSegment.Log();
                                        MailSegment.Log("Time stamp", "Processed values since previous entry", "Totally processed values", "All values");

                                        long WorkPoolCount = ValuesPerSegment / WorkPoolSizeL;
                                        long FilePointer = 0;

                                        PrepareFileThreads(MF, RS);
                                        if ((ValuesPerSegment % WorkPoolSizeL) > 0)
                                        {
                                            WorkPoolCount++;
                                        }

                                        MailSegment.Console_WriteLine("Data recovery started");
                                        for (long i = 0; i < WorkPoolCount; i++)
                                        {
                                            MF.DataValueParamsOffset(FilePointer);
                                            RS.DataValueParamsOffset(FilePointer);

                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                int i__ = i_;
                                                WorkFileThread[i_] = new Thread(() => ThreadDataReadCodeRead(i__));
                                                WorkFileThread[i_].Start();
                                            }

                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                WorkFileThread[i_].Join();
                                            }

                                            for (int i__ = 0; i__ < SegPerUnit; i__++)
                                            {
                                                for (int i_ = 0; i_ < TotalUnits; i_++)
                                                {
                                                    WorkPoolSegmentMod_[i__][i_] = 0;
                                                }
                                            }

                                            for (int i_ = 0; i_ < ReedSolomonComputeThreads; i_++)
                                            {
                                                int i__ = i_;
                                                WorkPoolThread[i_] = new Thread(() => WorkPoolDecode(i__));
                                                WorkPoolThread[i_].Start();
                                            }
                                            for (int i_ = 0; i_ < ReedSolomonComputeThreads; i_++)
                                            {
                                                WorkPoolThread[i_].Join();
                                            }

                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                int i__ = i_;
                                                WorkFileThread[i_] = new Thread(() => ThreadDataWriteCodeWrite(i__));
                                                WorkFileThread[i_].Start();
                                            }
                                            for (int i_ = 0; i_ < ReedSolomonFileThreads; i_++)
                                            {
                                                WorkFileThread[i_].Join();
                                            }

                                            if (SWProgress.ProgressTriggeringValue(ref SWWorkTime))
                                            {
                                                long I__ = ((i + 1) * WorkPoolSizeL);
                                                MailSegment.Console_WriteLine("Data recovery progress: " + I__ + "/" + ValuesPerSegment + " (" + (i * WorkPoolSizeL * 100L / ValuesPerSegment) + "%)");
                                                MailSegment.Log(TSW.Elapsed().ToString(), MailSegment.LogDiffB(I__).ToString(), I__.ToString(), ValuesPerSegment.ToString());
                                                if ((i + 1) == ValuesPerSegment)
                                                {
                                                    PrintLastProgress = false;
                                                }
                                            }

                                            FilePointer += PoolSizeBytes;
                                        }

                                        if (PrintLastProgress)
                                        {
                                            MailSegment.Console_WriteLine("Data recovery progress: " + ValuesPerSegment + "/" + ValuesPerSegment + " (100%)");
                                            MailSegment.Log(TSW.Elapsed().ToString(), MailSegment.LogDiffB(ValuesPerSegment).ToString(), (ValuesPerSegment).ToString(), ValuesPerSegment.ToString());
                                        }

                                        MailSegment.ConsoleLineToLog = true;
                                        MailSegment.ConsoleLineToLogSum = true;
                                        MailSegment.Console_WriteLine("");


                                        long ValsAll = 0;
                                        ValsAll += WorkPoolDecodeValsTrueNotChanged;
                                        ValsAll += WorkPoolDecodeValsTrueChangedData;
                                        ValsAll += WorkPoolDecodeValsTrueChangedCode;
                                        ValsAll += WorkPoolDecodeValsTrueChangedDataCode;
                                        ValsAll += WorkPoolDecodeValsFalse;
                                        MailSegment.Console_WriteLine("Total values per unit: " + ValsAll);
                                        MailSegment.Console_WriteLine("Values correct already: " + WorkPoolDecodeValsTrueNotChanged);
                                        MailSegment.Console_WriteLine("Recovered values in data only: " + WorkPoolDecodeValsTrueChangedData);
                                        MailSegment.Console_WriteLine("Recovered values in code only: " + WorkPoolDecodeValsTrueChangedCode);
                                        MailSegment.Console_WriteLine("Recovered values in both data and code: " + WorkPoolDecodeValsTrueChangedDataCode);
                                        MailSegment.Console_WriteLine("Unrecoverable incorrect values: " + WorkPoolDecodeValsFalse);
                                        MailSegment.Console_WriteLine("");

                                        int SegDataModified1 = 0;
                                        int SegCodeModified1 = 0;
                                        int SegDataModified0 = 0;
                                        int SegCodeModified0 = 0;
                                        int SegDataUnModified = 0;
                                        int SegCodeUnModified = 0;
                                        for (int i_ = 0; i_ < SegPerUnit; i_++)
                                        {
                                            for (int i = 0; i < DataUnits; i++)
                                            {
                                                switch (WorkPoolSegmentMod[i_][i])
                                                {
                                                    case 0:
                                                        SegDataUnModified++;
                                                        break;
                                                    case 1:
                                                        SegDataModified1++;
                                                        break;
                                                    case 2:
                                                        SegDataModified0++;
                                                        break;
                                                }
                                            }
                                            for (int i = 0; i < CodeUnits; i++)
                                            {
                                                switch (WorkPoolSegmentMod[i_][i + DataUnits])
                                                {
                                                    case 0:
                                                        SegCodeUnModified++;
                                                        break;
                                                    case 1:
                                                        SegCodeModified1++;
                                                        break;
                                                    case 2:
                                                        SegCodeModified0++;
                                                        break;
                                                }
                                            }
                                        }
                                        int SegDataAll = SegDataUnModified + SegDataModified1 + SegDataModified0;
                                        int SegCodeAll = SegCodeUnModified + SegCodeModified1 + SegCodeModified0;

                                        MailSegment.Console_WriteLine("Data segments - total: " + SegDataAll);
                                        MailSegment.Console_WriteLine("Data segments - modified and saved: " + SegDataModified1);
                                        MailSegment.Console_WriteLine("Data segments - modified and not saved: " + SegDataModified0);
                                        MailSegment.Console_WriteLine("Data segments - not modified: " + SegDataUnModified);
                                        MailSegment.Console_WriteLine("Code segments - total: " + SegCodeAll);
                                        MailSegment.Console_WriteLine("Code segments - modified and saved: " + SegCodeModified1);
                                        MailSegment.Console_WriteLine("Code segments - modified and not saved: " + SegCodeModified0);
                                        MailSegment.Console_WriteLine("Code segments - not modified: " + SegCodeUnModified);

                                        MailSegment.Console_WriteLine("");


                                        // Interpreting the result
                                        int ResultNumber = 0;
                                        if (WorkPoolDecodeValsFalse == 0)
                                        {
                                            if ((ValsAll == WorkPoolDecodeValsTrueNotChanged) && (SegDataAll == SegDataUnModified) && (SegCodeAll == SegCodeUnModified))
                                            {
                                                ResultNumber = 2;
                                            }
                                            if ((WorkPoolDecodeValsTrueChangedData > 0) || (WorkPoolDecodeValsTrueChangedCode > 0) || (WorkPoolDecodeValsTrueChangedDataCode > 0))
                                            {
                                                ResultNumber = 3;
                                            }
                                        }
                                        else
                                        {
                                            if (ValsAll == WorkPoolDecodeValsTrueNotChanged)
                                            {
                                                ResultNumber = 1;
                                            }
                                            if (ValsAll > WorkPoolDecodeValsTrueNotChanged)
                                            {
                                                ResultNumber = 4;
                                            }
                                        }

                                        // Printing the result message
                                        switch (ResultNumber)
                                        {
                                            case 0:
                                                MailSegment.Console_WriteLine("Other scenario, not automatically interpreted");
                                                break;
                                            case 1:
                                                MailSegment.Console_WriteLine("Data file or code file could not be recovered");
                                                break;
                                            case 2:
                                                MailSegment.Console_WriteLine("Data file matches to code file, both files was fully correct already");
                                                break;
                                            case 3:
                                                MailSegment.Console_WriteLine("Data file or code file was corrupted and fully recovered");
                                                break;
                                            case 4:
                                                MailSegment.Console_WriteLine("Data file or code file was corrupted and partially recovered");
                                                break;
                                        }

                                        // Printing the message about saved information
                                        if ((ResultNumber == 3) || (ResultNumber == 4))
                                        {
                                            if ((SegDataModified0 == 0) && (SegDataModified1 == 0))
                                            {
                                                MailSegment.Console_WriteLine("Data file: None of information was recovered or modified");
                                            }
                                            if ((SegDataModified0 > 0) && (SegDataModified1 == 0))
                                            {
                                                MailSegment.Console_WriteLine("Data file: None of recovered information was saved");
                                            }
                                            if ((SegDataModified0 > 0) && (SegDataModified1 > 0))
                                            {
                                                MailSegment.Console_WriteLine("Data file: Some of recovered information was saved");
                                            }
                                            if ((SegDataModified0 == 0) && (SegDataModified1 > 0))
                                            {
                                                MailSegment.Console_WriteLine("Data file: All of recovered information was saved");
                                            }

                                            if ((SegCodeModified0 == 0) && (SegCodeModified1 == 0))
                                            {
                                                MailSegment.Console_WriteLine("Code file: None of information was recovered or modified");
                                            }
                                            if ((SegCodeModified0 > 0) && (SegCodeModified1 == 0))
                                            {
                                                MailSegment.Console_WriteLine("Code file: None of recovered information was saved");
                                            }
                                            if ((SegCodeModified0 > 0) && (SegCodeModified1 > 0))
                                            {
                                                MailSegment.Console_WriteLine("Code file: Some of recovered information was saved");
                                            }
                                            if ((SegCodeModified0 == 0) && (SegCodeModified1 > 0))
                                            {
                                                MailSegment.Console_WriteLine("Code file: All of recovered information was saved");
                                            }
                                        }

                                        MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                                        MailSegment.ConsoleLineToLogSum = false;
                                        MailSegment.ConsoleLineToLog = false;

                                    }

                                    // Analyze map files for recovery
                                    if (Mode == 7)
                                    {
                                        int BunchTotal = 0;
                                        int BunchRecovery = 0;
                                        int BunchBad = 0;
                                        int BunchGood = 0;
                                        int BunchRecoveryData = 0;
                                        int BunchBadData = 0;
                                        int BunchGoodData = 0;
                                        int BunchRecoveryCode = 0;
                                        int BunchBadCode = 0;
                                        int BunchGoodCode = 0;

                                        WorkPoolSegmentMap = new byte[SegPerUnit][];
                                        for (int i = 0; i < SegPerUnit; i++)
                                        {
                                            WorkPoolSegmentMap[i] = new byte[TotalUnits];
                                        }

                                        // Read map to buffer
                                        for (int i = 0; i < DataSegmentsReal; i++)
                                        {
                                            WorkPoolSegmentMap[i % SegPerUnit][i / SegPerUnit] = MF.MapGet(i);
                                        }
                                        for (int i = DataSegments; i < (DataSegments + CodeSegments); i++)
                                        {
                                            WorkPoolSegmentMap[i % SegPerUnit][i / SegPerUnit] = RS.MapGet(i - DataSegments);
                                        }

                                        // Additional dummy data segments are always treated as surviving
                                        for (int i = DataSegmentsReal; i < DataSegments; i++)
                                        {
                                            WorkPoolSegmentMap[i % SegPerUnit][i / SegPerUnit] = 2;
                                        }

                                        // Analyze bunches
                                        MailSegment.Log();
                                        MailSegment.Log("Bunch", "Total", "Surviving", "Missing", "Data total", "Data surviving", "Data missing", "Code total", "Code surviving", "Code missing");
                                        for (int i_ = 0; i_ < SegPerUnit; i_++)
                                        {
                                            string BunchMsg = "Bunch " + (i_ + 1) + "/" + SegPerUnit + " - ";
                                            int MissingData = 0;
                                            int SurvivingData = 0;
                                            int MissingCode = 0;
                                            int SurvivingCode = 0;

                                            for (int i = 0; i < TotalUnits; i++)
                                            {
                                                if (WorkPoolSegmentMap[i_][i] == 0)
                                                {
                                                    if (i < DataUnits)
                                                    {
                                                        MissingData++;
                                                    }
                                                    else
                                                    {
                                                        MissingCode++;
                                                    }
                                                }
                                                else
                                                {
                                                    if (i < DataUnits)
                                                    {
                                                        SurvivingData++;
                                                    }
                                                    else
                                                    {
                                                        SurvivingCode++;
                                                    }
                                                }
                                            }

                                            string _TT = (SurvivingData + SurvivingCode + MissingData + MissingCode).ToString();
                                            string _TS = (SurvivingData + SurvivingCode).ToString();
                                            string _TM = (MissingData + MissingCode).ToString();
                                            string _DT = (SurvivingData + MissingData).ToString();
                                            string _DS = (SurvivingData).ToString();
                                            string _DM = (MissingData).ToString();
                                            string _CT = (SurvivingCode + MissingCode).ToString();
                                            string _CS = (SurvivingCode).ToString();
                                            string _CM = (MissingCode).ToString();
                                            MailSegment.Console_WriteLine(BunchMsg + "Total segments: " + _TT);
                                            MailSegment.Console_WriteLine(BunchMsg + "Total surviving: " + _TS);
                                            MailSegment.Console_WriteLine(BunchMsg + "Total missing: " + _TM);
                                            MailSegment.Console_WriteLine(BunchMsg + "Data segments: " + _DT);
                                            MailSegment.Console_WriteLine(BunchMsg + "Data surviving: " + _DS);
                                            MailSegment.Console_WriteLine(BunchMsg + "Data missing: " + _DM);
                                            MailSegment.Console_WriteLine(BunchMsg + "Code segments: " + _CT);
                                            MailSegment.Console_WriteLine(BunchMsg + "Code surviving: " + _CS);
                                            MailSegment.Console_WriteLine(BunchMsg + "Code missing: " + _CM);

                                            MailSegment.Log((i_ + 1).ToString(), _TT, _TS, _TM, _DT, _DS, _DM, _CT, _CS, _CM);

                                            BunchTotal++;
                                            if ((MissingData + MissingCode) == 0)
                                            {
                                                BunchGood++;
                                                BunchGoodData++;
                                                BunchGoodCode++;
                                            }
                                            else
                                            {
                                                if ((MissingData + MissingCode) > (MissingCode + SurvivingCode))
                                                {
                                                    BunchBad++;
                                                    if (MissingData > 0)
                                                    {
                                                        BunchBadData++;
                                                    }
                                                    else
                                                    {
                                                        BunchGoodData++;
                                                    }
                                                    if (MissingCode > 0)
                                                    {
                                                        BunchBadCode++;
                                                    }
                                                    else
                                                    {
                                                        BunchGoodCode++;
                                                    }
                                                }
                                                else
                                                {
                                                    BunchRecovery++;
                                                    if (MissingData > 0)
                                                    {
                                                        BunchRecoveryData++;
                                                    }
                                                    else
                                                    {
                                                        BunchGoodData++;
                                                    }
                                                    if (MissingCode > 0)
                                                    {
                                                        BunchRecoveryCode++;
                                                    }
                                                    else
                                                    {
                                                        BunchGoodCode++;
                                                    }
                                                }
                                            }
                                        }

                                        MailSegment.ConsoleLineToLog = true;
                                        MailSegment.ConsoleLineToLogSum = true;

                                        MailSegment.Console_WriteLine("");
                                        MailSegment.Console_WriteLine("Bunches - total: " + BunchTotal);
                                        MailSegment.Console_WriteLine("Bunches - good: " + BunchGood);
                                        MailSegment.Console_WriteLine("Bunches - recoverable: " + BunchRecovery);
                                        MailSegment.Console_WriteLine("Bunches - unrecoverable: " + BunchBad);
                                        MailSegment.Console_WriteLine("Bunches - data - good: " + BunchGoodData);
                                        MailSegment.Console_WriteLine("Bunches - data - recoverable: " + BunchRecoveryData);
                                        MailSegment.Console_WriteLine("Bunches - data - unrecoverable: " + BunchBadData);
                                        MailSegment.Console_WriteLine("Bunches - code - good: " + BunchGoodCode);
                                        MailSegment.Console_WriteLine("Bunches - code - recoverable: " + BunchRecoveryCode);
                                        MailSegment.Console_WriteLine("Bunches - code - unrecoverable: " + BunchBadCode);
                                        MailSegment.Console_WriteLine("");
                                        if (BunchTotal == BunchGood)
                                        {
                                            MailSegment.Console_WriteLine("Data and code are complete");
                                        }
                                        else
                                        {
                                            if ((BunchRecovery > 0) && (BunchBad == 0))
                                            {
                                                MailSegment.Console_WriteLine("Data and code are incomplete and fully recoverable");
                                            }
                                            if ((BunchRecovery > 0) && (BunchBad > 0))
                                            {
                                                MailSegment.Console_WriteLine("Data and code are incomplete and partially recoverable");
                                            }
                                            if ((BunchRecovery == 0) && (BunchBad > 0))
                                            {
                                                MailSegment.Console_WriteLine("Data and code are incomplete and recovery is not possible");
                                            }
                                        }

                                        MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                                        MailSegment.ConsoleLineToLogSum = false;
                                        MailSegment.ConsoleLineToLog = false;
                                    }

                                    MF.DataValueFileClose();
                                    RS.DataValueFileClose();

                                    // Filling in the map files
                                    if (Mode == 0)
                                    {
                                        for (int i = 0; i < DataSegments; i++)
                                        {
                                            MF.MapSet(i, 1);
                                        }
                                        for (int i = 0; i < CodeSegments; i++)
                                        {
                                            RS.MapSet(i, 1);
                                        }
                                        MF.ResizeMap();
                                        RS.ResizeMap();
                                    }
                                }
                            }
                            else
                            {
                                MailSegment.Console_WriteLine("Number of bits per segment in work pool must be divisible by 8");
                                MailSegment.Console_WriteLine("Number of bytes per segment in work pool must not greater than segment size");
                            }
                        }
                        else
                        {
                            MailSegment.ConsoleLineToLog = true;
                            MailSegment.ConsoleLineToLogSum = true;
                            MailSegment.Console_WriteLine("");
                            if (MessageSizeFound)
                            {
                                if ((SegmentSizeL % (long)NumberOfBits) > 0)
                                {
                                    MailSegment.Console_WriteLine("Segment size in bits (" + SegmentSizeL + ") is not divisible by bits per value (" + NumberOfBits + ")");
                                }
                                if (((TotalUnits) > ((1 << NumberOfBits) - 1)))
                                {
                                    MailSegment.Console_WriteLine("Number of all units (" + (TotalUnits) + ") exceedes " + NumberOfBits + "-bit limit (" + ((1 << NumberOfBits) - 1) + ")");
                                }
                            }
                            else
                            {
                                MailSegment.Console_WriteLine("Number of all segments (" + (TotalUnits) + ") - appropriate value size not found");
                            }
                            bool WasWritten = false;
                            string SegmentInfo = "Allowed number of bits per value for this file: ";
                            for (int i = 2; i <= 30; i++)
                            {
                                int ValX = (1 << i);
                                if (((TotalUnits) <= (ValX - 1)) && ((SegmentSizeL % (long)i) == 0))
                                {
                                    if (WasWritten)
                                    {
                                        SegmentInfo = SegmentInfo + ", ";
                                    }
                                    SegmentInfo = SegmentInfo + i.ToString();
                                    WasWritten = true;
                                }
                            }
                            if (WasWritten)
                            {
                                MailSegment.Console_WriteLine(SegmentInfo);
                            }
                            else
                            {
                                MailSegment.Console_WriteLine("Try using another segment size.");
                            }
                            MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                            MailSegment.ConsoleLineToLogSum = false;
                            MailSegment.ConsoleLineToLog = false;
                        }

                        RS.Close();
                    }
                    else
                    {
                        MailSegment.ConsoleLineToLog = true;
                        MailSegment.ConsoleLineToLogSum = true;
                        MailSegment.Console_WriteLine("Code file open error: " + RS.OpenError);
                        MailSegment.ConsoleLineToLogSum = false;
                        MailSegment.ConsoleLineToLog = false;
                    }
                    MF.Close();
                }
                else
                {
                    MailSegment.ConsoleLineToLog = true;
                    MailSegment.ConsoleLineToLogSum = true;
                    MailSegment.Console_WriteLine("Data file open error: " + MF.OpenError);
                    MailSegment.ConsoleLineToLogSum = false;
                    MailSegment.ConsoleLineToLog = false;
                }
            }
        }
    }
}
