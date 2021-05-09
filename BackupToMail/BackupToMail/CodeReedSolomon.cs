using System;
using System.Collections.Generic;

namespace BackupToMail
{
    public class CodeReedSolomon
    {
        public CodeReedSolomon()
        {
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
                    Console.WriteLine("Processing " + Prefix2 + " file");
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

                            if (SWWorkTime < SWProgress.Elapsed())
                            {
                                while (SWWorkTime < SWProgress.Elapsed())
                                {
                                    SWWorkTime += 1000L;
                                }
                                Console.WriteLine(Prefix1 + " file progress: " + (i + 1) + "/" + XSegments + " (" + (i * 100 / XSegments) + "%)");
                            }
                        }
                        Console.WriteLine(Prefix1 + " file progress: " + XSegments + "/" + XSegments + " (100%)");
                        Console.WriteLine();

                        if (LastGoodSegment < (XSegments - 1))
                        {
                            if (SizeMode == 1)
                            {
                                Console.WriteLine(Prefix1 + " file resizing started");
                                long NewSize = ((long)XSegments - 1L) * (long)SegmentSize + 1L;
                                MF.ResizeData(NewSize);
                                Console.WriteLine(Prefix1 + " file resizing finished");
                                Console.WriteLine();
                            }
                            if (SizeMode == 2)
                            {
                                Console.WriteLine(Prefix1 + " file resizing started");
                                long NewSize = ((long)LastGoodSegment + 1L) * (long)SegmentSize;
                                MF.ResizeData(NewSize);
                                Console.WriteLine(Prefix1 + " file resizing finished");
                                Console.WriteLine();
                            }
                        }

