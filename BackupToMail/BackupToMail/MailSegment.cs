/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-05-31
 * Time: 10:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;

namespace BackupToMail
{
	/// <summary>
	/// Fields and methods used both in uploading and downloading
	/// </summary>
	public partial class MailSegment
	{
		/// <summary>
		/// Account list loaded from configuration file
		/// </summary>
		public static List<MailAccount> MailAccountList;
		
		/// <summary>
		/// Number of simultaneous threads used in uploading
		/// </summary>
		public static int ThreadsUpload = 1;
		
		/// <summary>
		/// Number of simultaneous threads used in downloading
		/// </summary>
		public static int ThreadsDownload = 1;
		
		/// <summary>
		/// Subject separator - char used in parsing
		/// </summary>
		public const char InfoSeparatorC = 'X';
		
		/// <summary>
		/// Subject separator - string used in creating
		/// </summary>
		public const string InfoSeparatorS = "X";
		
		/// <summary>
		/// Default segment type
		/// </summary>
		public static int DefaultSegmentType = 0;
		
		/// <summary>
		/// Default segment size in bytes
		/// </summary>
		public static int DefaultSegmentSize = 16777216;
		
		/// <summary>
		/// Default image width in pixels
		/// </summary>
		public static int DefaultImageSize = 4096;
		
		/// <summary>
		/// Set configuration from configuration file
		/// </summary>
		/// <param name="CF"></param>
		public static void ConfigSet(ConfigFile CF)
		{
			ThreadsUpload = CF.ParamGetI("ThreadsUpload");
			ThreadsDownload = CF.ParamGetI("ThreadsDownload");
			if (ThreadsUpload < 1)
			{
				ThreadsUpload = 1;
			}
			if (ThreadsDownload < 1)
			{
				ThreadsDownload = 1;
			}

			DefaultSegmentType = CF.ParamGetI("DefaultSegmentType");
			if ((DefaultSegmentType < 0) || (DefaultSegmentType > 3))
			{
				DefaultSegmentType = 0;
			}
			
			DefaultSegmentSize = CF.ParamGetI("DefaultSegmentSize");
			if (DefaultSegmentSize < 1)
			{
				DefaultSegmentSize = 16777216;
			}
			
			DefaultImageSize = CF.ParamGetI("DefaultImageSize");
			if (DefaultImageSize < 1)
			{
				DefaultImageSize = 4096;
			}
		}
		
		/// <summary>
		/// Print general configuration
		/// </summary>
		public static void ConfigInfo()
		{
			string[] SegmentTypeDesc = new string[]
			{
				"Binary attachment",
				"PNG image attachment",
				"Base64 in plain text body",
				"PNG image in HTML body"
			};				
			
			Console.WriteLine("Accounts: " + MailAccountList.Count + " (from 0 to " + (MailAccountList.Count - 1) +")");
			Console.WriteLine("Upload threads: " + ThreadsUpload);
			Console.WriteLine("Download threads: " + ThreadsDownload);
			Console.WriteLine("Default segment type: " + SegmentTypeDesc[DefaultSegmentType]);
			Console.WriteLine("Default segment size: " + DefaultSegmentSize);
			Console.WriteLine("Default image size: " + DefaultImageSize + "x" + ImgHFromW(DefaultSegmentSize, DefaultImageSize));
		}

		static MailSegment()
		{

		}
		
		/// <summary>
		/// Force accepting all untrusted cerificates in connections to servers
		/// </summary>
		public static void AllowCert()
		{
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
		}

		/// <summary>
		/// All transfered bytes to display average transfer speed
		/// </summary>
		static long KBPS_Bytes;
		
		/// <summary>
		/// Whole transfer time to display average transfer speed
		/// </summary>
		static long KBPS_Time;
		
		/// <summary>
		/// Reset total transfer values
		/// </summary>
		public static void KBPSReset()
		{
			KBPS_Bytes = 0;
			KBPS_Time = 0;
		}
		
		/// <summary>
		/// Get transfered bytes
		/// </summary>
		/// <returns></returns>
		public static string KBPS_B()
		{
			return KBPS_Bytes.ToString();
		}

		/// <summary>
		/// Get transfer time in hh:mm:ss.ms form
		/// </summary>
		/// <returns></returns>
		public static string KBPS_T()
		{
			long T = KBPS_Time;
			long H = T / (1000 * 60 * 60);
			T = T - (H * 1000 * 60 * 60);
			long M = T / (1000 * 60);
			T = T - (M * 1000 * 60);
			long S = T / (1000);
			T = T - (S * 1000);
			return H.ToString().PadLeft(2, '0') + ":" + M.ToString().PadLeft(2, '0') + ":" + S.ToString().PadLeft(2, '0') + "." + T.ToString().PadLeft(3, '0');
		}
		
