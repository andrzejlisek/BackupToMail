/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-06-06
 * Time: 09:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Search;
using MimeKit;

namespace BackupToMail
{
    /// <summary>
    /// Downloading fields and methods
    /// </summary>
    public partial class MailSegment
    {
        public enum FileDownloadMode
        {
            Download,
            CheckExistHeader,
            CheckExistBody,
            CompareHeader,
            CompareBody,
            DownloadDigest,
            CompareHeaderDigest,
            CompareBodyDigest
        }

        [FlagsAttribute]
        public enum FileDeleteMode
        {
            None = 0,
            Bad = 1,
            Duplicate = 2,
            ThisFile = 4,
            OtherMsg = 8,
            OtherFiles = 16,
            Undownloadable = 32
        }

        public static byte[] MailReceive(string ConsoleInfo, string ConsoleDownCheck, int Account, IMailFolder ImapClient_, Pop3Client Pop3Client_, int Idx, out string MsgInfo_, ref int ErrorType, CancellationTokenSource cancel)
        {
            ErrorType = 0;
            if ((ImapClient_ == null) && (Pop3Client_ == null))
            {
                MsgInfo_ = "";
                ErrorType = 3;
                return null;
            }

            MimeMessage Msg = null;
            CleanUp();
            bool Work = true;
            while (Work)
            {
                try
                {
                    if (ImapClient_ != null)
                    {
                        Msg = ImapClient_.GetMessage(Idx, cancel.Token);
                        Work = false;
                    }
                    if (Pop3Client_ != null)
                    {
                        Msg = Pop3Client_.GetMessage(Idx, cancel.Token);
                        Work = false;
                    }
                }
                catch (Exception e)
                {
                    if (e is OutOfMemoryException)
                    {
                        CleanUp();
                    }
                    else
                    {
                        Console_WriteLine_Thr(ConsoleInfo + " - " + ConsoleDownCheck + " error: " + ExcMsg(e));
                        ErrorType = 1;
                        MsgInfo_ = "";
                        return null;
                    }
                }
            }
            CleanUp();
            MsgInfo_ = Msg.Headers["Subject"];

            foreach (MimeEntity MsgAtta_ in Msg.Attachments)
            {
                MimePart MsgAtta = (MimePart)MsgAtta_;
                if (MsgAtta.FileName == "data.bin")
                {
                    using (StreamReader SR = new StreamReader(MsgAtta.Content.Stream))
                    {
                        string MsgAtta_Raw = SR.ReadToEnd();
                        return Convert.FromBase64String(MsgAtta_Raw);
                    }
                }
                if (MsgAtta.FileName == "data.png")
                {
                    using (StreamReader SR = new StreamReader(MsgAtta.Content.Stream))
                    {
                        string MsgAtta_Raw = SR.ReadToEnd();
                        return ConvImg2Raw(new MemoryStream(Convert.FromBase64String(MsgAtta_Raw)));
                    }
                }
            }

            if ((Msg.TextBody != null) && (Msg.HtmlBody == null))
            {
                try
                {
                    return ConvTxt2Raw(Msg.TextBody);
                }
                catch
                {
                }
            }

            if ((Msg.TextBody == null) && (Msg.HtmlBody != null))
            {
                try
                {
                    string MsgAtta_Raw = Msg.HtmlBody;
                    int I1 = MsgAtta_Raw.IndexOf("src=\"data:image/png;base64,", StringComparison.InvariantCulture);
                    int I2 = MsgAtta_Raw.IndexOf("\"", I1 + 25, StringComparison.InvariantCulture);
                    MsgAtta_Raw = MsgAtta_Raw.Substring(I1 + 27, I2 - I1 - 27);
                    return ConvImg2Raw(new MemoryStream(Convert.FromBase64String(MsgAtta_Raw)));
                }
                catch
                {
                }
            }

            ErrorType = 2;
            return null;
        }


