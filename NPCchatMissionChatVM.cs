
using System.IO;
using System;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.ViewModelCollection.Conversation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.LogEntries;
using System.Threading.Tasks;
using OpenAI_API;
using Newtonsoft.Json.Linq;
using TaleWorlds.GauntletUI;
using OpenAI_API.Models;
using TaleWorlds.Localization;

namespace Bannerlord.ChatGPT
{
    public class NPCchatMissionChatVM : MissionConversationVM
    {
        private readonly ConversationManager _conversationManager;
        private string _newDialogText;
        private string _aIText;
        public string response;
        public OpenAIAPI bot;
        public OpenAI_API.Chat.Conversation _chat;
        string conversationCharacterId;
        private bool _isChating;
        public bool _isBotStarted;
        public bool isResponding;
        private String _APIkey;
        LoggingSystem _logSys;

        
        private int _fontsizeAIresponse;

        public NPCchatMissionChatVM(Func<string> getContinueInputText, bool isLinksDisabled = false): base(getContinueInputText, isLinksDisabled)
        {
            this._conversationManager = Campaign.Current.ConversationManager;
            _logSys = new LoggingSystem();
            FontsizeAIresponse = 27;
            AIText = "Enter your sentences and press ENTER to chat.";
            _isBotStarted = false;
            isResponding = false;
        }

        public async void StartBotAsync()
        {
            // read API file
            {
                try
                {
                    string modRelayerFolder = _logSys.modRelayerFolder;
                    // string FileName = modRelayerFolder + "APIkey.txt";
                    string FileName = Path.Combine(modRelayerFolder, "APIkey.txt");

                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader(FileName);
                    //Read the line of text
                    _APIkey = sr.ReadLine();
                    // close file
                    sr.Close();
                }
                catch (Exception e)
                {

                    _logSys.Addlog("API key is not properly read! Exception: " + e.Message);
                    InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=xO4QZbQ7XE}API key is not properly read! Please Check your /Modules/Bannerlord_ChatGPT/APIkey.txt").ToString()));
                    _isBotStarted = false;
                    return;
                }
                // remove all the space
                _APIkey = _APIkey.Replace(" ", "");
                _APIkey = _APIkey.Replace("\"", "");
                _APIkey = _APIkey.Replace("\n", "");


            }


            // Test chat 
            {
                

                try
                {
                    
                    PromotsEngine promotsEngine = new PromotsEngine(_conversationManager);  
                    bot = new OpenAIAPI(_APIkey); // shorthand


                    _chat = bot.Chat.CreateConversation();
                    _chat.Model = Model.ChatGPTTurbo;
                    _chat.AppendSystemMessage(promotsEngine.GetPrompts());
                    _chat.AppendUserInput(new TextObject("{=t4szG41Y1s}Hi! I want to talk with you. (ChatGPT)").ToString());
                    AIText = await _chat.GetResponseFromChatbotAsync();
                }
                catch (Exception e)
                {
                    _isBotStarted = false;
                    _logSys.Addlog("ChatGPT didn't pass the test! Exception: " + e.Message);
                    InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=c40K7kO5Yr}ChatGPT didn't start successfully!").ToString()));
                    return ;
                }
            }
            _isBotStarted = true;
            return ;
        }

        public void ExitChating()
        {
            if (this._conversationManager != null)
            {
                _conversationManager.EndConversation();

            }
        }

        public async Task SendPlayerInputAsync()
        {
            if (this.UserText == String.Empty || UserText == null || IsChating == false || isResponding == true)
            {
                
                return;
            }
            try
            {
                FontsizeAIresponse = 27;
                isResponding = true;
                _chat.AppendUserInput(UserText);
                AIText = await _chat.GetResponseFromChatbotAsync();
                if (AIText.Length > 250 && AIText!=null)
                {
                    FontsizeAIresponse = (int)((double)(27 * 250 / AIText.Length));
                }
            }
            catch (Exception e)
            {

                _logSys.Addlog("Conversation failed! Exception: " + e.Message);
            }
            isResponding = false;
            UserText = "";
            // SendJsonAsync(message);
        }


        public override void OnFinalize()
        {
            base.OnFinalize();
            CampaignEvents.PersuasionProgressCommittedEvent.ClearListeners((object)this);
            
        }

        internal void ExecuteContinue()
        {
            Debug.Print("ExecuteContinue", 0, Debug.DebugColor.White, 17592186044416UL);
            this._conversationManager.ContinueConversation();
        }


        // The following is the sentence sent to chatGPT API
        // NewDialogText is the sentence written by user
        // ConversationText is the AI generated response

        [DataSourceProperty]
        public string UserText
        {
            get
            {
                return _newDialogText;
            }
            set
            {
                if (_newDialogText != value)
                {
                    _newDialogText = value;
                    OnPropertyChangedWithValue(value, "UserText");
                }
            }
        }


        [DataSourceProperty]
        public string AIText
        {
            get
            {
                return _aIText;
            }
            set
            {
                if (_aIText != value && value!= null)
                {
                    
                    _aIText = value;
                    
                    OnPropertyChangedWithValue(value, "AIText");
                }
            }
        }

        [DataSourceProperty]
        public bool IsChating
        {
            get => this._isChating;
            set
            {
                if (this._isChating == value)
                    return;
                this._isChating = value;
                this.OnPropertyChangedWithValue((object)value, nameof(IsChating));
            }
        }
        
        [DataSourceProperty]
        public int FontsizeAIresponse
        {
            get => this._fontsizeAIresponse;
            set
            {
                if (_fontsizeAIresponse != value)
                {
                    _fontsizeAIresponse = value;
                    OnPropertyChangedWithValue(value, "FontsizeAIresponse");
                }
            }
        }
    }

}