		/// <summary>
		/// Return transfer speed of total transfer
		/// </summary>
		/// <returns></returns>
		public static string KBPS()
		{
			return KBPS(KBPS_Bytes, KBPS_Time);
		}
		
		/// <summary>
		/// Return transfer speed of provided size and time
		/// </summary>
		/// <param name="Bytes"></param>
		/// <param name="Time"></param>
		/// <returns></returns>
		public static string KBPS(long Bytes, long Time)
		{
			KBPS_Bytes += Bytes;
			KBPS_Time += Time;
			if (Time > 0)
			{
				double BPS = ((double)Bytes / (double)Time);
				return (BPS).ToString("F0", CultureInfo.InvariantCulture) + "kB/s";
			}
			else
			{
				if (Bytes > 0)
				{
					return "Infinity";
				}
				else
				{
					return KBPS(0, 1);
				}
			}
		}
		
		
		/// <summary>
		/// Send parameters and values as object used in file upload
		/// </summary>
		public class MailSendParam
		{
			public int Idx;
			public int AccountSrcN;
			public int AccountSrc;
			public bool Good = false;
			public byte[] SegmentBuf;
			public int FileSegmentSize;
			public int FileSegmentNum;
			public SmtpClient[,] SmtpClient_;
			public int SmtpClientSlot;
		}
		
		/// <summary>
		/// Receive parameters and values as object used in file download
		/// </summary>
		public class MailRecvParam
		{
			public int FileSegmentNum;
			public int Account;
			public bool Good = false;
			public string MsgInfo;
			public int Idx;
			public int MsgId;
			public int MsgCount;
			public Pop3Client Pop3Client_;
			public MailKit.IMailFolder ImapClient_Inbox_;
			public CancellationTokenSource cancel;
			public bool Reconnect;
			public FileDownloadMode FileDownloadMode_;
			public FileDeleteMode FileDeleteMode_;
			public bool ToDelete;
		}
		
		
		
		
		/// <summary>
		/// Object used as mutex to synchronize printing to console
		/// </summary>
		static object Console_ = new object();
		
		/// <summary>
		/// Calculate image height based on width and segment length
		/// </summary>
		/// <param name="Raw_Length"></param>
		/// <param name="ImgW"></param>
		/// <returns></returns>
		public static int ImgHFromW(int Raw_Length, int ImgW)
		{
			int Raw_Length3 = (Raw_Length / 3);
			if ((Raw_Length % 3) > 0)
			{
				Raw_Length3++;
			}
			int ImgH = Raw_Length3 / ImgW;
			if ((Raw_Length3 % ImgW) > 0)
			{
				ImgH++;
			}
			return ImgH;
		}
		
