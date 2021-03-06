﻿/*
 * Created by SharpDevelop.
 * User: SoporteSEM
 * Date: 12/06/2017
 * Time: 16:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;

namespace Client_DoorOpen
{
	public sealed class NotificationIcon
	{
		public NotifyIcon notifyIcon;
		private ContextMenu notificationMenu;
		private WebRequest request;
		private Stream dataStream;

		
		#region Initialize icon and menu
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			notifyIcon.DoubleClick += IconDoubleClick;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
			notifyIcon.ContextMenu = notificationMenu;
			
			
			Bitmap bitmap = new Bitmap("icon.ico");
			notifyIcon.Icon=Icon.FromHandle(bitmap.GetHicon());

			notifyIcon.Visible = true;


		}
		
		private MenuItem[] InitializeMenu()
		{
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("Salir", menuExitClick)
			};
			return menu;
		}
		#endregion
		
		#region Main - Program entry point
		/// <summary>Program entry point.</summary>
		/// <param name="args">Command Line Arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			bool isFirstInstance;
			// Please use a unique name for the mutex to prevent conflicts with other programs
			using (Mutex mtx = new Mutex(true, "Control", out isFirstInstance)) {
				if (isFirstInstance) {
					NotificationIcon notificationIcon = new NotificationIcon();
					notificationIcon.notifyIcon.Visible = true;
					Application.Run();
					notificationIcon.notifyIcon.Dispose();
				} else {
					// The application is already running
					// TODO: Display message box or change focus to existing application instance
				}
			} // releases the Mutex
		}
		#endregion
		
		#region Event Handlers

		
		private void menuExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e)
		{
			notifyIcon.BalloonTipIcon=ToolTipIcon.Info;
			notifyIcon.BalloonTipTitle="Puerta";
			notifyIcon.BalloonTipText="Abriendo...";
			notifyIcon.ShowBalloonTip(3000);
			
			sendHTTP();

			
			
			
		}
		
		
		#endregion
		void sendHTTP(){
			try
			{
				var post = new NameValueCollection();
				post.Add("Enviar", "Enviar");

				using (var wc = new WebClient())
				{
					wc.UploadValues("http://192.168.4.18:31001", post);
				}
				
				post = null;
			}
			catch (Exception)
			{


			}
			
			/*
			try {
				Uri myUri = new Uri("http://192.168.4.18:31001");
				
				request = WebRequest.Create(myUri);
				request.Method="POST";
				// Create POST data and convert it to a byte array.
				string postData = "Enviar";
				
				byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);

				// Set the ContentType property of the WebRequest.
				request.ContentType = "application/x-www-form-urlencoded";

				// Set the ContentLength property of the WebRequest.
				request.ContentLength = byteArray.Length;

				// Get the request stream.
				dataStream = request.GetRequestStream();

				// Write the data to the request stream.
				dataStream.Write(byteArray, 0, byteArray.Length);

				// Close the Stream object.
				dataStream.Close();
			}
			catch (Exception)
			{
				notifyIcon.BalloonTipIcon=ToolTipIcon.Error;
				notifyIcon.BalloonTipTitle="Error";
				notifyIcon.BalloonTipText="Error al abrir";
				notifyIcon.ShowBalloonTip(1000);

			}
			 */
			
		}

	}
}