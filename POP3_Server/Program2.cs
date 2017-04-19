using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace POP3_Server
{
        class Program
    {
       

        static void Main(string[] args)
        {

            for (;;)
            {
                string localhost = "150.254.145.66";
                IPAddress IPaD = IPAddress.Parse(localhost);
                int port = 110;
                TcpListener Listener = new TcpListener(IPaD, port);
                Listener.Start();
                Console.WriteLine("The server is running at port " + port);
                Console.WriteLine("The endpoint is at " + Listener.LocalEndpoint);
                Console.WriteLine("Waiting for connection......");
                Socket s = Listener.AcceptSocket();
                Console.WriteLine("Connection accepted from:\t" + s.RemoteEndPoint);
                string Data;
                Data = "+OK\tThe message was recieved.\n";
                UTF8Encoding asen = new UTF8Encoding();
                s.Send(asen.GetBytes(Data));
                Console.WriteLine("Send Acknowlegment.");
                TcpClient serv = new TcpClient(localhost, port);
                string user = null;
                bool loged = false;
                string[] com = new string[10];
                string com1;
                Serv Server = new Serv(serv);
                for (int j = 0; j < 1;)
                {
                    int n = 0;
                    string com12 = null;
                    byte[] b = new byte[100];
                    int k = 0;
                    while (k == 0)
                    {
                        k = s.Receive(b);
                    }
                    Console.WriteLine("Recived...");
                    for (int i = 0; i < k; i++)
                    {
                        com12 += Convert.ToChar(b[i]);
                    }
                    Regex rgx = new Regex(@"\t|\n|\r");
                    com1 = rgx.Replace(com12, " ");
                    string[] Foo = com1.Split(new char[] { ' ' });
                    int m = 0;
                    foreach (string Bar in Foo)
                    {
                        com[m] = Bar;
                        m++;
                    }
                    switch (com[n])
                    {
                        case "USER":
                            if (com[n + 1] != null)
                            {
                                if (Server.USER(com[n + 1], s))
                                {
                                    user = com[n + 1];
                                    n += 3;
                                }
                            }
                            else
                                s.Send(asen.GetBytes("-ERR\tPlease give your username.\n"));
                            break;

                        case "PASS":
                            if (user != null)
                            {
                                if (com[n] != null)
                                {
                                    loged = Server.PASS(com[n + 1], user, s);
                                    if (loged)
                                        n += 3;
                                }
                            }
                            else
                                s.Send(asen.GetBytes("-ERR\tPlease define User first.\n"));
                            break;

                        case "STAT":
                            if (loged == true)
                            {
                                Server.STAT(user, s);
                                n += 2;
                            }
                            else
                                s.Send(asen.GetBytes("-ERR\tPlease log in first.\n"));
                            break;

                        case "LIST":
                            if (loged == true)
                            {
                                Server.LIST(user, s);
                                n += 2;
                            }
                            else
                                s.Send(asen.GetBytes("-ERR\tPlease log in first.\n"));
                            break;

                        case "RETR":
                            if (loged == true)
                            {
                                if (com[n + 1] != null)
                                {
                                    int x = 0;
                                    Int32.TryParse(com[1], out x);
                                    Server.RETR(user, x, s);
                                    n += 3;
                                }
                                else
                                    s.Send(asen.GetBytes("-ERR\tPlease give the ID of message.\n"));
                            }
                            else
                                s.Send(asen.GetBytes("-ERR\tPlease log in first.\n"));
                            break;

                        case "DELE":
                            if (loged == true)
                            {
                                if (com[1] != null)
                                {
                                    int x = 0;
                                    Int32.TryParse(com[1], out x);
                                    Server.DELE(user, x, Server.D, s);
                                    n += 3;
                                }
                                else
                                    s.Send(asen.GetBytes("-ERR\tPlease give the ID of message.\n"));
                            }
                            else
                                s.Send(asen.GetBytes("-ERR\tPlease log in first.\n"));
                            break;

                        case "QUIT":
                        case "EXIT":
                            s.Send(asen.GetBytes("+OK\tlogged out.\n"));
                            com[n] = null;
                            j = 1;
                            if (loged == true)
                                Server.END(user, Server.D, s);
                            s.Close();
                            Listener.Stop();
                            break;


                        default:
                            s.Send(asen.GetBytes("-ERR\tWrong command\n"));
                            break;
                    }
                }
            }
        }
    }
}

