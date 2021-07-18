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
        /// Converts string to integer, returns -1 if convert is not possible 
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        static long StrToLong(string S)
        {
            long I;
            if (long.TryParse(S, out I))
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
            if (S == null)
            {
                return false;
            }
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

        public static bool PromptContinue(bool NeedPrompt)
        {
            if (NeedPrompt)
            {
                Console.Write("Do you want to continue (Yes/No)? ");
                string X = "";
                while (X == "")
                {
                    X = Console.ReadLine();
                    if (X == null)
                    {
                        return false;
                    }
                    X = X.Trim();
                }
                return StrToBool(X);
            }
            else
            {
                return true;
            }
        }

        public static bool PromptConfirm(bool NeedPrompt)
        {
            if (NeedPrompt)
            {
                Console.Write("Do you want to continue (Yes/No)? ");
                string X = "";
                while (X == "")
                {
                    X = Console.ReadLine();
                    if (X == null)
                    {
                        return false;
                    }
                    X = X.Trim();
                }
                return StrToBool(X);
            }
            else
            {
                return true;
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
                    case "BATCHUPLOAD": if (args.Length >= 6) { ProgMode = 11; } break;
                    case "BATCHDOWNLOAD": if (args.Length >= 5) { ProgMode = 12; } break;

                    case "DOWNLOAD": if (args.Length >= 5) { ProgMode = 2; } break;
                    case "UPLOADBATCH": if (args.Length >= 6) { ProgMode = 11; } break;
                    case "DOWNLOADBATCH": if (args.Length >= 5) { ProgMode = 12; } break;
                    
                    case "FILE": ProgMode = 4; break;
                    case "BATCHFILE": ProgMode = 14; break;
                    case "FILEBATCH": ProgMode = 14; break;

                    case "DIGEST": ProgMode = 5; break;
                    case "BATCHDIGEST": ProgMode = 15; break;
                    case "DIGESTBATCH": ProgMode = 15; break;
                    case "CONFIRMDIGEST": ProgMode = 25; break;
                    case "DIGESTCONFIRM": ProgMode = 25; break;

                    case "RSCODE": ProgMode = 7; break;
                    case "BATCHRSCODE": ProgMode = 17; break;
                    case "RSCODEBATCH": ProgMode = 17; break;
                    case "CONFIRMRSCODE": ProgMode = 27; break;
                    case "RSCODECONFIRM": ProgMode = 27; break;

                    case "CONFIG": ProgMode = 3; break;
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
                    ItemData[i_] = MailFile.FileNameToPath(ItemData[i_]);
                }
                for (int i_ = 0; i_ < ItemMap.Length; i_++)
                {
                    ItemMap[i_] = MailFile.FileNameToPath(ItemMap[i_]);
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
                
                bool Continue = PromptContinue(ProgMode == 1);
                if (Continue)
                {
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    MailSegment.LogSum("");
                    MailSegment.LogSum("");
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                        MailSegment.LogSum(WelcomeMsg[i]);
                    }
                    MailSegment.FileUpload(ItemCount, ItemName, ItemData, ItemMap, AccSrc.ToArray(), AccDst.ToArray(), SegmentSize, SegmentType, SegmentImgSize);
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    MailSegment.LogSum("");
                    MailSegment.LogSum("");
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
                    ItemData[i_] = MailFile.FileNameToPath(ItemData[i_]);
                }
                for (int i_ = 0; i_ < ItemMap.Length; i_++)
                {
                    ItemMap[i_] = MailFile.FileNameToPath(ItemMap[i_]);
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

                int RevOpt = 0;
                if (FileDownloadReverseOrder)
                {
                    RevOpt = 10;
                }

                List<string> WelcomeMsg = new List<string>();
                WelcomeMsg.Add("Download or check file:");
                switch (FileDownloadMode_)
                {
                    case MailSegment.FileDownloadMode.Download: WelcomeMsg.Add(DownloadTypeDesc[0 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CheckExistHeader: WelcomeMsg.Add(DownloadTypeDesc[1 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CheckExistBody: WelcomeMsg.Add(DownloadTypeDesc[2 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareHeader: WelcomeMsg.Add(DownloadTypeDesc[3 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareBody: WelcomeMsg.Add(DownloadTypeDesc[4 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.DownloadDigest: WelcomeMsg.Add(DownloadTypeDesc[5 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareHeaderDigest: WelcomeMsg.Add(DownloadTypeDesc[6 + RevOpt]); break;
                    case MailSegment.FileDownloadMode.CompareBodyDigest: WelcomeMsg.Add(DownloadTypeDesc[7 + RevOpt]); break;
                }
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

                bool Continue = PromptContinue(ProgMode == 2);
                if (Continue)
                {
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    MailSegment.LogSum("");
                    MailSegment.LogSum("");
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                        MailSegment.LogSum(WelcomeMsg[i]);
                    }
                    MailSegment.FileDownload(ItemCount, ItemName, ItemData, ItemMap, AccSrc.ToArray(), AccMin.ToArray(), AccMax.ToArray(), FileDownloadMode_, FileDeleteMode_, FileDownloadReverseOrder);
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    MailSegment.LogSum("");
                    MailSegment.LogSum("");
                }
                return;
            }

            // Configuration mode
            if (ProgMode == 3)
            {
                int NumOfTries = 1;
                int TestConn = 0;
                if (args.Length >= 3)
                {
                    TestConn = StrToInt(args[2]);
                    if (TestConn < 0)
                    {
                        TestConn = 0;
                    }
                }
                if (args.Length >= 4)
                {
                    NumOfTries = StrToInt(args[3]);
                    if (NumOfTries < 1)
                    {
                        NumOfTries = 1;
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
                                MailSegment.MailAccountList[Acc[i][0]].PrintInfo(TestConn, NumOfTries);
                            }
                            if (TestConn == 2)
                            {
                                MailSegment.MailAccountList[Acc[i][0]].PrintConnTest(Acc[i][0], NumOfTries);
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

                string FileNameSrc = MailFile.FileNameToPath(args[1]);
                string FileNameDst = MailFile.FileNameToPath(args[2]);
                int SegmentSize__ = 0;
                if (args.Length > 3)
                {
                    SegmentSize__ = StrToInt(args[3]);
                }
                if (SegmentSize__ <= 0)
                {
                    SegmentSize__ = MailSegment.DefaultSegmentSize;
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



                List<string> WelcomeMsg = new List<string>();

                if (FileNameSrc != null)
                {
                    WelcomeMsg.Add("Source file: " + FileNameSrc);
                }
                else
                {
                    WelcomeMsg.Add("No source file");
                }
                if (FileNameDst != null)
                {
                    WelcomeMsg.Add("Destination file: " + FileNameDst);
                }
                else
                {
                    WelcomeMsg.Add("No destination file");
                }
                WelcomeMsg.Add("Segment size: " + SegmentSize__);
                switch (CreateStats)
                {
                    case 0: WelcomeMsg.Add("File statistics: No statistics"); break;
                    case 1: WelcomeMsg.Add("File statistics: Simplified distribution table"); break;
                    case 2: WelcomeMsg.Add("File statistics: Value list with zeros"); break;
                    case 3: WelcomeMsg.Add("File statistics: Value list without zeros"); break;
                }
                switch (CreatePeriod)
                {
                    case 0: WelcomeMsg.Add("Period statistics: No statistics (period will not be searched)"); break;
                    case 1: WelcomeMsg.Add("Period statistics: Simplified distribution table"); break;
                    case 2: WelcomeMsg.Add("Period statistics: Value list with zeros"); break;
                    case 3: WelcomeMsg.Add("Period statistics: Value list without zeros"); break;
                }
                for (int i = 0; i < WelcomeMsg.Count; i++)
                {
                    Console.WriteLine(WelcomeMsg[i]);
                }
                Console.WriteLine();

                bool Continue = PromptContinue(ProgMode == 4);
                if (Continue)
                {
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    MailSegment.LogSum("");
                    MailSegment.LogSum("");
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                        MailSegment.LogSum(WelcomeMsg[i]);
                    }
                    RandomSequenceFile RandomSequenceFile_ = new RandomSequenceFile();
                    RandomSequenceFile_.CreateFile(FileNameSrc, FileNameDst, SegmentSize__, CreateStats, CreatePeriod);
                    MailSegment.Console_WriteLine_("");
                    MailSegment.Console_WriteLine_("");
                    MailSegment.LogSum("");
                    MailSegment.LogSum("");
                }
                return;
            }

            // Create digest or check data file against digest
            if (((ProgMode == 5) || (ProgMode == 15) || (ProgMode == 25)) && (args.Length > 4))
            {
                ItemData[0] = MailFile.FileNameToPath(args[2]);
                ItemMap[0] = MailFile.FileNameToPath(args[3]);
                ItemName[0] = MailFile.FileNameToPath(args[4]);
                int SegS = -1;
                if (args.Length > 5)
                {
                    if (StrToInt(args[5]) > 0)
                    {
                        SegS = StrToInt(args[5]);
                    }
                }

                if (SegS <= 0)
                {
                    SegS = MailSegment.DefaultSegmentSize;
                }

                List<string> WelcomeMsg = new List<string>();
                if ((StrToInt(args[1]) >= 0) && (StrToInt(args[1]) <= 3))
                {
                    DigestFile DF_ = new DigestFile();
                    if (StrToInt(args[1]) == 0)
                    {
                        WelcomeMsg.Add("Create the digest file from the data file");
                    }
                    if (StrToInt(args[1]) == 1)
                    {
                        WelcomeMsg.Add("Check the data file against the digest file");
                    }
                    if (StrToInt(args[1]) == 2)
                    {
                        WelcomeMsg.Add("Correct the data file size");
                    }
                    if (StrToInt(args[1]) == 3)
                    {
                        WelcomeMsg.Add("Correct the data file size and check the data file");
                    }
                    WelcomeMsg.Add("Data file: " + ItemData[0]);
                    if (ItemMap[0] != null)
                    {
                        WelcomeMsg.Add("Map file: " + ItemMap[0]);
                    }
                    else
                    {
                        WelcomeMsg.Add("No map file");
                    }
                    if (ItemName[0] != null)
                    {
                        WelcomeMsg.Add("Digest file: " + ItemName[0]);
                    }
                    else
                    {
                        WelcomeMsg.Add("No digest file");
                    }
                    WelcomeMsg.Add("Segment size: " + SegS);
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        Console.WriteLine(WelcomeMsg[i]);
                    }
                    Console.WriteLine();

                    bool Continue = PromptContinue((ProgMode == 5) || (ProgMode == 25));
                    if (Continue)
                    {
                        MailSegment.Console_WriteLine_("");
                        MailSegment.Console_WriteLine_("");
                        MailSegment.LogSum("");
                        MailSegment.LogSum("");
                        for (int i = 0; i < WelcomeMsg.Count; i++)
                        {
                            MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                            MailSegment.LogSum(WelcomeMsg[i]);
                        }
                        DF_.Proc(StrToInt(args[1]), ItemData[0], ItemMap[0], ItemName[0], SegS, (ProgMode == 25));
                        MailSegment.Console_WriteLine_("");
                        MailSegment.Console_WriteLine_("");
                        MailSegment.LogSum("");
                        MailSegment.LogSum("");
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
                    ItemData[i_] = MailFile.FileNameToPath(ItemData[i_]);
                }
                for (int i_ = 0; i_ < ItemMap.Length; i_++)
                {
                    ItemMap[i_] = MailFile.FileNameToPath(ItemMap[i_]);
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

            // Reed-Solomon code operations
            if (((ProgMode == 7) || (ProgMode == 17) || (ProgMode == 27)) && (args.Length >= 7))
            {
                CodeReedSolomon CRS = new CodeReedSolomon();

                string DataF = MailFile.FileNameToPath(args[2]);
                string DataM = MailFile.FileNameToPath(args[3]);
                string CodeF = MailFile.FileNameToPath(args[4]);
                string CodeM = MailFile.FileNameToPath(args[5]);

                int CodeSegs = (args.Length > 7) ? StrToInt(args[7]) : 0;

                int SegPerUnit = 0;
                if (args.Length > 6)
                {
                    if (StrToInt(args[6]) > 0)
                    {
                        SegPerUnit = StrToInt(args[6]);
                    }
                }

                int SegS = -1;
                if (args.Length > 8)
                {
                    if (StrToInt(args[8]) > 0)
                    {
                        SegS = StrToInt(args[8]);
                    }
                }

                int PolyInt = 0;
                if (args.Length > 9)
                {
                    if (StrToInt(args[9]) > 0)
                    {
                        PolyInt = StrToInt(args[9]);
                    }
                }

                if (CodeSegs <= 0)
                {
                    if (StrToInt(args[1]) == 9)
                    {
                        CodeSegs = 0;
                    }
                    else
                    {
                        CodeSegs = 1;
                    }
                }

                if (SegS <= 0)
                {
                    SegS = MailSegment.DefaultSegmentSize;
                }

                if (PolyInt <= 0)
                {
                    PolyInt = 0;
                }
                CRS.SetPolynomialNumber(PolyInt);

                if ((StrToInt(args[1]) >= 0) && (StrToInt(args[1]) <= 9))
                {
                    List<string> WelcomeMsg = new List<string>();

                    DigestFile DF_ = new DigestFile();

                    if (StrToInt(args[1]) == 0)
                    {
                        WelcomeMsg.Add("Create code file");
                    }
                    if (StrToInt(args[1]) == 1)
                    {
                        WelcomeMsg.Add("Recover files automatically - do not modify files");
                    }
                    if (StrToInt(args[1]) == 2)
                    {
                        WelcomeMsg.Add("Recover files based on the maps - do not modify files");
                    }
                    if (StrToInt(args[1]) == 3)
                    {
                        WelcomeMsg.Add("Recover files automatically - modify files according maps");
                    }
                    if (StrToInt(args[1]) == 4)
                    {
                        WelcomeMsg.Add("Recover files based on the maps - modify files according maps");
                    }
                    if (StrToInt(args[1]) == 5)
                    {
                        WelcomeMsg.Add("Recover files automatically - modify files regardless maps");
                    }
                    if (StrToInt(args[1]) == 6)
                    {
                        WelcomeMsg.Add("Recover files based on the maps - modify files regardless maps");
                    }
                    if (StrToInt(args[1]) == 7)
                    {
                        WelcomeMsg.Add("Analyze map files for recovery");
                    }
                    if (StrToInt(args[1]) == 8)
                    {
                        if (SegPerUnit < 0)
                        {
                            SegPerUnit = 0;
                        }
                        SegPerUnit = SegPerUnit % 2;
                        if (SegPerUnit == 0)
                        {
                            WelcomeMsg.Add("Resize files to specified size in bytes");
                        }
                        if (SegPerUnit == 1)
                        {
                            WelcomeMsg.Add("Resize files to specified size in segments");
                        }
                        DataM = StrToLong(args[3]).ToString();
                        CodeM = StrToLong(args[5]).ToString();

                        if (StrToLong(args[3]) < 0) { DataM = "0"; }
                        if (StrToLong(args[5]) < 0) { CodeM = "0"; }
                    }
                    if (StrToInt(args[1]) == 9)
                    {
                        WelcomeMsg.Add("Simulate incomplete download");
                    }
                    if (DataF != null)
                    {
                        WelcomeMsg.Add("Data file: " + DataF);
                        if (DataM != null)
                        {
                            if (StrToInt(args[1]) == 8)
                            {
                                if (SegPerUnit == 0)
                                {
                                    WelcomeMsg.Add("Desired data file size in bytes: " + DataM);
                                }
                                if (SegPerUnit == 1)
                                {
                                    WelcomeMsg.Add("Desired data file size in segments: " + DataM);
                                    WelcomeMsg.Add("Segment size: " + SegS);
                                    DataM = (long.Parse(DataM) * (long)SegS).ToString();
                                    WelcomeMsg.Add("Desired data file size in bytes: " + DataM);
                                }
                            }
                            else
                            {
                                WelcomeMsg.Add("Map file for data: " + DataM);
                            }
                        }
                        else
                        {
                            WelcomeMsg.Add("No map file for data");
                        }
                    }
                    if (CodeF != null)
                    {
                        WelcomeMsg.Add("Code file: " + CodeF);
                        if (CodeM != null)
                        {
                            if (StrToInt(args[1]) == 8)
                            {
                                if (SegPerUnit == 0)
                                {
                                    WelcomeMsg.Add("Desired code file size in bytes: " + CodeM);
                                }
                                if (SegPerUnit == 1)
                                {
                                    WelcomeMsg.Add("Desired code file size in segments: " + CodeM);
                                    WelcomeMsg.Add("Segment size: " + SegS);
                                    CodeM = (long.Parse(CodeM) * (long)SegS).ToString();
                                    WelcomeMsg.Add("Desired code file size in bytes: " + CodeM);
                                }
                            }
                            else
                            {
                                WelcomeMsg.Add("Map file for code: " + CodeM);
                            }
                        }
                        else
                        {
                            WelcomeMsg.Add("No map file for code");
                        }
                    }
                    if ((StrToInt(args[1]) != 8) && (StrToInt(args[1]) != 9))
                    {
                        if (SegPerUnit < 1)
                        {
                            SegPerUnit = 1;
                        }
                        WelcomeMsg.Add("Segments per unit: " + SegPerUnit);
                    }
                    if (StrToInt(args[1]) >= 0)
                    {
                        WelcomeMsg.Add("Code units: " + CodeSegs);
                    }
                    if (StrToInt(args[1]) == 9)
                    {
                        if (SegPerUnit < 0)
                        {
                            SegPerUnit = 0;
                        }
                        SegPerUnit = SegPerUnit % 3;
                        switch (SegPerUnit)
                        {
                            case 0: WelcomeMsg.Add("File resize: none"); break;
                            case 1: WelcomeMsg.Add("File resize: download process finished"); break;
                            case 2: WelcomeMsg.Add("File resize: download process broken"); break;
                        }
                    }
                    if (StrToInt(args[1]) != 8)
                    {
                        WelcomeMsg.Add("Segment size: " + SegS);
                    }

                    if (StrToInt(args[1]) <= 7)
                    {
                        if (CRS.PolynomialNumber > 0)
                        {
                            WelcomeMsg.Add("Bits per value: " + CRS.NumberOfBits);
                            WelcomeMsg.Add("Primitive polynomial: " + CRS.PolynomialNumber);
                        }
                        else
                        {
                            WelcomeMsg.Add("Bits per value: auto");
                            WelcomeMsg.Add("Primitive polynomial: auto");
                        }
                    }
                    for (int i = 0; i < WelcomeMsg.Count; i++)
                    {
                        Console.WriteLine(WelcomeMsg[i]);
                    }
                    Console.WriteLine();

                    bool Continue = PromptContinue((ProgMode == 7) || (ProgMode == 27));
                    if (Continue)
                    {
                        MailSegment.Console_WriteLine_("");
                        MailSegment.Console_WriteLine_("");
                        MailSegment.LogSum("");
                        MailSegment.LogSum("");
                        for (int i = 0; i < WelcomeMsg.Count; i++)
                        {
                            MailSegment.Console_WriteLine_(WelcomeMsg[i]);
                            MailSegment.LogSum(WelcomeMsg[i]);
                        }
                        CRS.Proc(StrToInt(args[1]), DataF, DataM, CodeF, CodeM, CodeSegs, SegPerUnit, SegS, (ProgMode == 27));
                        MailSegment.Console_WriteLine_("");
                        MailSegment.Console_WriteLine_("");
                        MailSegment.LogSum("");
                        MailSegment.LogSum("");
                    }
                    return;
                }



                return;
            }

            // Help and information
            Console.WriteLine("BackupToMail - command-line tool for using e-mail accounts as backup storage.");
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
            Console.WriteLine("Create digest or check data file against digest:");
            Console.WriteLine("BackupToMail DIGEST <mode> <data file> <map file> <digest file> [<seg size>]");
            Console.WriteLine("Available modes:");
            Console.WriteLine(" 0 - Create the digest file from the data file (default)");
            Console.WriteLine(" 1 - Check the data file against the digest file");
            Console.WriteLine(" 2 - Correct the data file size");
            Console.WriteLine(" 3 - Correct the data file size and check the data file");
            Console.WriteLine();
            Console.WriteLine("Create Reed-Solomon code or recover incomplete data file");
            Console.WriteLine("BackupToMail RSCODE <mode> <data file> <data map> <code file> <code map>");
            Console.WriteLine("<segments per unit> <code units> [<segment size> <polynomial number>]");
            Console.WriteLine("Available modes:");
            Console.WriteLine(" 0 - Create code file");
            Console.WriteLine(" 1 - Recover files automatically - do not modify files");
            Console.WriteLine(" 2 - Recover files based on the maps - do not modify files");
            Console.WriteLine(" 3 - Recover files automatically - modify files according maps");
            Console.WriteLine(" 4 - Recover files based on the maps - modify files according maps");
            Console.WriteLine(" 5 - Recover files automatically - modify files regardless maps");
            Console.WriteLine(" 6 - Recover files based on the maps - modify files regardless maps");
            Console.WriteLine(" 7 - Analyze map files for recovery");
            Console.WriteLine(" 8 - Resize files to specified size:");
            Console.WriteLine("     <segments per unit> = 0 - Size is specified in bytes");
            Console.WriteLine("     <segments per unit> = 1 - Size is specified in segments");
            Console.WriteLine(" 9 - Simulate incomplete download:");
            Console.WriteLine("     <segments per unit> = 0 - Do not resize files");
            Console.WriteLine("     <segments per unit> = 1 - Simulate, that download process was finished");
            Console.WriteLine("     <segments per unit> = 2 - Simulate, that download process was broken");
            Console.WriteLine();
            Console.WriteLine("Save dummy file or print stats:");
            Console.WriteLine("BackupToMail FILE <source file> <destination file>");
            Console.WriteLine("[<segment size> <file stats mode> <period stats mode>]");
            Console.WriteLine("File stats modes and period stats modes:");
            Console.WriteLine(" 0 - No statistics (default)");
            Console.WriteLine(" 1 - Simplified distribution table");
            Console.WriteLine(" 2 - Value list with zeros");
            Console.WriteLine(" 3 - Value list without zeros");
            Console.WriteLine();
            Console.WriteLine("Print configuration and connection test:");
            Console.WriteLine("BackupToMail CONFIG <account list by commas> [<test mode> <number of tries>]");
            Console.WriteLine("Test modes:");
            Console.WriteLine(" 0 - Print configuration without test (default)");
            Console.WriteLine(" 1 - Connection test and print full configuration");
            Console.WriteLine(" 2 - Connection test and print test results only");
            Console.WriteLine();
        }
    }
}