		/// <summary>
		/// Convert binary array to image
		/// </summary>
		/// <param name="Raw"></param>
		/// <param name="SegmentImageSize"></param>
		/// <returns></returns>
		public static MemoryStream ConvRaw2Img(byte[] Raw, int SegmentImageSize)
		{
			int Raw_Length = Raw.Length;
			int Raw_Length3 = (Raw_Length / 3);
			if ((Raw_Length % 3) > 0)
			{
				Raw_Length3++;
			}
			int ImgW = SegmentImageSize;
			int ImgH = ImgHFromW(Raw.Length, ImgW);
			Bitmap ImgBitmap = new Bitmap(ImgW, ImgH, PixelFormat.Format24bppRgb);
			
			// This loop is unfinished, but this function exists after successfully 
			// image created
			while (true)
			{
	            GraphicsUnit GUP = GraphicsUnit.Pixel;
	            BitmapData ImgBitmap_ = ImgBitmap.LockBits(Rectangle.Round(ImgBitmap.GetBounds(ref GUP)), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
	            unsafe
	            {
	            	int RawPointer = 0;
	                byte* ImgPointer = (byte*)ImgBitmap_.Scan0;
	                
	                // Create image iterated by rows and columns without the last, not full-width row
	                for (int Y = 0; Y < (ImgH - 1); Y++)
	                {
	                    for (int X = 0; X < ImgW; X++)
	                    {
	                    	ImgPointer[0] = Raw[RawPointer + 0];
	                        ImgPointer[1] = Raw[RawPointer + 1];
	                        ImgPointer[2] = Raw[RawPointer + 2];
	                        RawPointer += 3;
	                        ImgPointer += 3;
	                    }
	                    ImgPointer += ImgBitmap_.Stride - (3 * ImgW);
	                }
	                
	                // Put the last bytes in the last row, which may be not as length as image width
	                while (RawPointer < Raw_Length)
	                {
	                	ImgPointer[0] = Raw[RawPointer];
	                	RawPointer++;
	                    ImgPointer++;
	                }
	            }
	            ImgBitmap.UnlockBits(ImgBitmap_);

	            // Save image into stream
				MemoryStream ImgStr = new MemoryStream();
				ImgBitmap.Save(ImgStr, ImageFormat.Png);
				ImgStr.Seek(0, SeekOrigin.Begin);
	            
				// Test if image is created properly
				byte[] RawTest = ConvImg2Raw(ImgStr);
				bool Good = true;
				for (int I = 0; I < Raw_Length; I++)
				{
					if (Raw[I] != RawTest[I])
					{
						I = Raw_Length;
						Good = false;
					}
				}
				
				// If test passed, the functions returns stream, otherwise, the image creation will be repeated
				if (Good)
				{
					ImgStr.Seek(0, SeekOrigin.Begin);
					return ImgStr;
				}
			}
		}

		/// <summary>
		/// Convert image to binary array
		/// </summary>
		/// <param name="Img"></param>
		/// <returns></returns>
		public static byte[] ConvImg2Raw(Stream Img)
		{
			Bitmap ImgBitmap = new Bitmap(Img);
			int ImgW = ImgBitmap.Width;
			int ImgH = ImgBitmap.Height;
			int Raw_Length = ImgW * ImgH * 3;
			byte[] Raw = new byte[Raw_Length];

            GraphicsUnit GUP = GraphicsUnit.Pixel;
            BitmapData ImgBitmap_ = ImgBitmap.LockBits(Rectangle.Round(ImgBitmap.GetBounds(ref GUP)), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
            	int RawPointer = 0;
                byte* ImgPointer = (byte*)ImgBitmap_.Scan0;
                for (int Y = 0; Y < ImgH; Y++)
                {
                    for (int X = 0; X < ImgW; X++)
                    {
                    	Raw[RawPointer + 0] = ImgPointer[0];
                        Raw[RawPointer + 1] = ImgPointer[1];
                        Raw[RawPointer + 2] = ImgPointer[2];
                        RawPointer += 3;
                        ImgPointer += 3;
                    }
                    ImgPointer += ImgBitmap_.Stride - (3 * ImgW);
                }
                RawPointer.ToString();
            }
            ImgBitmap.UnlockBits(ImgBitmap_);

			return Raw;
		}

		/// <summary>
		/// Convert binary array to Base64 text
		/// </summary>
		/// <param name="Raw"></param>
		/// <returns></returns>
		public static string ConvRaw2Txt(byte[] Raw)
		{
			return Convert.ToBase64String(Raw);
		}
		
		/// <summary>
		/// Convert Base64 text to binary array
		/// </summary>
		/// <param name="Txt"></param>
		/// <returns></returns>
		public static byte[] ConvTxt2Raw(string Txt)
		{
			return Convert.FromBase64String(Txt);
		}
		
		/// <summary>
		/// Clear digest value by removing other characters than usen in digest
		/// </summary>
		/// <param name="S"></param>
		/// <returns></returns>
	    public static string DigestClear(string S)
	    {
	    	S = S.ToUpperInvariant();
	    	string S0 = "";
	    	for (int i = 0; i < S.Length; i++)
	    	{
	    		if ((S[i] >= '0') && (S[i] <= '9'))
	    		{
	    			S0 = S0 + S[i].ToString();
	    		}
	    		if ((S[i] >= 'A') && (S[i] <= 'F'))
	    		{
	    			S0 = S0 + S[i].ToString();
	    		}
	    	}
	    	return S0;
	    }
		
	    /// <summary>
	    /// Calculate the MD5 digest of the first bytes of binary array
	    /// </summary>
	    /// <param name="Src"></param>
	    /// <param name="S"></param>
	    /// <returns></returns>
	    public static string Digest(byte[] Src, int S)
	    {
	        byte[] ChecksumB;
	        MD5 ChecksumWorker = new MD5CryptoServiceProvider();
	        ChecksumB = ChecksumWorker.ComputeHash(Src, 0, S);
	        string ChecksumS = "";
	        for (int i = 0; i < ChecksumB.Length; i++)
	        {
        		ChecksumS = ChecksumS + Hex(ChecksumB[i]);
	        }
	        return ChecksumS;
	    }
		
	    /// <summary>
	    /// Calculate the MD5 digest of whole binary array
	    /// </summary>
	    /// <param name="Src"></param>
	    /// <returns></returns>
	    public static string Digest(byte[] Src)
	    {
	        byte[] ChecksumB;
	        MD5 ChecksumWorker = new MD5CryptoServiceProvider();
	        ChecksumB = ChecksumWorker.ComputeHash(Src);
	        string ChecksumS = "";
	        for (int i = 0; i < ChecksumB.Length; i++)
	        {
        		ChecksumS = ChecksumS + Hex(ChecksumB[i]);
	        }
	        return ChecksumS;
	    }
	    
	    /// <summary>
	    /// Calculate the MD5 digest of text
	    /// </summary>
	    /// <param name="Src"></param>
	    /// <returns></returns>
	    public static string Digest(string Src)
	    {
	        byte[] ChecksumB;
	        MD5 ChecksumWorker = new MD5CryptoServiceProvider();
	        ChecksumB = ChecksumWorker.ComputeHash(Encoding.UTF8.GetBytes(Src));
	        string ChecksumS = "";
	        for (int i = 0; i < ChecksumB.Length; i++)
	        {
        		ChecksumS = ChecksumS + Hex(ChecksumB[i]);
	        }
	        return ChecksumS;
	    }
	    
	    /// <summary>
	    /// Convert hex representation string into integer value
	    /// </summary>
	    /// <param name="Str"></param>
	    /// <returns></returns>
	    public static int HexToInt(string Str)
	    {
			return int.Parse(Str, NumberStyles.HexNumber);
	    }

	    /// <summary>
	    /// Convert integer value to hex representation
	    /// </summary>
	    /// <param name="Val"></param>
	    /// <returns></returns>
	    public static string IntToHex(int Val)
	    {
	    	return IntToHex(Val, 0);
	    }
	    
	    /// <summary>
	    /// Convert integer value to hex representation using as many digits as maximum value needs 
	    /// </summary>
	    /// <param name="Val"></param>
	    /// <param name="ValMax"></param>
	    /// <returns></returns>
	    public static string IntToHex(int Val, int ValMax)
	    {
	    	int ValMaxL = ValMax.ToString("X").Length;
	    	return Val.ToString("X").PadLeft(ValMaxL, '0');
	    }
	    
	    /// <summary>
	    /// Convert byte value to hex 2-digit representation
	    /// </summary>
	    /// <param name="Val"></param>
	    /// <returns></returns>
	    public static string Hex(byte Val)
	    {
	    	if (Val >= 16)
	    	{
	    		return Val.ToString("X");
	    	}
	    	else
	    	{
	    		return "0" + Val.ToString("X");
	    	}
	    }

		/// <summary>
		/// Force to run the garbage collector
		/// </summary>
	    public static void CleanUp()
	    {
	        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
	    }
	    
	    /// <summary>
	    /// Get exception message including the internal exceptions
	    /// </summary>
	    /// <param name="e"></param>
	    /// <returns></returns>
	    public static string ExcMsg(Exception e)
	    {
	    	string Msg = e.Message;
	    	while (e.InnerException != null)
	    	{
	    		e = e.InnerException;
	    		Msg = Msg + "/" + e.Message;
	    	}
	    	return Msg;
	    }
	    
	    /// <summary>
	    /// Convert stream to array by read all bytes from stream
	    /// </summary>
	    /// <param name="S"></param>
	    /// <returns></returns>
	    public static byte[] StreamToArray(Stream S)
	    {
	    	byte[] Raw = new byte[S.Length];
	    	S.Read(Raw, 0, (int)S.Length);
	    	return Raw;
	    }

	    /// <summary>
	    /// Check, if the text consists of hexadecimal digits (0-9,A-F) only
	    /// </summary>
	    /// <param name="S"></param>
	    /// <returns></returns>
	    public static bool ConsistsOfHex(string S)
	    {
	    	for (int i = 0; i < S.Length; i++)
	    	{
	    		if ((S[i] < '0') || (S[i] > '9'))
	    		{
		    		if ((S[i] < 'A') || (S[i] > 'F'))
		    		{
			    		if ((S[i] < 'a') || (S[i] > 'f'))
			    		{
			    			return false;
			    		}
		    		}
	    		}
	    	}
	    	return true;
	    }
	    
	    
	    /// <summary>
	    /// Write line to console from thread other than main thread
	    /// </summary>
	    /// <param name="Str"></param>
	    public static void Console_WriteLine_Thr(string Str)
	    {
			Monitor.Enter(Console_);
	    	Console.WriteLine(Str);
			Monitor.Exit(Console_);
	    }
	}
}