public class Serv
{
    public List <int> D;
    private TcpClient serv;
    private string directory = @"c:/pass";
    private UTF8Encoding asen;

    public Serv(TcpClient serv)
    {
        D = new List<int>();
        this.serv = serv;
        asen = new UTF8Encoding();
    }

    public bool USER(string user, Socket s)
    {
        Console.Write("Procedure_USER\n");
        System.IO.Directory.CreateDirectory(directory);
        user += ".txt";
        string path = System.IO.Path.Combine(directory, user);

        if (File.Exists(path))
        {
            s.Send(asen.GetBytes("+OK\tUser exists.\n"));
            return true;
        }
        else
        {
            s.Send(asen.GetBytes("-ERR\tUser doesn't exists.\n"));
            return false;
        }
    }

    public bool PASS(string pass, string user, Socket s)
    {
        Console.Write("Procedure_PASS\n");
        System.IO.Directory.CreateDirectory(directory);
        string path = System.IO.Path.Combine(directory, user + ".txt");
        StreamReader usser = new StreamReader(path);
        string read = usser.ReadToEnd();

        if (pass == read)
        {
            s.Send(asen.GetBytes("+OK\tPassword correct.\n"));
            return true;
        }
        else
        {
            s.Send(asen.GetBytes("-ERR\tWrong password.\n"));
            return false;
        }
    }

    public void STAT(string user, Socket s)
    {
        Console.Write("Procedure_STAT\n");
        System.IO.Directory.CreateDirectory(directory);
        string path = System.IO.Path.Combine(directory, user);
        int directoryCount = System.IO.Directory.GetDirectories(path).Length;

        long totalFileSize = 0;
        string[] dirFiles = Directory.GetFiles(path, "*.*",
                                System.IO.SearchOption.AllDirectories);

        foreach (string fileName in dirFiles)
        {
            FileInfo info = new FileInfo(fileName);
            totalFileSize = totalFileSize + info.Length;
        }
        s.Send(asen.GetBytes("+OK\t" + directoryCount + "\t" + totalFileSize + "\n"));
    }

