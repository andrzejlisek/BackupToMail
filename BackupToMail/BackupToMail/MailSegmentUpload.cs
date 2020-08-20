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
		public static void MailSend(ref MailSendParam MSP, string FileName, int SegmentSize0, int SegmentCount, List<MailAccount> MailAccountList, int[] AccountDst, int SegmentMode, int SegmentImageSize, CancellationTokenSource cancel)
		{
			string ConsoleInfo = CreateConsoleInfoU(MSP.FileSegmentNum, SegmentCount);
			string AccountInfo = "";
			if (MailAccountList[MSP.AccountSrc].SmtpConnect)
			{
				AccountInfo = "(group " + (MSP.AccountSrcG + 1) + ", account " + MSP.AccountSrc + ")";
			}
			else
			{
				AccountInfo = "(group " + (MSP.AccountSrcG + 1) + ", account " + MSP.AccountSrc + ", slot " + (MSP.SmtpClientSlot + 1) + ")";
			}
			MSP.Good = true;

            Console_WriteLine_Thr(ConsoleInfo + " - upload started " + AccountInfo);


			string AttaInfo = InfoSeparatorS + FileName + InfoSeparatorS + IntToHex(MSP.FileSegmentNum, SegmentCount - 1) + InfoSeparatorS + IntToHex(SegmentCount - 1) + InfoSeparatorS + IntToHex(MSP.FileSegmentSize - 1, SegmentSize0 - 1) + InfoSeparatorS + IntToHex(SegmentSize0 - 1) + InfoSeparatorS + Digest(MSP.SegmentBuf) + InfoSeparatorS;
			
			
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
				case 0:
					{
	            		BB.TextBody = "Attachment";
			            BB.Attachments.Add("data.bin", MSP.SegmentBuf, ContentType.Parse("application/octet-stream"));
					}
		            break;
				case 1:
		            {
	            		BB.TextBody = "Attachment";
			            BB.Attachments.Add("data.png", ConvRaw2Img(MSP.SegmentBuf, SegmentImageSize), ContentType.Parse("image/png"));
		            }
		            break;
				case 2:
		            {
	            		BB.TextBody = ConvRaw2Txt(MSP.SegmentBuf);
		            }
					break;
				case 3:
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
					Console_WriteLine_Thr(ConsoleInfo + " - connecting " + AccountInfo);
					using (SmtpClient SC = MailAccountList[MSP.AccountSrc].SmtpClient_(cancel))
					{
						Console_WriteLine_Thr(ConsoleInfo + " - connected, sending segment " + AccountInfo);
						SC.Send(Msg);
						Console_WriteLine_Thr(ConsoleInfo + " - disconnecting " + AccountInfo);
						SC.Disconnect(true, cancel.Token);
						Console_WriteLine_Thr(ConsoleInfo + " - disconnected " + AccountInfo);
					}
				}
				else
				{
					bool ServerReconn = MailAccountList[MSP.AccountSrc].SmtpClient_Need_Connect(MSP.SmtpClient_[MSP.AccountSrcN, MSP.SmtpClientSlot]);
					if (ServerReconn)
					{
						Console_WriteLine_Thr(ConsoleInfo + " - connecting " + AccountInfo);
					}
					SmtpClient SC = MailAccountList[MSP.AccountSrc].SmtpClient_Connect(MSP.SmtpClient_[MSP.AccountSrcN, MSP.SmtpClientSlot], cancel);
					if (ServerReconn) 
					{
						Console_WriteLine_Thr(ConsoleInfo + " - connected, sending segment " + AccountInfo);
					}
					else
					{
						Console_WriteLine_Thr(ConsoleInfo + " - sending segment " + AccountInfo);
					}
					MSP.SmtpClient_[MSP.AccountSrcN, MSP.SmtpClientSlot] = SC;
					SC.Send(Msg);
				}
				Console_WriteLine_Thr(ConsoleInfo + " - upload finished " + AccountInfo);
            }
            catch (Exception e)
            {
	            Console_WriteLine_Thr(ConsoleInfo + " - upload error " + AccountInfo + ": " + ExcMsg(e));
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
		public static bool FileUploadMsg(int[] AccountSrc, int[] AccountDst, List<MailSendParam> MailSendParam_, ref MailFile MF, string FileName, ref int FileSegmentProgress, int FileSegmentCount, int FileSegmentToDo, int FileSegmentSize, int SegmentMode, int SegmentImageSize, CancellationTokenSource cancel)
		{
			Stopwatch_ SW = new Stopwatch_();

			for (int i = 0; i < MailSendParam_.Count; i++)
			{
				string ConsoleInfo = CreateConsoleInfoU(MailSendParam_[i].FileSegmentNum, FileSegmentCount);
				Console_WriteLine(ConsoleInfo + " - to upload from group " + (MailSendParam_[i].AccountSrcG + 1) + ", account " + MailSendParam_[i].AccountSrc);
			}
			
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
				MailSend(ref MailSendParam___, FileName, FileSegmentSize, FileSegmentCount, MailAccountList, AccountDst, SegmentMode, SegmentImageSize, cancel);
			}
			else
			{
				List<Thread> Thr = new List<Thread>();
				while ((I < MailSendParam_.Count) && (I < ThreadsUpload))
				{
					MailSendParam MailSendParam___ = MailSendParam_[I];
					MailSendParam___.Idx = I;
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
					MF.MapSet(MailSendParam_[I].FileSegmentNum, 1);
					FileSegmentProgress++;
					TotalSize += MailSendParam_[I].FileSegmentSize;
					MailSendParam_.RemoveAt(I);
				}
				else
				{
					MF.MapSet(MailSendParam_[I].FileSegmentNum, 0);
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
			Console_WriteLine("Upload progress: " + FileSegmentProgress + "/" + FileSegmentToDo);
			Console_WriteLine("Upload speed: " + KBPS(TotalSize, SW.Elapsed()));
			Log(TSW.Elapsed().ToString(), LogDiffS(FileSegmentProgress).ToString(), FileSegmentProgress.ToString(), FileSegmentToDo.ToString(), LogDiffB(KBPS_Bytes).ToString(), KBPS_B(), MF.GetDataSize().ToString(), MF.GetDataSizeSeg().ToString());
			
			// If uploaded nothing several times at a row, return false, otherwise return true
			if (UploadedSomething)
			{
				UploadFailureCounter = 0;
				return true;
			}
			else
			{
				UploadFailureCounter++;
				if (UploadFailureCounter >= UploadGroupChange)
				{
					UploadFailureCounter = 0;
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		static int UploadFailureCounter;
		
		/// <summary>
		/// File upload action
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FilePath"></param>
		/// <param name="FileMap"></param>
		/// <param name="AccountSrc"></param>
		/// <param name="AccountDst"></param>
		/// <param name="FileSegmentSize"></param>
		/// <param name="SegmentMode"></param>
		/// <param name="SegmentImageSize"></param>
		public static void FileUpload(string FileName, string FilePath, string FileMap, int[] AccountSrc, int[] AccountDst, int FileSegmentSize, int SegmentMode, int SegmentImageSize)
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
			FileName = Digest(FileName);
			
			// Reset upload failure counter
			UploadFailureCounter = 0;
			
			MailFile MF = new MailFile();
			if (MF.Open(true, FilePath, FileMap))
			{
                Log("");
                LogReset();
                Log("Time stamp", "Uploaded segments since previous entry", "Totally uploaded segments", "All segments", "Uploaded bytes since previous entry", "Totally uploaded bytes", "All bytes by segment count", "All bytes by file size");
                TSW = new Stopwatch_();
				KBPSReset();
				MF.MapChange(1, 2);
				MF.SetSegmentSize(FileSegmentSize);
				if (MF.CalcSegmentCount() > 0)
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
						
						int FileSegmentProgress = 0;
						
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
						

						int FileSegmentCount = MF.GetSegmentCount();
						
						MF.MapCalcStats();
						int FileSegmentToDo = MF.MapCount(0);
	
						// The loop iterates over all file segments, but also iterates while there are some segments to upload  
						for (int FileSegmentNum = 0; FileSegmentNum < FileSegmentCount; FileSegmentNum++)
						{
							// Creating console information prefix
							string ConsoleInfo = CreateConsoleInfoU(FileSegmentNum, FileSegmentCount);
							
							// Upload only this segments, which must be uploaded based on map file
							if (MF.MapGet(FileSegmentNum) == 0)
							{
								byte[] SegmentBuf = MF.DataGet(FileSegmentNum);
		
								// Create a object, which keeps all needed data during whole sending process of current segment
								MailSendParam MailSendParam__ = new MailSendParam();
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
								
								// If the number of segments is at least the same as number of threads, there will be attemped to send,
								// if none of segments are sent, the attemp will be repeated immediately
					            while (MailSendParam_.Count >= ThreadsUpload)
								{
				            		// If FileUploadMsg returned false, the group must be changed
					            	if (!FileUploadMsg(AccountSrc, AccountDst, MailSendParam_, ref MF, FileName, ref FileSegmentProgress, FileSegmentCount, FileSegmentToDo, FileSegmentSize, SegmentMode, SegmentImageSize, cancel))
					            	{
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
					            		UploadGroup++;
					            		if (UploadGroup >= UploadGroupN.Count)
					            		{
					            			UploadGroup = 0;
					            		}

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
								}
							}
							else
							{
								Console_WriteLine(ConsoleInfo + " - not to upload");
							}
						}
						
						// If exists of some of segments to send, which was not sent at the first attemp,
						// there will be attemped to send once
						while (MailSendParam_.Count > 0)
						{
		            		// If FileUploadMsg returned false, the group must be changed
							if (!FileUploadMsg(AccountSrc, AccountDst, MailSendParam_, ref MF, FileName, ref FileSegmentProgress, FileSegmentCount, FileSegmentToDo, FileSegmentSize, SegmentMode, SegmentImageSize, cancel))
							{
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
			            		UploadGroup++;
			            		if (UploadGroup >= UploadGroupN.Count)
			            		{
			            			UploadGroup = 0;
			            		}

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
						}
		
						// Closing all opened connections
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
						Console_WriteLine("Disconnected");
		            }
		            MF.MapCalcStats();
                    Log("");
                    Console_WriteLine("");
                    ConsoleLineToLog = true;
                    Console_WriteLine("Total segments: " + MF.GetSegmentCount().ToString());
		            Console_WriteLine("Segments uploaded previously: " + MF.MapCount(2).ToString());
		            Console_WriteLine("Segments uploaded now: " + MF.MapCount(1).ToString());
	            	Console_WriteLine("Uploaded bytes: " + KBPS_B());
	            	Console_WriteLine("Upload time: " + KBPS_T());
	            	Console_WriteLine("Average upload speed: " + KBPS());
					Console_WriteLine("Total time: " + TimeHMSM(TSW.Elapsed()));
                    ConsoleLineToLog = false;
                }
                else
				{
                    Log("");
                    ConsoleLineToLog = true;
                    Console_WriteLine("File size is 0 bytes");
                    ConsoleLineToLog = false;
                }
                Log("");
                Log("");
                MF.Close();
			}
			else
			{
                ConsoleLineToLog = true;
                Console_WriteLine("File open error: " + MF.OpenError);
                ConsoleLineToLog = false;
                Log("");
                Log("");
                return;
			}
		}
		
		/// <summary>
		/// Upload message prefix
		/// </summary>
		/// <param name="MsgIdx"></param>
		/// <param name="MsgCount"></param>
		/// <returns></returns>
		public static string CreateConsoleInfoU(int MsgIdx, int MsgCount)
	    {
	    	return "Segment " + (MsgIdx + 1) + "/" + MsgCount;
	    }

	}
}
