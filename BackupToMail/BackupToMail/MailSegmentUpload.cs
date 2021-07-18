/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-06-06
 * Time: 09:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MailKit.Net.Smtp;
using MimeKit;

namespace BackupToMail
{
    /// <summary>
    /// Uploading fields and methods
    /// </summary>
    public partial class MailSegment
    {
        /// <summary>
        /// Send one file message
        /// </summary>
        /// <param name="MSP"></param>
        /// <param name="FileName"></param>
        /// <param name="SegmentSize0"></param>
        /// <param name="SegmentCount"></param>
        /// <param name="MailAccountList"></param>
        /// <param name="AccountDst"></param>
        /// <param name="SegmentMode"></param>
        /// <param name="SegmentImageSize"></param>
        /// <param name="cancel"></param>
        public static void MailSend(ref MailSendParam MSP, string[] FileName, int SegmentSize0, int[] SegmentCount, List<MailAccount> MailAccountList, int[] AccountDst, int SegmentMode, int SegmentImageSize, CancellationTokenSource cancel)
        {
            string ConsoleInfo = CreateConsoleInfoU(MSP.FileI, MSP.FileSegmentNum, SegmentCount[MSP.FileI]);
            if (MailAccountList[MSP.AccountSrc].SmtpConnect)
            {
                ConsoleInfo = ConsoleInfo + " - gr " + (MSP.AccountSrcG + 1) + ", acc " + MSP.AccountSrc + ", thr " + MSP.ThreadNo;
            }
            else
            {
                ConsoleInfo = ConsoleInfo + " - gr " + (MSP.AccountSrcG + 1) + ", acc " + MSP.AccountSrc + ", thr " + MSP.ThreadNo + ", sl " + (MSP.SmtpClientSlot + 1);
            }
            MSP.Good = true;

            Console_WriteLine_Thr(ConsoleInfo + " - upload started ");


            string AttaInfo = InfoSeparatorS + FileName[MSP.FileI] + InfoSeparatorS + IntToHex(MSP.FileSegmentNum, SegmentCount[MSP.FileI] - 1) + InfoSeparatorS + IntToHex(SegmentCount[MSP.FileI] - 1) + InfoSeparatorS + IntToHex(MSP.FileSegmentSize - 1, SegmentSize0 - 1) + InfoSeparatorS + IntToHex(SegmentSize0 - 1) + InfoSeparatorS + Digest(MSP.SegmentBuf) + InfoSeparatorS;
            
            
            MimeMessage Msg = new MimeMessage();
            
            Msg.From.Add(new MailboxAddress(MailAccountList[MSP.AccountSrc].Address, MailAccountList[MSP.AccountSrc].Address));
            for (int i = 0; i < AccountDst.Length; i++)
            {
                Msg.To.Add(new MailboxAddress(MailAccountList[AccountDst[i]].Address, MailAccountList[AccountDst[i]].Address));
            }
            Msg.Subject = AttaInfo;
            
            
            BodyBuilder BB = new BodyBuilder();
            switch (SegmentMode)
            {
                default:
                    {
                        BB.TextBody = "Attachment";
                        BB.Attachments.Add("data.bin", MSP.SegmentBuf, ContentType.Parse("application/octet-stream"));
                    }
                    break;
                case 1:
                case 11:
                    {
                        BB.TextBody = "Attachment";
                        BB.Attachments.Add("data.png", ConvRaw2Img(MSP.SegmentBuf, SegmentImageSize), ContentType.Parse("image/png"));
                    }
                    break;
                case 2:
                case 12:
                    {
                        BB.TextBody = ConvRaw2Txt(MSP.SegmentBuf);
                    }
                    break;
                case 3:
                case 13:
                    {
                        BB.HtmlBody = "<img src=\"data:image/png;base64," + ConvRaw2Txt(ConvRaw2Img(MSP.SegmentBuf, SegmentImageSize).ToArray()) + "\">";
                    }
                    break;
            }            
            Msg.Body = BB.ToMessageBody();
            
            try
            {
                // Check if there is set connecting before and disconnecting after message sending
                if (MailAccountList[MSP.AccountSrc].SmtpConnect)
                {
                    Console_WriteLine_Thr(ConsoleInfo + " - connecting");
                    using (SmtpClient SC = MailAccountList[MSP.AccountSrc].SmtpClient_(cancel))
                    {
                        Console_WriteLine_Thr(ConsoleInfo + " - connected");
                        Console_WriteLine_Thr(ConsoleInfo + " - sending segment");
                        SC.Send(Msg);
                        Console_WriteLine_Thr(ConsoleInfo + " - disconnecting");
                        SC.Disconnect(true, cancel.Token);
                        Console_WriteLine_Thr(ConsoleInfo + " - disconnected");
                    }
                }
                else
                {
                    bool ServerReconn = MailAccountList[MSP.AccountSrc].SmtpClient_Need_Connect(MSP.SmtpClient_[MSP.AccountSrcN, MSP.SmtpClientSlot]);
                    if (ServerReconn)
                    {
                        Console_WriteLine_Thr(ConsoleInfo + " - connecting");
                    }
                    SmtpClient SC = MailAccountList[MSP.AccountSrc].SmtpClient_Connect(MSP.SmtpClient_[MSP.AccountSrcN, MSP.SmtpClientSlot], cancel);
                    if (ServerReconn) 
                    {
                        Console_WriteLine_Thr(ConsoleInfo + " - connected");
                    }
                    Console_WriteLine_Thr(ConsoleInfo + " - sending segment");
                    MSP.SmtpClient_[MSP.AccountSrcN, MSP.SmtpClientSlot] = SC;
                    SC.Send(Msg);
                }
                Console_WriteLine_Thr(ConsoleInfo + " - upload finished");
            }
            catch (Exception e)
            {
                Console_WriteLine_Thr(ConsoleInfo + " - upload error: " + ExcMsg(e));
                MSP.Good = false;
            }
            Msg = null;
            CleanUp();
        }

