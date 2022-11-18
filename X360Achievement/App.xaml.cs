using MadMilkman.Ini;
using NAudio.Wave;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;

namespace X360Achievement
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {

        private readonly FileSystemWatcher watcher = new();
        static readonly List<String> list = new();
        private string path = "";
        private string id = "";
        private readonly string steamKey = "";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool isPathArgument = false;
            bool hasPathArgument = false;
            bool isSteamIdArgument = false;
            bool hasSteamIdArgument = false;
            foreach (string s in e.Args)
            {
                if (s == "-p")
                {
                    isPathArgument = true;
                    hasPathArgument = true;
                    continue;
                }
                if (isPathArgument)
                {
                    path = s;
                    isPathArgument = false;
                }
                if (s == "-i")
                {
                    isSteamIdArgument = true;
                    hasSteamIdArgument = true;
                    continue;
                }
                if (isSteamIdArgument)
                {
                    id = s;
                    isPathArgument = false;
                }
            }
            if (!hasPathArgument || !hasSteamIdArgument)
            {
                Console.WriteLine("Usage: X360Achievement.exe -p <path/to/steam(emu)/folder> -i <steamID>");
                Environment.Exit(0);
            }
            watcher.Path = path;
            watcher.Filter = "achievements.ini";
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(OnWrite);
            watcher.EnableRaisingEvents = true;

            IniFile iniFile = new(new IniOptions());
            do
            {
                iniFile.Load(path + "/achievements.ini");
                Thread.Sleep(10);
            } while (iniFile.Sections.Count == 0);
            foreach (var a in iniFile.Sections["SteamAchievements"].Keys)
            {
                if (a.Name != "Count")
                {
                    list.Add(a.Value);
                }
            }
            Thread.Sleep(Timeout.Infinite);




        }
        private void OnWrite(object sender, FileSystemEventArgs e)
        {
            IniFile iniFileNew = new(new IniOptions());
            do
            {
                iniFileNew.Load(path + "/achievements.ini");
                Thread.Sleep(10);
            } while (iniFileNew.Sections.Count == 0);
            
            foreach (var a in iniFileNew.Sections["SteamAchievements"].Keys)
            {
                if (a.Name != "Count" && !list.Contains(a.Value))
                {
                    list.Add(a.Value);
                    string DisplayName = "";
                    using (dynamic steamUserAuth = WebAPI.GetInterface("ISteamUserStats", steamKey))
                    {
                        
                        steamUserAuth.Timeout = TimeSpan.FromSeconds(5);

                        try
                        {
                            var tmp = steamUserAuth.GetSchemaForGame(appid: id, l: "en-US");
                            foreach(var s in tmp.Children[2].Children[1].Children)
                            {
                                if (s.Name == a.Value)
                                {
                                    DisplayName = s.Children[1].Value;
                                }
                            }
                  
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Unable to make SteamUserStats API Request: {0}", ex.Message);
                        }
                    }
                    Thread viewerThread = new(delegate ()
                    {

                        MainWindow wnd = new();
                        wnd.BottomText.Text = DisplayName;
                        wnd.BottomTextDummy.Text = DisplayName;
                        wnd.Show();
                        wnd.CenterColumn.Width = new GridLength(wnd.CenterColumn.ActualWidth, GridUnitType.Pixel);
                        ((Storyboard)wnd.Resources["MyStoryboard"]).Begin(wnd);


                        var waveOut = new WaveOut();
                        waveOut.Init(new WaveChannel32(new Mp3FileReader(new MemoryStream(X360Achievement.Properties.Resources.achievement))));
                        waveOut.Play();
                        System.Windows.Threading.Dispatcher.Run();

                    });

                    viewerThread.SetApartmentState(ApartmentState.STA);
                    viewerThread.Start();

                }
            }
        }







        
  
}
}
