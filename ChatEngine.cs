
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using OpenAI_API;
using OpenAI_API.Models;
using System.Threading.Tasks;
using LLama;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Net.Http;
using TaleWorlds.Localization;

namespace Bannerlord.ChatGPT
{

    public class ChatEngine
    {
        public OpenAIAPI bot;
        public bool UsingAPI;
        public OpenAI_API.Chat.Conversation _chat;
        public string conversationID;
        private System.Diagnostics.Process process;
        private string modelPath;
        private LoggingSystem log;
        public ChatSession session;
        private ClientWebSocket ws;
        private static readonly HttpClient client = new HttpClient();
        public ChatEngine(string APIkey)
        {
            log = new LoggingSystem();
            if (APIkey != null && APIkey.Length == 51) 
            {
                
                bot = new OpenAIAPI(APIkey);
                
            }
            else 
            { 
                UsingAPI = false;
                ws = new ClientWebSocket();
                
            }

        }


        public void CloseProcess()
        {
            if (!UsingAPI && ws != null)
            {
                
                ws.Abort();
                
            }
            
            
        }
        internal async Task AppendSystemMessage()
        {
            
            if (UsingAPI) 
            {
                PromotsEngine promotsEngine = new PromotsEngine(Campaign.Current.ConversationManager,null);
                string SystemMessage = promotsEngine.GetPrompts();
                _chat.AppendSystemMessage(SystemMessage);
            }
            else
            {
                string SystemMessage = "SystemMessage";
                var bytes = Encoding.UTF8.GetBytes(SystemMessage);
                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                await ws.SendAsync(arraySegment,
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);


                PromotsEngine promotsEngine = new PromotsEngine(Campaign.Current.ConversationManager,
                    "You are \"{character}\". We are living on the fictional continent of Calradia.");

                SystemMessage = promotsEngine.GetPrompts();
                bytes = Encoding.UTF8.GetBytes(SystemMessage);
                arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                await ws.SendAsync(arraySegment,
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);


            }


        }

        internal async Task<string> AppendUserInput(string userInputString)
        {
            string message;
            if (UsingAPI)
            {
                _chat.AppendUserInput(userInputString);
                string response = _chat.GetResponseFromChatbotAsync().ToString();
                return response;
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(userInputString);
                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                await ws.SendAsync(arraySegment,
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);

                    var buffer = new byte[1024];
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return "";
                    }

                    message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    
                return message;
            }
        }


