﻿using System.IO;
using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;


namespace Bannerlord.ChatGPT
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            // initialize the ChatAIbehavior

            if (gameStarterObject.GetType() == typeof(CampaignGameStarter))
            {
                base.OnCampaignStart(game, gameStarterObject);
                CampaignGameStarter starter = (CampaignGameStarter)gameStarterObject;
                starter.AddBehavior(new ChatAIbehavior());
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
                Console.WriteLine("Time: " + currentTime);
                outputFile.WriteLine(Str);
            }
        }
    }
}