        public static bool FileDownloadDeleteMark(int I, Pop3Client Pop3Client_, IMailFolder ImapClient_Inbox_, CancellationTokenSource cancel)
        {
            try
            {
                if (Pop3Client_ != null)
                {
                    Pop3Client_.DeleteMessage(I);
                }
                if (ImapClient_Inbox_ != null)
                {
                    ImapClient_Inbox_.AddFlags(I, MessageFlags.Deleted, true, cancel.Token);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void FileDownloadDeleteAction(Pop3Client Pop3Client_, IMailFolder ImapClient_Inbox_, CancellationTokenSource cancel)
        {
            if (ImapClient_Inbox_ != null)
            {
                try
                {
                    ImapClient_Inbox_.Expunge();
                }
                catch
                {

                }
            }
        }


        public static void FileDownloadAccount(int Account, bool FileDownloadReverseOrder, FileDownloadMode FileDownloadMode_, FileDeleteMode FileDeleteMode_, string[] FileName_, ref MailFile[] MF)
        {
            int DeletedMsgs = 0;
            int MF_Length = MF.Length;
            string[] FileName = new string[MF_Length];
            for (int i = 0; i < MF_Length; i++)
            {
                FileName[i] = Digest(FileName_[i]);
            }
            int HeaderThread = 0;
            bool DownloadMessage = false;
            if (FileDownloadMode_ == FileDownloadMode.Download) { DownloadMessage = true; }
            if (FileDownloadMode_ == FileDownloadMode.DownloadDigest) { DownloadMessage = true; }
            if (FileDownloadMode_ == FileDownloadMode.CheckExistBody) { DownloadMessage = true; }
            if (FileDownloadMode_ == FileDownloadMode.CompareBody) { DownloadMessage = true; }
            if (FileDownloadMode_ == FileDownloadMode.CompareBodyDigest) { DownloadMessage = true; }
            int ThreadsDownload_ = DownloadMessage ? ThreadsDownload : 1;
            int IdxMinAccount = MailAccountList[Account].DownloadMinAccount;
            int IdxMaxAccount = MailAccountList[Account].DownloadMaxAccount;
            int IdxEnd = 0;
            int IdxMinExists = -1;
            int IdxMaxExists = -1;
            int HeaderRetryC = DownloadRetry;
            int HeaderRetryI = HeaderRetryC;
            bool Undownloadable = false;
            using (CancellationTokenSource cancel = new CancellationTokenSource())
            {
                bool Pop3Use = MailAccountList[Account].Pop3Use;
                bool DecreaseIndexAfterDelete = MailAccountList[Account].DeleteIdx;

                Pop3Client[] Pop3Client_ = new Pop3Client[ThreadsDownload_];
                ImapClient[] ImapClient_ = new ImapClient[ThreadsDownload_];
                IMailFolder[] ImapClient_Inbox_ = new IMailFolder[ThreadsDownload_];


                List<MailRecvParam> MailRecvParam_ = new List<MailRecvParam>();

                int[] SegmentDownloadedAlready = new int[MF_Length];
                int[] FileSegmentProgress = new int[MF_Length];
                int[] FileSegmentCount = new int[MF_Length];
                int[] FileSegmentSize0 = new int[MF_Length];
                for (int i = 0; i < MF_Length; i++)
                {
                    SegmentDownloadedAlready[i] = 0;
                    FileSegmentProgress[i] = 0;
                    FileSegmentCount[i] = 0;
                    FileSegmentSize0[i] = 0;
                }

                int MsgCount = -1;
                bool NeedConnect = true;
                int DownloadCounter = 0;

                for (int i = 0; (i <= IdxEnd) || (MailRecvParam_.Count > 0); i++)
                {
                    while (NeedConnect)
                    {
                        DeletedMsgs = 0;
                        Console_WriteLine("Account " + Account + " - preparing connection");
                        bool ConnGood = true;
                        for (int I = 0; I < ThreadsDownload_; I++)
                        {
                            try
                            {
                                if (Pop3Use)
                                {
                                    if (Pop3Client_[I] != null)
                                    {
                                        if (Pop3Client_[I].IsConnected)
                                        {
                                            Console_WriteLine("Account " + Account + " - thread " + (I + 1).ToString() + " - disconnecting");
                                            Pop3Client_[I].Disconnect(true, cancel.Token);
                                        }
                                        Pop3Client_[I].Dispose();
                                    }
                                    Console_WriteLine("Account " + Account + " - thread " + (I + 1).ToString() + " - connecting");
                                    Pop3Client_[I] = MailAccountList[Account].Pop3Client_(cancel);
                                }
                                else
                                {
                                    if (ImapClient_[I] != null)
                                    {
                                        if (ImapClient_[I].IsConnected)
                                        {
                                            Console_WriteLine("Account " + Account + " - thread " + (I + 1).ToString() + " - disconnecting");
                                            if (ImapClient_Inbox_[I] != null)
                                            {
                                                if (ImapClient_Inbox_[I].IsOpen)
                                                {
                                                    if (FileDeleteMode_ != FileDeleteMode.None)
                                                    {
                                                        ImapClient_Inbox_[I].Close(true, cancel.Token);
                                                    }
                                                    else
                                                    {
                                                        ImapClient_Inbox_[I].Close(false, cancel.Token);
                                                    }
                                                }
                                            }
                                            ImapClient_[I].Disconnect(true, cancel.Token);
                                        }
                                        ImapClient_[I].Dispose();
                                    }
                                    Console_WriteLine("Account " + Account + " - thread " + (I + 1).ToString() + " - connecting");
                                    if ((FileDeleteMode_ != FileDeleteMode.None) && (I == HeaderThread))
                                    {
                                        ImapClient_[I] = MailAccountList[Account].ImapClient_(cancel, true);
                                    }
                                    else
                                    {
                                        ImapClient_[I] = MailAccountList[Account].ImapClient_(cancel, false);
                                    }
                                }
                                Console_WriteLine("Account " + Account + " - thread " + (I + 1).ToString() + " - connected");
                            }
                            catch (Exception e)
                            {
                                Console_WriteLine("Account " + Account + " - thread " + (I + 1).ToString() + " - connection error: " + ExcMsg(e));
                                Pop3Client_[I] = null;
                                ImapClient_[I] = null;
                                ImapClient_Inbox_[I] = null;
                                ConnGood = false;
                            }
                        }

                        if (ConnGood)
                        {
                            Console_WriteLine("Account " + Account + " - connection ready");
                            MsgCount = -1;
                            for (int I = 0; I < ThreadsDownload_; I++)
                            {
                                if (Pop3Use)
                                {
                                    int MsgCountT = Pop3Client_[I].Count;
                                    if ((MsgCount >= 0) && (MsgCount != MsgCountT))
                                    {
                                        if (ConnGood)
                                        {
                                            Console_WriteLine("Account " + Account + " - message count not the same in all threads");
                                        }
                                        ConnGood = false;
                                    }
                                    MsgCount = MsgCountT;
                                }
                                else
                                {
                                    ImapClient_Inbox_[I] = ImapClient_[I].Inbox;
                                    int MsgCountT = ImapClient_Inbox_[I].Count;
                                    if ((MsgCount >= 0) && (MsgCount != MsgCountT))
                                    {
                                        if (ConnGood)
                                        {
                                            Console_WriteLine("Account " + Account + " - message count not the same in all threads");
                                        }
                                        ConnGood = false;
                                    }
                                    MsgCount = MsgCountT;
                                }
                            }

                            IdxEnd = MsgCount - 1;
                            if (FileDownloadReverseOrder)
                            {
                                if (IdxMinAccount > 0)
                                {
                                    IdxEnd = Math.Min(MsgCount - IdxMinAccount, MsgCount - 1);
                                }
                                if (IdxMaxAccount > 0)
                                {
                                    if (i < (MsgCount - IdxMaxAccount))
                                    {
                                        i = Math.Max(MsgCount - IdxMaxAccount, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (IdxMinAccount > 0)
                                {
                                    if (i < (IdxMinAccount - 1))
                                    {
                                        i = Math.Max(IdxMinAccount - 1, 0);
                                    }
                                }
                                if (IdxMaxAccount > 0)
                                {
                                    IdxEnd = Math.Min(IdxMaxAccount - 1, MsgCount - 1);
                                }
                            }

                        }

                        if (ConnGood)
                        {
                            if ((FileDownloadMode_ == FileDownloadMode.Download) || (FileDownloadMode_ == FileDownloadMode.DownloadDigest))
                            {
                                Console_WriteLine("Account " + Account + " - ready to download");
                            }
                            else
                            {
                                Console_WriteLine("Account " + Account + " - ready to check");
                            }
                            NeedConnect = false;
                        }
                    }

                    if (i <= IdxEnd)
                    {
                        int MsgIdx = i;
                        if (FileDownloadReverseOrder)
                        {
                            MsgIdx = MsgCount - 1 - i;
                        }

                        if ((MailRecvParam_.Count < ThreadsDownload_) || (!DownloadMessage))
                        {
                            HeaderList MsgH = null;
                            if (MsgCount > 0)
                            {
                                try
                                {
                                    Console_Write(CreateConsoleInfoD(Account, MsgIdx, MsgCount));
                                    if (Pop3Use)
                                    {
                                        MsgH = Pop3Client_[HeaderThread].GetMessageHeaders(MsgIdx, cancel.Token);
                                    }
                                    else
                                    {
                                        MsgH = ImapClient_Inbox_[HeaderThread].GetHeaders(MsgIdx, cancel.Token);
                                    }
                                    HeaderRetryI = HeaderRetryC;
                                    Undownloadable = false;
                                }
                                catch (Exception e)
                                {
                                    MsgH = null;
                                    Console_WriteLine("header download error: " + ExcMsg(e));
                                    if (HeaderRetryI > 0)
                                    {
                                        i--;
                                        i -= DeletedMsgs;
                                        HeaderRetryI--;
                                        Undownloadable = false;
                                    }
                                    else
                                    {
                                        HeaderRetryI = HeaderRetryC;
                                        Undownloadable = true;
                                    }
                                    NeedConnect = true;
                                }
                            }
                            else
                            {
                                Console_WriteLine("Account " + Account + " - no messages");
                            }


                            bool DeleteBasedOnHeader = false;

                            if (MsgH != null)
                            {
                                string MsgInfoS = MsgH["Subject"];
                                string[] MsgInfo = (MsgInfoS != null) ? MsgInfoS.Split(InfoSeparatorC) : new string[0];

                                bool IsFileMessage = (MsgInfo.Length == 8);
                                if (IsFileMessage)
                                {
                                    if (!ConsistsOfHex(MsgInfo[1])) { IsFileMessage = false; }
                                    if (!ConsistsOfHex(MsgInfo[2])) { IsFileMessage = false; }
                                    if (!ConsistsOfHex(MsgInfo[3])) { IsFileMessage = false; }
                                    if (!ConsistsOfHex(MsgInfo[4])) { IsFileMessage = false; }
                                    if (!ConsistsOfHex(MsgInfo[5])) { IsFileMessage = false; }
                                    if (!ConsistsOfHex(MsgInfo[6])) { IsFileMessage = false; }
                                }

                                if (IsFileMessage)
                                {

                                    int FileI = -1;
                                    for (int i__ = 0; i__ < MF_Length; i__++)
                                    {
                                        if (MsgInfo[1] == FileName[i__])
                                        {
                                            FileI = i__;
                                        }
                                    }

                                    string ConsoleInfoS = CreateConsoleInfoDS(Account, MsgIdx, MsgCount, FileI, MsgInfo);

                                    if (FileI >= 0)
                                    {
                                        Console_Write(ConsoleInfoS + " - ");
                                        bool Good = true;
                                        if (FileSegmentCount[FileI] == 0)
                                        {
                                            FileSegmentCount[FileI] = HexToInt(MsgInfo[3]) + 1;
                                            FileSegmentSize0[FileI] = HexToInt(MsgInfo[5]) + 1;
                                            MF[FileI].SetSegmentCount(FileSegmentCount[FileI]);
                                            MF[FileI].SetSegmentSize(FileSegmentSize0[FileI]);
                                            MF[FileI].MapCalcStats();
                                            SegmentDownloadedAlready[FileI] = MF[FileI].MapCount(1) + MF[FileI].MapCount(2);
                                        }
                                        else
                                        {
                                            if (FileSegmentCount[FileI] != (HexToInt(MsgInfo[3]) + 1))
                                            {
                                                Console_WriteLine("segment count mismatch");
                                                if ((FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                                                {
                                                    DeleteBasedOnHeader = true;
                                                }
                                                Good = false;
                                            }
                                            if (FileSegmentSize0[FileI] != (HexToInt(MsgInfo[5]) + 1))
                                            {
                                                Console_WriteLine("segment nominal size mismatch");
                                                if ((FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                                                {
                                                    DeleteBasedOnHeader = true;
                                                }
                                                Good = false;
                                            }
                                        }

                                        int FileSegmentNum = HexToInt(MsgInfo[2]);

                                        if (Good)
                                        {
                                            if (MF[FileI].MapGet(FileSegmentNum) == 2)
                                            {
                                                if ((FileDownloadMode_ == FileDownloadMode.Download) || (FileDownloadMode_ == FileDownloadMode.DownloadDigest))
                                                {
                                                    Console_WriteLine("not to download");
                                                }
                                                else
                                                {
                                                    Console_WriteLine("not to check");
                                                }
                                                Good = false;
                                            }
                                            if (MF[FileI].MapGet(FileSegmentNum) == 1)
                                            {
                                                if ((FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
                                                {
                                                    DeleteBasedOnHeader = true;
                                                }
                                                Console_WriteLine("duplicate");
                                                Good = false;
                                            }
                                        }

                                        // 1 - File name
                                        // 2 - Number of segment
                                        // 3 - Count of segments
                                        // 4 - Current segment size
                                        // 5 - Nominal segment size
                                        // 6 - Digest
                                        if (Good)
                                        {
                                            if ((IdxMinExists < 0) || (IdxMinExists > i))
                                            {
                                                IdxMinExists = i;
                                            }
                                            if ((IdxMaxExists < 0) || (IdxMaxExists < i))
                                            {
                                                IdxMaxExists = i;
                                            }
                                            if (DownloadMessage)
                                            {
                                                bool MailRecvParam__Exists = false;
                                                for (int i___ = 0; i___ < MailRecvParam_.Count; i___++)
                                                {
                                                    if (MailRecvParam_[i___].Idx == MsgIdx)
                                                    {
                                                        MailRecvParam__Exists = true;
                                                    }
                                                }

                                                if (MailRecvParam__Exists)
                                                {
                                                    Console_WriteLine("in another thread");
                                                }
                                                else
                                                {
                                                    if ((FileDownloadMode_ == FileDownloadMode.Download) || (FileDownloadMode_ == FileDownloadMode.DownloadDigest))
                                                    {
                                                        Console_WriteLine("to download");
                                                    }
                                                    else
                                                    {
                                                        Console_WriteLine("to check");
                                                    }
                                                    MailRecvParam MailRecvParam__ = new MailRecvParam();
                                                    MailRecvParam__.AttemptUndownloadable = 0;
                                                    MailRecvParam__.AttemptBad = 0;
                                                    MailRecvParam__.FileI = FileI;
                                                    MailRecvParam__.Idx = MsgIdx;
                                                    MailRecvParam__.MsgCount = MsgCount;
                                                    MailRecvParam__.MsgInfo = MsgInfoS;
                                                    MailRecvParam__.Account = Account;
                                                    MailRecvParam__.FileSegmentNum = FileSegmentNum;
                                                    MailRecvParam__.cancel = cancel;
                                                    MailRecvParam__.Reconnect = false;
                                                    MailRecvParam__.FileDownloadMode_ = FileDownloadMode_;
                                                    MailRecvParam__.FileDeleteMode_ = FileDeleteMode_;
                                                    MailRecvParam__.ToDeleteFile = ((FileDeleteMode_ & FileDeleteMode.ThisFile) == FileDeleteMode.ThisFile);
                                                    MailRecvParam__.ToDelete = false;
                                                    MailRecvParam_.Add(MailRecvParam__);
                                                }
                                            }
                                            else
                                            {
                                                if (FileDownloadMode_ == FileDownloadMode.CheckExistHeader)
                                                {
                                                    Console_WriteLine("exists");
                                                    MF[FileI].MapSet(FileSegmentNum, 1);
                                                }
                                                if ((FileDownloadMode_ == FileDownloadMode.CompareHeader) || (FileDownloadMode_ == FileDownloadMode.CompareHeaderDigest))
                                                {
                                                    bool Good_ = false;
                                                    int FileSegmentSize = HexToInt(MsgInfo[4]) + 1;
                                                    byte[] Temp = MF[FileI].DataGet(FileSegmentNum);
                                                    if (FileDownloadMode_ == FileDownloadMode.CompareHeaderDigest)
                                                    {
                                                        if (BinToStr(Temp) == MsgInfo[6])
                                                        {
                                                            Good_ = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if ((Temp.Length == FileSegmentSize) && (Digest(Temp) == MsgInfo[6]))
                                                        {
                                                            Good_ = true;
                                                        }
                                                    }

                                                    if (Good_)
                                                    {
                                                        Console_WriteLine("good");
                                                        MF[FileI].MapSet(FileSegmentNum, 1);
                                                    }
                                                    else
                                                    {
                                                        Console_WriteLine("bad");
                                                        if ((FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                                                        {
                                                            DeleteBasedOnHeader = true;
                                                        }
                                                    }
                                                }
                                                if ((FileDeleteMode_ & FileDeleteMode.ThisFile) == FileDeleteMode.ThisFile)
                                                {
                                                    DeleteBasedOnHeader = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console_WriteLine(ConsoleInfoS);
                                        if ((FileDeleteMode_ & FileDeleteMode.OtherFiles) == FileDeleteMode.OtherFiles)
                                        {
                                            DeleteBasedOnHeader = true;
                                        }
                                    }
                                }
                                else
                                {
                                    Console_WriteLine("other message");
                                    if ((FileDeleteMode_ & FileDeleteMode.OtherMsg) == FileDeleteMode.OtherMsg)
                                    {
                                        DeleteBasedOnHeader = true;
                                    }
                                }
                            }
                            else
                            {
                                if (Undownloadable)
                                {
                                    Console_Write(CreateConsoleInfoD(Account, MsgIdx, MsgCount));
                                    Console_WriteLine("undownloadable");
                                    if ((FileDeleteMode_ & FileDeleteMode.Undownloadable) == FileDeleteMode.Undownloadable)
                                    {
                                        DeleteBasedOnHeader = true;
                                    }
                                }
                            }

                            if (DeleteBasedOnHeader)
                            {
                                if (FileDownloadDeleteMark(MsgIdx, Pop3Client_[HeaderThread], ImapClient_Inbox_[HeaderThread], cancel))
                                {
                                    DeletedMsgs++;
                                    if (DecreaseIndexAfterDelete)
                                    {
                                        for (int ii = 0; ii < MailRecvParam_.Count; ii++)
                                        {
                                            if (MailRecvParam_[ii].Idx > MsgIdx)
                                            {
                                                MailRecvParam_[ii].Idx--;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            i--;
                        }
                        if (DecreaseIndexAfterDelete)
                        {
                            MsgCount -= DeletedMsgs;
                            IdxEnd -= DeletedMsgs;
                            i -= DeletedMsgs;
                            DeletedMsgs = 0;
                        }
                    }

                    // Downloading messages
                    if (MailRecvParam_.Count > 0)
                    {
                        DownloadCounter++;
                    }
                    else
                    {
                        DownloadCounter = 0;
                    }
                    if ((i >= IdxEnd) || (MailRecvParam_.Count >= ThreadsDownload_) || (DownloadCounter >= ThreadsDownload_))
                    {
                        if ((MailRecvParam_.Count > 0) && (!NeedConnect))
                        {
                            DownloadCounter = 0;
                            for (int ii = 0; ii < MailRecvParam_.Count; ii++)
                            {
                                MailRecvParam_[ii].Reconnect = false;
                                MailRecvParam_[ii].Good = false;
                                if (Pop3Use)
                                {
                                    MailRecvParam_[ii].Pop3Client_ = Pop3Client_[ii];
                                }
                                else
                                {
                                    MailRecvParam_[ii].ImapClient_Inbox_ = ImapClient_Inbox_[ii];
                                }
                            }
                            FileDownloadMsg(ref MailRecvParam_, MF, ref FileSegmentProgress, FileSegmentCount, SegmentDownloadedAlready);
                            List<int> ToDeleteIdx = new List<int>();
                            for (int ii = 0; ii < MailRecvParam_.Count; ii++)
                            {
                                if (MailRecvParam_[ii].Reconnect)
                                {
                                    NeedConnect = true;
                                }
                                else
                                {
                                    if ((MailRecvParam_[ii].ToDelete) || (MailRecvParam_[ii].ToDeleteFile))
                                    {
                                        ToDeleteIdx.Add(MailRecvParam_[ii].Idx);
                                    }
                                    MailRecvParam_.RemoveAt(ii);
                                    ii--;
                                }
                            }
                            ToDeleteIdx.Sort();
                            for (int ii = (ToDeleteIdx.Count - 1); ii >= 0; ii--)
                            {
                                if (FileDownloadDeleteMark(ToDeleteIdx[ii], Pop3Client_[HeaderThread], ImapClient_Inbox_[HeaderThread], cancel))
                                {
                                    DeletedMsgs++;
                                }
                            }
                        }
                    }

                    // Finish downloading if all segments of all files are downloaded
                    if (((FileDownloadMode_ == FileDownloadMode.Download) || (FileDownloadMode_ == FileDownloadMode.DownloadDigest)) && (FileDeleteMode_ == FileDeleteMode.None))
                    {
                        bool FileAllDownloaded = true;
                        for (int i___ = 0; i___ < MF_Length; i___++)
                        {
                            if (MF[i___].GetSegmentCount() > 0)
                            {
                                MF[i___].MapCalcStats();
                                if (MF[i___].MapCount(0) > 0)
                                {
                                    FileAllDownloaded = false;
                                    break;
                                }
                            }
                            else
                            {
                                FileAllDownloaded = false;
                                break;
                            }
                        }
                        if (FileAllDownloaded)
                        {
                            if (i <= IdxEnd)
                            {
                                Console_WriteLine("Downloaded all segments of all items, no need to iterate over next messages.");
                                MailRecvParam_.Clear();
                                i = IdxEnd + 1;
                            }
                        }
                    }
                }

                Console_WriteLine("Account " + Account + " - disconnecting");
                for (int I = 0; I < ThreadsDownload_; I++)
                {
                    try
                    {
                        if (Pop3Client_[I] != null)
                        {
                            if (Pop3Client_[I].IsConnected)
                            {
                                if ((FileDeleteMode_ != FileDeleteMode.None) && (I == HeaderThread))
                                {
                                    FileDownloadDeleteAction(Pop3Client_[HeaderThread], null, cancel);
                                }
                                Pop3Client_[I].Disconnect(true, cancel.Token);
                            }
                            Pop3Client_[I].Dispose();
                            Pop3Client_[I] = null;
                        }
                        if (ImapClient_[I] != null)
                        {
                            if (ImapClient_[I].IsConnected)
                            {
                                if ((FileDeleteMode_ != FileDeleteMode.None) && (I == HeaderThread))
                                {
                                    FileDownloadDeleteAction(null, ImapClient_Inbox_[HeaderThread], cancel);
                                }
                                if (ImapClient_Inbox_[I].IsOpen)
                                {
                                    if (FileDeleteMode_ != FileDeleteMode.None)
                                    {
                                        ImapClient_Inbox_[I].Close(true, cancel.Token);
                                    }
                                    else
                                    {
                                        ImapClient_Inbox_[I].Close(false, cancel.Token);
                                    }
                                }
                                ImapClient_Inbox_[I] = null;
                                ImapClient_[I].Disconnect(true, cancel.Token);
                            }
                            ImapClient_Inbox_[I] = null;
                            ImapClient_[I].Dispose();
                            ImapClient_[I] = null;
                        }
                    }
                    catch
                    {

                    }
                }
                Console_WriteLine("Account " + Account + " - disconnected");

            }

            MailAccountList[Account].DownloadMinExists = IdxMinExists;
            MailAccountList[Account].DownloadMaxExists = IdxMaxExists;
        }



        public static void FileDownloadMsg(ref List<MailRecvParam> MRP, MailFile[] MF, ref int[] FileSegmentProgress, int[] FileSegmentCount, int[] FileSegmentCountA)
        {
            Stopwatch_ SW = new Stopwatch_();
            long TotalSize = 0;

            List<Thread> Thr = new List<Thread>();

            for (int I = 0; I < MRP.Count; I++)
            {
                MailRecvParam MRP_ = MRP[I];
                if (MRP.Count > 1)
                {
                    MRP_.ThreadNo = Thr.Count + 1;
                    Thread Thr_ = new Thread(() => FileDownloadMsgThr(MRP_, MF));
                    Thr_.Start();
                    Thr.Add(Thr_);
                }
                else
                {
                    MRP_.ThreadNo = 1;
                    FileDownloadMsgThr(MRP_, MF);
                }
            }
            foreach (Thread Thr_ in Thr)
            {
                Thr_.Join();
            }
            CleanUp();
            for (int I = 0; I < MRP.Count; I++)
            {
                if (MRP[I].Good & (!MRP[I].DuplicateInOtherThr))
                {
                    TotalSize = TotalSize + ((long)(HexToInt(MRP[I].MsgInfo.Split(InfoSeparatorC)[4]) + 1));
                    FileSegmentProgress[MRP[I].FileI]++;
                }
            }

            long GetDataSizeSum = 0;
            long GetDataSizeSegSum = 0;
            int FileSegmentProgressSum = 0;
            int FileSegmentCountSum = 0;
            for (int i = 0; i < MF.Length; i++)
            {
                GetDataSizeSum += MF[i].GetDataSize();
                GetDataSizeSegSum += MF[i].GetDataSizeSeg();
                FileSegmentProgressSum += FileSegmentProgress[i];
                FileSegmentCountSum += FileSegmentCount[i];
                FileSegmentCountSum -= FileSegmentCountA[i];
            }
            Console_WriteLine("Download progress: " + FileSegmentProgressSum + "/" + FileSegmentCountSum);
            Console_WriteLine("Download speed: " + KBPS(TotalSize, SW.Elapsed()));
            Log(TSW.Elapsed().ToString(), LogDiffS(FileSegmentProgressSum).ToString(), FileSegmentProgressSum.ToString(), FileSegmentCountSum.ToString(), LogDiffB(KBPS_Bytes).ToString(), KBPS_B(), GetDataSizeSum.ToString(), GetDataSizeSegSum.ToString());
        }

        public static void FileDownloadMsgThr(MailRecvParam MRP_, MailFile[] MF)
        {
            if (MRP_.Good)
            {
                return;
            }

            string[] MsgInfo__ = MRP_.MsgInfo.Split(InfoSeparatorC);
            string ConsoleInfo = CreateConsoleInfoD(MRP_.Account, MRP_.Idx, MRP_.MsgCount, MRP_.FileI, MsgInfo__) + " - thr " + MRP_.ThreadNo.ToString();
            string DownCheck = "";
            if ((MRP_.FileDownloadMode_ == FileDownloadMode.Download) || (MRP_.FileDownloadMode_ == FileDownloadMode.DownloadDigest))
            {
                DownCheck = "download";
            }
            else
            {
                DownCheck = "check";
            }
            Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " started");
            string MsgInfo_ = "";

            int ErrorType = 0;
            byte[] RawData = MailReceive(ConsoleInfo, DownCheck, MRP_.Account, MRP_.ImapClient_Inbox_, MRP_.Pop3Client_, MRP_.Idx, out MsgInfo_, ref ErrorType, MRP_.cancel);

            if (RawData == null)
            {
                MRP_.Reconnect = false;
                MRP_.ToDelete = false;
                switch (ErrorType)
                {
                    case 1:
                        {
                            if (MRP_.AttemptUndownloadable < DownloadRetry)
                            {
                                MRP_.AttemptUndownloadable++;
                                Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: data undownloadable, retry " + MRP_.AttemptUndownloadable.ToString() + "/" + DownloadRetry.ToString());
                                MRP_.Reconnect = true;
                            }
                            else
                            {
                                Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: data undownloadable");
                                if ((MRP_.FileDeleteMode_ & FileDeleteMode.Undownloadable) == FileDeleteMode.Undownloadable)
                                {
                                    MRP_.ToDelete = true;
                                }
                            }
                        }
                        break;
                    case 2:
                        {
                            if (MRP_.AttemptBad < DownloadRetry)
                            {
                                MRP_.AttemptBad++;
                                Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: bad data, retry " + MRP_.AttemptBad.ToString() + "/" + DownloadRetry.ToString());
                                MRP_.Reconnect = true;
                            }
                            else
                            {
                                Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: bad data");
                                if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                                {
                                    MRP_.ToDelete = true;
                                }
                            }
                        }
                        break;
                    case 3:
                        {
                            Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: no reference to connection");
                            MRP_.Reconnect = true;
                        }
                        break;
                }
                return;
            }

            if (MRP_.MsgInfo != MsgInfo_)
            {
                Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: instance mismatch");
                MRP_.Reconnect = true;
                return;
            }

            int SegmentSize = HexToInt(MsgInfo__[4]) + 1;
            int SegmentNum = HexToInt(MsgInfo__[2]);

            if (RawData.Length < SegmentSize)
            {
                if (MRP_.AttemptBad < DownloadRetry)
                {
                    MRP_.AttemptBad++;
                    Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: data segment too short, retry " + MRP_.AttemptBad.ToString() + "/" + DownloadRetry.ToString());
                    MRP_.Reconnect = true;
                }
                else
                {
                    Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: data segment too short");
                    if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                    {
                        MRP_.ToDelete = true;
                    }
                }
                return;
            }

            if (DigestClear(MsgInfo__[6]) != Digest(RawData, SegmentSize))
            {
                if (MRP_.AttemptBad < DownloadRetry)
                {
                    MRP_.AttemptBad++;
                    Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: bad digest, retry " + MRP_.AttemptBad.ToString() + "/" + DownloadRetry.ToString());
                    MRP_.Reconnect = true;
                }
                else
                {
                    Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: bad digest");
                    if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                    {
                        MRP_.ToDelete = true;
                    }
                }
                return;
            }

            Monitor.Enter(MF);
            if ((MRP_.FileDownloadMode_ == FileDownloadMode.CompareBody) || (MRP_.FileDownloadMode_ == FileDownloadMode.CompareBodyDigest))
            {
                if (MF[MRP_.FileI].MapGet(SegmentNum) == 0)
                {
                    bool Good_ = true;
                    byte[] RawDataX = MF[MRP_.FileI].DataGet(SegmentNum);
                    if (MRP_.FileDownloadMode_ == FileDownloadMode.CompareBodyDigest)
                    {
                        byte[] RawDataDigest = StrToBin(Digest(RawData));
                        for (int I = 0; I < MailFile.DigestSize; I++)
                        {
                            if (RawDataX[I] != RawDataDigest[I])
                            {
                                Good_ = false;
                                I = MailFile.DigestSize;
                            }
                        }
                    }
                    else
                    {
                        Good_ = (RawDataX.Length == SegmentSize);
                        if (Good_)
                        {
                            for (int I = 0; I < SegmentSize; I++)
                            {
                                if (RawDataX[I] != RawData[I])
                                {
                                    Good_ = false;
                                    I = SegmentSize;
                                }
                            }
                        }
                    }

                    if (Good_)
                    {
                        MF[MRP_.FileI].MapSet(SegmentNum, 1);
                        Console_WriteLine_Thr(ConsoleInfo + " - check finished - good");
                    }
                    else
                    {
                        Console_WriteLine_Thr(ConsoleInfo + " - check finished - bad");
                        if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
                        {
                            MRP_.ToDelete = true;
                        }
                    }
                }
                else
                {
                    Console_WriteLine_Thr(ConsoleInfo + " - duplicate in other thread");
                    MRP_.DuplicateInOtherThr = true;
                    if ((MRP_.FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
                    {
                        MRP_.ToDelete = true;
                    }
                }
            }
            if (MRP_.FileDownloadMode_ == FileDownloadMode.CheckExistBody)
            {
                if (MF[MRP_.FileI].MapGet(SegmentNum) == 0)
                {
                    MF[MRP_.FileI].MapSet(SegmentNum, 1);
                    Console_WriteLine_Thr(ConsoleInfo + " - check finished - good");
                }
                else
                {
                    Console_WriteLine_Thr(ConsoleInfo + " - duplicate in other thread");
                    MRP_.DuplicateInOtherThr = true;
                    if ((MRP_.FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
                    {
                        MRP_.ToDelete = true;
                    }
                }
            }
            if ((MRP_.FileDownloadMode_ == FileDownloadMode.Download) || (MRP_.FileDownloadMode_ == FileDownloadMode.DownloadDigest))
            {
                if (MF[MRP_.FileI].MapGet(SegmentNum) == 0)
                {
                    MF[MRP_.FileI].DataSet(SegmentNum, RawData, SegmentSize);
                    MF[MRP_.FileI].MapSet(SegmentNum, 1);
                    Console_WriteLine_Thr(ConsoleInfo + " - download finished");
                }
                else
                {
                    Console_WriteLine_Thr(ConsoleInfo + " - duplicate in other thread");
                    MRP_.DuplicateInOtherThr = true;
                    if ((MRP_.FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
                    {
                        MRP_.ToDelete = true;
                    }
                }
            }
            Monitor.Exit(MF);

            MRP_.Good = true;
            MRP_.Reconnect = false;
        }



        static Stopwatch_ TSW;

        public static void FileDownload(int FileCount, string[] FileName, string[] FilePath, string[] FileMap, int[] Account, int[] IdxMin, int[] IdxMax, FileDownloadMode FileDownloadMode_, FileDeleteMode FileDeleteMode_, bool FileDownloadReverseOrder)
        {
            if (Account.Length == 0)
            {
                Console_WriteLine("No accounts");
                return;
            }

            int[] IdxMin_ = new int[IdxMin.Length];
            int[] IdxMax_ = new int[IdxMax.Length];
            MailFile[] MF = new MailFile[FileCount];
            bool[] MF_Open = new bool[FileCount];
            for (int i = 0; i < FileCount; i++)
            {
                MF[i] = new MailFile();
                if ((FileDownloadMode_ == FileDownloadMode.CheckExistHeader) || (FileDownloadMode_ == FileDownloadMode.CheckExistBody))
                {
                    FilePath[i] = null;
                }
            }

            bool _Digest = false;
            _Digest = _Digest || (FileDownloadMode_ == FileDownloadMode.DownloadDigest);
            _Digest = _Digest || (FileDownloadMode_ == FileDownloadMode.CompareHeaderDigest);
            _Digest = _Digest || (FileDownloadMode_ == FileDownloadMode.CompareBodyDigest);
            bool _NotDownload = true;
            _NotDownload = _NotDownload && (FileDownloadMode_ != FileDownloadMode.Download);
            _NotDownload = _NotDownload && (FileDownloadMode_ != FileDownloadMode.DownloadDigest);

            bool OpenAll = true;
            for (int i = 0; i < FileCount; i++)
            {
                MF_Open[i] = MF[i].Open(_Digest, _NotDownload, FilePath[i], FileMap[i]);
                if (!MF_Open[i])
                {
                    OpenAll = false;
                }
            }
            if (OpenAll)
            {
                Log("");
                LogReset();
                Log("Time stamp", "Downloaded segments since previous entry", "Totally downloaded segments", "All segments", "Downloaded bytes since previous entry", "Totally downloaded bytes", "All bytes by segment count", "All bytes by file size");
                TSW = new Stopwatch_();
                KBPSReset();

                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    MF[i_].MapChange(1, 2);
                }
                for (int i = 0; i < Account.Length; i++)
                {
                    bool FileGood = true;
                    for (int i_ = 0; i_ < FileCount; i_++)
                    {
                        MF[i_].MapCalcStats();
                        if ((MF[i_].MapCount(0) <= 0) && (MF[i_].GetSegmentCount() != 0))
                        {
                            FileGood = false;
                        }
                    }
                    if (FileGood)
                    {
                        MailAccountList[Account[i]].DownloadMinAccount = IdxMin[i];
                        MailAccountList[Account[i]].DownloadMaxAccount = IdxMax[i];
                        FileDownloadAccount(Account[i], FileDownloadReverseOrder, FileDownloadMode_, FileDeleteMode_, FileName, ref MF);
                        IdxMin_[i] = MailAccountList[Account[i]].DownloadMinExists + 1;
                        IdxMax_[i] = MailAccountList[Account[i]].DownloadMaxExists + 1;
                    }
                }
                ConsoleLineToLog = true;
                ConsoleLineToLogSum = true;
                Console_WriteLine("");
                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    if (MF[i_].ResizeNeed())
                    {
                        Console_WriteLine("Item " + (i_ + 1).ToString() + " - resize started");
                        MF[i_].Resize();
                        Console_WriteLine("Item " + (i_ + 1).ToString() + " - resize finished");
                    }
                    else
                    {
                        Console_WriteLine("Item " + (i_ + 1).ToString() + " - not found");
                    }
                }
                Console_WriteLine("");
                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    MF[i_].MapCalcStats();
                    Console_WriteLine("Item " + (i_ + 1).ToString() + ":");
                    Console_WriteLine(" Total segments: " + MF[i_].GetSegmentCount().ToString());
                    if ((FileDownloadMode_ == FileDownloadMode.Download) || (FileDownloadMode_ == FileDownloadMode.DownloadDigest))
                    {
                        Console_WriteLine(" Segments downloaded previously: " + MF[i_].MapCount(2).ToString());
                        Console_WriteLine(" Segments downloaded now: " + MF[i_].MapCount(1).ToString());
                        Console_WriteLine(" Segments not downloaded: " + MF[i_].MapCount(0).ToString());
                    }
                    else
                    {
                        Console_WriteLine(" Segments checked previously as good: " + MF[i_].MapCount(2).ToString());
                        Console_WriteLine(" Good segments: " + MF[i_].MapCount(1).ToString());
                        Console_WriteLine(" Bad or missing segments: " + MF[i_].MapCount(0).ToString());
                    }
                }
                Console_WriteLine("Downloaded bytes: " + KBPS_B());
                Console_WriteLine("Download time: " + KBPS_T());
                Console_WriteLine("Average download speed: " + KBPS());
                for (int i = 0; i < Account.Length; i++)
                {
                    string TempInfo = "";
                    TempInfo = TempInfo + "Account " + Account[i];
                    TempInfo = TempInfo + " from " + ((IdxMin[i] > 0) ? IdxMin[i].ToString() : "the first message");
                    TempInfo = TempInfo + " to " + ((IdxMax[i] > 0) ? IdxMax[i].ToString() : "the last message");
                    if (IdxMin_[i] > 0)
                    {
                        TempInfo = TempInfo + " - found from " + IdxMin_[i] + " to " + IdxMax_[i];
                    }
                    else
                    {
                        TempInfo = TempInfo + " - not found";
                    }
                    Console_WriteLine(TempInfo);
                }
                Console_WriteLine("Total time: " + TimeHMSM(TSW.Elapsed()));
                ConsoleLineToLogSum = false;
                ConsoleLineToLog = false;
                LogSum("");
                LogSum("");
                for (int i_ = 0; i_ < FileCount; i_++)
                {
                    MF[i_].Close();
                }
            }
            else
            {
                ConsoleLineToLog = true;
                ConsoleLineToLogSum = false;
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
                LogSum("");
                LogSum("");
                return;
            }
        }


        private static string CreateConsoleInfo_Msg1(int AccNo, int MsgIdx, int MsgCount)
        {
            return "Account " + AccNo.ToString() + ", msg " + (MsgIdx + 1).ToString() + "/" + MsgCount.ToString() + ": ";
        }

        private static string CreateConsoleInfo_Msg2(int FileI, string[] MsgInfo)
        {
            if (FileI >= 0)
            {
                return "item " + (FileI + 1).ToString() + " - seg " + (HexToInt(MsgInfo[2]) + 1) + "/" + (HexToInt(MsgInfo[3]) + 1);
            }
            else
            {
                return "other item - seg " + (HexToInt(MsgInfo[2]) + 1) + "/" + (HexToInt(MsgInfo[3]) + 1);
            }
        }

        /// <summary>
        /// Download message prefix for file message
        /// </summary>
        /// <param name="AccNo"></param>
        /// <param name="MsgIdx"></param>
        /// <param name="MsgCount"></param>
        /// <param name="MsgInfo"></param>
        /// <returns></returns>
        public static string CreateConsoleInfoD(int AccNo, int MsgIdx, int MsgCount, int FileI, string[] MsgInfo)
        {
            return CreateConsoleInfo_Msg1(AccNo, MsgIdx, MsgCount) + CreateConsoleInfo_Msg2(FileI, MsgInfo);
        }

        public static string CreateConsoleInfoDS(int AccNo, int MsgIdx, int MsgCount, int FileI, string[] MsgInfo)
        {
            return CreateConsoleInfo_Msg2(FileI, MsgInfo);
        }

        /// <summary>
        /// Download message prefix for message other than file
        /// </summary>
        /// <param name="AccNo"></param>
        /// <param name="MsgIdx"></param>
        /// <param name="MsgCount"></param>
        /// <returns></returns>
        public static string CreateConsoleInfoD(int AccNo, int MsgIdx, int MsgCount)
        {
            return CreateConsoleInfo_Msg1(AccNo, MsgIdx, MsgCount);
        }
    }
}
