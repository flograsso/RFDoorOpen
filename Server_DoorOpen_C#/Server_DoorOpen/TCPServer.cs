using System;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net.Http;

namespace Server_DoorOpen
{
	/// <summary>
	/// TCPServer is the Server class. When "StartServer" method is called
	/// this Server object tries to connect to a IP Address specified on a port
	/// configured. Then the server start listening for client socket requests.
	/// As soon as a requestcomes in from any client then a Client Socket
	/// Listening thread will be started. That thread is responsible for client
	/// communication.
	/// </summary>
	public class TCPServer
	{
		/// <summary>
		/// Default Constants.
		/// </summary>
		
		/*LocalHost*/
		//public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");
		
		/*Notebook Fede*/
		//public static IPAddress DEFAULT_SERVER = IPAddress.Parse("163.10.123.161");
		
		/*Notebook Monitoreo*/
		/*public static IPAddress DEFAULT_SERVER = IPAddress.Parse("163.10.123.181");*/
		
		public static int DEFAULT_PORT=31001;
		public static BlockingQueue<string> queue;
		public static SerialPort serialPort = new SerialPort();
		public static byte[] HTTPresponse;
		
		private static readonly HttpClient client = new HttpClient();
		
		
		
		/// <summary>
		/// Local Variables Declaration.
		/// </summary>
		private TcpListener m_server = null;
		private static bool m_stopServer=false;
		private bool m_stopPurging=false;
		private bool m_stopProccesing = false;
		private Thread m_serverThread = null;
		private Thread m_purgingThread = null;
		private Thread m_processingThread = null;
		private ArrayList m_socketListenersList = null;
		/// <summary>
		/// Constructors.
		/// </summary>
		public TCPServer()
		{
			try
			{
				IPAddress serverIP = IPAddress.Parse(GetLocalIPAddress());
				Init(new IPEndPoint(serverIP, DEFAULT_PORT));
			}
			catch(Exception){
				
				MessageBox.Show("Error al adquirir IP");
				
			}
			
		}
		
		/*
		public TCPServer(IPAddress serverIP)
		{

			Init(new IPEndPoint(serverIP, DEFAULT_PORT));
		}

		public TCPServer(int port)
		{
			Init(new IPEndPoint(DEFAULT_SERVER, port));
		}

		public TCPServer(IPAddress serverIP, int port)
		{
			Init(new IPEndPoint(serverIP, port));
		}

		public TCPServer(IPEndPoint ipNport)
		{
			Init(ipNport);
		}
		 */
		/// <summary>
		/// Destructor.
		/// </summary>
		~TCPServer()
		{
			StopServer();
		}

		/// <summary>
		/// Init method that create a server (TCP Listener) Object based on the
		/// IP Address and Port information that is passed in.
		/// </summary>
		/// <param name="ipNport"></param>
		private void Init(IPEndPoint ipNport)
		{
			try
			{
				m_server = new TcpListener(ipNport);

			}
			catch(Exception)
			{
				m_server=null;

			}
		}
		public static bool isServerRunning(){
			return m_stopServer;
		}
		/// <summary>
		/// Method that starts TCP/IP Server.
		/// </summary>
		public void StartServer()
		{
			
			if (m_server!=null)
			{
				
				/*Creo la rta HTTP que luego enviaran los Threads*/
				StringBuilder str = new StringBuilder();
				str.Append("HTTP/1.1 200 OK\r\n");
				str.Append("\r\n");
				HTTPresponse = Encoding.ASCII.GetBytes(str.ToString());
				
				// Create a ArrayList for storing SocketListeners before
				// starting the server.
				m_socketListenersList = new ArrayList();
				
				queue = new BlockingQueue<string>();
				
				// Start the Server and start the thread to listen client
				// requests.
				m_server.Start();
				m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
				m_serverThread.Start();

				// Create a low priority thread that checks and deletes client
				// SocktConnection objcts that are marked for deletion.
				m_purgingThread = new Thread(new ThreadStart(PurgingThreadStart));
				m_purgingThread.Priority=ThreadPriority.Lowest;
				m_purgingThread.Start();
				
				//Doy inicio al thread que procesa los SMS
				m_processingThread = new Thread (new ThreadStart(processingQueueThreadStart));
				m_processingThread.Start();
			}
		}

