/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-05-31
 * Time: 07:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MailKit;
using MailKit.Net.Smtp;

namespace BackupToMail
{
    class Program
    {
        public static void Main(string[] args)
        {
            Main_(args);
        }
        
        /// <summary>
        /// Converts string to integer, returns -1 if convert is not possible 
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        static int StrToInt(string S)
        {
            int I;
            if (int.TryParse(S, out I))
            {
                return I;
            }
            else
            {
                return -1;
            }
        }
        
        /// <summary>
        /// Converts string to boolean
        /// true -  "1", "TRUE", "YES", "T", "Y"
        /// false - "0", "FALSE", "NO", "F", "N"
        /// other strings than above are false
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        static bool StrToBool(string S)
        {
            S = S.Trim();
            if ((S == "1") || (S.ToUpperInvariant() == "TRUE") || (S.ToUpperInvariant() == "YES") || (S.ToUpperInvariant() == "T") || (S.ToUpperInvariant() == "Y"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// Parse number list separated by comma
        /// Parameter can be a number or the interval (NumMin..NumMax)
        /// </summary>
        /// <param name="Raw"></param>
        /// <returns>List of three-number arrays, the first number of array is the number from the list</returns>
        public static List<int[]> CommaList(string Raw)
        {
            List<int[]> Lst = new List<int[]>();
            string[] Raw_ = Raw.Split(',');
            int[] Buf = new int[3];
            for (int i = 0; i < Raw_.Length; i++)
            {
                if (Raw_[i].Contains("."))
                {
                    string[] Raw__ = Raw_[i].Split('.');
                    if ((Lst.Count > 0) && (Raw__.Length == 3))
                    {
                        Lst[Lst.Count - 1][1] = StrToInt(Raw__[0]);
                        Lst[Lst.Count - 1][2] = StrToInt(Raw__[Raw__.Length - 1]);
                    }
                }
                else
                {
                    Buf = new int[3];
                    Buf[0] = StrToInt(Raw_[i]);
                    Lst.Add(Buf);
                }
            }
            return Lst;
        }
        
        public static void Main_(string[] args)
        {
            // Load configuration
            MailSegment.MailAccountList = new List<MailAccount>();
            ConfigFile CF = new ConfigFile();
            CF.FileLoad("Config.txt");
            bool Work = true;
            int I = 0;
            while (Work)
            {
                MailAccount MA = new MailAccount();
                Work = MA.ConfigLoad(CF, I);
                I++;
                if (Work)
                {
                    MailSegment.MailAccountList.Add(MA);
                }
            }
            MailSegment.ConfigSet(CF);


            int ProgMode = 0;
            string ItemName = "";
            string ItemData = "";
            string ItemMap = "";
            int SegmentSize = MailSegment.DefaultSegmentSize;
            int SegmentType = MailSegment.DefaultSegmentType;
            int SegmentImgSize = MailSegment.DefaultImageSize;
            List<int> AccSrc = new List<int>();
            List<int> AccDst = new List<int>();
            List<int> AccMin = new List<int>();
            List<int> AccMax = new List<int>();

            string[] SegmentTypeDesc = new string[]
            {
                "Binary attachment",
                "PNG image attachment",
                "Base64 in plain text body",
                "PNG image in HTML body"
            };            

            string[] DownloadTypeDesc = new string[]
            {
                "Download data file",
                "Check existence without body control",
                "Check existence with body control",
                "Check the header digest using data file",
                "Check the body contents using data file",
                "Download digest file",
                "Check the header digest using digest file",
                "Check the body contents using digest file"
            };            

            string[] DeleteTypeDesc = new string[]
            {
                "None",
                "Bad",
                "Duplicate",
                "This file",
                "Other messages",
                "Other files"
            };                
            
            MailSegment.FileDeleteMode[] DeleteTypeFlag = new MailSegment.FileDeleteMode[]
            {
                MailSegment.FileDeleteMode.None,
                MailSegment.FileDeleteMode.Bad,
                MailSegment.FileDeleteMode.Duplicate,
                MailSegment.FileDeleteMode.ThisFile,
                MailSegment.FileDeleteMode.OtherMsg,
                MailSegment.FileDeleteMode.OtherFiles
            };
            
            // Determine program mode based on the first parameter
            if (args.Length > 0)
            {
                switch (args[0].ToUpperInvariant())
                {
                    case "UPLOAD": if (args.Length >= 6) { ProgMode = 1; } break;
                    case "DOWNLOAD": if (args.Length >= 5) { ProgMode = 2; } break;
                    case "BATCHUPLOAD": if (args.Length >= 6) { ProgMode = 11; } break;
                    case "BATCHDOWNLOAD": if (args.Length >= 5) { ProgMode = 12; } break;
                    case "UPLOADBATCH": if (args.Length >= 6) { ProgMode = 11; } break;
                    case "DOWNLOADBATCH": if (args.Length >= 5) { ProgMode = 12; } break;
                    case "CONFIG": ProgMode = 3; break;
                    case "TEST": ProgMode = 13; break;
                    case "FILE": ProgMode = 4; break;
                    case "BATCHFILE": ProgMode = 14; break;
                    case "FILEBATCH": ProgMode = 14; break;
                    case "DIGEST": ProgMode = 5; break;
                    case "BATCHDIGEST": ProgMode = 15; break;
                    case "DIGESTBATCH": ProgMode = 15; break;
                }
            }
            
            
            // Upload mode
            if (((ProgMode == 1) || (ProgMode == 11)) && (args.Length >= 6))
            {
                ItemName = args[1];
                ItemData = args[2];
                ItemMap = args[3];
                if ((ItemMap == "") || (ItemMap == "/"))
                {
                    ItemMap = null;
                }
                List<int[]> AccSrc_ = CommaList(args[4]);
                List<int[]> AccDst_ = CommaList(args[5]);

                for (int i = 0; i < AccSrc_.Count; i++)
                {
                    if ((AccSrc_[i][0] >= 0) && (AccSrc_[i][0] < MailSegment.MailAccountList.Count))
                    {
                        AccSrc.Add(AccSrc_[i][0]);
                        if ((AccSrc_[i][1] != 0) || (AccSrc_[i][2] != 0))
                        {
                            AccSrc.Add(-1);
                        }
                    }
                }
                while ((AccSrc.Count > 0) && (AccSrc[0] < 0))
                {
                    AccSrc.RemoveAt(0);
                }
                while ((AccSrc.Count > 0) && (AccSrc[AccSrc.Count - 1] < 0))
                {
                    AccSrc.RemoveAt(AccSrc.Count - 1);
                }
                for (int i = 0; i < (AccSrc.Count - 1); i++)
                {
                    if ((AccSrc[i] < 0) && (AccSrc[i + 1] < 0))
                    {
                        AccSrc.RemoveAt(i);
                        i--;
                    }
                }
                AccSrc.Insert(0, -1);
                AccSrc.Add(-1);
                
                for (int i = 0; i < AccDst_.Count; i++)
                {
                    if ((AccDst_[i][0] >= 0) && (AccDst_[i][0] < MailSegment.MailAccountList.Count))
                    {
                        AccDst.Add(AccDst_[i][0]);
                    }
                }

                if (args.Length >= 7)
                {
                    if (StrToInt(args[6]) > 0)
                    {
                        SegmentSize = StrToInt(args[6]);
                    }
                }
                if (args.Length >= 8)
                {
                    if ((StrToInt(args[7]) >= 0) && (StrToInt(args[7]) <= 3))
                    {
                        SegmentType = StrToInt(args[7]);
                    }
                }
                if (args.Length >= 9)
                {
                    if (StrToInt(args[8]) > 0)
                    {
                        SegmentImgSize = StrToInt(args[8]);
                    }
                }


                List<string> WelcomeMsg = new List<string>();
                WelcomeMsg.Add("Upload file");
                WelcomeMsg.Add("Item name: " + ItemName);
                WelcomeMsg.Add("Data file: " + ItemData);
                if (ItemMap != null)
                {
                    WelcomeMsg.Add("Map file: " + ItemMap);
                }
                else
                {
                    WelcomeMsg.Add("No map file");
                }
                WelcomeMsg.Add("Source accounts:");
                int GroupN = 0;
                for (int i = 0; i < (AccSrc.Count - 1); i++)
                {
                    if (AccSrc[i] >= 0)
                    {
                        WelcomeMsg.Add("  Account " + AccSrc[i] + " - " + MailSegment.MailAccountList[AccSrc[i]].Address);
                    }
                    else
                    {
                        GroupN++;
                        WelcomeMsg.Add(" Group " + GroupN + ": ");
                    }
                }
                WelcomeMsg.Add("Destination accounts:");
                for (int i = 0; i < AccDst.Count; i++)
                {
                    WelcomeMsg.Add(" Account " + AccDst[i] + " - " + MailSegment.MailAccountList[AccDst[i]].Address);
                }
                WelcomeMsg.Add("Segment size: " + SegmentSize);
                WelcomeMsg.Add("Segment type: " + SegmentTypeDesc[SegmentType]);
                WelcomeMsg.Add("Segment image size: " + SegmentImgSize + "x" + MailSegment.ImgHFromW(SegmentSize, SegmentImgSize));
                for (int i = 0; i < WelcomeMsg.Count; i++)
                {
                    Console.WriteLine(WelcomeMsg[i]);
                }
                Console.WriteLine();
                
                bool Continue = true;
                if (ProgMode == 1)
                {
                    Console.Write("Do you want to continue (Yes/No)? ");
                    Continue = StrToBool(Console.ReadLine());
                }
                if (Continue)
                {
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Log(WelcomeMsg[i]);
                    }
                    MailSegment.FileUpload(ItemName, ItemData, ItemMap, AccSrc.ToArray(), AccDst.ToArray(), SegmentSize, SegmentType, SegmentImgSize);
                }
            }
            
            // Download mode
            if (((ProgMode == 2) || (ProgMode == 12)) && (args.Length >= 5))
            {
                ItemName = args[1];
                ItemData = args[2];
                ItemMap = args[3];
                if ((ItemMap == "") || (ItemMap == "/"))
                {
                    ItemMap = null;
                }
                List<int[]> AccSrc_ = CommaList(args[4]);
                for (int i = 0; i < AccSrc_.Count; i++)
                {
                    if ((AccSrc_[i][0] >= 0) && (AccSrc_[i][0] < MailSegment.MailAccountList.Count))
                    {
                        AccSrc.Add(AccSrc_[i][0]);
                        AccMin.Add(AccSrc_[i][1] > 0 ? AccSrc_[i][1] : -1);
                        AccMax.Add(AccSrc_[i][2] > 0 ? AccSrc_[i][2] : -1);
                        
                        if ((AccMin[AccMin.Count - 1] > 0) && (AccMax[AccMax.Count - 1] > 0))
                        {
                            if ((AccMin[AccMin.Count - 1]) > (AccMax[AccMax.Count - 1]))
                            {
                                AccMin[AccMin.Count - 1] = -1;
                                AccMax[AccMax.Count - 1] = -1;
                            }
                        }
                    }
                }
                MailSegment.FileDownloadMode FileDownloadMode_ = MailSegment.FileDownloadMode.Download;
                if (args.Length >= 6)
                {
                    switch (StrToInt(args[5]))
                    {
                        case 0: FileDownloadMode_ = MailSegment.FileDownloadMode.Download; break;
                        case 1: FileDownloadMode_ = MailSegment.FileDownloadMode.CheckExistHeader; break;
                        case 2: FileDownloadMode_ = MailSegment.FileDownloadMode.CheckExistBody; break;
                        case 3: FileDownloadMode_ = MailSegment.FileDownloadMode.CompareHeader; break;
                        case 4: FileDownloadMode_ = MailSegment.FileDownloadMode.CompareBody; break;
                        case 5: FileDownloadMode_ = MailSegment.FileDownloadMode.DownloadDigest; break;
                        case 6: FileDownloadMode_ = MailSegment.FileDownloadMode.CompareHeaderDigest; break;
                        case 7: FileDownloadMode_ = MailSegment.FileDownloadMode.CompareBodyDigest; break;
                    }
                }
                MailSegment.FileDeleteMode FileDeleteMode_ = MailSegment.FileDeleteMode.None;
                if (args.Length >= 7)
                {
                    List<int[]> DeleteMode__ = CommaList(args[6]);
                    for (int i = 0; i < DeleteMode__.Count; i++)
                    {
                        if ((DeleteMode__[i][0] > 0) && (DeleteMode__[i][0] < DeleteTypeDesc.Length))
                        {
                            switch (DeleteMode__[i][0])
                            {
                                case 1: FileDeleteMode_ = FileDeleteMode_ | MailSegment.FileDeleteMode.Bad; break;
                                case 2: FileDeleteMode_ = FileDeleteMode_ | MailSegment.FileDeleteMode.Duplicate; break;
                                case 3: FileDeleteMode_ = FileDeleteMode_ | MailSegment.FileDeleteMode.ThisFile; break;
                                case 4: FileDeleteMode_ = FileDeleteMode_ | MailSegment.FileDeleteMode.OtherMsg; break;
                                case 5: FileDeleteMode_ = FileDeleteMode_ | MailSegment.FileDeleteMode.OtherFiles; break;
                            }
                        }
                    }
                }

                
                List<string> WelcomeMsg = new List<string>();
                WelcomeMsg.Add("Download or check file");
                WelcomeMsg.Add("Item name: " + ItemName);
                WelcomeMsg.Add("Data file: " + ItemData);
                if (ItemMap != null)
                {
                    WelcomeMsg.Add("Map file: " + ItemMap);
                }
                else
                {
                    WelcomeMsg.Add("No map file");
                }
                WelcomeMsg.Add("Download from accounts:");
                string Temp;

                for (int i = 0; i < AccSrc.Count; i++)
                {
                    Temp = " Account " + AccSrc[i] + " - " + MailSegment.MailAccountList[AccSrc[i]].Address + " - ";
                    if ((AccMin[i] > 0) || (AccMax[i] > 0))
                    {
                        Temp = Temp + "messages";
                        if (AccMin[i] > 0)
                        {
                            Temp = Temp + " from " + AccMin[i];
                        }
                        if (AccMax[i] > 0)
                        {
                            Temp = Temp + " to " + AccMax[i];
                        }
                    }
                    else
                    {
                        Temp = Temp + "all messages";
                    }
                    WelcomeMsg.Add(Temp);
                }

                switch (FileDownloadMode_)
                {
                    case MailSegment.FileDownloadMode.Download: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[0]); break;
                    case MailSegment.FileDownloadMode.CheckExistHeader: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[1]); break;
                    case MailSegment.FileDownloadMode.CheckExistBody: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[2]); break;
                    case MailSegment.FileDownloadMode.CompareHeader: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[3]); break;
                    case MailSegment.FileDownloadMode.CompareBody: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[4]); break;
                    case MailSegment.FileDownloadMode.DownloadDigest: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[5]); break;
                    case MailSegment.FileDownloadMode.CompareHeaderDigest: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[6]); break;
                    case MailSegment.FileDownloadMode.CompareBodyDigest: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[7]); break;
                }

                Temp = "Delete messages: ";
                if (FileDeleteMode_ == MailSegment.FileDeleteMode.None)
                {
                    Temp = Temp + DeleteTypeDesc[0];
                }
                else
                {
                    bool Other = false;
                    if ((FileDeleteMode_ & MailSegment.FileDeleteMode.Bad) == MailSegment.FileDeleteMode.Bad)
                    {
                        if (Other) { Temp = Temp + ", "; }
                        Temp = Temp + DeleteTypeDesc[1];
                        Other = true;
                    }
                    if ((FileDeleteMode_ & MailSegment.FileDeleteMode.Duplicate) == MailSegment.FileDeleteMode.Duplicate)
                    {
                        if (Other) { Temp = Temp + ", "; }
                        Temp = Temp + DeleteTypeDesc[2];
                        Other = true;
                    }
                    if ((FileDeleteMode_ & MailSegment.FileDeleteMode.ThisFile) == MailSegment.FileDeleteMode.ThisFile)
                    {
                        if (Other) { Temp = Temp + ", "; }
                        Temp = Temp + DeleteTypeDesc[3];
                        Other = true;
                    }
                    if ((FileDeleteMode_ & MailSegment.FileDeleteMode.OtherMsg) == MailSegment.FileDeleteMode.OtherMsg)
                    {
                        if (Other) { Temp = Temp + ", "; }
                        Temp = Temp + DeleteTypeDesc[4];
                        Other = true;
                    }
                    if ((FileDeleteMode_ & MailSegment.FileDeleteMode.OtherFiles) == MailSegment.FileDeleteMode.OtherFiles)
                    {
                        if (Other) { Temp = Temp + ", "; }
                        Temp = Temp + DeleteTypeDesc[5];
                        Other = true;
                    }
                }
                WelcomeMsg.Add(Temp);
                
                for (int i = 0; i < WelcomeMsg.Count; i++)
                {
                    Console.WriteLine(WelcomeMsg[i]);
                }
                Console.WriteLine();

                bool Continue = true;
                if (ProgMode == 2)
                {
                    Console.Write("Do you want to continue (Yes/No)? ");
                    Continue = StrToBool(Console.ReadLine());
                }
                if (Continue)
                {
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Log(WelcomeMsg[i]);
                    }
                    MailSegment.FileDownload(ItemName, ItemData, ItemMap, AccSrc.ToArray(), AccMin.ToArray(), AccMax.ToArray(), FileDownloadMode_, FileDeleteMode_);
                }
            }
            
            // Configuration mode
            if ((ProgMode == 3) || (ProgMode == 13))
            {
                MailSegment.ConfigInfo();
                if (args.Length >= 2)
                {
                    bool TestConn = (ProgMode == 13);
                    List<int[]> Acc = CommaList(args[1]);
                    for (int i = 0; i < Acc.Count; i++)
                    {
                        if ((Acc[i][0] >= 0) && (Acc[i][0] < MailSegment.MailAccountList.Count))
                        {
                            Console.WriteLine();
                            Console.WriteLine("Account " + Acc[i][0] + ":");
                            MailSegment.MailAccountList[Acc[i][0]].PrintInfo(TestConn);
                        }
                    }
                }
            }

            // Create file
            if (((ProgMode == 4) || (ProgMode == 14)) && (args.Length >= 3))
            {
                int CreateStats = 0;
                int CreatePeriod = 0;

                long DummyFileSize = 0;

                string DummyName = args[1];
                string FileName = args[2];
                long DummySegmentSize = 0;
                if (args.Length > 3)
                {
                    DummySegmentSize = StrToInt(args[3]);
                }
                if (DummySegmentSize <= 0)
                {
                    DummySegmentSize = MailSegment.DefaultSegmentSize;
                }
                if (args.Length > 4)
                {
                    CreateStats = StrToInt(args[4]);
                    if ((CreateStats < 0) || (CreateStats > 3))
                    {
                        CreateStats = 0;
                    }
                }
                if (args.Length > 5)
                {
                    CreatePeriod = StrToInt(args[5]);
                    if ((CreatePeriod < 0) || (CreatePeriod > 3))
                    {
                        CreatePeriod = 0;
                    }
                }
                Console.WriteLine("Dummy file: " + DummyName);
                Console.WriteLine("Real file: " + FileName);
                Console.WriteLine("Segment size: " + DummySegmentSize);
                switch (CreateStats)
                {
                    case 0: Console.WriteLine("File distribution: None"); break;
                    case 1: Console.WriteLine("File distribution: Simplified dist table"); break;
                    case 2: Console.WriteLine("File distribution: Value list with zeros"); break;
                    case 3: Console.WriteLine("File distribution: Value list without zeros"); break;
                }
                switch (CreatePeriod)
                {
                    case 0: Console.WriteLine("Search period: None"); break;
                    case 1: Console.WriteLine("Search period: Simplified dist table"); break;
                    case 2: Console.WriteLine("Search period: Value list with zeros"); break;
                    case 3: Console.WriteLine("Search period: Value list without zeros"); break;
                }
                Console.WriteLine();

                bool Continue = true;
                if (ProgMode == 4)
                {
                    Console.Write("Do you want to continue (Yes/No)? ");
                    Continue = StrToBool(Console.ReadLine());
                }
                if (Continue)
                {
                    if (DummyName.StartsWith(MailFile.DummyFileSign, StringComparison.InvariantCulture))
                    {
                        RandomSequence RandomSequence_ = RandomSequence.CreateRS(DummyName.Substring(1), MailSegment.RandomCacheStep);
                        if (RandomSequence_ == null)
                        {
                            throw new Exception(RandomSequence.ErrorMsg);
                        }
                        DummyFileSize = RandomSequence.DummyFileSize;
                        long SegmentI = 0;
                        long DispI = 1;
                        long DispL = DummyFileSize / DummySegmentSize;
                        if ((DummyFileSize % DummySegmentSize) > 0)
                        {
                            DispL++;
                        }
                        try
                        {
                            Stopwatch_ SW = new Stopwatch_();
                            FileStream FS_ = new FileStream(FileName, FileMode.Create, FileAccess.Write);

                            while (SegmentI < DummyFileSize)
                            {
                                Console.Write(DispI + "/" + DispL + " - ");
                                if (DummySegmentSize > (DummyFileSize - SegmentI))
                                {
                                    DummySegmentSize = (DummyFileSize - SegmentI);
                                }

                                if (CreateStats > 0)
                                {
                                    RandomSequence_.StatsEnabled = true;
                                    RandomSequence_.StatsReset();
                                }
                                byte[] Raw = RandomSequence_.GenSeq(SegmentI, DummySegmentSize);
                                FS_.Write(Raw, 0, (int)DummySegmentSize);

                                SegmentI += DummySegmentSize;
                                DispI++;
                                Console.WriteLine("OK - " + MailSegment.TimeHMSM(SW.Elapsed()));
                            }
                            FS_.Close();
                            Console.WriteLine("File created in time: " + MailSegment.TimeHMSM(SW.Elapsed()));
                            if (CreateStats > 0)
                            {
                                Console.WriteLine();
                                DisplayStats("File distribution", RandomSequence_.Stats, CreateStats);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("File creation error: " + e.Message);
                        }


                        if (CreatePeriod > 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Searching for sequence period");

                            FileStream FS = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                            long PeriodBufferSize = DummySegmentSize;
                            byte[] PeriodArray0 = new byte[PeriodBufferSize];
                            byte[] PeriodArray1 = new byte[PeriodBufferSize];

                            if (PeriodBufferSize > DummyFileSize)
                            {
                                PeriodBufferSize = (int)DummyFileSize;
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
                            for (long i = 1; i < DummyFileSize; i++)
                            {
                                FS.Seek(i - 1, SeekOrigin.Begin);
                                FS.Read(PeriodArray0, 0, 1);
                                PeriodStats[PeriodArray0[0]]++;

                                bool IsPeriodical = true;

                                PeriodChunkSize = PeriodBufferSize;
                                PeriodChunkSize0 = i;
                                PeriodChunks = (int)(i / PeriodBufferSize);
                                if ((i % PeriodBufferSize) > 0)
                                {
                                    PeriodChunks++;
                                }

                                int PeriodCount = (int)(DummyFileSize / i);
                                if ((DummyFileSize % i) > 0)
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
                                    FS.Seek(PeriodFilePos, SeekOrigin.Begin);
                                    FS.Read(PeriodArray0, 0, (int)PeriodChunkSize);

                                    int PeriodChunkSize__ = 0;
                                    for (int iii = (PeriodCount - 2); iii >= 0; iii--)
                                    {
                                        PeriodFilePos += i;
                                        PeriodChunkSize__ = (int)PeriodChunkSize;
                                        if (iii == 0)
                                        {
                                            int FileRemain = (int)(DummyFileSize - PeriodFilePos);
                                            if (PeriodChunkSize__ > FileRemain)
                                            {
                                                PeriodChunkSize__ = FileRemain;
                                            }
                                        }

                                        // Read the period occurence other than first and compare with pattern,
                                        // if doest match the pattern, the reading and comparing will be broken
                                        if (PeriodChunkSize__ > 0)
                                        {
                                            FS.Seek(PeriodFilePos, SeekOrigin.Begin);
                                            FS.Read(PeriodArray1, 0, (int)PeriodChunkSize);

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

                                        if (WorkTime < SW__.Elapsed())
                                        {
                                            while (WorkTime < SW__.Elapsed())
                                            {
                                                WorkTime += 1000L;
                                            }
                                            Console.WriteLine("Period " + i + "/" + DummyFileSize + " (" + (DummyFileSize > 0 ? (i * 100 / DummyFileSize) : 0) + "%); occurence " + (PeriodCount - iii - 1) + "/" + PeriodCount + " (" + (PeriodCount > 0 ? (((PeriodCount - iii - 1) * 100) / PeriodCount) : 0) + "%)");
                                        }
                                    }
                                    PeriodChunkOffset += (int)PeriodChunkSize;

                                    PeriodChunkSize0 -= PeriodChunkSize;

                                }

                                if (IsPeriodical)
                                {
                                    PeriodSize = i;
                                    break;
                                }
                            }
                            FS.Close();

                            if (PeriodSize > 0)
                            {
                                Console.WriteLine("Sequence period length: " + PeriodSize);
                                Console.WriteLine("Period search time: " + MailSegment.TimeHMSM(SW__.Elapsed()));
                                Console.WriteLine();
                                DisplayStats("Sequence period distribution", PeriodStats, CreatePeriod);
                            }
                            else
                            {
                                Console.WriteLine("Sequence has no period");
                                Console.WriteLine("Period search time: " + MailSegment.TimeHMSM(SW__.Elapsed()));
                                Console.WriteLine();
                            }

                        }
                    }
                }
            }

            // Create or check digest
            if (((ProgMode == 5) || (ProgMode == 15)) && (args.Length > 3))
            {
                ItemData = args[1];
                ItemMap = args[2];
                int SegS = -1;
                if (args.Length > 4)
                {
                    if (StrToInt(args[4]) > 0)
                    {
                        SegS = StrToInt(args[4]);
                    }
                }

                if (SegS <= 0)
                {
                    SegS = MailSegment.DefaultSegmentSize;
                }

                if ((StrToInt(args[3]) == 0) || (StrToInt(args[3]) == 1))
                {
                    DigestFile DF_ = new DigestFile();
                    if (StrToInt(args[3]) == 0)
                    {
                        Console.WriteLine("Create digest file");
                    }
                    if (StrToInt(args[3]) == 1)
                    {
                        Console.WriteLine("Check digest file");
                    }
                    Console.WriteLine("Data file: " + ItemData);
                    Console.WriteLine("Digest file: " + ItemMap);
                    Console.WriteLine("Segment size: " + SegS);
                    Console.WriteLine();

                    bool Continue = true;
                    if (ProgMode == 5)
                    {
                        Console.Write("Do you want to continue (Yes/No)? ");
                        Continue = StrToBool(Console.ReadLine());
                    }
                    if (Continue)
                    {
                        if (StrToInt(args[3]) == 0)
                        {
                            DF_.Proc(true, ItemData, ItemMap, SegS);
                        }
                        if (StrToInt(args[3]) == 1)
                        {
                            DF_.Proc(false, ItemData, ItemMap, SegS);
                        }
                    }
                }
            }

            // Help and information
            if (ProgMode == 0)
            {
                Console.WriteLine("BackupToMail - command-line application to use mailbox as backup storage.");
                Console.WriteLine("");
                Console.WriteLine("Upload file:");
                Console.WriteLine("BackupToMail UPLOAD <item name> <data file> <map file>");
                Console.WriteLine("<source account list by commas> <destination account list by commas>");
                Console.WriteLine("[<segment size> <segment type> <image width>]");
                Console.WriteLine("Segment types:");
                Console.WriteLine(" 0 - " + SegmentTypeDesc[0] + " (default)");
                Console.WriteLine(" 1 - " + SegmentTypeDesc[1]);
                Console.WriteLine(" 2 - " + SegmentTypeDesc[2]);
                Console.WriteLine(" 3 - " + SegmentTypeDesc[3]);
                Console.WriteLine();
                Console.WriteLine("Download file:");
                Console.WriteLine("BackupToMail DOWNLOAD <item name> <data file> <map file> <account list>");
                Console.WriteLine("[<download or check mode> <delete option list by commas>]");
                Console.WriteLine("Download or check modes:");
                Console.WriteLine(" 0 - " + DownloadTypeDesc[0] + " (default)");
                Console.WriteLine(" 1 - " + DownloadTypeDesc[1]);
                Console.WriteLine(" 2 - " + DownloadTypeDesc[2]);
                Console.WriteLine(" 3 - " + DownloadTypeDesc[3]);
                Console.WriteLine(" 4 - " + DownloadTypeDesc[4]);
                Console.WriteLine(" 5 - " + DownloadTypeDesc[5]);
                Console.WriteLine(" 6 - " + DownloadTypeDesc[6]);
                Console.WriteLine(" 7 - " + DownloadTypeDesc[7]);
                Console.WriteLine("Delete options:");
                Console.WriteLine(" 0 - " + DeleteTypeDesc[0] + " (default, ignored if other provided)");
                Console.WriteLine(" 1 - " + DeleteTypeDesc[1]);
                Console.WriteLine(" 2 - " + DeleteTypeDesc[2]);
                Console.WriteLine(" 3 - " + DeleteTypeDesc[3]);
                Console.WriteLine(" 4 - " + DeleteTypeDesc[4]);
                Console.WriteLine(" 5 - " + DeleteTypeDesc[5]);
                Console.WriteLine();
                Console.WriteLine("Create or check digest file:");
                Console.WriteLine("BackupToMail DIGEST <mode> <data file> <digest file> [<segment size>]");
                Console.WriteLine("Available modes:");
                Console.WriteLine(" 0 - Create the digest file from the data file");
                Console.WriteLine(" 1 - Check the digest file against the data file");
                Console.WriteLine();
                Console.WriteLine("Create file based on dummy file generator:");
                Console.WriteLine("BackupToMail FILE <dummy file definition> <file name>");
                Console.WriteLine("[<segment size> <dist mode> <period mode>]");
                Console.WriteLine("Dist mode and period modes possible values:");
                Console.WriteLine(" 0 - None (default)");
                Console.WriteLine(" 1 - Simplified dist table");
                Console.WriteLine(" 2 - Value list with zeros");
                Console.WriteLine(" 3 - Value list without zeros");
                Console.WriteLine();
                Console.WriteLine("Print general and account configuration without connection test:");
                Console.WriteLine("BackupToMail CONFIG <account list by commas>");
                Console.WriteLine();
                Console.WriteLine("Print general and account configuration with connection test:");
                Console.WriteLine("BackupToMail TEST <account list by commas>");
                Console.WriteLine();
            }
            Console.WriteLine();
        }

    
        public static void DisplayStats(string Msg, ulong[] StatData, int Mode)
        {
            ulong ValMax = 0;
            ulong ValMin = ulong.MaxValue;
            ulong ValMin0 = ulong.MaxValue;
            ulong ValSum = 0;
            ulong ValCount0 = 0;
            for (int i = 0; i < 256; i++)
            {
                if (ValMin > StatData[i])
                {
                    ValMin = StatData[i];
                }
                if (StatData[i] > 0)
                {
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

            if (Mode == 1)
            {
                ulong GreatVal = ValMax / 10000UL;
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

                Console.WriteLine(Msg + " n/" + ValD + ":");
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

                    Console.Write(Stat_[i].ToString().PadLeft(4, ' '));
                    if (((i + 1) % 16) == 0)
                    {
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
            }
            if ((Mode == 2) || (Mode == 3))
            {
                Console.WriteLine(Msg + ":");

                int PadI = (int)(Math.Floor(Math.Log10(ValMax)) + 1);
                for (int i = 0; i < 256; i++)
                {
                    if ((Mode == 2) || (StatData[i] > 0))
                    {
                        Console.Write(i.ToString().PadLeft(3, ' '));
                        Console.Write(" - ");
                        Console.Write(StatData[i].ToString().PadLeft(PadI, ' '));
                        Console.WriteLine();
                    }
                }
            }
            Console.WriteLine("Minimum including 0: " + ValMin);
            if (ValCount0 > 0)
            {
                Console.WriteLine("Minimum excluding 0: " + ValMin0);
            }
            Console.WriteLine("Maximum: " + ValMax);
            Console.WriteLine("Sum: " + ValSum);
            Console.WriteLine("Number of non-zeros: " + ValCount0);
            Console.WriteLine("Number of zeros: " + (255 - ValCount0));
        }
    }
}