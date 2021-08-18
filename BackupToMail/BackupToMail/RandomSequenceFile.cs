using System;
using System.IO;

namespace BackupToMail
{
    public class RandomSequenceFile
    {
        public void CreateFile(string FileNameSrc, string FileNameDst, int SegmentSize, int CreateStats, int CreatePeriod)
        {
            string FileNameSta = FileNameDst;

            MailFile FileSrc = new MailFile();
            MailFile FileDst = new MailFile();
            if (FileSrc.Open(false, true, FileNameSrc, null))
            {
                Stopwatch_ TSW = new Stopwatch_();

                FileSrc.SetSegmentSize(SegmentSize);
                FileSrc.CalcSegmentCount();
                long FileSize = FileSrc.GetDataSize();
                bool DstFileGood = false;
                if (FileNameDst != null)
                {
                    if (FileDst.Open(false, false, FileNameDst, null))
                    {
                        DstFileGood = true;
                    }
                    else
                    {
                        FileNameSta = FileNameSrc;
                        MailSegment.ConsoleLineToLog = true;
                        MailSegment.ConsoleLineToLogSum = true;
                        MailSegment.Console_WriteLine("");
                        MailSegment.Console_WriteLine("Destination file create error: " + FileDst.OpenError);
                        MailSegment.ConsoleLineToLogSum = false;
                        MailSegment.ConsoleLineToLog = false;
                    }
                }


                if (DstFileGood || (CreateStats > 0))
                {
                    ulong[] Stats = new ulong[256];
                    for (int i = 0; i < 256; i++)
                    {
                        Stats[i] = 0;
                    }

                    MailSegment.Console_WriteLine("");
                    Stopwatch_ SW = new Stopwatch_();
                    MailSegment.Log();
                    MailSegment.LogReset();
                    MailSegment.Log("Time stamp", "Processed segments since previous entry", "Totally processed segments", "All segments", "Processed bytes since previous entry", "Totally processed bytes", "All bytes");

                    long SWWorkTime = 0;
                    if (DstFileGood)
                    {
                        FileDst.SetSegmentSize(SegmentSize);
                        FileDst.CalcSegmentCount();
                    }
                    long ToLogSize = 0;


                    int SegmentCount__ = FileSrc.GetSegmentCount();
                    bool PrintLastProgress = true;
                    for (int i = 0; i < SegmentCount__; i++)
                    {
                        byte[] Temp = FileSrc.DataGet(i);
                        if (DstFileGood)
                        {
                            FileDst.DataSet(i, Temp, Temp.Length);
                        }
                        for (int ii = Temp.Length - 1; ii >= 0; ii--)
                        {
                            Stats[Temp[ii]]++;
                        }
                        ToLogSize += Temp.LongLength;

                        if (SW.ProgressTriggeringValue(ref SWWorkTime))
                        {
                            MailSegment.Console_WriteLine("Segment " + (i + 1) + "/" + SegmentCount__ + " (" + ((i + 1) * 100 / SegmentCount__) + "%)");
                            MailSegment.Log(SW.Elapsed().ToString(), MailSegment.LogDiffS(i + 1).ToString(), (i + 1).ToString(), SegmentCount__.ToString(), MailSegment.LogDiffB(ToLogSize).ToString(), ToLogSize.ToString(), FileSize.ToString());
                            if ((i + 1) == SegmentCount__)
                            {
                                PrintLastProgress = false;
                            }
                        }
                    }
                    if (PrintLastProgress)
                    {
                        MailSegment.Console_WriteLine("Segment " + SegmentCount__ + "/" + SegmentCount__ + " (100%)");
                        MailSegment.Log(SW.Elapsed().ToString(), MailSegment.LogDiffS(SegmentCount__).ToString(), SegmentCount__.ToString(), SegmentCount__.ToString(), MailSegment.LogDiffB(ToLogSize).ToString(), ToLogSize.ToString(), FileSize.ToString());
                    }

                    if (DstFileGood)
                    {
                        FileDst.Close();
                    }

                    MailSegment.ConsoleLineToLog = true;
                    MailSegment.ConsoleLineToLogSum = true;
                    MailSegment.Console_WriteLine("");
                    MailSegment.Console_WriteLine("File created in time: " + MailSegment.TimeHMSM(SW.Elapsed()));
                    if (CreateStats > 0)
                    {
                        MailSegment.Console_WriteLine("");
                        DisplayStats("File distribution", Stats, CreateStats);
                    }
                    MailSegment.ConsoleLineToLogSum = false;
                    MailSegment.ConsoleLineToLog = false;
                }

                FileSrc.Close();

                if (CreatePeriod > 0)
                {
                    MailSegment.Console_WriteLine("");
                    MailSegment.Console_WriteLine("Searching for sequence period");

                    MailFile FileSta = new MailFile();
                    if (FileSta.Open(false, true, FileNameSta, null))
                    {
                        FileSta.DataValueFileOpen();

                        long PeriodBufferSize = SegmentSize;
                        byte[] PeriodArray0 = new byte[PeriodBufferSize];
                        byte[] PeriodArray1 = new byte[PeriodBufferSize];

                        if (PeriodBufferSize > FileSize)
                        {
                            PeriodBufferSize = (int)FileSize;
                        }

                        long PeriodSize = 0;
                        int PeriodChunks;
                        int PeriodChunkOffset;
                        long PeriodChunkSize;
                        long PeriodChunkSize0;
                        long PeriodFilePos;

                        Stopwatch_ SW__ = new Stopwatch_();
                        SW__.Reset();

                        ulong[] PeriodStats = new ulong[256];
                        for (int i = 0; i < 256; i++)
                        {
                            PeriodStats[i] = 0;
                        }

                        long WorkTime = 0;

                        MailSegment.Log();
                        MailSegment.LogReset();
                        MailSegment.Log("Time stamp", "Current period length", "File length", "Current period occurence", "All period occurences");

                        for (long i = 1; i < FileSize; i++)
                        {
                            FileSta.DataGetBytes(i - 1, 1, PeriodArray0);
                            PeriodStats[PeriodArray0[0]]++;

                            bool IsPeriodical = true;
                            bool PeriodicalPrint = true;

                            PeriodChunkSize = PeriodBufferSize;
                            PeriodChunkSize0 = i;
                            PeriodChunks = (int)(i / PeriodBufferSize);
                            if ((i % PeriodBufferSize) > 0)
                            {
                                PeriodChunks++;
                            }

                            long PeriodCount = (FileSize / i);
                            if ((FileSize % i) > 0)
                            {
                                PeriodCount++;
                            }

                            PeriodChunkOffset = 0;
                            for (int ii = 0; ii < PeriodChunks; ii++)
                            {
                                PeriodFilePos = PeriodChunkOffset;

                                if (PeriodChunkSize > PeriodChunkSize0)
                                {
                                    PeriodChunkSize = PeriodChunkSize0;
                                }

                                // Read the first period occurence and treat as pattern
                                FileSta.DataGetBytes(PeriodFilePos, PeriodChunkSize, PeriodArray0);

                                int PeriodChunkSize__ = 0;
                                for (long iii = (PeriodCount - 2); iii >= 0; iii--)
                                {
                                    PeriodFilePos += i;
                                    PeriodChunkSize__ = (int)PeriodChunkSize;
                                    if (iii == 0)
                                    {
                                        int FileRemain = (int)(FileSize - PeriodFilePos);
                                        if (PeriodChunkSize__ > FileRemain)
                                        {
                                            PeriodChunkSize__ = FileRemain;
                                        }
                                    }

                                    // Read the period occurence other than first and compare with pattern,
                                    // if doest match the pattern, the reading and comparing will be broken
                                    if (PeriodChunkSize__ > 0)
                                    {
                                        FileSta.DataGetBytes(PeriodFilePos, PeriodChunkSize, PeriodArray1);

                                        for (int iiii = (PeriodChunkSize__ - 1); iiii >= 0; iiii--)
                                        {
                                            if (PeriodArray0[iiii] != PeriodArray1[iiii])
                                            {
                                                IsPeriodical = false;

                                                // Break all check iteration if data has no period by length given by i
                                                ii = PeriodChunks;
                                                iii = (-1);
                                                break;
                                            }
                                        }
                                    }

                                    if (SW__.ProgressTriggeringValue(ref WorkTime))
                                    {
                                        MailSegment.Console_WriteLine("Period " + i + "/" + FileSize + " (" + (FileSize > 0 ? (i * 100 / FileSize) : 0) + "%); occurence " + (PeriodCount - iii - 1) + "/" + PeriodCount + " (" + (PeriodCount > 0 ? (((PeriodCount - iii - 1) * 100) / PeriodCount) : 0) + "%)");
                                        MailSegment.Log(SW__.Elapsed().ToString(), i.ToString(), FileSize.ToString(), (PeriodCount - iii - 1).ToString(), PeriodCount.ToString());
                                        if (((PeriodCount - iii - 1) == PeriodCount))
                                        {
                                            PeriodicalPrint = false;
                                        }
                                    }
                                }
                                PeriodChunkOffset += (int)PeriodChunkSize;

                                PeriodChunkSize0 -= PeriodChunkSize;

                            }

                            if (IsPeriodical && PeriodicalPrint)
                            {
                                MailSegment.Console_WriteLine("Period " + i + "/" + FileSize + " (" + (FileSize > 0 ? (i * 100 / FileSize) : 0) + "%); occurence " + PeriodCount + "/" + PeriodCount + " (100%)");
                                MailSegment.Log(SW__.Elapsed().ToString(), i.ToString(), FileSize.ToString(), PeriodCount.ToString(), PeriodCount.ToString());
                                PeriodSize = i;
                                break;
                            }
                        }
                        if (PeriodSize == 0)
                        {
                            MailSegment.Console_WriteLine("Period " + FileSize + "/" + FileSize + " (100%); occurence " + "1/1" + " (100%)");
                            MailSegment.Log(SW__.Elapsed().ToString(), FileSize.ToString(), FileSize.ToString(), "1", "1");
                        }

                        FileSta.DataValueFileClose();
                        FileSta.Close();

                        MailSegment.ConsoleLineToLog = true;
                        MailSegment.ConsoleLineToLogSum = true;
                        MailSegment.Console_WriteLine("");
                        if (PeriodSize > 0)
                        {
                            MailSegment.Console_WriteLine("Sequence period length: " + PeriodSize);
                            MailSegment.Console_WriteLine("Period search time: " + MailSegment.TimeHMSM(SW__.Elapsed()));
                            MailSegment.Console_WriteLine("");
                            DisplayStats("Sequence period distribution", PeriodStats, CreatePeriod);
                        }
                        else
                        {
                            MailSegment.Console_WriteLine("Sequence has no period");
                            MailSegment.Console_WriteLine("Period search time: " + MailSegment.TimeHMSM(SW__.Elapsed()));
                        }
                        MailSegment.ConsoleLineToLogSum = false;
                        MailSegment.ConsoleLineToLog = false;
                    }
                    else
                    {
                        MailSegment.ConsoleLineToLog = true;
                        MailSegment.ConsoleLineToLogSum = true;
                        MailSegment.Console_WriteLine("Period search error: " + FileSta.OpenError);
                        MailSegment.ConsoleLineToLogSum = false;
                        MailSegment.ConsoleLineToLog = false;
                    }

                }

                MailSegment.ConsoleLineToLog = true;
                MailSegment.ConsoleLineToLogSum = true;
                MailSegment.Console_WriteLine("");
                MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                MailSegment.ConsoleLineToLogSum = false;
                MailSegment.ConsoleLineToLog = false;

            }
            else
            {
                MailSegment.ConsoleLineToLog = true;
                MailSegment.ConsoleLineToLogSum = true;
                MailSegment.Console_WriteLine("");
                MailSegment.Console_WriteLine("Source file open error: " + FileSrc.OpenError);
                MailSegment.ConsoleLineToLogSum = false;
                MailSegment.ConsoleLineToLog = false;
            }
        }