                        Console.WriteLine(Prefix1 + " file processed");
                    }
                    else
                    {
                        Console.WriteLine(Prefix1 + " file open error: " + MF.OpenError);
                    }
                    return;
                }
            }
            Console.WriteLine(Prefix1 + " file not specified");
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
                            Console.WriteLine(Prefix1 + " file resizing started");
                            MF.ResizeData(FileSize);
                            Console.WriteLine(Prefix1 + " file resizing finished");
                        }
                        else
                        {
                            Console.WriteLine(Prefix1 + " file open error: " + MF.OpenError);
                        }
                        return;
                    }
                    else
                    {
                        Console.WriteLine(Prefix1 + " - invalid desired file size");
                        return;
                    }
                }
            }
            Console.WriteLine(Prefix1 + " file not specified");
        }

        public void Proc(int Mode, string DataFile, string DataMapFile, string CodeFile, string CodeMapFile, int CodeSegments, int SegmentSize)
        {
            // Resize data and map files
            if (Mode == 7)
            {
                ResizeFile("Data", "data", DataFile, DataMapFile);
                Console.WriteLine();
                ResizeFile("Code", "code", CodeFile, CodeMapFile);
                Console.WriteLine();
            }

            // Clear some segments according the map file
            if (Mode == 8)
            {
                SimulateClear("Data", "data", DataFile, DataMapFile, SegmentSize, CodeSegments);
                Console.WriteLine();
                SimulateClear("Code", "code", CodeFile, CodeMapFile, SegmentSize, CodeSegments);
                Console.WriteLine();
            }

            // Creating RS code and recovering data file
            if ((Mode >= 0) && (Mode <= 6))
            {
                MailFile MF = new MailFile();
                MailFile RS = new MailFile();

                if (MF.Open(false, Mode == 0, DataFile, DataMapFile))
                {
                    if (RS.Open(false, false, CodeFile, CodeMapFile))
                    {
                        // Set desired segment size for both data and code files
                        MF.SetSegmentSize(SegmentSize);
                        RS.SetSegmentSize(SegmentSize);
                        int DataSegments = MF.CalcSegmentCount();
                        if (Mode != 0)
                        {
                            CodeSegments = RS.CalcSegmentCount();
                        }

                        long SegmentSizeL = SegmentSize;
                        SegmentSizeL = SegmentSizeL * 8;

                        int TotalSegments = DataSegments + CodeSegments;

                        Console.WriteLine("Data segments: " + DataSegments);
                        Console.WriteLine("Code segments: " + CodeSegments);
                        Console.WriteLine("All segments: " + TotalSegments);

                        bool MessageSizeFound = true;

                        if (PolynomialNumber < 4)
                        {
                            int PolyBase = 4;
                            NumberOfBits = 2;

                            while (MessageSizeFound && ((TotalSegments > (PolyBase - 1)) || ((SegmentSizeL % (long)NumberOfBits) != 0)))
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
                            Console.WriteLine("Bits per value: " + NumberOfBits);
                            Console.WriteLine("Primitive polynomial: " + PolynomialNumber);
                        }

                        if (MessageSizeFound && ((SegmentSizeL % (long)NumberOfBits) == 0) && ((DataSegments + CodeSegments) <= ((1 << NumberOfBits) - 1)))
                        {
                            long ValuesPerSegment = SegmentSize * 8 / NumberOfBits;

                            Console.WriteLine("Values per segment: " + ValuesPerSegment);

                            int[] ValueSerie = new int[TotalSegments];
                            int[] ValueSerie_ = new int[TotalSegments];

                            Console.WriteLine();

                            Stopwatch_ SWProgress = new Stopwatch_();
                            long SWWorkTime = 0;
                            SWProgress.Reset();

                            // Prepare blank code file
                            if (Mode == 0)
                            {
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
                            }

                            MF.DataValueFileOpen();
                            RS.DataValueFileOpen();

                            // Create Galois field for RS code
                            STH1123.ReedSolomon.GenericGF GGF = new STH1123.ReedSolomon.GenericGF(PolynomialNumber, 1 << NumberOfBits, 1);

                            // Create code
                            if (Mode == 0)
                            {
                                STH1123.ReedSolomon.ReedSolomonEncoder RSE = new STH1123.ReedSolomon.ReedSolomonEncoder(GGF);

                                for (long i = 0; i < ValuesPerSegment; i++)
                                {
                                    MF.DataValueParams(i, NumberOfBits);
                                    RS.DataValueParams(MF);

                                    for (int ii = 0; ii < DataSegments; ii++)
                                    {
                                        ValueSerie[ii] = MF.DataValueGet(ii);
                                    }
                                    for (int ii = 0; ii < CodeSegments; ii++)
                                    {
                                        ValueSerie[ii + DataSegments] = 0;
                                    }
                                    RSE.Encode(ValueSerie, CodeSegments);
                                    for (int ii = 0; ii < CodeSegments; ii++)
                                    {
                                        RS.DataValueSet(ii, ValueSerie[ii + DataSegments]);

                                    }


                                    if (SWWorkTime < SWProgress.Elapsed())
                                    {
                                        while (SWWorkTime < SWProgress.Elapsed())
                                        {
                                            SWWorkTime += 1000L;
                                        }
                                        Console.WriteLine("Code generation progress: " + (i + 1) + "/" + ValuesPerSegment + " (" + (i * 100 / ValuesPerSegment) + "%)");
                                    }
                                }
                                Console.WriteLine("Code generation progress: " + ValuesPerSegment + "/" + ValuesPerSegment + " (100%)");
                                Console.WriteLine();
                                Console.WriteLine("Code file created");
                            }

                            // Check data and code
                            if ((Mode >= 1) && (Mode <= 6))
                            {
                                int[] ErasureArray = null;
                                long ValsTrueNotChanged = 0;
                                long ValsTrueChangedData = 0;
                                long ValsTrueChangedCode = 0;
                                long ValsTrueChangedDataCode = 0;
                                long ValsFalse = 0;
                                byte[] SegmentMap = new byte[TotalSegments];
                                byte[] SegmentMod = new byte[TotalSegments];
                                bool AllowOutsideMap = ((Mode == 5) || (Mode == 6)) ? true : false;
                                bool AllowModify = (Mode >= 3) ? true : false;

                                // Read map to buffer
                                for (int i = 0; i < DataSegments; i++)
                                {
                                    SegmentMap[i] = MF.MapGet(i);
                                    SegmentMod[i] = 0;
                                }
                                for (int i = 0; i < CodeSegments; i++)
                                {
                                    SegmentMap[i + DataSegments] = RS.MapGet(i);
                                    SegmentMod[i] = 0;
                                }

                                // Create erasure array
                                if ((Mode == 2) || (Mode == 4) || (Mode == 6))
                                {
                                    List<int> ErasureArray_ = new List<int>();
                                    for (int i = 0; i < TotalSegments; i++)
                                    {
                                        if (SegmentMap[i] == 0)
                                        {
                                            ErasureArray_.Add(i);
                                        }
                                    }
                                    ErasureArray = ErasureArray_.ToArray();
                                }

                                STH1123.ReedSolomon.ReedSolomonDecoder RSD = new STH1123.ReedSolomon.ReedSolomonDecoder(GGF);

                                for (long i = 0; i < ValuesPerSegment; i++)
                                {
                                    MF.DataValueParams(i, NumberOfBits);
                                    RS.DataValueParams(MF);

                                    for (int ii = 0; ii < DataSegments; ii++)
                                    {
                                        ValueSerie[ii] = MF.DataValueGet(ii);
                                        ValueSerie_[ii] = ValueSerie[ii];
                                    }
                                    for (int ii = 0; ii < CodeSegments; ii++)
                                    {
                                        ValueSerie[ii + DataSegments] = RS.DataValueGet(ii);
                                        ValueSerie_[ii + DataSegments] = ValueSerie[ii + DataSegments];
                                    }
                                    if (RSD.Decode(ValueSerie, CodeSegments, ErasureArray))
                                    {
                                        bool ChangedData = false;
                                        bool ChangedCode = false;
                                        for (int ii = 0; ii < DataSegments; ii++)
                                        {
                                            if (ValueSerie[ii] != ValueSerie_[ii])
                                            {
                                                ChangedData = true;
                                                if (AllowModify && (AllowOutsideMap || (SegmentMap[ii] == 0)))
                                                {
                                                    MF.DataValueSet(ii, ValueSerie[ii]);
                                                    SegmentMod[ii] = 1;
                                                }
                                                else
                                                {
                                                    SegmentMod[ii] = 2;
                                                }
                                            }
                                        }
                                        for (int ii = 0; ii < CodeSegments; ii++)
                                        {
                                            if (ValueSerie[ii + DataSegments] != ValueSerie_[ii + DataSegments])
                                            {
                                                ChangedCode = true;
                                                if (AllowModify && (AllowOutsideMap || (SegmentMap[ii + DataSegments] == 0)))
                                                {
                                                    RS.DataValueSet(ii, ValueSerie[ii + DataSegments]);
                                                    SegmentMod[ii + DataSegments] = 1;
                                                }
                                                else
                                                {
                                                    SegmentMod[ii + DataSegments] = 2;
                                                }
                                            }
                                        }

                                        if (ChangedData)
                                        {
                                            if (ChangedCode)
                                            {
                                                ValsTrueChangedDataCode++;
                                            }
                                            else
                                            {
                                                ValsTrueChangedData++;
                                            }
                                        }
                                        else
                                        {
                                            if (ChangedCode)
                                            {
                                                ValsTrueChangedCode++;
                                            }
                                            else
                                            {
                                                ValsTrueNotChanged++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ValsFalse++;
                                    }


                                    if (SWWorkTime < SWProgress.Elapsed())
                                    {
                                        while (SWWorkTime < SWProgress.Elapsed())
                                        {
                                            SWWorkTime += 1000L;
                                        }
                                        Console.WriteLine("Recovery progress: " + (i + 1) + "/" + ValuesPerSegment + " (" + (i * 100 / ValuesPerSegment) + "%)");
                                    }
                                }
                                Console.WriteLine("Recovery progress: " + ValuesPerSegment + "/" + ValuesPerSegment + " (100%)");

                                Console.WriteLine();
                                long ValsAll = 0;
                                ValsAll += ValsTrueNotChanged;
                                ValsAll += ValsTrueChangedData;
                                ValsAll += ValsTrueChangedCode;
                                ValsAll += ValsTrueChangedDataCode;
                                ValsAll += ValsFalse;
                                Console.WriteLine("Total values per segment: " + ValsAll);
                                Console.WriteLine("Values correct already: " + ValsTrueNotChanged);
                                Console.WriteLine("Recovered values in data only: " + ValsTrueChangedData);
                                Console.WriteLine("Recovered values in code only: " + ValsTrueChangedCode);
                                Console.WriteLine("Recovered values in both data and code: " + ValsTrueChangedDataCode);
                                Console.WriteLine("Unrecoverable incorrect values: " + ValsFalse);
                                Console.WriteLine();

                                int SegDataModified1 = 0;
                                int SegCodeModified1 = 0;
                                int SegDataModified0 = 0;
                                int SegCodeModified0 = 0;
                                int SegDataUnModified = 0;
                                int SegCodeUnModified = 0;
                                for (int i = 0; i < DataSegments; i++)
                                {
                                    switch (SegmentMod[i])
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
                                for (int i = 0; i < CodeSegments; i++)
                                {
                                    switch (SegmentMod[i + DataSegments])
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
                                int SegDataAll = SegDataUnModified + SegDataModified1 + SegDataModified0;
                                int SegCodeAll = SegCodeUnModified + SegCodeModified1 + SegCodeModified0;

                                Console.WriteLine("Data segments - total: " + SegDataAll);
                                Console.WriteLine("Data segments - modified and saved: " + SegDataModified1);
                                Console.WriteLine("Data segments - modified and not saved: " + SegDataModified0);
                                Console.WriteLine("Data segments - not modified: " + SegDataUnModified);
                                Console.WriteLine("Code segments - total: " + SegCodeAll);
                                Console.WriteLine("Code segments - modified and saved: " + SegCodeModified1);
                                Console.WriteLine("Code segments - modified and not saved: " + SegCodeModified0);
                                Console.WriteLine("Code segments - not modified: " + SegCodeUnModified);

                                Console.WriteLine();


                                // Interpreting the result
                                int ResultNumber = 0;
                                if (ValsFalse == 0)
                                {
                                    if ((ValsAll == ValsTrueNotChanged) && (SegDataAll == SegDataUnModified) && (SegCodeAll == SegCodeUnModified))
                                    {
                                        ResultNumber = 2;
                                    }
                                    if ((ValsTrueChangedData > 0) || (ValsTrueChangedCode > 0) || (ValsTrueChangedDataCode > 0))
                                    {
                                        ResultNumber = 3;
                                    }
                                }
                                else
                                {
                                    if (ValsAll == ValsTrueNotChanged)
                                    {
                                        ResultNumber = 1;
                                    }
                                    if (ValsAll > ValsTrueNotChanged)
                                    {
                                        ResultNumber = 4;
                                    }
                                }

                                // Printing the result message
                                switch (ResultNumber)
                                {
                                    case 0:
                                        Console.WriteLine("Other scenario, not automatically interpreted");
                                        break;
                                    case 1:
                                        Console.WriteLine("Data file or code file could not be recovered");
                                        break;
                                    case 2:
                                        Console.WriteLine("Data file matches to code file, both files was fully correct already");
                                        break;
                                    case 3:
                                        Console.WriteLine("Data file or code file was corrupted and fully recovered");
                                        break;
                                    case 4:
                                        Console.WriteLine("Data file or code file was corrupted and partially recovered");
                                        break;
                                }

                                // Printing the message about saved information
                                if ((ResultNumber == 3) || (ResultNumber == 4))
                                {
                                    if ((SegDataModified0 == 0) && (SegDataModified1 == 0))
                                    {
                                        Console.WriteLine("Data file: None of information was recovered or modified");
                                    }
                                    if ((SegDataModified0 > 0) && (SegDataModified1 == 0))
                                    {
                                        Console.WriteLine("Data file: None of recovered information was saved");
                                    }
                                    if ((SegDataModified0 > 0) && (SegDataModified1 > 0))
                                    {
                                        Console.WriteLine("Data file: Some of recovered information was saved");
                                    }
                                    if ((SegDataModified0 == 0) && (SegDataModified1 > 0))
                                    {
                                        Console.WriteLine("Data file: All of recovered information was saved");
                                    }

                                    if ((SegCodeModified0 == 0) && (SegCodeModified1 == 0))
                                    {
                                        Console.WriteLine("Code file: None of information was recovered or modified");
                                    }
                                    if ((SegCodeModified0 > 0) && (SegCodeModified1 == 0))
                                    {
                                        Console.WriteLine("Code file: None of recovered information was saved");
                                    }
                                    if ((SegCodeModified0 > 0) && (SegCodeModified1 > 0))
                                    {
                                        Console.WriteLine("Code file: Some of recovered information was saved");
                                    }
                                    if ((SegCodeModified0 == 0) && (SegCodeModified1 > 0))
                                    {
                                        Console.WriteLine("Code file: All of recovered information was saved");
                                    }
                                }

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
                        else
                        {
                            Console.WriteLine();
                            if (MessageSizeFound)
                            {
                                if ((SegmentSizeL % (long)NumberOfBits) > 0)
                                {
                                    Console.WriteLine("Segment size in bits (" + SegmentSizeL + ") is not divisible by bits per value (" + NumberOfBits + ")");
                                }
                                if (((DataSegments + CodeSegments) > ((1 << NumberOfBits) - 1)))
                                {
                                    Console.WriteLine("Number of all segments (" + (DataSegments + CodeSegments) + ") exceedes " + NumberOfBits + "-bit limit (" + ((1 << NumberOfBits) - 1) + ")");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Number of all segments (" + (DataSegments + CodeSegments) + ") - appropriate value size not found");
                            }
                            bool WasWritten = false;
                            for (int i = 2; i <= 30; i++)
                            {
                                int ValX = (1 << i);
                                if (((DataSegments + CodeSegments) <= (ValX - 1)) && ((SegmentSizeL % (long)i) == 0))
                                {
                                    if (WasWritten)
                                    {
                                        Console.Write(", ");
                                    }
                                    else
                                    {
                                        Console.Write("Allowed number of bits per value for this file: ");
                                    }
                                    Console.Write(i);
                                    WasWritten = true;
                                }
                            }
                            if (!WasWritten)
                            {
                                Console.WriteLine("Try using another segment size.");
                            }
                            Console.WriteLine();
                        }

                        RS.Close();
                    }
                    else
                    {
                        Console.WriteLine("Code file open error: " + RS.OpenError);
                    }
                    MF.Close();
                }
                else
                {
                    Console.WriteLine("Data file open error: " + MF.OpenError);
                }
            }
        }
    }
}
