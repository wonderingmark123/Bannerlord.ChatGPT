using System.IO;
using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using LLama.Common;
using LLama;
using System.Diagnostics;
using TaleWorlds.Diamond;
using TaleWorlds.Localization;

namespace Bannerlord.ChatGPT
{
    public class SubModule : MBSubModuleBase
    {
        internal Process _server;
        internal bool didIStarted;
        internal LoggingSystem log;
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            // initialize the ChatAIbehavior
            log = new LoggingSystem();
            StartServer();
            log.Removelog("Chat GPT mod is loaded");
            

            if (gameStarterObject.GetType() == typeof(CampaignGameStarter))
            {
                base.OnCampaignStart(game, gameStarterObject);
                CampaignGameStarter starter = (CampaignGameStarter)gameStarterObject;
                starter.AddBehavior(new ChatAIbehavior());
            }
        }

        private void StartServer()
        {
            string modelPath = "";
            {
                try
                {
                    string modRelayerFolder = log.modRelayerFolder;
                    // string FileName = modRelayerFolder + "APIkey.txt";
                    string FileName = Path.Combine(modRelayerFolder, "APIkey.txt");

                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader(FileName);
                    //Read the line of text
                    modelPath = sr.ReadLine();
                    // close file
                    sr.Close();
                }
                catch (Exception e)
                {

                    log.Addlog("model path is not properly read! Exception: " + e.Message);
                    InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=xO4QZbQ7XE}API key is not properly read! Please Check your /Modules/Bannerlord_ChatGPT/APIkey.txt").ToString()));

                }


            }

            try
            {
                if (modelPath.Substring(0, 3) == "sk-" && modelPath.Length == 51)
                {
                    return;
                }
                // string moduleFullPath = ModuleHelper.GetModuleFullPath("Inworld");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = log.modRelayerFolder,
                    FileName = log.modRelayerFolder + "LLamaWebsocket.exe",
                    WindowStyle = ProcessWindowStyle.Minimized,
                    Arguments = modelPath,
                };
                didIStarted = true;
                _server = Process.Start(startInfo);
                _server.PriorityClass = ProcessPriorityClass.RealTime;
            }
            catch(Exception ex) 
            {
                log.Addlog("LLamaWebsocket.exe didn't start!" + ex.Message);
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();


            InformationManager.DisplayMessage(new InformationMessage(
                "Bannerlord.ChatGPT has been loaded".ToString()));

        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new NPCchatMissionConversationView());
        }
        
        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            _server.Close();
        }
    }


    class LoggingSystem
    {
        public string modRelayerFolder;
        public LoggingSystem()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string parentFolder = Directory.GetParent(path: Directory.GetParent(path: Directory.GetParent(path).FullName).FullName).FullName;
            modRelayerFolder = System.IO.Path.Combine(parentFolder, "ModRelayer\\");
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(modRelayerFolder, "log.txt")))
            {
                DateTime currentTime = DateTime.Now;
                Console.WriteLine("Time: " + currentTime);
                outputFile.WriteLine("Chat GPT mod is loaded");
            }
        }
        public void Addlog(string Str)
        {


            Str = Str + System.DateTime.Now.ToString();

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(modRelayerFolder, "log.txt"),true))
            {
                DateTime currentTime = DateTime.Now;
                outputFile.WriteLine("Time: " + currentTime);
                outputFile.WriteLine(Str);
            }
        }
        public void Removelog(string Str)
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(modRelayerFolder, "log.txt")))
            {
                DateTime currentTime = DateTime.Now;
                outputFile.WriteLine("Time: " + currentTime);
                outputFile.WriteLine(Str);
            }
        }
    }
}