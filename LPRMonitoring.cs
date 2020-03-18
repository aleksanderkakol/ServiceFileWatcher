using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace LPRMonitoring
{
    public partial class LPRMonitoring : ServiceBase
    {
        string WatchPath = ConfigurationManager.AppSettings["WatchPath"];
        string ftpURL = ConfigurationManager.AppSettings["NetworkShare"];
        string secondaryIP = ConfigurationManager.AppSettings["SecondaryIP"];
        string WinUser = ConfigurationManager.AppSettings["User"];
        string WinPwd = ConfigurationManager.AppSettings["Pwd"];
        string ip = ConfigurationManager.AppSettings["Postgresip"];
        string port = ConfigurationManager.AppSettings["PostgresPort"];
        string user = ConfigurationManager.AppSettings["PostgresUser"];
        string password = ConfigurationManager.AppSettings["PostgresPwd"];
        string database = ConfigurationManager.AppSettings["PostgresDB"];
        public LPRMonitoring()
        {
            InitializeComponent();
            fileWatcherWatchDdriveArticleimagefolder.Created += FileWatcherWatchDdriveArticleimagefolder_Created;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                fileWatcherWatchDdriveArticleimagefolder.Path = WatchPath;

            } catch (Exception ex)
            {
                CreateErrorLog(ex.Message, ex.StackTrace);
            }
        }

        protected override void OnStop()
        {

            try
            {
                Create_ServiceStoptextfile();
            }
            catch (Exception ex)
            {

                CreateErrorLog(ex.Message, ex.StackTrace);
            }
        }

        void FileWatcherWatchDdriveArticleimagefolder_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                Thread.Sleep(1000);
                //Then we need to check file is exist or not which is created.  
                if (CheckFileExistance(WatchPath, e.Name))
                {
                    //Then write code for log detail of file in text file.  
                    CreateTextFile(WatchPath, e.Name);
                    PostgresInsert(e.FullPath);
                    using (var impersonation = new ImpersonateUser(WinUser, ip, WinPwd, 9))
                    {
                        CopyFile(e.FullPath, e.Name);
                    }
                    
                }

            }
            catch (Exception ex)
            {

                CreateErrorLog(ex.Message);
            }

        }

        private bool CheckFileExistance(string FullPath, string FileName)
        {
            // Get the subdirectories for the specified directory.'  
            bool IsFileExist = false;
            DirectoryInfo dir = new DirectoryInfo(FullPath);
            if (!dir.Exists)
                IsFileExist = false;
            else
            {
                string FileFullPath = Path.Combine(FullPath, FileName);
                if (File.Exists(FileFullPath))
                    IsFileExist = true;
            }
            return IsFileExist;
        }

        private void CreateTextFile(string FullPath, string FileName)
        {
            StreamWriter SW;
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "txtStatus_" + DateTime.Now.ToString("yyyyMMdd") + ".txt")))
            {
                SW = File.CreateText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "txtStatus_" + DateTime.Now.ToString("yyyyMMdd") + ".txt"));
                SW.Close();
            }
            using (SW = File.AppendText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "txtStatus_" + DateTime.Now.ToString("yyyyMMdd") + ".txt")))
            {
                SW.WriteLine(DateTime.Now.ToString("dd-MM-yyyy H:mm:ss") + " File  " + FileName + " created at this location: " + FullPath);
                SW.Close();
            }
        }

        private void CreateErrorLog(string Error, string StackTrace = null)
        {
            StreamWriter SW;
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Error_" + DateTime.Now.ToString("yyyyMMdd") + ".log")))
            {
                SW = File.CreateText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Error_" + DateTime.Now.ToString("yyyyMMdd") + ".log"));
                SW.Close();
            }
            using (SW = File.AppendText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Error_" + DateTime.Now.ToString("yyyyMMdd") + ".log")))
            {
                SW.WriteLine(DateTime.Now.ToString("dd-MM-yyyy H:mm:ss") + $" {Error} {StackTrace}");
                SW.Close();
            }
        }

        private void PostgresInsert(string FileName)
        {
            string file = Path.Combine(Path.GetDirectoryName(FileName), Path.GetFileNameWithoutExtension(FileName));
            file.ToUpper();
            if (file.Contains("VEHICLE_DETECTION") && file.Contains("LPR"))
            {
                string[] words = file.Split('_');
                string plateNumber = words[4];

                NpgsqlParameter pnl = new NpgsqlParameter("evt_pnl_id", DbType.Int32);
                NpgsqlParameter evd_id = new NpgsqlParameter("evt_evd_id", DbType.Int32);
                NpgsqlParameter is_exit = new NpgsqlParameter("evt_salto_is_exit", DbType.Boolean);
                NpgsqlParameter user_type = new NpgsqlParameter("evt_salto_user_type", DbType.Int32);
                NpgsqlParameter user_name = new NpgsqlParameter("evt_salto_user_name", DbType.String);
                NpgsqlParameter door = new NpgsqlParameter("evt_salto_door_name", DbType.String);

                pnl.Value = 37;
                user_type.Value = 0;
                user_name.Value = plateNumber;


                if (file.Contains("LPR 1 WJAZD"))
                {
                    evd_id.Value = 2500;
                    is_exit.Value = false;
                    door.Value = "K42";
                }
                else if (file.Contains("LPR 2 WYJAZD"))
                {
                    evd_id.Value = 2501;
                    is_exit.Value = true;
                    door.Value = "K43";
                }

                NpgsqlConnection conn = new NpgsqlConnection($"Server={ip}; Port={port}; User Id={user}; Password={password}; Database={database}");

                string pg_is_in_recovery = "SELECT pg_is_in_recovery()";
                string insert = "INSERT INTO zewng.evt(evt_pnl_id,evt_evd_id,evt_salto_ts,evt_salto_is_exit, evt_salto_user_type, evt_salto_user_name, evt_salto_door_name) values(:evt_pnl_id,:evt_evd_id,current_timestamp,:evt_salto_is_exit, :evt_salto_user_type, :evt_salto_user_name, :evt_salto_door_name)";

                try
                {
                    conn.Open();
                    NpgsqlCommand pg_status = new NpgsqlCommand(pg_is_in_recovery, conn);
                    bool pg_recovery_status = (bool)pg_status.ExecuteScalar();
                    if (!pg_recovery_status)
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand(insert, conn);

                        cmd.Parameters.Add(pnl);
                        cmd.Parameters.Add(evd_id);
                        cmd.Parameters.Add(is_exit);
                        cmd.Parameters.Add(user_type);
                        cmd.Parameters.Add(user_name);
                        cmd.Parameters.Add(door);

                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                    
                    conn.Close();
                } catch (Exception ex)
                {
                    CreateErrorLog(ex.Message);
                }
                
            }
        }

        private void CopyFile(string SourcePath, string FileName)
        {
            string destinationPath = $@"{ftpURL}";
            string sourceFile = Path.Combine(SourcePath);
            string destinationFile = Path.Combine(destinationPath, FileName);
            try
            {
                File.Copy(sourceFile, destinationFile);
            } catch (Exception ex)
            {
                CreateErrorLog(ex.Message);
            }
        }

        public static void Create_ServiceStoptextfile()
        {
            string Destination = "C:\\Server\\service";
            StreamWriter SW;
            if (Directory.Exists(Destination))
            {
                Destination = System.IO.Path.Combine(Destination, "txtServiceStop_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                if (!File.Exists(Destination))
                {
                    SW = File.CreateText(Destination);
                    SW.Close();
                }
            }
            using (SW = File.AppendText(Destination))
            {
                SW.Write("\r\n\n");
                SW.WriteLine("Service Stopped at: " + DateTime.Now.ToString("dd-MM-yyyy H:mm:ss"));
                SW.Close();
            }
        }
    }
}
