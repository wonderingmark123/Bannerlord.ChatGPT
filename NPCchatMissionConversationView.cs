using SandBox.View.Missions;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;


namespace Bannerlord.ChatGPT
{
    public class NPCchatMissionConversationView : MissionView
    {
        private NPCchatMissionChatVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private ConversationManager _conversationManager;
        private MissionConversationCameraView _conversationCameraView;
        private MissionGauntletEscapeMenuBase _escapeView;
        private SpriteCategory _conversationCategory;
        public string conversationID;

        public bool OnAIchat = false;
        public NPCchatMissionConversationView() => this.ViewOrderPriority = 100;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            
            conversationID = null;
        }
        public override void OnConversationBegin()
        {
            base.OnConversationBegin();
            LoadLayerAndInitializeVM();
            

        }
        public override void OnConversationEnd()
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _dataSource.IsChating = false;
            _gauntletLayer = null;
            _conversationCameraView = null;
            _dataSource = null;
            
            base.OnConversationEnd();
            



        }
        public override void OnMissionScreenFinalize()
        {
            
            
            base.OnMissionScreenFinalize();


        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            MissionGauntletEscapeMenuBase escapeView = this._escapeView;
            if (Input.IsKeyPressed(InputKey.Tab))
            {
                var a = 1;
                }

            if (_conversationManager != null && _gauntletLayer != null && _gauntletLayer.Input != null && _dataSource.IsChating == true)
            {
                if (Input.IsKeyPressed(InputKey.RightMouseButton) || this._gauntletLayer.Input.IsKeyDown(InputKey.RightMouseButton))
                    {
                        this._dataSource.PreviousPage();
                        return;

                    }
                if (Input.IsKeyPressed(InputKey.LeftMouseButton) || this._gauntletLayer.Input.IsKeyDown(InputKey.LeftMouseButton))
                {
                    this._dataSource.NextPage();
                    return;

                }
                if (Input.IsKeyPressed(InputKey.Tab) || this.IsGameKeyReleasedInAnyLayer("Leave", true) || this._gauntletLayer.Input.IsKeyDown(InputKey.Tab))
                {
                    this._dataSource.ExitChating();
                    return;
                    
                }
                else if (Input.IsKeyDown(InputKey.Enter) || this.IsGameKeyReleasedInAnyLayer("Confirm", true) || _gauntletLayer.Input.IsKeyDown(InputKey.Enter))
                {
                    if (_dataSource.GetType() == typeof(NPCchatMissionChatVM) && !_dataSource.isResponding)
                    {
                        
                        _dataSource.SendPlayerInputAsync();
                        
                    }
                }
            }

            if ((escapeView != null ? (!escapeView.IsActive ? 1 : 0) : 1) == 0 || _gauntletLayer == null)
                return;
            NPCchatMissionChatVM dataSource1 = _dataSource;


        }

        public void UpdateChatStatus(bool isChating)
        {
            _dataSource.IsChating = isChating;
            if ( isChating && !_dataSource._isBotStarted)
            {
                _dataSource.StartBotAsync();
            }
        }

        public void LoadLayerAndInitializeVM()
        {
            MissionScreen.SetConversationActive(true);
            SpriteData spriteData = UIResourceManager.SpriteData;
            TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
            ResourceDepot uiResourceDepot = UIResourceManager.UIResourceDepot;
            _conversationCategory = spriteData.SpriteCategories["ui_conversation"];
            _conversationCategory.Load((ITwoDimensionResourceContext)resourceContext, uiResourceDepot);
            _dataSource = new NPCchatMissionChatVM(new Func<string>(this.GetContinueKeyText));
            _gauntletLayer = new GauntletLayer(this.ViewOrderPriority, "Conversation");
            _gauntletLayer.LoadMovie("NPCChatConversation", (ViewModel)this._dataSource);
            GameKeyContext category = HotKeyManager.GetCategory("ConversationHotKeyCategory");
            _gauntletLayer.Input.RegisterHotKeyCategory(category);
            if (!MissionScreen.SceneLayer.Input.IsCategoryRegistered(category))
                MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(category);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions();
            this._escapeView = this.Mission.GetMissionBehavior<MissionGauntletEscapeMenuBase>();
            this.MissionScreen.AddLayer((ScreenLayer)this._gauntletLayer);
            this.MissionScreen.SetLayerCategoriesStateAndDeactivateOthers(new string[2]
            {
        "Conversation",
        "SceneLayer"
            }, true);
            ScreenManager.TrySetFocus((ScreenLayer)this._gauntletLayer);
            this._conversationManager = Campaign.Current.ConversationManager;
            InformationManager.ClearAllMessages();
        }


        private string GetContinueKeyText()
        {
            GameTexts.SetVariable("CONSOLE_KEY_NAME", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("ConversationHotKeyCategory", "ContinueKey")));
            return GameTexts.FindText("str_click_to_continue_console").ToString();
        }


        private bool IsGameKeyReleasedInAnyLayer(string hotKeyID, bool isDownAndReleased) => this.IsReleasedInSceneLayer(hotKeyID, isDownAndReleased) | this.IsReleasedInGauntletLayer(hotKeyID, isDownAndReleased);

        private bool IsReleasedInSceneLayer(string hotKeyID, bool isDownAndReleased)
        {
            if (isDownAndReleased)
            {
                SceneLayer sceneLayer = this.MissionScreen.SceneLayer;
                return sceneLayer != null && sceneLayer.Input.IsHotKeyDownAndReleased(hotKeyID);
            }
            SceneLayer sceneLayer1 = this.MissionScreen.SceneLayer;
            return sceneLayer1 != null && sceneLayer1.Input.IsHotKeyReleased(hotKeyID);
        }

        private bool IsReleasedInGauntletLayer(string hotKeyID, bool isDownAndReleased)
        {
            if (isDownAndReleased)
            {
                GauntletLayer gauntletLayer = this._gauntletLayer;
                return gauntletLayer != null && gauntletLayer.Input.IsHotKeyDownAndReleased(hotKeyID);
            }
            GauntletLayer gauntletLayer1 = this._gauntletLayer;
            return gauntletLayer1 != null && gauntletLayer1.Input.IsHotKeyReleased(hotKeyID);
        }
    }
}