    public void LIST(string user, Socket s)
    {
        Console.Write("Procedure_LIST\n");
        System.IO.Directory.CreateDirectory(directory);
        string path = System.IO.Path.Combine(directory, user);
        int directoryCount = System.IO.Directory.GetDirectories(path).Length;
        string message;
        if (directoryCount != 0)
        {
            s.Send(asen.GetBytes("+OK\t" + (directoryCount - D.Count) + " messages.\n"));
            for (int i = 1; i <= directoryCount;)
            {
                foreach (string st in Directory.GetDirectories(path))
                {
                    string i1 = st.Remove(0, path.Length+1);
                    if (D.Count != 0)
                    {
                        int x = 0;
                        Int32.TryParse(i1, out x);
                        if (D.Contains(x)) { }
                        else
                        {
                            DirectoryInfo di = new DirectoryInfo(st);
                            message = i1 + "\t" + 
                                di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length) + "\n";
                            s.Send(asen.GetBytes(message));
                        }
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(st);
                        message = i1 + "\t" +
                                di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length) + "\n";
                        s.Send(asen.GetBytes(message));
                    }
                    i++;
                }
                s.Send(asen.GetBytes(".\n"));
            }
        }
        else
            s.Send(asen.GetBytes("+OK\tNo messages.\n"));
    }

    private bool TryLIST(string user, int id, List<int> D, Socket s)
    {
        System.IO.Directory.CreateDirectory(directory);
        string path = System.IO.Path.Combine(directory, user);
        int directoryCount = System.IO.Directory.GetDirectories(path).Length;
        int t = 0;
        if (directoryCount != 0)
        {
            for (int i = 1; i <= directoryCount;)
            {
                foreach (string st in Directory.GetDirectories(path))
                {
                    string i1 = st.Remove(0, path.Length + 1);
                    if (i1 == id.ToString())
                    {
                        i = directoryCount;
                        t = 1;
                        if (D.Contains(id))
                        {
                            t = 0;
                        }
                    }
                    i++;
                }
            }
        }
        if (t == 1)
            return true;
        else
            return false;
    }

    public void RETR(string user, int id, Socket s)
    {
        Console.Write("Procedure_RETR\n");
        if (TryLIST(user, id, this.D, s))
        {
            System.IO.Directory.CreateDirectory(directory);
            string path1 = System.IO.Path.Combine(directory, user);
            string path2 = System.IO.Path.Combine(path1, id.ToString());
            string[] dirFiles = Directory.GetFiles(path2, "*.*",
                             System.IO.SearchOption.AllDirectories);
            string path = System.IO.Path.Combine(path2, id.ToString() + ".txt");
            int directoryCount = 0;
            foreach (string fileName in dirFiles)
            {
                FileInfo info = new FileInfo(fileName);
                directoryCount++;
            }
            long totalFileSize = 0;

            foreach (string fileName in dirFiles)
            {
                FileInfo info = new FileInfo(fileName);
                totalFileSize = totalFileSize + info.Length;
            }
            s.Send(asen.GetBytes("+OK\tSending " + totalFileSize + " bytes.\n"));
            s.Send(asen.GetBytes("From: Daniel Lachowicz <dannylach@wp.pl>\nDate: Mon, 21 Nov 2016 15:15:26\nSubject: Test"));
            string read;
            s.Send(asen.GetBytes("\n--===123==\n"));
            StreamReader mesage = new StreamReader(path);
            while ((read = mesage.ReadLine()) != null)
            {
                s.Send(asen.GetBytes(read + "\n"));
            }
            if (directoryCount > 1)
            {   
                for (int i = 0; i < directoryCount; i++)
                {
                    mesage = new StreamReader(dirFiles[i]);
                    string i1 = dirFiles[i].Remove(0, path2.Length + 1);
                    string i2 = i1.Remove(0, i1.Length - 3);
                    if (i1 != (id.ToString() + ".txt"))
                    {
                        s.Send(asen.GetBytes("\n--===123==\n"));
                        s.Send(asen.GetBytes("filename=\"" + i1 + "\"\n"));
                        byte[] bytes = File.ReadAllBytes(dirFiles[i]);
                        string message = Convert.ToBase64String(bytes);
                        s.Send(asen.GetBytes(message));
                        /*string ms = null;
                        for (int a=0;a<=message.Length;a++)
                        {
                            do
                            {
                                ms = ms + message[a];
                                s.Send(asen.GetBytes(ms));
                                a++;
                            } while (a%2000000==0);
                        }*/
                    }
                }
            }
            s.Send(asen.GetBytes("\n--===123==--\n"));
            s.Send(asen.GetBytes(".\n"));
        }
        else
            s.Send(asen.GetBytes("-ERR\tMessage with that ID doesnt exist.\n"));  
    }

    public void DELE(string user, int id, List<int> D, Socket s)
    {
        Console.Write("Procedure_DELE\n");
        if (TryLIST(user, id, D, s))
        {
            D.Add(id);
            s.Send(asen.GetBytes("+OK\tMessage deleted.\n"));
        }
        else
            s.Send(asen.GetBytes("-ERR\tCouldn't delete message.\n"));
    }

    public void END(string user, List<int> D, Socket s)
    {
        Console.WriteLine("Ending");
        if (D.Count != 0)
        {
            Console.WriteLine("In if");
            System.IO.Directory.CreateDirectory(directory);
            string path1 = System.IO.Path.Combine(directory, user);
            int id = D.Count;
            while (id != 0)
            {
                Console.WriteLine("deleting");

                string path = System.IO.Path.Combine(path1, D[id-1].ToString());
                Directory.CreateDirectory(path);
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                Directory.Delete(path);
                id--;
            }
        }
    }
}