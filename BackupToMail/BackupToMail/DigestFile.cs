using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace BackupToMail
{
    // The digest file used for checking data file or uploaded data
    public class DigestFile
    {
        /// <summary>
        /// Create or check the digest file
        /// </summary>
        /// <param name="DigestMode">Digest mode</param>
        /// <param name="DataFile_">Data file name</param>
        /// <param name="MapFile_">Map file name</param>
        /// <param name="DigestFile_">Digest file name</param>
        /// <param name="SegmentSize_">Segment size</param>
        public void Proc(int DigestMode, string DataFile_, string MapFile_, string DigestFile_, int SegmentSize_, bool PromptConfirm)
        {
            long DigestFileSize = 0;
            int DigestSegmentSize = 0;

            int SegmentSize = SegmentSize_;
            DigestSegmentSize = SegmentSize;

            MailFile MF = new MailFile();
            if (MF.Open(false, true, DataFile_, MapFile_))
            {
                MF.SetSegmentSize(SegmentSize);
                MF.CalcSegmentCount();

                long FileSize = MF.GetDataSize();
                DigestFileSize = FileSize;
                int SegmentCount__ = MF.GetSegmentCount();
                MailSegment.ConsoleLineToLog = true;
                MailSegment.ConsoleLineToLogSum = true;
                MailSegment.Console_WriteLine("");
                MailSegment.Console_WriteLine("Data file size: " + FileSize);
                MailSegment.Console_WriteLine("Data segment count: " + SegmentCount__);
                MailSegment.ConsoleLineToLogSum = false;
                MailSegment.ConsoleLineToLog = false;
                MailSegment.Console_WriteLine("");

                if (DigestMode != 0)
                {
                    MailFile MD = new MailFile();
                    if (MD.Open(true, DigestMode != 0, DigestFile_, null))
                    {
                        MD.SetSegmentSize(SegmentSize);
                        if (DigestFile_ != null)
                        {
                            MD.CalcSegmentCount();
                            DigestSegmentSize = MD.DigestSegmentSize;
                            DigestFileSize = MD.DigestFileSize;
                        }
                        else
                        {
                            DigestSegmentSize = 0;
                            DigestFileSize = 0;
                        }
                        MailSegment.Console_WriteLine("Data file size from digest file: " + DigestFileSize);
                        MailSegment.Console_WriteLine("Segment size from digest file: " + DigestSegmentSize);
                        MailSegment.Console_WriteLine("");
                        MD.Close();
                    }
                    else
                    {
                        MailSegment.Console_WriteLine("Digest file open error");
                    }
                }

                if (Program.PromptConfirm(PromptConfirm))
                {

                    if ((DigestMode == 1) && (DigestMode == 3))
                    {
                        MF.MapChange(1, 0);
                        MF.MapChange(2, 0);
                    }

                    if (DigestMode == 0)
                    {
                        try
                        {
                            if (File.Exists(DigestFile_))
                            {
                                File.Delete(DigestFile_);
                            }
                        }
                        catch
                        {
                            MailSegment.ConsoleLineToLog = true;
                            MailSegment.ConsoleLineToLogSum = true;
                            MailSegment.Console_WriteLine("Digest file creation error");
                            MailSegment.ConsoleLineToLogSum = false;
                            MailSegment.ConsoleLineToLog = false;
                            return;
                        }
                    }

                    MailFile MD = new MailFile();
                    if (MD.Open(true, DigestMode != 0, DigestFile_, null))
                    {
                        Stopwatch_ TSW = new Stopwatch_();
                        MailSegment.Log();
                        MailSegment.LogReset();
                        MailSegment.Log("Time stamp", "Processed segments since previous entry", "Totally processed segments", "All segments", "Processed bytes since previous entry", "Totally processed bytes", "All bytes");

                        MD.SetSegmentSize(SegmentSize);
                        if (DigestMode != 0)
                        {
                            if (DigestFile_ != null)
                            {
                                MD.CalcSegmentCount();
                                DigestSegmentSize = MD.DigestSegmentSize;
                                DigestFileSize = MD.DigestFileSize;
                            }
                            else
                            {
                                DigestSegmentSize = 0;
                                DigestFileSize = 0;
                            }
                        }
                        if ((DigestMode == 2) || (DigestMode == 3))
                        {
                            if (DigestSegmentSize == SegmentSize)
                            {
                                if (FileSize != DigestFileSize)
                                {
                                    MailSegment.Console_WriteLine("Data file size correction started");
                                    MF.ResizeData(DigestFileSize);
                                    MF.CalcSegmentCount();
                                    FileSize = MF.GetDataSize();
                                    MailSegment.Console_WriteLine("Data file size correction finished");
                                    MailSegment.Console_WriteLine("");
                                }
                                else
                                {
                                    MailSegment.Console_WriteLine("Data file size is correct");
                                    MailSegment.Console_WriteLine("");
                                }
                            }
                        }
                        if ((DigestSegmentSize == SegmentSize) && (FileSize == DigestFileSize))
                        {
                            if (DigestMode != 2)
                            {
                                Stopwatch_ SWProgress = new Stopwatch_();
                                long SWWorkTime = 0;
                                SWProgress.Reset();

                                int DigestG = 0;
                                int DigestB = 0;

                                long ToLogSize = 0;
                                bool PrintLastProgress = true;

                                for (int i = 0; i < SegmentCount__; i++)
                                {
                                    ToLogSize += MF.DataGetSize(i);
                                    if (DigestMode == 0)
                                    {
                                        if (DigestFile_ != null)
                                        {
                                            byte[] Temp = MF.DataGet(i);
                                            MD.DataSet(i, Temp, Temp.Length);
                                        }
                                        MF.MapSet(i, 1);
                                        DigestG++;
                                    }
                                    else
                                    {
                                        if ((DigestFile_ != null) && (MailSegment.BinToStr(MD.DataGetDigest(i)) == MailSegment.BinToStr(MF.DataGetDigest(i))))
                                        {
                                            MF.MapSet(i, 1);
                                            DigestG++;
                                        }
                                        else
                                        {
                                            MF.MapSet(i, 0);
                                            DigestB++;
                                        }
                                    }

                                    if (SWWorkTime < SWProgress.Elapsed())
                                    {
                                        while (SWWorkTime < SWProgress.Elapsed())
                                        {
                                            SWWorkTime += 1000L;
                                        }
                                        MailSegment.Console_WriteLine("Segment " + (i + 1) + "/" + SegmentCount__ + " (" + ((i + 1) * 100 / SegmentCount__) + "%)");
                                        MailSegment.Log(TSW.Elapsed().ToString(), MailSegment.LogDiffS(i + 1).ToString(), (i + 1).ToString(), SegmentCount__.ToString(), MailSegment.LogDiffB(ToLogSize).ToString(), ToLogSize.ToString(), FileSize.ToString());
                                        if ((i + 1) == SegmentCount__)
                                        {
                                            PrintLastProgress = false;
                                        }
                                    }

                                }
                                MF.ResizeMap();

                                if (PrintLastProgress)
                                {
                                    MailSegment.Console_WriteLine("Segment " + SegmentCount__ + "/" + SegmentCount__ + " (100%)");
                                    MailSegment.Log(TSW.Elapsed().ToString(), MailSegment.LogDiffS(SegmentCount__).ToString(), SegmentCount__.ToString(), SegmentCount__.ToString(), MailSegment.LogDiffB(ToLogSize).ToString(), ToLogSize.ToString(), FileSize.ToString());
                                }

                                MailSegment.ConsoleLineToLog = true;
                                MailSegment.ConsoleLineToLogSum = true;
                                MailSegment.Console_WriteLine("");
                                MailSegment.Console_WriteLine("Total segments: " + (DigestG + DigestB));
                                if (DigestMode != 0)
                                {
                                    MailSegment.Console_WriteLine("Good segments: " + DigestG);
                                    MailSegment.Console_WriteLine("Bad segments: " + DigestB);
                                }
                                MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                                MailSegment.ConsoleLineToLogSum = false;
                                MailSegment.ConsoleLineToLog = false;
                            }
                            else
                            {
                                MailSegment.ConsoleLineToLog = true;
                                MailSegment.ConsoleLineToLogSum = true;
                                MailSegment.Console_WriteLine("Data file contents are not checked");
                                MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                                MailSegment.ConsoleLineToLogSum = false;
                                MailSegment.ConsoleLineToLog = false;
                            }
                        }
                        else
                        {
                            MailSegment.ConsoleLineToLog = true;
                            MailSegment.ConsoleLineToLogSum = true;
                            if (FileSize != DigestFileSize)
                            {
                                MailSegment.Console_WriteLine("Data file size mismatch");
                            }
                            if (DigestSegmentSize != SegmentSize)
                            {
                                MailSegment.Console_WriteLine("Segment size mismatch");
                            }
                            MailSegment.Console_WriteLine("Total time: " + MailSegment.TimeHMSM(TSW.Elapsed()));
                            MailSegment.ConsoleLineToLogSum = false;
                            MailSegment.ConsoleLineToLog = false;
                        }

                        MD.Close();
                    }
                    else
                    {
                        MailSegment.ConsoleLineToLog = true;
                        MailSegment.ConsoleLineToLogSum = true;
                        if (DigestMode == 0)
                        {
                            MailSegment.Console_WriteLine("Digest file creation error");
                        }
                        else
                        {
                            MailSegment.Console_WriteLine("Digest file open error");
                        }
                        MailSegment.ConsoleLineToLogSum = false;
                        MailSegment.ConsoleLineToLog = false;
                    }
                }

                MF.Close();
            }
            else
            {
                MailSegment.ConsoleLineToLog = true;
                MailSegment.ConsoleLineToLogSum = true;
                MailSegment.Console_WriteLine("Data file open error");
                MailSegment.ConsoleLineToLogSum = false;
                MailSegment.ConsoleLineToLog = false;
            }

        }

    }
}
