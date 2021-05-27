using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{
    public delegate void VAGChoiceEventHandler(VAGChoicesUI sender, int choiceIndex);
    public class VAGUIChoiceButton : VAMUIButton
    {
        public int ChoiceIndex;
        public VAGUIChoiceButton(VAGChoicesUI ownerWnd, string name, string caption = "", float width = 100, float height = 35) : base(ownerWnd, name, caption, width, height) { }
    }
    public class VAGChoicesUI : VAMCustomVerticalLayoutUIWnd
    {
        public event VAGChoiceEventHandler OnChoiceSelected;
        private VAGChoicesCollection _activeChoices;
        public VAGChoicesCollection ActiveChoices { set { SetActiveChoices(value); } get { return _activeChoices; } }
        protected List<VAGUIChoiceButton> ButtonList;
        public VAGChoicesUI(MVRScript plugin) : base(plugin, 500)
        {
            ButtonList = new List<VAGUIChoiceButton>();
            SetUIInteractive();

            AutoHeight = true;
        }
        protected void HideAllButtons()
        {
            BeginUpdate();
            try
            {
                for (int i = 0; i < ButtonList.Count; i++)
                {
                    ButtonList[i].Visible = false;
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        protected virtual void DoOnChoiceSelected(int choiceIndex)
        {
            if (ActiveChoices != null)
            {
                if (OnChoiceSelected != null)
                {
                    OnChoiceSelected(this, choiceIndex);
                }
                ActiveChoices.ExecuteChoosen((OwnerPlugin as VAGPlugin).QuestHandler, choiceIndex);
            }
        }
        protected VAGUIChoiceButton CreateButton()
        {
            VAGUIChoiceButton btn = new VAGUIChoiceButton(this, "");

            btn.button.button.onClick.AddListener(() =>
            {
                DoOnChoiceSelected(btn.ChoiceIndex);
            });

            ButtonList.Add(btn);
            return btn;
        }

        public void SetActiveChoices(VAGChoicesCollection choices)
        {
            _activeChoices = choices;
            VAGUIChoiceButton btn;

            BeginUpdate();
            try
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    if (i >= ButtonList.Count) { btn = CreateButton(); } else { btn = ButtonList[i]; }
                    btn.button.buttonText.text = choices.ByIndex(i).ButtonText;
                    btn.ChoiceIndex = i;
                    btn.Visible = true;
                }

                for (int i = choices.Count; i < ButtonList.Count; i++)
                {
                    ButtonList[i].Visible = false;
                }

            }
            finally
            {
                EndUpdate();
            }
        }
    }

    public class VAGSettingsUI : VAMCustomVerticalLayoutUIWnd
    {
        public readonly VAMUIButton btnStopStart;

        public VAGSettingsUI(VAGPlugin plugin) : base(plugin, 200)
        {
            SetUIInteractive();

            btnStopStart = new VAMUIButton(this, "btn1", "Start");
            btnStopStart.button.button.onClick.AddListener(() =>
            {
                plugin.QuestHandler.Active = !plugin.QuestHandler.Active;
                UpdateStartStopBtn();
            });

            AutoHeight = true;
        }
        protected override void SetVisible(bool value)
        {
            if (value)
            {
                UpdateStartStopBtn();
            }
            base.SetVisible(value);
        }

        private void UpdateStartStopBtn()
        {
            if ((OwnerPlugin as VAGPlugin).QuestHandler.Active)
            {
                btnStopStart.button.buttonText.text = "Pause";
            }
            else
            {
                btnStopStart.button.buttonText.text = "Continue";
            }
        }
    }
    public class VAMMainMenuDialogUI : VAMCustomHorizontalLayoutUIWnd
    {
        private readonly float _buttonHeight = 40;
        public readonly VAMUIWindowPopUpButton btnChoicesUI;
        public readonly VAMUIWindowPopUpButton btnInventoryUI;
        public readonly VAMUIWindowPopUpButton btnQuestsUI;
        public readonly VAMUIWindowPopUpButton btnMapUI;
        public readonly VAMUIWindowPopUpButton btnSettingsUI;
        public readonly VAGChoicesUI ChoicesUI;
        public readonly VAGSettingsUI SettingsUI;

        public VAMMainMenuDialogUI(VAGPlugin plugin) : base(plugin, 400, 40)
        {
            SetUIInteractive();

            ChoicesUI = new VAGChoicesUI(plugin);
            btnChoicesUI = new VAMUIWindowPopUpButton(this, ChoicesUI, "btnChoicesUI", "Talk", 80, _buttonHeight);

            SettingsUI = new VAGSettingsUI(plugin);
            btnSettingsUI = new VAMUIWindowPopUpButton(this, SettingsUI, "BtnSettingsUI", "Game", 80, _buttonHeight);

            /*
            btnInventoryUI = new VAMUIWindowPopUpButton(this, "BtnInventoryUI", "Inventory");
            btnInventoryUI.button.button.onClick.AddListener(() =>
            {

            });

            btnQuestsUI = new VAMUIWindowPopUpButton(this, "btnQuestsUI", "Quests");
            btnQuestsUI.button.button.onClick.AddListener(() =>
            {

            });

            btnMapUI = new VAMUIWindowPopUpButton(this, "btnMapUI", "Map");
            btnMapUI.button.button.onClick.AddListener(() =>
            {

            });            


            });
            */
        }
    }

}