        void DisplayStats(string Msg, ulong[] StatData, int Mode)
        {
            ulong ValMax = 0;
            ulong ValMin = ulong.MaxValue;
            ulong ValMin0 = ulong.MaxValue;
            ulong ValAvg = 0;
            ulong ValAvg0 = 0;
            ulong ValDev = 0;
            ulong ValDev0 = 0;
            ulong ValSum = 0;
            ulong ValCount0 = 0;
            for (int i = 0; i < 256; i++)
            {
                if (ValMin > StatData[i])
                {
                    ValMin = StatData[i];
                }
                ValAvg = ValAvg + StatData[i];
                if (StatData[i] > 0)
                {
                    ValAvg0 = ValAvg0 + StatData[i];
                    ValCount0++;
                    if (ValMin0 > StatData[i])
                    {
                        ValMin0 = StatData[i];
                    }
                }
                if (ValMax < StatData[i])
                {
                    ValMax = StatData[i];
                }
                ValSum += StatData[i];
            }
            ValAvg = ValAvg / 256;
            if (ValCount0 > 0)
            {
                ValAvg0 = ValAvg0 / ValCount0;
            }
            for (int i = 0; i < 256; i++)
            {
                ulong DevDiff = 0;
                ulong DevDiff0 = 0;
                if (StatData[i] >= ValAvg)
                {
                    DevDiff = StatData[i] - ValAvg;
                }
                else
                {
                    DevDiff = ValAvg - StatData[i];
                }
                ValDev += DevDiff;
                if (StatData[i] > 0)
                {
                    if (StatData[i] >= ValAvg0)
                    {
                        DevDiff0 = StatData[i] - ValAvg0;
                    }
                    else
                    {
                        DevDiff0 = ValAvg0 - StatData[i];
                    }
                    ValDev0 += DevDiff0;
                }
            }

            if (Mode == 1)
            {
                ulong GreatVal = ValMax / 10000UL;
                if ((ValMax % 10000UL) > 0)
                {
                    GreatVal++;
                }
                uint[] Stat_ = new uint[256];
                ulong ValD = 1;
                if (GreatVal == 0)
                {
                    GreatVal = 1;
                }
                while (ValD < GreatVal)
                {
                    ValD = ValD * 10;
                }
                ulong ValDx = ValD / 10;
                if (ValDx < 1)
                {
                    ValDx = 1;
                }

                MailSegment.Console_WriteLine(Msg + " n/" + ValD + ":");
                string ConBuf = "";
                for (int i = 0; i < 256; i++)
                {
                    Stat_[i] = (uint)(StatData[i] / ValD);
                    if ((ValD > 1) && ((StatData[i] / ValDx) % 10) > 0)
                    {
                        if (Stat_[i] < 9999)
                        {
                            Stat_[i]++;
                        }
                    }

                    ConBuf += (Stat_[i].ToString().PadLeft(4, ' '));
                    if (((i + 1) % 16) == 0)
                    {
                        MailSegment.Console_WriteLine(ConBuf);
                        ConBuf = "";
                    }
                    else
                    {
                        ConBuf += " ";
                    }
                }
            }
            if ((Mode == 2) || (Mode == 3))
            {
                MailSegment.Console_WriteLine(Msg + ":");

                int PadI = (int)(Math.Floor(Math.Log10(ValMax)) + 1);
                for (int i = 0; i < 256; i++)
                {
                    if ((Mode == 2) || (StatData[i] > 0))
                    {
                        MailSegment.Console_WriteLine(i.ToString().PadLeft(3, ' ') + ": " + StatData[i].ToString().PadLeft(PadI, ' '));
                    }
                }
            }

            MailSegment.Console_WriteLine("Minimum including 0: " + ValMin);
            if (ValCount0 > 0)
            {
                MailSegment.Console_WriteLine("Minimum excluding 0: " + ValMin0);
            }
            MailSegment.Console_WriteLine("Maximum: " + ValMax);
            MailSegment.Console_WriteLine("Average including 0: " + ValAvg);
            if (ValCount0 > 0)
            {
                MailSegment.Console_WriteLine("Average excluding 0: " + ValAvg0);
            }

            ValDev = (ulong)(Math.Sqrt((double)ValDev / 256.0) * 1000.0);
            MailSegment.Console_WriteLine("Standard deviation x1000 including 0: " + ValDev);
            if (ValCount0 > 0)
            {
                ValDev0 = (ulong)(Math.Sqrt((double)ValDev0 / (double)ValCount0) * 1000.0);
                MailSegment.Console_WriteLine("Standard deviation x1000 excluding 0: " + ValDev0);
            }

            MailSegment.Console_WriteLine("Sum: " + ValSum);
            MailSegment.Console_WriteLine("Number of non-zeros: " + ValCount0);
            MailSegment.Console_WriteLine("Number of zeros: " + (256UL - ValCount0));
        }

    }
}
