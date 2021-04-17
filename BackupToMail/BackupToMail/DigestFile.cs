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
        /// <param name="Create">Create mode</param>
        /// <param name="DataFile_">Data file name</param>
        /// <param name="DigestFile_">Digest file name</param>
        /// <param name="SegmentSize_">Segment size</param>
        public void Proc(bool Create, string DataFile_, string DigestFile_, int SegmentSize_)
        {
            long DigestFileSize = 0;
            int DigestSegmentSize = 0;



            int SegmentSize = SegmentSize_;
            DigestSegmentSize = SegmentSize;

            MailFile MF = new MailFile();
            if (MF.Open(false, true, DataFile_, null))
            {
                MF.SetSegmentSize(SegmentSize);
                MF.CalcSegmentCount();

                long FileSize = MF.GetDataSize();
                DigestFileSize = FileSize;
                int SegmentCount__ = MF.GetSegmentCount();
                Console.WriteLine("Data file size: " + FileSize);
                Console.WriteLine("Data segment count: " + SegmentCount__);
                Console.WriteLine();

                if (Create)
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
                if (MD.Open(true, !Create, DigestFile_, null))
                {
                    MD.SetSegmentSize(SegmentSize);
                    if (!Create)
                    {
                        MD.CalcSegmentCount();
                        DigestSegmentSize = MD.DigestSegmentSize;
                        DigestFileSize = MD.DigestFileSize;
                        Console.WriteLine("Data file size from digest file: " + DigestFileSize);
                        Console.WriteLine("Segment size from digest file: " + DigestSegmentSize);
                        Console.WriteLine();
                    }
                    if ((DigestSegmentSize == SegmentSize) && (FileSize == DigestFileSize))
                    {
                        int DigestG = 0;
                        int DigestB = 0;
                        for (int i = 0; i < SegmentCount__; i++)
                        {
                            Console.Write("Segment " + (i + 1) + "/" + SegmentCount__ + " - ");
                            if (Create)
                            {
                                byte[] Temp = MF.DataGet(i);
                                MD.DataSet(i, Temp, Temp.Length);
                                Console.WriteLine("OK");
                                DigestG++;
                            }
                            else
                            {
                                if (MailSegment.BinToStr(MD.DataGet(i)) == MailSegment.Digest(MF.DataGet(i)))
                                {
                                    Console.WriteLine("good");
                                    DigestG++;
                                }
                                else
                                {
                                    Console.WriteLine("bad");
                                    DigestB++;
                                }
                            }
                        }
                        Console.WriteLine();
                        Console.WriteLine("Total segments: " + (DigestG + DigestB));
                        if (!Create)
                        {
                            Console.WriteLine("Good segments: " + DigestG);
                            Console.WriteLine("Bad segments: " + DigestB);
                        }
                        Console.WriteLine();
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
                    if (Create)
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