		/// <summary>
		/// Method that stops the TCP/IP Server.
		/// </summary>
		public void StopServer()
		{
			if (m_server!=null)
			{
				// It is important to Stop the server first before doing
				// any cleanup. If not so, clients might being added as
				// server is running, but supporting data structures
				// (such as m_socketListenersList) are cleared. This might
				// cause exceptions.

				// Stop the TCP/IP Server.
				m_stopServer=true;
				m_server.Stop();

				// Wait for one second for the the thread to stop.
				m_serverThread.Join(1000);
				
				// If still alive; Get rid of the thread.
				if (m_serverThread.IsAlive)
				{
					m_serverThread.Abort();
				}
				m_serverThread=null;
				
				m_stopPurging=true;
				m_purgingThread.Join(1000);
				if (m_purgingThread.IsAlive)
				{
					m_purgingThread.Abort();
				}
				m_purgingThread=null;
				
				m_stopProccesing=true;
				m_processingThread.Join(1000);
				if (m_processingThread.IsAlive)
				{
					m_processingThread.Abort();
				}
				m_processingThread=null;
				

				// Free Server Object.
				m_server = null;

				// Stop All clients.
				StopAllSocketListers();
			}
		}


		/// <summary>
		/// Method that stops all clients and clears the list.
		/// </summary>
		private void StopAllSocketListers()
		{
			foreach (TCPSocketListener socketListener
			         in m_socketListenersList)
			{
				socketListener.StopSocketListener();
			}
			// Remove all elements from the list.
			m_socketListenersList.Clear();
			m_socketListenersList=null;
		}

		/// <summary>
		/// TCP/IP Server Thread that is listening for clients.
		/// </summary>
		private void ServerThreadStart()
		{
			// Client Socket variable;
			Socket clientSocket = null;
			TCPSocketListener socketListener = null;
			
			while(!m_stopServer)
			{
				try
				{
					// Wait for any client requests and if there is any
					// request from any client accept it (Wait indefinitely).
					//Bloqueante. Espera hasta nueva peticion de cliente o un close.
					clientSocket = m_server.AcceptSocket();

					// Create a SocketListener object for the client.
					socketListener = new TCPSocketListener(clientSocket);

					// Add the socket listener to an array list in a thread
					// safe fashon.
					//Monitor.Enter(m_socketListenersList);
					lock(m_socketListenersList)
					{
						m_socketListenersList.Add(socketListener);
					}
					//Monitor.Exit(m_socketListenersList);

					// Start a communicating with the client in a different
					// thread.
					socketListener.StartSocketListener();
				}
				catch (SocketException)
				{
					m_stopServer = true;
					
				}
			}
		}

		/// <summary>
		/// Thread method for purging Client Listeneres that are marked for
		/// deletion (i.e. clients with socket connection closed). This thead
		/// is a low priority thread and sleeps for 10 seconds and then check
		/// for any client SocketConnection obects which are obselete and
		/// marked for deletion.
		/// </summary>
		private void PurgingThreadStart()
		{
			while (!m_stopPurging)
			{
				ArrayList deleteList = new ArrayList();

				// Check for any clients SocketListeners that are to be
				// deleted and put them in a separate list in a thread sage
				// fashon.
				//Monitor.Enter(m_socketListenersList);
				lock(m_socketListenersList)
				{
					foreach (TCPSocketListener socketListener
					         in m_socketListenersList)
					{
						if (socketListener.IsMarkedForDeletion())
						{
							deleteList.Add(socketListener);
							socketListener.StopSocketListener();
						}
					}

					// Delete all the client SocketConnection ojects which are
					// in marked for deletion and are in the delete list.
					for(int i=0; i<deleteList.Count;++i)
					{
						m_socketListenersList.Remove(deleteList[i]);
					}
				}
				//Monitor.Exit(m_socketListenersList);

				deleteList=null;
				Thread.Sleep(10000);
			}
		}
		
		
		/// <summary>
		///Thread que procesa los mensajes encolados
		/// </summary>
		private void processingQueueThreadStart(){
			
			string aux;


			while(!m_stopProccesing){
				
			
				
				TCPServer.queue.TryDequeue(out aux);

				try
				{
					if(TCPServer.serialPort.IsOpen){
						abrirPuerta();
					}
				}
				
				catch(Exception)
				{
		
				}
				
				
				
			}
		}
		
		


		
		
		
		public static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("Local IP Address Not Found!");
		}
		
		public static void abrirPuerta(){
			serialPort.Write("Abrir");
		}


	}
}
