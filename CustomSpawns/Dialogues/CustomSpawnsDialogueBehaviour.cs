﻿using CustomSpawns.Data.Model;
using CustomSpawns.Data.Reader.Impl;
using CustomSpawns.Dialogues.DialogueAlgebra;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;

namespace CustomSpawns.Dialogues
{
    //TODO Improve upon delegate logic. add more options. 
    public class CustomSpawnsDialogueBehaviour : CampaignBehaviorBase
    {
        private readonly DialogueDataReader _dialogueDataReader;

        public CustomSpawnsDialogueBehaviour(DialogueDataReader dialogueDataReader)
        {
            _dialogueDataReader = dialogueDataReader;
        }
        
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, AddCustomDialogues);
            DialogueManager.CustomSpawnsDialogueBehaviour = this;
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        // basic dialogue overview: 
        // ids are used to differentiate similar dialogues (must be unique)
        // tokens are like impulses, so an out token leads into an in token. 'magic tokens' are tokens which have special functions, they are start (in token) and close_window (out token)
        // conditions are delegates that must anonymously return a bool, which will be wheteher or not the line is displayed
        // consequences are void delegates, just pieces of code run after a line has been selected
        // i think? priority determines which line is shown if multiple lines meet the requirements,
        // and also maybe what order player lines are displayed in (speculation)
        // -ComradeCheekiBreeki

        //It seems that the first condition that is met is run, and all after it are ignored.
        //The higher the priority (higher number), the more likelihood it has of being run first.
        //However, there also seems to be an option to turn off this sorting of conditions based on priority through
        //ConversationManager.[Enable/Disable]Sort(). Sorting seems to be enabled by default in the time of writing (Bannerlord 1.5.8)
        //-Ozan

        public void AddCustomDialogues(CampaignGameStarter starter)
        {
            foreach (Dialogue d in _dialogueDataReader.Data) // handle the dialogues
            {
                AddDialogLine(starter, d, "start");
            }
        }

        private void AddDialogLine(CampaignGameStarter starter, Dialogue d, string in_token)
        {

            if (d.InjectedToTaleworlds)
            {
                return;
            }

            d.InjectedToTaleworlds = true;

            ConversationSentence.OnConditionDelegate cond = null;

            if(d.Condition != null)
            {
                cond = delegate
                {
                    return EvalulateDialogueCondition(d.Condition);
                };
            }

            ConversationSentence.OnConsequenceDelegate conseq = null;

            if(d.Consequence != null)
            {
                conseq = delegate
                {
                    ExecuteDialogueConsequence(d.Consequence);
                };
            }

            if (d.IsPlayerDialogue)
            {
                starter.AddPlayerLine(d.Dialogue_ID,
                    in_token,
                    d.Children.Count == 0? "close_window" : d.Dialogue_ID,
                    d.DialogueText,
                    cond,
                    conseq,
                    int.MaxValue
                    );
            }
            else
            {
                starter.AddDialogLine(d.Dialogue_ID,
                    in_token,
                    d.Children.Count == 0 ? "close_window" : d.Dialogue_ID,
                    d.DialogueText,
                    cond,
                    conseq,
                    int.MaxValue
                    );
            }

            foreach(Dialogue child in d.Children)
            {
                AddDialogLine(starter, child, d.Dialogue_ID);
            }
        }



        private DialogueParams CurrentDialogueParam //TODO Cache in same frame?
        {
            get
            {
                return new ()
                {
                    AdversaryParty = PlayerEncounter.EncounteredParty?.MobileParty,
                    PlayerParty = Hero.MainHero?.PartyBelongedTo
                };
            }
        }

        private bool EvalulateDialogueCondition(DialogueCondition condition)
        {
            return condition.ConditionEvaluator(CurrentDialogueParam);
        }

        private void ExecuteDialogueConsequence(DialogueConsequence consequence)
        {
            consequence.ConsequenceExecutor(CurrentDialogueParam);
        }
    }
}
