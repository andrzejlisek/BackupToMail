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
		}

		[FlagsAttribute]
		public enum FileDeleteMode
		{
			None = 0,
			Bad = 1,
			Duplicate = 2,
			ThisFile = 4,
			OtherMsg = 8,
			OtherFiles = 16
		}

		public static byte[] MailReceive(int Account, IMailFolder ImapClient_, Pop3Client Pop3Client_, int Idx, out string MsgInfo_, ref bool Reconnect, CancellationTokenSource cancel)
		{
			if ((ImapClient_ == null) && (Pop3Client_ == null))
			{
				MsgInfo_ = "";
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
	            		Console_WriteLine_Thr("Account " + Account + " - message download error: " + ExcMsg(e));
	    				Reconnect = true;
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
			
			return null;
		}


		public static void FileDownloadDeleteMark(int I, Pop3Client Pop3Client_, IMailFolder ImapClient_Inbox_, CancellationTokenSource cancel)
		{
			if (Pop3Client_ != null)
			{
				Pop3Client_.DeleteMessage(I);
			}
			if (ImapClient_Inbox_ != null)
			{
            	ImapClient_Inbox_.AddFlags(I, MessageFlags.Deleted, true, cancel.Token);
			}
		}

		public static void FileDownloadDeleteAction(Pop3Client Pop3Client_, CancellationTokenSource cancel)
		{
		}

		public static void FileDownloadDeleteAction(IMailFolder ImapClient_Inbox_, CancellationTokenSource cancel)
		{
			ImapClient_Inbox_.Expunge();
		}

		
		public static void FileDownloadAccount(int Account, FileDownloadMode FileDownloadMode_, FileDeleteMode FileDeleteMode_, string FileName_, ref MailFile MF)
		{
            int DeletedMsgs = 0;
            int SegmentDownloadedAlready = 0;
			string FileName = Digest(FileName_);
			bool DownloadMessage = false;
			if (FileDownloadMode_ == FileDownloadMode.Download) { DownloadMessage = true; }
			if (FileDownloadMode_ == FileDownloadMode.CheckExistBody) { DownloadMessage = true; }
			if (FileDownloadMode_ == FileDownloadMode.CompareBody) { DownloadMessage = true; }
			int ThreadsDownload_ = DownloadMessage ? ThreadsDownload : 0;
			int IdxMin = MailAccountList[Account].DownloadMin;
			int IdxMax = MailAccountList[Account].DownloadMax;
			int IdxMin_ = (IdxMin > 0) ? (IdxMin - 1) : 0;
			int IdxMax_ = IdxMin_;
			int IdxMin___ = -1;
			int IdxMax___ = -1;
            using (CancellationTokenSource cancel = new CancellationTokenSource ())
            {
            	bool Pop3Use = MailAccountList[Account].Pop3Use;
                bool DecreaseIndexAfterDelete = MailAccountList[Account].DeleteIdx;

                Pop3Client[] Pop3Client_ = new Pop3Client[ThreadsDownload_ + 1];
				ImapClient[] ImapClient_ = new ImapClient[ThreadsDownload_ + 1];
				IMailFolder[] ImapClient_Inbox_ = new IMailFolder[ThreadsDownload_ + 1];

	
				List<MailRecvParam> MailRecvParam_ = new List<MailRecvParam>();
	            
				int FileSegmentProgress = 0;
				int FileSegmentCount = 0;
				int FileSegmentSize0 = 0;
	            
				int MsgCount = 1;
				bool NeedConnect = true;
				int BlankIndexes = 0;
				for (int i = IdxMin_; (i <= IdxMax_) || (MailRecvParam_.Count > 0); i++)
	            {
	            	while (NeedConnect)
	            	{
                        DeletedMsgs = 0;
                        Console_WriteLine("Account " + Account + " - connecting (disconnecting existing connections)");
	            		bool ConnGood = true;
						for (int I = 0; I <= ThreadsDownload_; I++)
						{
							try
							{
								if (Pop3Use)
								{
									if (Pop3Client_[I] != null)
									{
										if (Pop3Client_[I].IsConnected)
										{
											Pop3Client_[I].Disconnect(true, cancel.Token);
										}
										Pop3Client_[I].Dispose();
									}
									Pop3Client_[I] = MailAccountList[Account].Pop3Client_(cancel);
								}
								else
								{
									if (ImapClient_[I] != null)
									{
										if (ImapClient_[I].IsConnected)
										{
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
									if ((FileDeleteMode_ != FileDeleteMode.None) && (I == ThreadsDownload_))
									{
										ImapClient_[I] = MailAccountList[Account].ImapClient_(cancel, true);
									}
									else
									{
										ImapClient_[I] = MailAccountList[Account].ImapClient_(cancel, false);
									}
								}
							}
							catch (Exception e)
							{
								Console_WriteLine("Account " + Account + " - connection error: " + ExcMsg(e));
								Pop3Client_[I] = null;
								ImapClient_[I] = null;
								ImapClient_Inbox_[I] = null;
								ConnGood = false;
							}
						}

						if (ConnGood)
						{
							Console_WriteLine("Account " + Account + " - connected");
							MsgCount = -1;
							for (int I = 0; I <= ThreadsDownload_; I++)
							{
								if (Pop3Use)
								{
									int MsgCountT = Pop3Client_[I].Count;
									if ((MsgCount >= 0) && (MsgCount != MsgCountT))
									{
										Console_WriteLine("Account " + Account + " - message count not the same in all threads");
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
										Console_WriteLine("Account " + Account + " - message count not the same in all threads");
										ConnGood = false;
									}
									MsgCount = MsgCountT;
								}
							}
							if (IdxMax < 0)
							{
								IdxMax_ = MsgCount - 1;
							}
							else
							{
								IdxMax_ = Math.Min(MsgCount - 1, IdxMax - 1);
							}
						}
						
						if (ConnGood)
						{
		    				if (FileDownloadMode_ == FileDownloadMode.Download)
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

	            	if (i <= IdxMax_)
	            	{
	            		if ((MailRecvParam_.Count < ThreadsDownload_) || (!DownloadMessage))
	            		{
			            	HeaderList MsgH = null;
		            		if (MsgCount > 0)
		            		{
				            	try
				            	{
				            		Console_Write(CreateConsoleInfoD(Account, i, MsgCount));
					            	if (Pop3Use)
					            	{
					            		MsgH = Pop3Client_[ThreadsDownload_].GetMessageHeaders(i, cancel.Token);
					            	}
					            	else
					            	{
					            		MsgH = ImapClient_Inbox_[ThreadsDownload_].GetHeaders(i, cancel.Token);
					            	}
                                }
				            	catch (Exception e)
				            	{
				            		MsgH = null;
				            		Console_WriteLine("header download error: " + ExcMsg(e));
				            		i--;
                                    i -= DeletedMsgs;
				            		NeedConnect = true;
				            	}
			            	}
		            		else
		            		{
		            			Console_WriteLine("Account " + Account + " - no messages");
		            		}
			            	
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
				            		string ConsoleInfoS = CreateConsoleInfoDS(Account, i, MsgCount, MsgInfo);
				            		Console_Write(ConsoleInfoS + " - ");

				            		if (MsgInfo[1] == FileName)
					            	{
					            		bool Good = true;
					            		if (FileSegmentCount == 0)
					            		{
								    		FileSegmentCount = HexToInt(MsgInfo[3]) + 1;
								    		FileSegmentSize0 = HexToInt(MsgInfo[5]) + 1;
								    		MF.SetSegmentCount(FileSegmentCount);
					    		    		MF.SetSegmentSize(FileSegmentSize0);
					    		    		MF.MapCalcStats();
					    		    		SegmentDownloadedAlready = MF.MapCount(1) + MF.MapCount(2);
					    		    		
							    			if ((FileDownloadMode_ == FileDownloadMode.Download) && (FileDeleteMode_ == FileDeleteMode.None))
								    		{
							    				if (MF.MapCount(0) == 0)
								    			{
								    				if (FileDownloadMode_ == FileDownloadMode.Download)
								    				{
											    		Console_WriteLine("not to download");
								    				}
								    				else
								    				{
											    		Console_WriteLine("not to check");
								    				}
								    				Console_WriteLine("Downloaded all segments, no need to iterate over next messages.");
								    				MailRecvParam_.Clear();
								    				i = IdxMax_ + 1;
									    			Good = false;
								    			}
								    		}
					            		}
					            		else
					            		{
					            			if (FileSegmentCount != (HexToInt(MsgInfo[3]) + 1))
					            			{
									    		Console_WriteLine("segment count mismatch");
					            				if ((FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
					            				{
							            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                                    DeletedMsgs++;
                                                }
					            				Good = false;
					            			}
					            			if (FileSegmentSize0 != (HexToInt(MsgInfo[5]) + 1))
					            			{
									    		Console_WriteLine("segment nominal size mismatch");
					            				if ((FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
					            				{
							            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                                    DeletedMsgs++;
                                                }
                                                Good = false;
					            			}
					            		}
					            		
					            		int FileSegmentNum = HexToInt(MsgInfo[2]);
					            		
					            		if (Good)
					            		{
					            			if (MF.MapGet(FileSegmentNum) == 2)
					            			{
							    				if (FileDownloadMode_ == FileDownloadMode.Download)
							    				{
										    		Console_WriteLine("not to download");
							    				}
							    				else
							    				{
										    		Console_WriteLine("not to check");
							    				}
					            				Good = false;
					            			}
					            			if (MF.MapGet(FileSegmentNum) == 1)
					            			{
					            				if ((FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
					            				{
							            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                                    DeletedMsgs++;
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
						    				if ((IdxMin___ < 0) || (IdxMin___ > i))
						    				{
						    					IdxMin___ = i;
						    				}
						    				if ((IdxMax___ < 0) || (IdxMax___ < i))
						    				{
						    					IdxMax___ = i;
						    				}
							    			if (DownloadMessage)
							    			{
							    				BlankIndexes = 0;
							    				if (FileDownloadMode_ == FileDownloadMode.Download)
							    				{
									    			Console_WriteLine("to download");
							    				}
							    				else
							    				{
									    			Console_WriteLine("to check");
							    				}
									    		MailRecvParam MailRecvParam__ = new MailRecvParam();
									    		MailRecvParam__.Idx = i;
									    		MailRecvParam__.MsgCount = MsgCount;
									    		MailRecvParam__.MsgInfo = MsgInfoS;
									    		MailRecvParam__.Account = Account;
									    		MailRecvParam__.FileSegmentNum = FileSegmentNum;
									    		MailRecvParam__.cancel = cancel;
									    		MailRecvParam__.Reconnect = false;
									    		MailRecvParam__.FileDownloadMode_ = FileDownloadMode_;
									    		MailRecvParam__.FileDeleteMode_ = FileDeleteMode_;
									    		MailRecvParam__.ToDelete = ((FileDeleteMode_ & FileDeleteMode.ThisFile) == FileDeleteMode.ThisFile);
									    		MailRecvParam_.Add(MailRecvParam__);
							    			}
							    			else
							    			{
									    		if (FileDownloadMode_ == FileDownloadMode.CheckExistHeader)
									    		{
										    		Console_WriteLine("exists");
										    		MF.MapSet(FileSegmentNum, 1);
									    		}
									    		if (FileDownloadMode_ == FileDownloadMode.CompareHeader)
									    		{
										    		bool Good_ = false;
								    				int FileSegmentSize = HexToInt(MsgInfo[4]) + 1;
								    				byte[] Temp = MF.DataGet(FileSegmentNum);
								    				if ((Temp.Length == FileSegmentSize) && (Digest(Temp) == MsgInfo[6]))
								    				{
								    					Good_ = true;
								    				}
										    		
										    		if (Good_)
										    		{
											    		Console_WriteLine("good");
											    		MF.MapSet(FileSegmentNum, 1);
										    		}
										    		else
										    		{
											    		Console_WriteLine("bad");
							            				if ((FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
							            				{
									            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                                            DeletedMsgs++;
                                                        }
                                                    }
									    		}
								    			if ((FileDeleteMode_ & FileDeleteMode.ThisFile) == FileDeleteMode.ThisFile)
							            		{
							            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                                    DeletedMsgs++;
                                                }
                                            }
							    		}
							    		else
							    		{
						            		BlankIndexes++;
							    		}
					            	}
					            	else
					            	{
					            		BlankIndexes++;
					            		Console_WriteLine("other file");
										if ((FileDeleteMode_ & FileDeleteMode.OtherFiles) == FileDeleteMode.OtherFiles)
					            		{
					            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                            DeletedMsgs++;
                                        }
                                    }
				            	}
								else
								{
				            		BlankIndexes++;
									Console_WriteLine("other message");
									if ((FileDeleteMode_ & FileDeleteMode.OtherMsg) == FileDeleteMode.OtherMsg)
				            		{
				            			FileDownloadDeleteMark(i, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                        DeletedMsgs++;
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
                            if (IdxMax > 0)
                            {
                                IdxMax -= DeletedMsgs;
                                IdxMax_ = Math.Min(MsgCount - 1, IdxMax - 1);
                                if (IdxMax < 0)
                                {
                                    IdxMax = 0;
                                }
                            }
                            else
                            {
                                IdxMax_ = MsgCount - 1;
                            }

                            i -= DeletedMsgs;
                            DeletedMsgs = 0;
                        }
                    }

                    if ((i >= IdxMax_) || (MailRecvParam_.Count >= ThreadsDownload_) || (BlankIndexes >= ThreadsDownload_))
		    		{
						if ((MailRecvParam_.Count > 0) && (!NeedConnect))
						{
			    			BlankIndexes = 0;
			    			for (int ii = 0; ii < MailRecvParam_.Count; ii++)
			    			{
			    				if (Pop3Use)
			    				{
				    				MailRecvParam_[ii].Pop3Client_ = Pop3Client_[ii];
			    				}
			    				else
			    				{
			        				MailRecvParam_[ii].ImapClient_Inbox_ = ImapClient_Inbox_[ii];
			    				}
			    			}
			    			FileDownloadMsg(ref MailRecvParam_, MF, ref FileSegmentProgress, FileSegmentCount - SegmentDownloadedAlready);
	            			for (int ii = 0; ii < MailRecvParam_.Count; ii++)
	            			{
	            				if (MailRecvParam_[ii].ToDelete)
	            				{
			            			FileDownloadDeleteMark(MailRecvParam_[ii].Idx, Pop3Client_[ThreadsDownload_], ImapClient_Inbox_[ThreadsDownload_], cancel);
                                    DeletedMsgs++;
                                }
                                if (MailRecvParam_[ii].Reconnect)
	            				{
	            					NeedConnect = true;
	            				}
	            				else
	            				{
	            					MailRecvParam_.RemoveAt(ii);
	            					ii--;
	            				}
	            			}
			    			if ((FileDownloadMode_ == FileDownloadMode.Download) && (FileDeleteMode_ == FileDeleteMode.None))
				    		{
				    			if (FileSegmentProgress == (FileSegmentCount - SegmentDownloadedAlready))
				    			{
				    				Console_WriteLine("Downloaded all segments, no need to iterate over next messages.");
				    				i = IdxMax_ + 1;
				    				MailRecvParam_.Clear();
				    			}
				    		}
						}
		    		}
	            }
	            		
				Console_WriteLine("Account " + Account + " - disconnecting");
				for (int I = 0; I <= ThreadsDownload_; I++)
				{
					if (Pop3Client_[I] != null)
					{
						if (Pop3Client_[I].IsConnected)
			            {
							if ((FileDeleteMode_ != FileDeleteMode.None) && (I == ThreadsDownload_))
		            		{	
		            			FileDownloadDeleteAction(Pop3Client_[ThreadsDownload_], cancel);
	            			}
			                Pop3Client_[I].Disconnect(true, cancel.Token);
			            }            
			            Pop3Client_[I].Dispose();
					}
					if (ImapClient_[I] != null)
					{
						if (ImapClient_[I].IsConnected)
			            {
							if ((FileDeleteMode_ != FileDeleteMode.None) && (I == ThreadsDownload_))
		            		{	
		            			FileDownloadDeleteAction(ImapClient_Inbox_[ThreadsDownload_], cancel);
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
					}
				}
				Console_WriteLine("Account " + Account + " - disconnected");

            }
            
            MailAccountList[Account].DownloadMin_ = IdxMin___;
            MailAccountList[Account].DownloadMax_ = IdxMax___;
		}



		public static void FileDownloadMsg(ref List<MailRecvParam> MRP, MailFile MF, ref int FileSegmentProgress, int FileSegmentCount)
		{
			Stopwatch_ SW = new Stopwatch_();
			long TotalSize = 0;
			
			List<Thread> Thr = new List<Thread>();

			for (int I = 0; I < MRP.Count; I++)
			{
    			MailRecvParam MRP_ = MRP[I];
	    		if (MRP.Count > 1)
	    		{
	    			Thread Thr_ = new Thread(() => FileDownloadMsgThr(MRP_, MF));
	    			Thr_.Start();
	    			Thr.Add(Thr_);
	    		}
	    		else
	    		{
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
				if (MRP[I].Good)
				{
					TotalSize = TotalSize + ((long)(HexToInt(MRP[I].MsgInfo.Split(InfoSeparatorC)[4]) + 1));
					FileSegmentProgress++;
				}
			}
			
			Console_WriteLine("Download progress: " + FileSegmentProgress + "/" + FileSegmentCount);
			Console_WriteLine("Download speed: " + KBPS(TotalSize, SW.Elapsed()));
			Log(TSW.Elapsed().ToString(), LogDiffS(FileSegmentProgress).ToString(), FileSegmentProgress.ToString(), FileSegmentCount.ToString(), LogDiffB(KBPS_Bytes).ToString(), KBPS_B(), MF.GetDataSize().ToString(), MF.GetDataSizeSeg().ToString());
		}

		public static void FileDownloadMsgThr(MailRecvParam MRP_, MailFile MF)
		{
			MRP_.Good = false;

			string[] MsgInfo__ = MRP_.MsgInfo.Split(InfoSeparatorC);
			string ConsoleInfo = CreateConsoleInfoD(MRP_.Account, MRP_.Idx, MRP_.MsgCount, MsgInfo__);
			string DownCheck = "";
			if (MRP_.FileDownloadMode_ == FileDownloadMode.Download)
			{
				DownCheck = "download";
			}
			else
			{
				DownCheck = "check";
			}
    		Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " started");
			string MsgInfo_ = "";
			bool Reconnect = MRP_.Reconnect;
			byte[] RawData = MailReceive(MRP_.Account, MRP_.ImapClient_Inbox_, MRP_.Pop3Client_, MRP_.Idx, out MsgInfo_, ref Reconnect, MRP_.cancel);
			MRP_.Reconnect = Reconnect;

			if (RawData == null)
			{
	    		Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: not found");
				if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
				{
					MRP_.ToDelete = true;
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
	    		Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: data segment too short");
				if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
				{
					MRP_.ToDelete = true;
				}
    			return;
    		}

    		if (DigestClear(MsgInfo__[6]) != Digest(RawData, SegmentSize))
    		{
	    		Console_WriteLine_Thr(ConsoleInfo + " - " + DownCheck + " error: bad digest");
				if ((MRP_.FileDeleteMode_ & FileDeleteMode.Bad) == FileDeleteMode.Bad)
				{
					MRP_.ToDelete = true;
				}
    			return;
    		}
    		
    		Monitor.Enter(MF);
    		if (MRP_.FileDownloadMode_ == FileDownloadMode.CompareBody)
    		{
	    		if (MF.MapGet(SegmentNum) == 0)
	    		{
	    			byte[] RawDataX = MF.DataGet(SegmentNum);
	    			bool Good_ = (RawDataX.Length == SegmentSize);
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
	    			
	    			if (Good_)
	    			{
		    			MF.MapSet(SegmentNum, 1);
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
					if ((MRP_.FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
					{
						MRP_.ToDelete = true;
					}
	    		}
    		}
    		if (MRP_.FileDownloadMode_ == FileDownloadMode.CheckExistBody)
    		{
	    		if (MF.MapGet(SegmentNum) == 0)
	    		{
		    		MF.MapSet(SegmentNum, 1);
		    		Console_WriteLine_Thr(ConsoleInfo + " - check finished - good");
	    		}
	    		else
	    		{
		    		Console_WriteLine_Thr(ConsoleInfo + " - duplicate in other thread");
					if ((MRP_.FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
					{
						MRP_.ToDelete = true;
					}
	    		}
    		}
    		if (MRP_.FileDownloadMode_ == FileDownloadMode.Download)
    		{
	    		if (MF.MapGet(SegmentNum) == 0)
	    		{
		    		MF.DataSet(SegmentNum, RawData, SegmentSize);
		    		MF.MapSet(SegmentNum, 1);
		    		Console_WriteLine_Thr(ConsoleInfo + " - download finished");
	    		}
	    		else
	    		{
		    		Console_WriteLine_Thr(ConsoleInfo + " - duplicate in other thread");
					if ((MRP_.FileDeleteMode_ & FileDeleteMode.Duplicate) == FileDeleteMode.Duplicate)
					{
						MRP_.ToDelete = true;
					}
	    		}
    		}
    		Monitor.Exit(MF);

			MRP_.Good = true;
		}

		
		
		static Stopwatch_ TSW;
		
		public static void FileDownload(string FileName, string FilePath, string FileMap, int[] Account, int[] IdxMin, int[] IdxMax, FileDownloadMode FileDownloadMode_, FileDeleteMode FileDeleteMode_)
		{
			if (Account.Length == 0)
			{
				Console_WriteLine("No accounts");
				return;
			}
			
			int[] IdxMin_ = new int[IdxMin.Length];
			int[] IdxMax_ = new int[IdxMax.Length];
			MailFile MF = new MailFile();
			if (FileDownloadMode_ == FileDownloadMode.CheckExistHeader)
			{
				FilePath = null;
			}
			if (FileDownloadMode_ == FileDownloadMode.CheckExistBody)
			{
				FilePath = null;
			}
			if (MF.Open((FileDownloadMode_ != FileDownloadMode.Download), FilePath, FileMap))
			{
                Log("");
                LogReset();
                Log("Time stamp", "Downloaded segments since previous entry", "Totally downloaded segments", "All segments", "Downloaded bytes since previous entry", "Totally downloaded bytes", "All bytes by segment count", "All bytes by file size");
				TSW = new Stopwatch_();
                KBPSReset();
				MF.MapChange(1, 2);
                for (int i = 0; i < Account.Length; i++)
				{
		            MF.MapCalcStats();
		            if ((MF.MapCount(0) > 0) || (MF.GetSegmentCount() == 0))
		            {
		            	MailAccountList[Account[i]].DownloadMin = IdxMin[i];
		            	MailAccountList[Account[i]].DownloadMax = IdxMax[i];
		            	FileDownloadAccount(Account[i], FileDownloadMode_, FileDeleteMode_, FileName, ref MF);
		            	IdxMin_[i] = MailAccountList[Account[i]].DownloadMin_ + 1;
		            	IdxMax_[i] = MailAccountList[Account[i]].DownloadMax_ + 1;
		            }
				}
                Log("");

                MF.MapCalcStats();
	            Console_WriteLine("");
                ConsoleLineToLog = true;
                Console_WriteLine("Total segments: " + MF.GetSegmentCount().ToString());
	            if (FileDownloadMode_ == FileDownloadMode.Download)
	            {
	            	Console_WriteLine("Segments downloaded previously: " + MF.MapCount(2).ToString());
	            	Console_WriteLine("Segments downloaded now: " + MF.MapCount(1).ToString());
	            	Console_WriteLine("Segments not downloaded: " + MF.MapCount(0).ToString());
	            }
	            else
	            {
	            	Console_WriteLine("Segments checked previously as good: " + MF.MapCount(2).ToString());
	            	Console_WriteLine("Good segments: " + MF.MapCount(1).ToString());
	            	Console_WriteLine("Bad or missing segments: " + MF.MapCount(0).ToString());
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
                    Log(TempInfo);
				}
                Console_WriteLine("Total time: " + TimeHMSM(TSW.Elapsed()));
                ConsoleLineToLog = false;
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
		/// Download message prefix for file message
		/// </summary>
		/// <param name="AccNo"></param>
		/// <param name="MsgIdx"></param>
		/// <param name="MsgCount"></param>
		/// <param name="MsgInfo"></param>
		/// <returns></returns>
	    public static string CreateConsoleInfoD(int AccNo, int MsgIdx, int MsgCount, string[] MsgInfo)
	    {
	    	return "Account " + AccNo.ToString() + ", message " + (MsgIdx + 1).ToString() + "/" + MsgCount.ToString() + ": segment " + (HexToInt(MsgInfo[2]) + 1) + "/" + (HexToInt(MsgInfo[3]) + 1);
	    }

	    public static string CreateConsoleInfoDS(int AccNo, int MsgIdx, int MsgCount, string[] MsgInfo)
	    {
	    	return "segment " + (HexToInt(MsgInfo[2]) + 1) + "/" + (HexToInt(MsgInfo[3]) + 1);
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
	    	return "Account " + AccNo.ToString() + ", message " + (MsgIdx + 1).ToString() + "/" + MsgCount.ToString() + ": ";
	    }
	}
}
