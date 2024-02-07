using SandBox.CampaignBehaviors;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.MountAndBlade;
using System.Linq;
using System.Text;
using System.Reflection;
using static TaleWorlds.CampaignSystem.CharacterDevelopment.DefaultPerks;

namespace Bannerlord.ChatGPT
{
    internal class ChatAIbehavior : CampaignBehaviorBase
    {
        internal readonly string MOD_VERSION = "1.0.0";

        // The data in this field will persist across saving
        public string _APIkey = "546";

        public override void SyncData(IDataStore dataStore)
        {
            // First argument is an identifier, only needs to be unique to this behavior
            dataStore.SyncData("NPCchatAPIkey", ref _APIkey);
        }



        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogs(starter);
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            // villigers
            starter.AddPlayerLine("town_or_village_direct_question", "town_or_village_player", "Chat_GPT_Dialog_Start", "{=t4szG41Y1s}Hi! I want to talk with you. (ChatGPT)", new ConversationSentence.OnConditionDelegate(this.AIchatOnConditionDelegate), (ConversationSentence.OnConsequenceDelegate)(this.AIchatOnConsequenceDelegate));
            
            // compion
            starter.AddPlayerLine("companion_start_role", "hero_main_options", "Chat_GPT_Dialog_Start", "{=t4szG41Y1s}Hi! I want to talk with you. (ChatGPT)", new ConversationSentence.OnConditionDelegate(this.AIchatOnConditionDelegate), (ConversationSentence.OnConsequenceDelegate)(this.AIchatOnConsequenceDelegate), 100, null, null);

            // governor
            //GovernorCampaignBehavior governorCampaignBehavior = new GovernorCampaignBehavior();
            // Type type = governorCampaignBehavior.GetType();
            // starter.AddPlayerLine("governor_talk_start", "hero_main_options", "governor_talk_start_reply", "{=t4szG41Y1s}Hi! I want to talk with you. (ChatGPT)", new ConversationSentence.OnConditionDelegate(governorCampaignBehavior.governor_talk_start_on_condition), null, 100, null, null);


            // general responsee after choosing "chat with you (ChatGPT)"
            starter.AddDialogLine("ChatGPT_no_response", "Chat_GPT_Dialog_Start", "Chat_GPT_Dialog_End", "{=}", () => true, (ConversationSentence.OnConsequenceDelegate)null);
            starter.AddPlayerLine("ChatGPT_end_conversation", "Chat_GPT_Dialog_End", "close_window", "{=CHCUOZxcnR}Click here to exit", (ConversationSentence.OnConditionDelegate)null, (ConversationSentence.OnConsequenceDelegate)null);
        }

        private void AIchatOnConsequenceDelegate()
        {

            var CurrentNPCchatMissionConversationView = Mission.Current.GetMissionBehavior<NPCchatMissionConversationView>();



            if(CurrentNPCchatMissionConversationView != null) 
            {
                // make current state become isChating = true;
                CurrentNPCchatMissionConversationView.UpdateChatStatus(true); 
            }

            Campaign.Current.ConversationManager.UpdateCurrentSentenceText();

        }

        private bool AIchatOnConditionDelegate()
        {
            if (Mission.Current != null)
            {
                return true;
            }
            else
            { 
                return false; 
            }
            
        }

    }
    

}