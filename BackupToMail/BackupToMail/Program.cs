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
				"Download file",
				"Check existence without body control",
				"Check existence with body control",
				"Compare with header digest",
				"Compare with body contents"
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
					case "CONFIGTEST": ProgMode = 13; break;
					case "TESTCONFIG": ProgMode = 13; break;
                    case "FILE": ProgMode = 4; break;
                    case "BATCHFILE": ProgMode = 14; break;
                    case "FILEBATCH": ProgMode = 14; break;
                }
            }
			
			
			// Upload mode
			if (((ProgMode == 1) || (ProgMode == 11)) && (args.Length >= 6))
			{
				ItemName = args[1];
				ItemData = args[2];
				ItemMap = args[3];
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
				WelcomeMsg.Add("File name: " + ItemName);
				WelcomeMsg.Add("Data file: " + ItemData);
				WelcomeMsg.Add("Map file: " + ItemMap);
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
				WelcomeMsg.Add("File name: " + ItemName);
				WelcomeMsg.Add("Data file: " + ItemData);
				WelcomeMsg.Add("Map file: " + ItemMap);
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
                Console.WriteLine("Dummy file: " + DummyName);
                Console.WriteLine("Real file: " + FileName);
                Console.WriteLine("Segment size: " + DummySegmentSize);
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
                        long DummyFileSize = RandomSequence.DummyFileSize;
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

                                byte[] Raw = RandomSequence_.GenSeq(SegmentI, DummySegmentSize);
                                FS_.Write(Raw, 0, (int)DummySegmentSize);

                                SegmentI += DummySegmentSize;
                                DispI++;
                                Console.WriteLine("OK - " + MailSegment.TimeHMSM(SW.Elapsed()));
                            }
                            FS_.Close();
                            Console.WriteLine("File created in time: " + MailSegment.TimeHMSM(SW.Elapsed()));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("File creation error: " + e.Message);
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
				Console.WriteLine("Use BATCHUPLOAD or UPLOADBATCH to ommit upload confirmation.");
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
				Console.WriteLine("Delete options:");
				Console.WriteLine(" 0 - " + DeleteTypeDesc[0] + " (default, ignored if other provided)");
				Console.WriteLine(" 1 - " + DeleteTypeDesc[1]);
				Console.WriteLine(" 2 - " + DeleteTypeDesc[2]);
				Console.WriteLine(" 3 - " + DeleteTypeDesc[3]);
				Console.WriteLine(" 4 - " + DeleteTypeDesc[4]);
				Console.WriteLine(" 5 - " + DeleteTypeDesc[5]);
				Console.WriteLine("Use BATCHDOWNLOAD or DOWNLOADBATCH to ommit download confirmation.");
				Console.WriteLine();
				Console.WriteLine("Print general configuration:");
				Console.WriteLine("BackupToMail CONFIG <account list by commas>");
				Console.WriteLine();
				Console.WriteLine("Print general and account configuration without connection test:");
				Console.WriteLine("BackupToMail CONFIG <account list by commas>");
				Console.WriteLine();
				Console.WriteLine("Print general and account configuration with connection test:");
				Console.WriteLine("BackupToMail CONFIGTEST <account list by commas>");
				Console.WriteLine("BackupToMail TESTCONFIG <account list by commas>");
                Console.WriteLine();
                Console.WriteLine("Create file based on dummy file generator:");
                Console.WriteLine("BackupToMail FILE <dummy file definition> <file name> <segment size>");
                Console.WriteLine();
            }
            Console.WriteLine();
		}
	}
}