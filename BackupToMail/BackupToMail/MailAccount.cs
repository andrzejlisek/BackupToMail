/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-05-31
 * Time: 08:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;

namespace BackupToMail
{
	/// <summary>
	/// Mail account
	/// </summary>
	public class MailAccount
	{
        public bool DeleteIdx = false;
        public int DownloadMin_ = 0;
		public int DownloadMax_ = 0;
		public int DownloadMin = 0;
		public int DownloadMax = 0;
		public int MutexNo = 0;
		public string Address = "";
		public string Login = "";
		public string Password = "";
		public string SmtpHost = "";
		public int SmtpPort = 0;
		public bool SmtpSsl = false;
		public string Pop3Host = "";
		public int Pop3Port = 0;
		public bool Pop3Ssl = false;
		public string ImapHost = "";
		public int ImapPort = 0;
		public bool ImapSsl = false;
		public bool Pop3Use = false;
		public bool SmtpConnect = false;
		
		/// <summary>
		/// Load account configuration from configuration file
		/// </summary>
		/// <param name="Cfg"></param>
		/// <param name="Idx"></param>
		/// <returns></returns>
		public bool ConfigLoad(ConfigFile Cfg, int Idx)
		{
			Address = Cfg.ParamGetS("Mail" + Idx + "Address");
			Login = Cfg.ParamGetS("Mail" + Idx + "Login");
			Password = Cfg.ParamGetS("Mail" + Idx + "Password");
			SmtpHost = Cfg.ParamGetS("Mail" + Idx + "SmtpHost");
			SmtpPort = Cfg.ParamGetI("Mail" + Idx + "SmtpPort");
			SmtpSsl = Cfg.ParamGetB("Mail" + Idx + "SmtpSsl");
			Pop3Host = Cfg.ParamGetS("Mail" + Idx + "Pop3Host");
			Pop3Port = Cfg.ParamGetI("Mail" + Idx + "Pop3Port");
			Pop3Ssl = Cfg.ParamGetB("Mail" + Idx + "Pop3Ssl");
			ImapHost = Cfg.ParamGetS("Mail" + Idx + "ImapHost");
			ImapPort = Cfg.ParamGetI("Mail" + Idx + "ImapPort");
			ImapSsl = Cfg.ParamGetB("Mail" + Idx + "ImapSsl");
			Pop3Use = Cfg.ParamGetB("Mail" + Idx + "Pop3Use");
			SmtpConnect = Cfg.ParamGetB("Mail" + Idx + "SmtpConnect");
            DeleteIdx = Cfg.ParamGetB("Mail" + Idx + "DeleteIdx");

            if (Address != "")
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>
		/// Print account information on the console
		/// </summary>
		/// <param name="TestConn"></param>
		public void PrintInfo(bool TestConn)
		{
			Console.WriteLine("E-mail: " + Address);
			Console.WriteLine("Logn: " + Login);
			Console.WriteLine("Password: " + Password);
			Console.Write("SMTP: " + SmtpHost + ":" + SmtpPort + (SmtpSsl ? " with SSL" : " without SSL"));
			if (TestConn)
			{
				if ((SmtpHost != "") && (SmtpPort > 0))
				{
					Console.Write(" - ");
					Console.WriteLine(SmtpClient_Test());
				}
				else
				{
					Console.WriteLine();
				}
			}
			else
			{
				Console.WriteLine();
			}
			Console.Write("IMAP: " + ImapHost + ":" + ImapPort + (ImapSsl ? " with SSL" : " without SSL"));
			if (TestConn)
			{
				if ((ImapHost != "") && (ImapPort > 0))
				{
					Console.Write(" - ");
					Console.WriteLine(ImapClient_Test());
				}
				else
				{
					Console.WriteLine();
				}
			}
			else
			{
				Console.WriteLine();
			}
			Console.Write("POP3: " + Pop3Host + ":" + Pop3Port + (Pop3Ssl ? " with SSL" : " without SSL"));
			if (TestConn)
			{
				if ((Pop3Host != "") && (Pop3Port > 0))
				{
					Console.Write(" - ");
					Console.WriteLine(Pop3Client_Test());
				}
				else
				{
					Console.WriteLine();
				}
			}
			else
			{
				Console.WriteLine();
			}
			Console.WriteLine("SMTP connect: " + SmtpConnect);
			if (Pop3Use)
			{
				Console.WriteLine("Download through: POP3");
			}
			else
			{
				Console.WriteLine("Download through: IMAP");
			}
            Console.WriteLine("Decrease index and count after deletion: " + DeleteIdx);
        }
		
		/// <summary>
		/// Checks if SMTP server needs to be connected
		/// </summary>
		/// <param name="SmtpClient__"></param>
		/// <returns></returns>
		public bool SmtpClient_Need_Connect(SmtpClient SmtpClient__)
		{
			if (SmtpClient__ == null)
			{
				return true;
			}
			if (SmtpClient__.IsConnected)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		
		/// <summary>
		/// Connect to SMTP server only if is null or not connected already
		/// </summary>
		/// <param name="SmtpClient__"></param>
		/// <param name="cancel"></param>
		/// <returns></returns>
		public SmtpClient SmtpClient_Connect(SmtpClient SmtpClient__, CancellationTokenSource cancel)
		{
			if (SmtpClient__ == null)
			{
				return SmtpClient_(cancel);
			}
			if (SmtpClient__.IsConnected)
			{
				return SmtpClient__;
			}
			else
			{
				SmtpClient__.Dispose();
				return SmtpClient_(cancel);
			}
		}
		
		/// <summary>
		/// Test SMTP connection possibility
		/// </summary>
		/// <returns>Test result message</returns>
		public string SmtpClient_Test()
		{
            using (CancellationTokenSource cancel = new CancellationTokenSource ())
			{
				try
				{
					using(SmtpClient SmtpClient__ = SmtpClient_(cancel))
					{
						SmtpClient__.Disconnect(true, cancel.Token);
					}
					return "OK";
				}
				catch (Exception e)
				{
					return "Error: " + MailSegment.ExcMsg(e);
				}
			}
		}

		/// <summary>
		/// Test IMAP connection possibility
		/// </summary>
		/// <returns>Test result message</returns>
		public string ImapClient_Test()
		{
            using (CancellationTokenSource cancel = new CancellationTokenSource ())
			{
				try
				{
					int Msg = -1;
					using(ImapClient ImapClient__ = ImapClient_(cancel, false))
					{
						Msg = ImapClient__.Inbox.Count;
						ImapClient__.Inbox.Close();
						ImapClient__.Disconnect(true, cancel.Token);
					}
					return "OK (" + Msg + " messages)";
				}
				catch (Exception e)
				{
					return "Error: " + MailSegment.ExcMsg(e);
				}
			}
		}

		/// <summary>
		/// Test POP3 connection possibility
		/// </summary>
		/// <returns>Test result message</returns>
		public string Pop3Client_Test()
		{
            using (CancellationTokenSource cancel = new CancellationTokenSource ())
			{
				try
				{
					int Msg = -1;
					using(Pop3Client Pop3Client__ = Pop3Client_(cancel))
					{
						Msg = Pop3Client__.Count;
						Pop3Client__.Disconnect(true, cancel.Token);
					}
					return "OK (" + Msg + " messages)";
				}
				catch (Exception e)
				{
					return "Error: " + MailSegment.ExcMsg(e);
				}
			}
		}
		
		/// <summary>
		/// Create new SMTP connection
		/// </summary>
		/// <param name="cancel"></param>
		/// <returns></returns>
		public SmtpClient SmtpClient_(CancellationTokenSource cancel)
		{
			MailSegment.AllowCert();
			SmtpClient SmtpClient__ = new SmtpClient();
			SmtpClient__.Connect(SmtpHost, SmtpPort, SmtpSsl, cancel.Token);

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            SmtpClient__.AuthenticationMechanisms.Remove("XOAUTH2");

            SmtpClient__.Authenticate(Login, Password);
        	return SmtpClient__;
		}
		
		/// <summary>
		/// Create new IMAP connection
		/// </summary>
		/// <param name="cancel"></param>
		/// <param name="InboxReadWrite"></param>
		/// <returns></returns>
		public ImapClient ImapClient_(CancellationTokenSource cancel, bool InboxReadWrite)
		{
			MailSegment.AllowCert();
			ImapClient ImapClient__ = new ImapClient();
			ImapClient__.Connect(ImapHost, ImapPort, ImapSsl, cancel.Token);

            // If you want to disable an authentication mechanism,
            // you can do so by removing the mechanism like this:
            ImapClient__.AuthenticationMechanisms.Remove("XOAUTH");

            ImapClient__.Authenticate(Login, Password, cancel.Token);
            ImapClient__.Inbox.Open(InboxReadWrite ? FolderAccess.ReadWrite : FolderAccess.ReadOnly, cancel.Token);
            return ImapClient__;
		}

		/// <summary>
		/// Create new POP3 connection
		/// </summary>
		/// <param name="cancel"></param>
		/// <returns></returns>
		public Pop3Client Pop3Client_(CancellationTokenSource cancel)
		{
			MailSegment.AllowCert();
			Pop3Client Pop3Client__ = new Pop3Client();
			Pop3Client__.Connect(Pop3Host, Pop3Port, Pop3Ssl, cancel.Token);
			Pop3Client__.Authenticate(Login, Password, cancel.Token);
			return Pop3Client__;
		}
	}
}
