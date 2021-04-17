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
            string[] ItemName = new string[1];
            string[] ItemData = new string[1];
            string[] ItemMap = new string[1];
            int SegmentSize = MailSegment.DefaultSegmentSize;
            int SegmentType = MailSegment.DefaultSegmentType;
            int SegmentImgSize = MailSegment.DefaultImageSize;
            List<int> AccSrc = new List<int>();
            List<int> AccDst = new List<int>();
            List<int> AccMin = new List<int>();
            List<int> AccMax = new List<int>();

            string[] SegmentTypeDesc = new string[]
            {
                "Binary attachment, ascending segment order",
                "PNG image attachment, ascending segment order",
                "Base64 in plain text body, ascending segment order",
                "PNG image in HTML body, ascending segment order",
                "Binary attachment, ascending segment order",
                "Binary attachment, ascending segment order",
                "Binary attachment, ascending segment order",
                "Binary attachment, ascending segment order",
                "Binary attachment, ascending segment order",
                "Binary attachment, ascending segment order",
                "Binary attachment, descending segment order",
                "PNG image attachment, descending segment order",
                "Base64 in plain text body, descending segment order",
                "PNG image in HTML body, descending segment order",
                "Binary attachment, descending segment order",
                "Binary attachment, descending segment order",
                "Binary attachment, descending segment order",
                "Binary attachment, descending segment order",
                "Binary attachment, descending segment order",
                "Binary attachment, descending segment order"
            };            

            string[] DownloadTypeDesc = new string[]
            {
                "Download data file, forward browsing direction",
                "Check existence without body control, forward browsing direction",
                "Check existence with body control, forward browsing direction",
                "Check the header digest using data file, forward browsing direction",
                "Check the body contents using data file, forward browsing direction",
                "Download digest file, forward browsing direction",
                "Check the header digest using digest file, forward browsing direction",
                "Check the body contents using digest file, forward browsing direction",
                "",
                "",
                "Download data file, backward browsing direction",
                "Check existence without body control, backward browsing direction",
                "Check existence with body control, backward browsing direction",
                "Check the header digest using data file, backward browsing direction",
                "Check the body contents using data file, backward browsing direction",
                "Download digest file, backward browsing direction",
                "Check the header digest using digest file, backward browsing direction",
                "Check the body contents using digest file, backward browsing direction",
                "",
                ""
            };            

            string[] DeleteTypeDesc = new string[]
            {
                "None",
                "Bad",
                "Duplicate",
                "This file",
                "Other messages",
                "Other files",
                "Undownloadable"
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
                    case "FILE": ProgMode = 4; break;
                    case "BATCHFILE": ProgMode = 14; break;
                    case "FILEBATCH": ProgMode = 14; break;
                    case "DIGEST": ProgMode = 5; break;
                    case "BATCHDIGEST": ProgMode = 15; break;
                    case "DIGESTBATCH": ProgMode = 15; break;
                    case "MAP": ProgMode = 6; break;
                }
            }
            
            
            // Upload mode
            if (((ProgMode == 1) || (ProgMode == 11)) && (args.Length >= 6))
            {
                ItemName[0] = args[1];
                ItemData[0] = args[2];
                ItemMap[0] = args[3];
                if (MailSegment.NameSeparator.Length > 0)
                {
                    ItemName = args[1].Split(MailSegment.NameSeparator[0]);
                    ItemData = args[2].Split(MailSegment.NameSeparator[0]);
                    ItemMap = args[3].Split(MailSegment.NameSeparator[0]);
                }
                for (int i_ = 0; i_ < ItemData.Length; i_++)
                {
                    if ((ItemData[i_] == "") || (ItemData[i_] == "/"))
                    {
                        ItemData[i_] = null;
                    }
                    else
                    {
                        ItemData[i_] = MailFile.FileNameToPath(ItemData[i_]);
                    }
                }
                for (int i_ = 0; i_ < ItemMap.Length; i_++)
                {
                    if ((ItemMap[i_] == "") || (ItemMap[i_] == "/"))
                    {
                        ItemMap[i_] = null;
                    }
                    else
                    {
                        ItemMap[i_] = MailFile.FileNameToPath(ItemMap[i_]);
                    }
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
                    if (StrToInt(args[7]) >= 0)
                    {
                        SegmentType = StrToInt(args[7]) % 20;
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
                WelcomeMsg.Add("Upload files");
                int ItemCount = Math.Min(Math.Min(ItemName.Length, ItemData.Length), ItemMap.Length);
                for (int i_ = 0; i_ < ItemCount; i_++)
                {
                    WelcomeMsg.Add("Item " + (i_ + 1).ToString() + ":");
                    WelcomeMsg.Add(" Item name: " + ItemName[i_]);
                    if (ItemData[i_] != null)
                    {
                        WelcomeMsg.Add(" Data file: " + ItemData[i_]);
                    }
                    else
                    {
                        WelcomeMsg.Add(" No data file");
                    }
                    if (ItemMap[i_] != null)
                    {
                        WelcomeMsg.Add(" Map file: " + ItemMap[i_]);
                    }
                    else
                    {
                        WelcomeMsg.Add(" No map file");
                    }
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
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                        MailSegment.LogSum(WelcomeMsg[i]);
                    }
                    MailSegment.FileUpload(ItemCount, ItemName, ItemData, ItemMap, AccSrc.ToArray(), AccDst.ToArray(), SegmentSize, SegmentType, SegmentImgSize);
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                }
                return;
            }

            // Download mode
            if (((ProgMode == 2) || (ProgMode == 12)) && (args.Length >= 5))
            {
                ItemName[0] = args[1];
                ItemData[0] = args[2];
                ItemMap[0] = args[3];
                if (MailSegment.NameSeparator.Length > 0)
                {
                    ItemName = args[1].Split(MailSegment.NameSeparator[0]);
                    ItemData = args[2].Split(MailSegment.NameSeparator[0]);
                    ItemMap = args[3].Split(MailSegment.NameSeparator[0]);
                }
                for (int i_ = 0; i_ < ItemData.Length; i_++)
                {
                    if ((ItemData[i_] == "") || (ItemData[i_] == "/"))
                    {
                        ItemData[i_] = null;
                    }
                    else
                    {
                        ItemData[i_] = MailFile.FileNameToPath(ItemData[i_]);
                    }
                }
                for (int i_ = 0; i_ < ItemMap.Length; i_++)
                {
                    if ((ItemMap[i_] == "") || (ItemMap[i_] == "/"))
                    {
                        ItemMap[i_] = null;
                    }
                    else
                    {
                        ItemMap[i_] = MailFile.FileNameToPath(ItemMap[i_]);
                    }
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
                bool FileDownloadReverseOrder = false;
                MailSegment.FileDownloadMode FileDownloadMode_ = MailSegment.FileDownloadMode.Download;
                if (args.Length >= 6)
                {
                    switch (StrToInt(args[5]) % 10)
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
                    if ((StrToInt(args[5]) % 20) >= 10)
                    {
                        FileDownloadReverseOrder = true;
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
                                case 6: FileDeleteMode_ = FileDeleteMode_ | MailSegment.FileDeleteMode.Undownloadable; break;
                            }
                        }
                    }
                }

                
                List<string> WelcomeMsg = new List<string>();
                WelcomeMsg.Add("Download or check file");
                int ItemCount = Math.Min(Math.Min(ItemName.Length, ItemData.Length), ItemMap.Length);

                for (int i_ = 0; i_ < ItemCount; i_++)
                {
                    WelcomeMsg.Add("Item " + (i_ + 1).ToString() + ":");
                    WelcomeMsg.Add(" Item name: " + ItemName[i_]);
                    if (ItemData[i_] != null)
                    {
                        WelcomeMsg.Add(" Data file: " + ItemData[i_]);
                    }
                    else
                    {
                        WelcomeMsg.Add(" No data file");
                    }
                    if (ItemMap[i_] != null)
                    {
                        WelcomeMsg.Add(" Map file: " + ItemMap[i_]);
                    }
                    else
                    {
                        WelcomeMsg.Add(" No map file");
                    }
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

                int RevOpt = 0;
                if (FileDownloadReverseOrder)
                {
                    RevOpt = 10;
                }
                switch (FileDownloadMode_)
                {
                    case MailSegment.FileDownloadMode.Download: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[0 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CheckExistHeader: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[1 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CheckExistBody: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[2 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareHeader: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[3 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareBody: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[4 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.DownloadDigest: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[5 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareHeaderDigest: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[6 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareBodyDigest: WelcomeMsg.Add("Download or check mode: " + DownloadTypeDesc[7 + RevOpt]); break;
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
                    if ((FileDeleteMode_ & MailSegment.FileDeleteMode.Undownloadable) == MailSegment.FileDeleteMode.Undownloadable)
                    {
                        if (Other) { Temp = Temp + ", "; }
                        Temp = Temp + DeleteTypeDesc[6];
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
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                        MailSegment.LogSum(WelcomeMsg[i]);
                    }
                    MailSegment.FileDownload(ItemCount, ItemName, ItemData, ItemMap, AccSrc.ToArray(), AccMin.ToArray(), AccMax.ToArray(), FileDownloadMode_, FileDeleteMode_, FileDownloadReverseOrder);
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                }
                return;
            }

            // Configuration mode
            if (ProgMode == 3)
            {
                int TestConn = 0;
                if (args.Length >= 3)
                {
                    TestConn = StrToInt(args[2]);
                    if (TestConn < 0)
                    {
                        TestConn = 0;
                    }
                }
                if (TestConn != 2)
                {
                    MailSegment.ConfigInfo();
                }
                if (args.Length >= 2)
                {
                    List<int[]> Acc = CommaList(args[1]);
                    if (TestConn == 2)
                    {
                        for (int i = 0; i < Acc.Count; i++)
                        {
                            if ((Acc[i][0] >= 0) && (Acc[i][0] < MailSegment.MailAccountList.Count))
                            {
                                Console.WriteLine("Account " + Acc[i][0] + ": " + MailSegment.MailAccountList[Acc[i][0]].Address);
                            }
                        }
                        Console.WriteLine();
                    }
                    for (int i = 0; i < Acc.Count; i++)
                    {
                        if ((Acc[i][0] >= 0) && (Acc[i][0] < MailSegment.MailAccountList.Count))
                        {
                            if (TestConn < 2)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Account " + Acc[i][0] + ":");
                                MailSegment.MailAccountList[Acc[i][0]].PrintInfo(TestConn);
                            }
                            if (TestConn == 2)
                            {
                                MailSegment.MailAccountList[Acc[i][0]].PrintConnTest(Acc[i][0]);
                            }
                        }
                    }
                }
                return;
            }

            // Create file
            if (((ProgMode == 4) || (ProgMode == 14)) && (args.Length >= 3))
            {
                int CreateStats = 0;
                int CreatePeriod = 0;

                long DummyFileSize = 0;

                string DummyName = MailFile.FileNameToPath(args[1]);
                string FileName = MailFile.FileNameToPath(args[2]);
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
                    case 0: Console.WriteLine("File statistics: No statistics"); break;
                    case 1: Console.WriteLine("File statistics: Simplified distribution table"); break;
                    case 2: Console.WriteLine("File statistics: Value list with zeros"); break;
                    case 3: Console.WriteLine("File statistics: Value list without zeros"); break;
                }
                switch (CreatePeriod)
                {
                    case 0: Console.WriteLine("Period statistics: No statistics (period will not be searched)"); break;
                    case 1: Console.WriteLine("Period statistics: Simplified distribution table"); break;
                    case 2: Console.WriteLine("Period statistics: Value list with zeros"); break;
                    case 3: Console.WriteLine("Period statistics: Value list without zeros"); break;
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
                        if (RandomSequence_ != null)
                        {
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

                                if (CreateStats > 0)
                                {
                                    RandomSequence_.StatsReset(true);
                                }
                                else
                                {
                                    RandomSequence_.StatsReset(false);
                                }
                                while (SegmentI < DummyFileSize)
                                {
                                    Console.Write(DispI + "/" + DispL + " - ");
                                    if (DummySegmentSize > (DummyFileSize - SegmentI))
                                    {
                                        DummySegmentSize = (DummyFileSize - SegmentI);
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

                                    long PeriodCount = (DummyFileSize / i);
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
                                        for (long iii = (PeriodCount - 2); iii >= 0; iii--)
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
                        else
                        {
                            Console.WriteLine(RandomSequence.ErrorMsg);
                        }
                    }
                }
                return;
            }

            // Create or check digest
            if (((ProgMode == 5) || (ProgMode == 15)) && (args.Length > 3))
            {
                ItemData[0] = MailFile.FileNameToPath(args[2]);
                ItemMap[0] = MailFile.FileNameToPath(args[3]);
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

                if ((StrToInt(args[1]) == 0) || (StrToInt(args[1]) == 1))
                {
                    DigestFile DF_ = new DigestFile();
                    if (StrToInt(args[1]) == 0)
                    {
                        Console.WriteLine("Create digest file");
                    }
                    if (StrToInt(args[1]) == 1)
                    {
                        Console.WriteLine("Check digest file");
                    }
                    Console.WriteLine("Data file: " + ItemData[0]);
                    Console.WriteLine("Digest file: " + ItemMap[0]);
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
                        if (StrToInt(args[1]) == 0)
                        {
                            DF_.Proc(true, ItemData[0], ItemMap[0], SegS);
                        }
                        if (StrToInt(args[1]) == 1)
                        {
                            DF_.Proc(false, ItemData[0], ItemMap[0], SegS);
                        }
                    }
                    return;
                }
            }


            // Map file information
            if ((ProgMode == 6) && (args.Length > 3))
            {
                ItemData[0] = args[2];
                ItemMap[0] = args[3];
                if (MailSegment.NameSeparator.Length > 0)
                {
                    ItemData = args[2].Split(MailSegment.NameSeparator[0]);
                    ItemMap = args[3].Split(MailSegment.NameSeparator[0]);
                }
                for (int i_ = 0; i_ < ItemData.Length; i_++)
                {
                    if ((ItemData[i_] == "") || (ItemData[i_] == "/"))
                    {
                        ItemData[i_] = null;
                    }
                    else
                    {
                        ItemData[i_] = MailFile.FileNameToPath(ItemData[i_]);
                    }
                }
                for (int i_ = 0; i_ < ItemMap.Length; i_++)
                {
                    if ((ItemMap[i_] == "") || (ItemMap[i_] == "/"))
                    {
                        ItemMap[i_] = null;
                    }
                    else
                    {
                        ItemMap[i_] = MailFile.FileNameToPath(ItemMap[i_]);
                    }
                }

                if (args.Length > 4)
                {
                    if (StrToInt(args[4]) > 0)
                    {
                        SegmentSize = StrToInt(args[4]);
                    }
                }

                if ((StrToInt(args[1]) >= 0) && (StrToInt(args[1]) <= 3))
                {
                    bool IsDigest = false;
                    bool FullInfo = true;
                    if ((StrToInt(args[1]) == 2) || (StrToInt(args[1]) == 3))
                    {
                        FullInfo = false;
                    }

                    int FileCount = Math.Min(ItemData.Length, ItemMap.Length);
                    for (int i_ = 0; i_ < FileCount; i_++)
                    {
                        if (FullInfo)
                        {
                            Console.WriteLine("Item " + (i_ + 1).ToString() + ":");
                        }

                        if ((StrToInt(args[1]) == 0) || (StrToInt(args[1]) == 2))
                        {
                            IsDigest = false;
                            if (FullInfo)
                            {
                                if (ItemData[0] != null)
                                {
                                    Console.WriteLine(" Data file: " + ItemData[i_]);
                                }
                                else
                                {
                                    Console.WriteLine(" No data file");
                                }
                            }
                            else
                            {
                                if (ItemData[0] != null)
                                {
                                    Console.Write(Path.GetFileName(ItemData[i_]));
                                }
                            }
                        }
                        if ((StrToInt(args[1]) == 1) || (StrToInt(args[1]) == 3))
                        {
                            IsDigest = true;
                            if (FullInfo)
                            {
                                if (ItemData[0] != null)
                                {
                                    Console.WriteLine(" Digest file: " + ItemData[i_]);
                                }
                                else
                                {
                                    Console.WriteLine(" No digest file");
                                }
                            }
                            else
                            {
                                if (ItemData[0] != null)
                                {
                                    Console.Write(Path.GetFileName(ItemData[i_]));
                                }
                            }
                        }
                        if (FullInfo)
                        {
                            if (ItemMap[0] != null)
                            {
                                Console.WriteLine(" Map file: " + ItemMap[i_]);
                            }
                            else
                            {
                                Console.WriteLine(" No map file");
                            }
                        }
                        else
                        {
                            if (ItemMap[0] != null)
                            {
                                Console.Write("/");
                                Console.Write(Path.GetFileName(ItemMap[i_]));
                            }
                            Console.Write(": ");
                        }

                        MailFile MF = new MailFile();
                        if (MF.Open(IsDigest, true, ItemData[i_], ItemMap[i_]))
                        {
                            if (IsDigest)
                            {
                                if (FullInfo)
                                {
                                    Console.WriteLine(" Segment size: " + MF.DigestSegmentSize.ToString());
                                }
                                MF.SetSegmentSize(MF.DigestSegmentSize);
                            }
                            else
                            {
                                if (FullInfo)
                                {
                                    Console.WriteLine(" Segment size: " + SegmentSize.ToString());
                                }
                                MF.SetSegmentSize(SegmentSize);
                            }
                            MF.CalcSegmentCount();
                            MF.MapCalcStats();
                            if (FullInfo)
                            {
                                Console.WriteLine(" File size: " + MF.GetDataSize().ToString());
                                Console.WriteLine(" Total segments: " + MF.GetSegmentCount().ToString());
                                Console.WriteLine(" Segments good previously: " + MF.MapCount(2).ToString());
                                Console.WriteLine(" Good segments: " + MF.MapCount(1).ToString());
                                Console.WriteLine(" Bad or missing segments: " + MF.MapCount(0).ToString());
                            }
                            else
                            {
                                Console.Write(MF.GetSegmentCount().ToString());
                                Console.Write(" - ");
                                Console.Write(MF.MapCount(2).ToString());
                                Console.Write(" - ");
                                Console.Write(MF.MapCount(1).ToString());
                                Console.Write(" - ");
                                Console.Write(MF.MapCount(0).ToString());

                                Console.WriteLine();
                            }
                            MF.Close();
                        }
                        else
                        {
                            Console.WriteLine(" File open error: " + MF.OpenError);
                        }
                    }
                    return;
                }
            }

            // Help and information
            Console.WriteLine("BackupToMail - command-line application to use mailbox as backup storage.");
            Console.WriteLine("");
            Console.WriteLine("Upload file:");
            Console.WriteLine("BackupToMail UPLOAD <item name> <data file> <map file>");
            Console.WriteLine("<source account list by commas> <destination account list by commas>");
            Console.WriteLine("[<segment size> <segment type> <image width>]");
            Console.WriteLine("Segment types:");
            Console.WriteLine(" 0 - " + SegmentTypeDesc[0] + (((MailSegment.DefaultSegmentType == 0) || ((MailSegment.DefaultSegmentType >= 4) && (MailSegment.DefaultSegmentType <= 9))) ? " (default)" : ""));
            Console.WriteLine(" 1 - " + SegmentTypeDesc[1] + ((MailSegment.DefaultSegmentType == 1) ? " (default)" : ""));
            Console.WriteLine(" 2 - " + SegmentTypeDesc[2] + ((MailSegment.DefaultSegmentType == 2) ? " (default)" : ""));
            Console.WriteLine(" 3 - " + SegmentTypeDesc[3] + ((MailSegment.DefaultSegmentType == 3) ? " (default)" : ""));
            Console.WriteLine(" 10 - " + SegmentTypeDesc[10] + (((MailSegment.DefaultSegmentType == 10) || ((MailSegment.DefaultSegmentType >= 14) && (MailSegment.DefaultSegmentType <= 19))) ? " (default)" : ""));
            Console.WriteLine(" 11 - " + SegmentTypeDesc[11] + ((MailSegment.DefaultSegmentType == 11) ? " (default)" : ""));
            Console.WriteLine(" 12 - " + SegmentTypeDesc[12] + ((MailSegment.DefaultSegmentType == 12) ? " (default)" : ""));
            Console.WriteLine(" 13 - " + SegmentTypeDesc[13] + ((MailSegment.DefaultSegmentType == 13) ? " (default)" : ""));
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
            Console.WriteLine(" 10 - " + DownloadTypeDesc[10]);
            Console.WriteLine(" 11 - " + DownloadTypeDesc[11]);
            Console.WriteLine(" 12 - " + DownloadTypeDesc[12]);
            Console.WriteLine(" 13 - " + DownloadTypeDesc[13]);
            Console.WriteLine(" 14 - " + DownloadTypeDesc[14]);
            Console.WriteLine(" 15 - " + DownloadTypeDesc[15]);
            Console.WriteLine(" 16 - " + DownloadTypeDesc[16]);
            Console.WriteLine(" 17 - " + DownloadTypeDesc[17]);
            Console.WriteLine("Delete options:");
            Console.WriteLine(" 0 - " + DeleteTypeDesc[0] + " (default, ignored if other provided)");
            Console.WriteLine(" 1 - " + DeleteTypeDesc[1]);
            Console.WriteLine(" 2 - " + DeleteTypeDesc[2]);
            Console.WriteLine(" 3 - " + DeleteTypeDesc[3]);
            Console.WriteLine(" 4 - " + DeleteTypeDesc[4]);
            Console.WriteLine(" 5 - " + DeleteTypeDesc[5]);
            Console.WriteLine(" 6 - " + DeleteTypeDesc[6]);
            Console.WriteLine();
            Console.WriteLine("Print map file information:");
            Console.WriteLine("BackupToMail MAP <mode> <data file> <map file> [<segment size>]");
            Console.WriteLine("Available modes:");
            Console.WriteLine(" 0 - Use data file (real file or dummy file)");
            Console.WriteLine(" 1 - Use digest file");
            Console.WriteLine(" 2 - Use data file, brief information");
            Console.WriteLine(" 3 - Use digest file, brief information");
            Console.WriteLine();
            Console.WriteLine("Create or check digest file:");
            Console.WriteLine("BackupToMail DIGEST <mode> <data file> <digest file> [<segment size>]");
            Console.WriteLine("Available modes:");
            Console.WriteLine(" 0 - Create the digest file from the data file (default)");
            Console.WriteLine(" 1 - Check the digest file against the data file");
            Console.WriteLine();
            Console.WriteLine("Create file based on dummy file generator:");
            Console.WriteLine("BackupToMail FILE <dummy file definition> <file name>");
            Console.WriteLine("[<segment size> <file stats mode> <period stats mode>]");
            Console.WriteLine("File stats modes and period stats modes:");
            Console.WriteLine(" 0 - No statistics (default)");
            Console.WriteLine(" 1 - Simplified distribution table");
            Console.WriteLine(" 2 - Value list with zeros");
            Console.WriteLine(" 3 - Value list without zeros");
            Console.WriteLine();
            Console.WriteLine("Print configuration and connection test:");
            Console.WriteLine("BackupToMail CONFIG <account list by commas> [<connection test mode>]");
            Console.WriteLine("Connection test modes:");
            Console.WriteLine(" 0 - Print configuration without test (default)");
            Console.WriteLine(" 1 - Connection test and print full configuration");
            Console.WriteLine(" 2 - Connection test and print test results only");
            Console.WriteLine();
        }


        public static void DisplayStats(string Msg, ulong[] StatData, int Mode)
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
            Console.WriteLine("Average including 0: " + ValAvg);
            if (ValCount0 > 0)
            {
                Console.WriteLine("Average excluding 0: " + ValAvg0);
            }

            ValDev = (ulong)(Math.Sqrt((double)ValDev / 256.0) * 1000.0);
            Console.WriteLine("Standard deviation x1000 including 0: " + ValDev);
            if (ValCount0 > 0)
            {
                ValDev0 = (ulong)(Math.Sqrt((double)ValDev0 / (double)ValCount0) * 1000.0);
                Console.WriteLine("Standard deviation x1000 excluding 0: " + ValDev0);
            }

            Console.WriteLine("Sum: " + ValSum);
            Console.WriteLine("Number of non-zeros: " + ValCount0);
            Console.WriteLine("Number of zeros: " + (256UL - ValCount0));
        }
    }
}