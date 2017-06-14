/*
 * Created by SharpDevelop.
 * User: usuario
 * Date: 10/06/2017
 * Time: 11:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Server_DoorOpen
{
	public sealed class NotificationIcon
	{
		public NotifyIcon notifyIcon;
		private ContextMenu notificationMenu;
		private TCPServer server=null;
		
		#region Initialize icon and menu
		public NotificationIcon()
		{
			notifyIcon = new NotifyIcon();
			notificationMenu = new ContextMenu(InitializeMenu());
			notifyIcon.DoubleClick += IconDoubleClick;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationIcon));
			notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
			notifyIcon.ContextMenu = notificationMenu;
			notifyIcon.Icon = SystemIcons.Exclamation;
			
			Bitmap bitmap = new Bitmap("icon.ico");
			notifyIcon.Icon=Icon.FromHandle(bitmap.GetHicon());
			notifyIcon.Visible = true;

			iniciarConexion("COM4");
			

		}
		
		private MenuItem[] InitializeMenu()
		{
			MenuItem[] menu = new MenuItem[] {
				new MenuItem("Reconectar", menuReconnectClick),
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
		private void menuReconnectClick(object sender, EventArgs e)
		{
			iniciarConexion("COM4");
		}
		
		private void menuExitClick(object sender, EventArgs e)
		{
			server.StopServer();
			Application.Exit();
		}
		
		private void IconDoubleClick(object sender, EventArgs e)
		{
			if(TCPServer.serialPort.IsOpen)
			{
				TCPServer.abrirPuerta();
				notifyIcon.BalloonTipIcon=ToolTipIcon.Info;
				notifyIcon.BalloonTipText="Abriendo";
				notifyIcon.BalloonTipTitle="Ok";
				notifyIcon.ShowBalloonTip(1000);
			}
			else
			{
				notifyIcon.BalloonTipIcon=ToolTipIcon.Error;
				notifyIcon.BalloonTipText="Error de conexion";
				notifyIcon.BalloonTipTitle="Atención";
				notifyIcon.ShowBalloonTip(3000);
			}
			
			
			
		}
		#endregion
		
		private void iniciarConexion(string puertoCOM)
		{
			
			try {
				TCPServer.serialPort.PortName=puertoCOM;
				TCPServer.serialPort.BaudRate=9600;
				if(!TCPServer.serialPort.IsOpen)
				{
					TCPServer.serialPort.Open();
				}
				
				if(TCPServer.serialPort.IsOpen){
					notifyIcon.BalloonTipIcon=ToolTipIcon.Info;
					notifyIcon.BalloonTipText="Conectado";
					notifyIcon.BalloonTipTitle="Ok";
					notifyIcon.ShowBalloonTip(3000);
					
					if(!TCPServer.isServerRunning()){
						server = new TCPServer();
						server.StartServer();
					}
					
				}
				
				
				else
				{
					notifyIcon.BalloonTipIcon=ToolTipIcon.Error;
					notifyIcon.BalloonTipText="Error de conexion";
					notifyIcon.BalloonTipTitle="Atención";
					notifyIcon.ShowBalloonTip(3000);
				}
			}
			catch(Exception)
			{
				notifyIcon.BalloonTipIcon=ToolTipIcon.Error;
				notifyIcon.BalloonTipText="Error de conexion";
				notifyIcon.BalloonTipTitle="Atención";
				notifyIcon.ShowBalloonTip(3000);
			}
		}
	}
}