        /// <summary>
        /// File upload packet of segments to upload in separated threads
        /// </summary>
        /// <param name="AccountSrc"></param>
        /// <param name="AccountDst"></param>
        /// <param name="MailSendParam_"></param>
        /// <param name="MF"></param>
        /// <param name="FileName"></param>
        /// <param name="FileSegmentProgress"></param>
        /// <param name="FileSegmentCount"></param>
        /// <param name="FileSegmentToDo"></param>
        /// <param name="FileSegmentSize"></param>
        /// <param name="SegmentMode"></param>
        /// <param name="SegmentImageSize"></param>
        /// <param name="cancel"></param>
        public static bool FileUploadMsg(int[] AccountSrc, int[] AccountDst, List<MailSendParam> MailSendParam_, ref MailFile[] MF, string[] FileName, ref int[] FileSegmentProgress, int[] FileSegmentCount, int[] FileSegmentToDo, int FileSegmentSize, int SegmentMode, int SegmentImageSize, CancellationTokenSource cancel)
        {
            Stopwatch_ SW = new Stopwatch_();

            // Assign slots to each used account to reuse as many of existing connection as possible
            for (int i = 0; i < AccountSrc.Length; i++)
            {
                int SmtpClientSlot = 0;
                HashSet<int> SmtpClientSlotUsed = new HashSet<int>();
                SmtpClientSlotUsed.Clear();
                for (int ii = 0; ii < MailSendParam_.Count; ii++)
                {
                    if (MailSendParam_[ii].AccountSrcN == i)
                    {
                        SmtpClientSlotUsed.Add(MailSendParam_[ii].SmtpClientSlot);
                    }
                }
                while (SmtpClientSlotUsed.Contains(SmtpClientSlot))
                {
                    SmtpClientSlot++;
                }
                for (int ii = 0; ii < MailSendParam_.Count; ii++)
                {
                    if (MailSendParam_[ii].AccountSrcN == i)
                    {
                        if (MailSendParam_[ii].SmtpClientSlot < 0)
                        {
                            MailSendParam_[ii].SmtpClientSlot = SmtpClientSlot;
                            SmtpClientSlot++;
                        }
                    }
                }
            }
            
            // Size of segment packet
            int TotalSize = 0;

            int I = 0;

            // If number of sending threads is 1, there is not necessary to create separate thread
            if (MailSendParam_.Count == 1)
            {
                MailSendParam MailSendParam___ = MailSendParam_[0];
                MailSendParam___.Idx = I;
                MailSendParam___.ThreadNo = 1;
                MailSend(ref MailSendParam___, FileName, FileSegmentSize, FileSegmentCount, MailAccountList, AccountDst, SegmentMode, SegmentImageSize, cancel);
            }
            else
            {
                List<Thread> Thr = new List<Thread>();
                while ((I < MailSendParam_.Count) && (I < ThreadsUpload))
                {
                    MailSendParam MailSendParam___ = MailSendParam_[I];
                    MailSendParam___.Idx = I;
                    MailSendParam___.ThreadNo = Thr.Count + 1;
                    Thread Thr_ = new Thread(() => MailSend(ref MailSendParam___, FileName, FileSegmentSize, FileSegmentCount, MailAccountList, AccountDst, SegmentMode, SegmentImageSize, cancel));
                    Thr_.Start();
                    Thr.Add(Thr_);
                    I++;
                }
                foreach (Thread Thr_ in Thr)
                {
                    Thr_.Join();
                }
                I--;
            }

            // Encounting data size and removing segments from packet, which are sent correctly,
            // changing account number to next number in current group for not sent segments
            bool UploadedSomething = false;
            while (I >= 0)
            {
                if (MailSendParam_[I].Good)
                {
                    UploadedSomething = true;
                    MF[MailSendParam_[I].FileI].MapSet(MailSendParam_[I].FileSegmentNum, 1);
                    FileSegmentProgress[MailSendParam_[I].FileI]++;
                    TotalSize += MailSendParam_[I].FileSegmentSize;
                    MailSendParam_.RemoveAt(I);
                }
                else
                {
                    MF[MailSendParam_[I].FileI].MapSet(MailSendParam_[I].FileSegmentNum, 0);
                    MailSendParam_[I].AccountSrcN++;
                    if (AccountSrc[MailSendParam_[I].AccountSrcN] < 0)
                    {
                        MailSendParam_[I].AccountSrcN--;
                        while (AccountSrc[MailSendParam_[I].AccountSrcN] >= 0)
                        {
                            MailSendParam_[I].AccountSrcN--;
                        }
                        MailSendParam_[I].AccountSrcN++;
                    }
                    MailSendParam_[I].AccountSrc = AccountSrc[MailSendParam_[I].AccountSrcN];
                    MailSendParam_[I].SmtpClientSlot = -1;
                }
                I--;
            }

            // Print progress after packet send attemp and transfer of the packet segments
            long GetDataSizeSum = 0;
            long GetDataSizeSegSum = 0;
            int FileSegmentProgressSum = 0;
            int FileSegmentToDoSum = 0;
            for (int i_ = 0; i_ < MF.Length; i_++)
            {
                GetDataSizeSum += MF[i_].GetDataSize();
                GetDataSizeSegSum += MF[i_].GetDataSizeSeg();
                FileSegmentProgressSum += FileSegmentProgress[i_];
                FileSegmentToDoSum += FileSegmentToDo[i_];
            }
            Console_WriteLine("Upload progress: " + FileSegmentProgressSum + "/" + FileSegmentToDoSum);
            Console_WriteLine("Upload speed: " + KBPS(TotalSize, SW.Elapsed()));
            Log(TSW.Elapsed().ToString(), LogDiffS(FileSegmentProgressSum).ToString(), FileSegmentProgressSum.ToString(), FileSegmentToDoSum.ToString(), LogDiffB(KBPS_Bytes).ToString(), KBPS_B(), GetDataSizeSum.ToString(), GetDataSizeSegSum.ToString());
            
            // If uploaded something, return true, otherwise return false
            if (UploadedSomething)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// File upload action
        /// </summary>
        /// <param name="FileName_"></param>
        /// <param name="FilePath"></param>
        /// <param name="FileMap"></param>
        /// <param name="AccountSrc"></param>
        /// <param name="AccountDst"></param>
        /// <param name="FileSegmentSize"></param>
        /// <param name="SegmentMode"></param>
        /// <param name="SegmentImageSize"></param>
        public static void FileUpload(int FileCount, string[] FileName_, string[] FilePath, string[] FileMap, int[] AccountSrc, int[] AccountDst, int FileSegmentSize, int SegmentMode, int SegmentImageSize)
        {
            // Check account lists
            if (AccountSrc.Length == 2)
            {
                Console_WriteLine("No source accounts");
                return;
            }
            if (AccountDst.Length == 0)
            {
                Console_WriteLine("No destination accounts");
                return;
            }

            // The file name is stored as digest 
            int MF_Length = FileCount;
            string[] FileName = new string[MF_Length];
            for (int i = 0; i < MF_Length; i++)
            {
                FileName[i] = Digest(FileName_[i]);
            }

            MailFile[] MF = new MailFile[MF_Length];
            bool[] MF_Open = new bool[MF_Length];
            bool OpenAll = true;
            for (int i = 0; i < MF_Length; i++)
            {
                MF[i] = new MailFile();
                MF_Open[i] = MF[i].Open(false, true, FilePath[i], FileMap[i]);
                if (!MF_Open[i])
                {
                    OpenAll = false;
                }
            }
            if (OpenAll)
            {
                Log();
                LogReset();
                Log("Time stamp", "Uploaded segments since previous entry", "Totally uploaded segments", "All segments", "Uploaded bytes since previous entry", "Totally uploaded bytes", "All bytes by segment count", "All bytes by file size");
                TSW = new Stopwatch_();
                KBPSReset();
                bool FileNotBlank = false;
                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    MF[i_].MapChange(1, 2);
                    MF[i_].SetSegmentSize(FileSegmentSize);
                    if (MF[i_].CalcSegmentCount() > 0)
                    {
                        FileNotBlank = true;
                    }
                }
                if (FileNotBlank)
                {
                    List<MailSendParam> MailSendParam_ = new List<MailSendParam>();
                    
                    using (CancellationTokenSource cancel = new CancellationTokenSource ())
                    {
                        // Create SMTP client array (not create SMTP connections)
                        SmtpClient[,] SmtpClient_ = new SmtpClient[AccountSrc.Length, ThreadsUpload];
                        for (int i = 0; i < AccountSrc.Length; i++)
                        {
                            for (int ii = 0; ii < ThreadsUpload; ii++)
                            {
                                SmtpClient_[i, ii] = null;
                            }
                        }
                        
                        int[] FileSegmentProgress = new int[MF_Length];
                        int[] FileSegmentCount = new int[MF_Length];
                        int[] FileSegmentToDo = new int[MF_Length];
                        for (int i_ = 0; i_ < MF_Length; i_++)
                        {
                            FileSegmentProgress[i_] = 0;
                        }

                        // Upload failure counter
                        int UploadFailureCounter = 0;

                        // Current upload group
                        int UploadGroup = 0;
                        
                        // Map of upload account counters
                        List<int> UploadGroupN = new List<int>();
                        
                        // Number of account which will be aggigned to each segment to send, for each group separatelly
                        for (int i = 0; i < (AccountSrc.Length - 1); i++)
                        {
                            if (AccountSrc[i] < 0)
                            {
                                UploadGroupN.Add(-1);
                            }
                            else
                            {
                                if (UploadGroupN[UploadGroupN.Count - 1] < 0)
                                {
                                    UploadGroupN[UploadGroupN.Count - 1] = i;
                                }
                            }
                        }

                        for (int FileItemI = 0; FileItemI < MF_Length; FileItemI++)
                        {
                            if (MF[FileItemI].CalcSegmentCount() > 0)
                            {
                                FileSegmentCount[FileItemI] = MF[FileItemI].GetSegmentCount();

                                MF[FileItemI].MapCalcStats();
                                FileSegmentToDo[FileItemI] = MF[FileItemI].MapCount(0);

                                // The loop iterates over all file segments, but also iterates while there are some segments to upload  
                                int FileSegmentNum = 0;
                                int FileSegmentNumDelta = 1;
                                if (SegmentMode > 9)
                                {
                                    FileSegmentNum = FileSegmentCount[FileItemI] - 1;
                                    FileSegmentNumDelta = -1;
                                }
                                int UploadCounter = 0;

                                for (int FileSegmentNum_ = 0; FileSegmentNum_ < FileSegmentCount[FileItemI]; FileSegmentNum_++)
                                {
                                    // Creating console information prefix
                                    string ConsoleInfo = CreateConsoleInfoU(FileItemI, FileSegmentNum, FileSegmentCount[FileItemI]);

                                    // Upload only this segments, which must be uploaded based on map file
                                    if (MF[FileItemI].MapGet(FileSegmentNum) == 0)
                                    {
                                        byte[] SegmentBuf = MF[FileItemI].DataGet(FileSegmentNum);

                                        // Create a object, which keeps all needed data during whole sending process of current segment
                                        MailSendParam MailSendParam__ = new MailSendParam();
                                        MailSendParam__.FileI = FileItemI;
                                        MailSendParam__.SegmentBuf = SegmentBuf;
                                        MailSendParam__.FileSegmentSize = SegmentBuf.Length;
                                        MailSendParam__.FileSegmentNum = FileSegmentNum;
                                        MailSendParam__.AccountSrcG = UploadGroup;
                                        MailSendParam__.AccountSrcN = UploadGroupN[UploadGroup];
                                        MailSendParam__.AccountSrc = AccountSrc[MailSendParam__.AccountSrcN];
                                        MailSendParam__.SmtpClient_ = SmtpClient_;
                                        MailSendParam__.SmtpClientSlot = -1;
                                        MailSendParam_.Add(MailSendParam__);

                                        // Set the next account to assign to next segment
                                        UploadGroupN[UploadGroup]++;
                                        if (AccountSrc[UploadGroupN[UploadGroup]] < 0)
                                        {
                                            UploadGroupN[UploadGroup]--;
                                            while (AccountSrc[UploadGroupN[UploadGroup]] >= 0)
                                            {
                                                UploadGroupN[UploadGroup]--;
                                            }
                                            UploadGroupN[UploadGroup]++;
                                        }
                                        Console_WriteLine(ConsoleInfo + " - to upload");
                                    }
                                    else
                                    {
                                        Console_WriteLine(ConsoleInfo + " - not to upload");
                                    }

                                    bool PerformUploadCondition = false;
                                    if (MailSendParam_.Count >= ThreadsUpload)
                                    {
                                        PerformUploadCondition = true;
                                    }
                                    if (MailSendParam_.Count > 0)
                                    {
                                        UploadCounter++;
                                        if (FileSegmentNum_ == (FileSegmentCount[FileItemI] - 1))
                                        {
                                            PerformUploadCondition = true;
                                        }
                                        if (UploadCounter >= ThreadsUpload)
                                        {
                                            PerformUploadCondition = true;
                                        }
                                    }
                                    else
                                    {
                                        UploadCounter = 0;
                                    }

                                    // If the number of segments is at least the same as number of threads, there will be attemped to send,
                                    // if none of segments are sent, the attemp will be repeated immediately
                                    while (PerformUploadCondition)
                                    {
                                        if (FileUploadMsg(AccountSrc, AccountDst, MailSendParam_, ref MF, FileName, ref FileSegmentProgress, FileSegmentCount, FileSegmentToDo, FileSegmentSize, SegmentMode, SegmentImageSize, cancel))
                                        {
                                            UploadFailureCounter = 0;
                                        }
                                        else
                                        {
                                            UploadFailureCounter++;
                                        }

                                        // If upload failed several times at a row, the group must be changed
                                        if (UploadFailureCounter >= UploadGroupChange)
                                        {
                                            UploadFailureCounter = 0;

                                            // Close all opened connections
                                            Console_WriteLine("Disconnecting existing connections");
                                            for (int i = 0; i < AccountSrc.Length; i++)
                                            {
                                                for (int ii = 0; ii < ThreadsUpload; ii++)
                                                {
                                                    if (SmtpClient_[i, ii] != null)
                                                    {
                                                        if (SmtpClient_[i, ii].IsConnected)
                                                        {
                                                            SmtpClient_[i, ii].Disconnect(true, cancel.Token);
                                                        }
                                                        SmtpClient_[i, ii].Dispose();
                                                    }
                                                }
                                            }

                                            // Change group
                                            Console_Write("Changing account group from " + UploadGroup);
                                            UploadGroup++;
                                            if (UploadGroup >= UploadGroupN.Count)
                                            {
                                                UploadGroup = 0;
                                            }
                                            Console_Write(" to " + UploadGroup);

                                            // Assign accounts from next group
                                            for (int i = 0; i < MailSendParam_.Count; i++)
                                            {
                                                MailSendParam_[i].AccountSrcG = UploadGroup;
                                                MailSendParam_[i].AccountSrcN = UploadGroupN[UploadGroup];
                                                MailSendParam_[i].AccountSrc = AccountSrc[MailSendParam_[i].AccountSrcN];

                                                // Set the next account to assign to next segment
                                                UploadGroupN[UploadGroup]++;
                                                if (AccountSrc[UploadGroupN[UploadGroup]] < 0)
                                                {
                                                    UploadGroupN[UploadGroup]--;
                                                    while (AccountSrc[UploadGroupN[UploadGroup]] >= 0)
                                                    {
                                                        UploadGroupN[UploadGroup]--;
                                                    }
                                                    UploadGroupN[UploadGroup]++;
                                                }
                                            }
                                        }

                                        UploadCounter = 0;
                                        PerformUploadCondition = false;
                                        if (MailSendParam_.Count > 0)
                                        {
                                            PerformUploadCondition = true;
                                        }
                                    }

                                    FileSegmentNum += FileSegmentNumDelta;
                                }
                            }
                        }

                        // Closing all opened connections
                        Console_WriteLine("Disconnecting existing connections");
                        for (int i = 0; i < AccountSrc.Length; i++)
                        {
                            for (int ii = 0; ii < ThreadsUpload; ii++)
                            {
                                try
                                {
                                    if (SmtpClient_[i, ii] != null)
                                    {
                                        if (SmtpClient_[i, ii].IsConnected)
                                        {
                                            SmtpClient_[i, ii].Disconnect(true, cancel.Token);
                                        }
                                        SmtpClient_[i, ii].Dispose();
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        Console_WriteLine("Disconnected");
                    }
                    ConsoleLineToLog = true;
                    ConsoleLineToLogSum = true;
                    Console_WriteLine("");
                    for (int i_ = 0; i_ < FileCount; i_++)
                    {
                        MF[i_].MapCalcStats();
                        Console_WriteLine("Item " + (i_ + 1).ToString() + ":");
                        Console_WriteLine(" Item name: " + FileName_[i_]);
                        Console_WriteLine(" Total segments: " + MF[i_].GetSegmentCount().ToString());
                        Console_WriteLine(" Segments uploaded previously: " + MF[i_].MapCount(2).ToString());
                        Console_WriteLine(" Segments uploaded now: " + MF[i_].MapCount(1).ToString());
                    }
                    Console_WriteLine("Uploaded bytes: " + KBPS_B());
                    Console_WriteLine("Upload time: " + KBPS_T());
                    Console_WriteLine("Average upload speed: " + KBPS());
                    Console_WriteLine("Total time: " + TimeHMSM(TSW.Elapsed()));
                    ConsoleLineToLogSum = false;
                    ConsoleLineToLog = false;
                }
                else
                {
                    ConsoleLineToLog = true;
                    ConsoleLineToLogSum = true;
                    Console_WriteLine("");
                    Console_WriteLine("Every file is blank");
                    ConsoleLineToLogSum = false;
                    ConsoleLineToLog = false;
                }
                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    MF[i_].Close();
                }
            }
            else
            {
                ConsoleLineToLog = true;
                ConsoleLineToLogSum = true;
                Console_WriteLine("");
                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    if (!MF_Open[i_])
                    {
                        Console_WriteLine("Item " + (i_ + 1).ToString() + " - file open error: " + MF[i_].OpenError);
                    }
                }
                ConsoleLineToLogSum = false;
                ConsoleLineToLog = false;
                return;
            }
        }
        
        /// <summary>
        /// Upload message prefix
        /// </summary>
        /// <param name="MsgIdx"></param>
        /// <param name="MsgCount"></param>
        /// <returns></returns>
        public static string CreateConsoleInfoU(int ItemIdx, int MsgIdx, int MsgCount)
        {
            return "Item " + (ItemIdx + 1).ToString() + " - segment " + (MsgIdx + 1).ToString() + "/" + MsgCount;
        }

    }
}