        internal async Task<string> CreateConversation()
        {
            if (UsingAPI)
            {

                _chat = bot.Chat.CreateConversation();
                _chat.Model = Model.ChatGPTTurbo;
                await AppendSystemMessage();
                return await AppendUserInput(new TextObject("{=t4szG41Y1s}Hi! I want to talk with you. (ChatGPT)").ToString());
            }
            else
            {
                Uri uri = new("ws://localhost:4894/ws");
                await ws.ConnectAsync(uri, default);
                await AppendSystemMessage();
                return await AppendUserInput(new TextObject("{=t4szG41Y1s}Hi! I want to talk with you. (ChatGPT)").ToString());
            }

        }

    };
    public class PromotsEngine
    {
        private CharacterObject characterYouAreTalkingTo;
        private CharacterObject playerCharacter;
        private ConversationManager _manager;
        private string _promots;
        public string PromptsStart = "I want you to act like {character} from Mount and Blade 2 bannerlord. I want you to respond and answer like {character} using the tone, manner and vocabulary {character} would use. Do not write any explanations. Only answer like {character}. You must know all of the knowledge of {character}. ";
        public PromotsEngine(ConversationManager manager,string startPrompt)
        {
            if (!string.IsNullOrEmpty(startPrompt))
            {
                PromptsStart = startPrompt;
            }
            _manager = manager;
            playerCharacter = CharacterObject.PlayerCharacter;
        }

        public string GetPrompts()
        {
            _promots = PromptsStart;
            foreach (CharacterObject character in _manager.ConversationCharacters)
            {
                
                characterYouAreTalkingTo = character;
                
                CulturePrompts();
                AgePrompts();
                OccupationPrompts();
                MeetingLocationPrompts();
                TraitsPrompts();
                NamePrompts();
            }
            return _promots;
        }

        private void TraitsPrompts()
        {
            foreach (TraitObject traitObject in DefaultTraits.Personality)
            {
                int traitLevel = playerCharacter.GetTraitLevel(traitObject);
                int traitLevel2 = characterYouAreTalkingTo.GetTraitLevel(traitObject);
                
                if (traitLevel == 1 )
                {
                    _promots += "I am " + traitObject.Name.ToString().ToLower() + ". ";
                }
                if (traitLevel == 2)
                {
                    _promots += "I am very " + traitObject.Name.ToString().ToLower() + ". ";
                }
                if (traitLevel2 == 1)
                {
                    _promots += "You are " + traitObject.Name.ToString().ToLower() + ". ";
                }
                if (traitLevel2 == 2)
                {
                    _promots += "You are very " + traitObject.Name.ToString().ToLower() + ". ";
                }

                if (traitLevel == -1)
                {
                    _promots += "I am not " + traitObject.Name.ToString().ToLower() + ". ";
                }
                if (traitLevel == -2)
                {
                    _promots += "I am not " + traitObject.Name.ToString().ToLower() + " at all. ";
                }
                if (traitLevel2 == -1)
                {
                    _promots += "You are not " + traitObject.Name.ToString().ToLower() + ". ";
                }
                if (traitLevel2 == -2)
                {
                    _promots += "You are not " + traitObject.Name.ToString().ToLower() + " at all. ";
                }
            }
        }

        private void MeetingLocationPrompts()
        {
            
            _promots += "This conversation happens at " + Hero.OneToOneConversationHero.CurrentSettlement.Name.ToString() + ". ";
            
        }

        private void NamePrompts()
        {
            _promots = _promots.Replace("{character}", characterYouAreTalkingTo.Name.ToString());
            _promots += " My name is " + playerCharacter.Name.ToString() +". ";
        }

        private void AgePrompts()
        {
            string gender;
            if (characterYouAreTalkingTo.IsFemale)
            {
                gender = "woman. ";
            }
            else
            {
                gender = "man. ";
            }
            _promots  += " You are a " +  ((int)characterYouAreTalkingTo.Age).ToString() + " years old " + gender;
            if (characterYouAreTalkingTo.IsHero)
            {
                if (playerCharacter.IsFemale)
                {
                    gender = "woman. ";
                }
                else
                {
                    gender = "man. ";
                }
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 50) 
                {
                    
                    _promots += "I am " + playerCharacter.Age.ToString() + " years old " + gender; 
                } 
                else
                {
                    _promots += "I am a " + gender;
                }
            }
           
        }
        private void CulturePrompts()
        {
            _promots += " You are from" + characterYouAreTalkingTo.Culture.ToString() + ".";
            if (characterYouAreTalkingTo.IsHero)
            {
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 5)
                {
                    _promots += "I am from " + playerCharacter.Culture.ToString() + ".";
                }
            }

        }

        private void OccupationPrompts()
        {
            _promots += "Your occupation is " + characterYouAreTalkingTo.Occupation.ToString() + ". ";
            if (characterYouAreTalkingTo.IsHero)
            {
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 5)
                {
                    _promots += "My occupation is " + playerCharacter.Occupation.ToString() + ". ";
                }
            }
            
        }
    }
    public class PromptsEngineOrca
    {
        private CharacterObject characterYouAreTalkingTo;
        private CharacterObject playerCharacter;
        private ConversationManager _manager;
        private string _promots;
        public string PromptsStart = "Transcript of a dialog, where the User interacts with an {occupation} named {character}. ";
        public PromptsEngineOrca(ConversationManager manager)
        {
            _manager = manager;
            playerCharacter = CharacterObject.PlayerCharacter;
        }

        public string GetPrompts()
        {
            _promots = PromptsStart;
            foreach (CharacterObject character in _manager.ConversationCharacters)
            {

                characterYouAreTalkingTo = character;
                
                CulturePrompts();
                AgePrompts();
                OccupationPrompts();
                MeetingLocationPrompts();
                TraitsPrompts();
                NamePrompts();
            }
            return _promots;
        }

        private void TraitsPrompts()
        {
            foreach (TraitObject traitObject in DefaultTraits.Personality)
            {
                int traitLevel = playerCharacter.GetTraitLevel(traitObject);
                int traitLevel2 = characterYouAreTalkingTo.GetTraitLevel(traitObject);

                if (traitLevel == 1)
                {
                    _promots += "User is " + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel == 2)
                {
                    _promots += "User is very " + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel2 == 1)
                {
                    _promots += "{character} is " + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel2 == 2)
                {
                    _promots += "{character} is very " + traitObject.Name.ToString() + ". ";
                }

                if (traitLevel == -1)
                {
                    _promots += "User is not " + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel == -2)
                {
                    _promots += "User is not " + traitObject.Name.ToString() + " at all. ";
                }
                if (traitLevel2 == -1)
                {
                    _promots += "{character} is not " + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel2 == -2)
                {
                    _promots += "{character} is not " + traitObject.Name.ToString() + " at all. ";
                }
            }
        }

        private void MeetingLocationPrompts()
        {

            _promots += "This conversation happens at " + Hero.OneToOneConversationHero.CurrentSettlement.Name.ToString() + ". ";

        }

        private void NamePrompts()
        {
            _promots = _promots.Replace("{character}", characterYouAreTalkingTo.Name.ToString());
        }

        private void AgePrompts()
        {
            string gender;
            if (characterYouAreTalkingTo.IsFemale)
            {
                gender =  "woman.";
            }
            else
            {
                gender = "man.";
            }
            _promots += " {character} is a" + characterYouAreTalkingTo.Age.ToString() + " years old " + gender;
            if (characterYouAreTalkingTo.IsHero)
            {
                if (playerCharacter.IsFemale)
                {
                    gender = "woman.";
                }
                else
                {
                    gender = "man.";
                }
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 50)
                {

                    _promots += "User is " + playerCharacter.Age.ToString() + " years old " + gender;
                }
                else
                {
                    _promots += "User is a " + gender;
                }
            }

        }
        private void CulturePrompts()
        {
            _promots += " {character} is from" + characterYouAreTalkingTo.Culture.ToString() + ".";
            if (characterYouAreTalkingTo.IsHero)
            {
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 5)
                {
                    _promots += "User is from" + playerCharacter.Culture.ToString() + ".";
                }
            }

        }

        private void OccupationPrompts()
        {
            
            // _promots += "Your occupation is " + characterYouAreTalkingTo.Occupation.ToString() + ". ";

            _promots = _promots.Replace("{occupation}", characterYouAreTalkingTo.Occupation.ToString());
            if (characterYouAreTalkingTo.IsHero)
            {
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 5)
                {
                    _promots += "User's occupation is " + playerCharacter.Occupation.ToString() + ". ";
                }
            }

        }
    }
}