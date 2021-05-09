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
        public void Proc(int DigestMode, string DataFile_, string MapFile_, string DigestFile_, int SegmentSize_)
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

                if ((DigestMode != 0) && (DigestMode != 2))
                {
                    MF.MapChange(1, 0);
                    MF.MapChange(2, 0);
                }


                long FileSize = MF.GetDataSize();
                DigestFileSize = FileSize;
                int SegmentCount__ = MF.GetSegmentCount();
                Console.WriteLine("Data file size: " + FileSize);
                Console.WriteLine("Data segment count: " + SegmentCount__);
                Console.WriteLine();

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
                        Console.WriteLine("Digest file creation error");
                        return;
                    }
                }

                MailFile MD = new MailFile();
                if (MD.Open(true, DigestMode != 0, DigestFile_, null))
                {
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
                        Console.WriteLine("Data file size from digest file: " + DigestFileSize);
                        Console.WriteLine("Segment size from digest file: " + DigestSegmentSize);
                        Console.WriteLine();
                    }
                    if ((DigestMode == 2) || (DigestMode == 3))
                    {
                        if (DigestSegmentSize == SegmentSize)
                        {
                            if (FileSize != DigestFileSize)
                            {
                                Console.WriteLine("Data file size correction started");
                                MF.ResizeData(DigestFileSize);
                                MF.CalcSegmentCount();
                                FileSize = MF.GetDataSize();
                                Console.WriteLine("Data file size correction finished");
                                Console.WriteLine();
                            }
                            else
                            {
                                Console.WriteLine("Data file size is correct");
                                Console.WriteLine();
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
                            for (int i = 0; i < SegmentCount__; i++)
                            {
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
                                    Console.WriteLine("Segment " + (i + 1) + "/" + SegmentCount__ + " (" + ((i + 1) * 100 / SegmentCount__) + "%)");
                                }

                            }
                            MF.ResizeMap();

                            Console.WriteLine("Segment " + SegmentCount__ + "/" + SegmentCount__ + " (100%)");

                            Console.WriteLine();
                            Console.WriteLine("Total segments: " + (DigestG + DigestB));
                            if (DigestMode != 0)
                            {
                                Console.WriteLine("Good segments: " + DigestG);
                                Console.WriteLine("Bad segments: " + DigestB);
                            }
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Data file contents are not checked");
                        }
                    }
                    else
                    {
                        if (FileSize != DigestFileSize)
                        {
                            Console.WriteLine("Data file size mismatch");
                        }
                        if (DigestSegmentSize != SegmentSize)
                        {
                            Console.WriteLine("Segment size mismatch");
                        }
                    }

                    MD.Close();
                }
                else
                {
                    if (DigestMode == 0)
                    {
                        Console.WriteLine("Digest file creation error");
                    }
                    else
                    {
                        Console.WriteLine("Digest file open error");
                    }
                }

                MF.Close();
            }
            else
            {
                Console.WriteLine("Data file open error");
            }

        }

    }
}
