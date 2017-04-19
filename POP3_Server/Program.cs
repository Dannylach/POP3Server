using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                string localhost = "10.160.34.107";
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
                string message;
                for (int j = 0; j < 1;)
                {
                    int n = 0;
                    string com12 = null;
                    byte[] b = new byte[100];
                    int k = s.Receive(b);
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
    private string message;
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
            message = "+OK\tUser exists.\n";
            s.Send(asen.GetBytes(message));
            return true;
        }
        else
        {
            message = "-ERR\tUser doesn't exists.\n";
            s.Send(asen.GetBytes(message));
            Console.Write(message);
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
            message = "+OK\tPassword correct.\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
            return true;
        }
        else
        {
            message = "-ERR\tWrong password.\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
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
        message = "+OK\t" + directoryCount + "\t" + totalFileSize + "\n";
        Console.Write(message);
        s.Send(asen.GetBytes(message));
    }

    public void LIST(string user, Socket s)
    {
        Console.Write("Procedure_LIST\n");
        System.IO.Directory.CreateDirectory(directory);
        string path = System.IO.Path.Combine(directory, user);
        int directoryCount = System.IO.Directory.GetDirectories(path).Length;
        if (directoryCount != 0)
        {
            message = "+OK\t" + (directoryCount-D.Count) + " messages.\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
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
                            Console.Write(message);
                            s.Send(asen.GetBytes(message));
                        }
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(st);
                        message = i1 + "\t" +
                                di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length) + "\n";
                        Console.Write(message);
                        s.Send(asen.GetBytes(message));
                    }
                    i++;
                }
                s.Send(asen.GetBytes(".\n"));
            }
        }
        else
        {
            message = "+OK\tNo messages.\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
        }
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
            Console.WriteLine(directoryCount);
            long totalFileSize = 0;

            foreach (string fileName in dirFiles)
            {
                FileInfo info = new FileInfo(fileName);
                totalFileSize = totalFileSize + info.Length;
            }
            message = "+OK\tSending " + totalFileSize + " bytes.\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
            string read;
            message = "\n--===123==\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
            StreamReader mesage = new StreamReader(path);
            while ((read = mesage.ReadLine()) != null)
            {
                message = read + "\n";
                Console.Write(message);
                s.Send(asen.GetBytes(message));
            }
            message = "--===123==--\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
            if (directoryCount > 1)
            {   
                for (int i = 0; i < directoryCount; i++)
                {
                    mesage = new StreamReader(dirFiles[i]);
                    string i1 = dirFiles[i].Remove(0, path2.Length + 1);
                    string i2 = i1.Remove(0, i1.Length - 3);
                    Console.WriteLine(i1);
                    Console.WriteLine(i2);
                    if (i1 != (id.ToString() + ".txt"))
                    {
                        message = "\n--===z123==\n";
                        Console.Write(message);
                        s.Send(asen.GetBytes(message));
                        message = "File_name: " + i1 + "\n";
                        Console.Write(message);
                        s.Send(asen.GetBytes(message));
                        Byte[] bytes = File.ReadAllBytes(dirFiles[i]);
                        message = Convert.ToBase64String(bytes);
                        Console.Write(message);
                        s.Send(asen.GetBytes(message));
                        message = "\n--===123==--\n";
                        Console.Write(message);
                        s.Send(asen.GetBytes(message));
                    }
                }
            }
            message = ".\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
        }
        else
        {
            message = "-ERR\tMessage with that ID doesnt exist.\n";
            Console.Write(message);
            s.Send(asen.GetBytes(message));
        }
    }

    public void DELE(string user, int id, List<int> D, Socket s)
    {
        Console.Write("Procedure_DELE\n");
        if (TryLIST(user, id, D, s))
        {
            D.Add(id);
            message = "+OK\tMessage deleted.\n";
            s.Send(asen.GetBytes(message));
        }
        else
            message = "-ERR\tCouldn't delete message.\n";
        Console.Write(message);
            s.Send(asen.GetBytes(message));
    }

    public void END(string user, List<int> D, Socket s)
    {
        if (D.Count != 0)
        {
            System.IO.Directory.CreateDirectory(directory);
            string path1 = System.IO.Path.Combine(directory, user);
            int id = D.Count - 1;
            while (id != 0)
            {
                string path = System.IO.Path.Combine(path1, D[id].ToString());
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