
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;

namespace Bannerlord.ChatGPT
{

    public class PromotsEngine
    {
        private CharacterObject characterYouAreTalkingTo;
        private CharacterObject playerCharacter;
        private ConversationManager _manager;
        private string _promots;
        String PromptsStart = "  Your response is better to be within 30 words. I want you to act like {character} from Mount and Blade 2 bannerlord. I want you to respond and answer like {character} using the tone, manner and vocabulary {character} would use. Do not write any explanations. Only answer like {character}. You must know all of the knowledge of {character}. ";
        public PromotsEngine(ConversationManager manager)
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
                NamePromots();
                CulturePromots();
                AgePromots();
                OccupationPromots();
                MeetingLocationPromots();
                TraitsPromots();  
            }
            return _promots;
        }

        private void TraitsPromots()
        {
            foreach (TraitObject traitObject in DefaultTraits.Personality)
            {
                int traitLevel = playerCharacter.GetTraitLevel(traitObject);
                int traitLevel2 = characterYouAreTalkingTo.GetTraitLevel(traitObject);
                
                if (traitLevel == 1 )
                {
                    _promots += "I am" + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel == 2)
                {
                    _promots += "I am very" + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel2 == 1)
                {
                    _promots += "You are " + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel2 == 2)
                {
                    _promots += "You are very" + traitObject.Name.ToString() + ". ";
                }

                if (traitLevel == -1)
                {
                    _promots += "I am not" + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel == -2)
                {
                    _promots += "I am not" + traitObject.Name.ToString() + "at all. ";
                }
                if (traitLevel2 == -1)
                {
                    _promots += "You are not" + traitObject.Name.ToString() + ". ";
                }
                if (traitLevel2 == -2)
                {
                    _promots += "You are not" + traitObject.Name.ToString() + "at all. ";
                }
            }
        }

        private void MeetingLocationPromots()
        {
            
            _promots += "This conversation happens at " + Hero.OneToOneConversationHero.CurrentSettlement.Name.ToString() + ". ";
            
        }

        private void NamePromots()
        {
            _promots = _promots.Replace("{character}", characterYouAreTalkingTo.Name.ToString());
        }

        private void AgePromots()
        {
            string gender;
            if (characterYouAreTalkingTo.IsFemale)
            {
                gender = "woman.";
            }
            else
            {
                gender = "man.";
            }
            _promots  += " You are a" + characterYouAreTalkingTo.Age.ToString() + " years old " + gender;
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
                    
                    _promots += "I am " + playerCharacter.Age.ToString() + " years old " + gender; 
                } 
                else
                {
                    _promots += "I am a " + gender;
                }
            }
           
        }
        private void CulturePromots()
        {
            _promots += " You are from" + characterYouAreTalkingTo.Culture.ToString() + ".";
            if (characterYouAreTalkingTo.IsHero)
            {
                if (characterYouAreTalkingTo.HeroObject.HasMet && characterYouAreTalkingTo.HeroObject.GetRelationWithPlayer() > 5)
                {
                    _promots += "I am from" + playerCharacter.Culture.ToString() + ".";
                }
            }

        }

        private void OccupationPromots()
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
}