﻿using System.Collections.Generic;
using System.Windows.Forms;
using TaleWorlds.Library;

namespace CustomSpawns.Utils
{
    public class MessageBoxService
    {
        private readonly Dictionary<string, int> _numberOfTimesShown = new();

        public void ShowCustomSpawnsErrorMessage(System.Exception? e, string during = "")
        {
            string duringMessage = "";
            string errorMessage = "";
            if (!string.IsNullOrWhiteSpace(during))
            {
                duringMessage = "DURING " + during + " ";
            }
            if (e != null)
            {
                errorMessage = e.Message + " AT " + e.Source + " " + duringMessage + "TRACE: " + e.StackTrace;
            }
            string shown = "CustomSpawns error has occured, please report to mod developer: " + errorMessage;
            ShowMessage(shown);
        }

        public void ShowMessage(string errorMessage)
        {
            if (!_numberOfTimesShown.ContainsKey(errorMessage))
            {
                _numberOfTimesShown.Add(errorMessage, 1);
            }
            else
            {
                _numberOfTimesShown[errorMessage]++;
            }

            if (_numberOfTimesShown[errorMessage] > 1)
            {
                UX.ShowMessage(errorMessage, Color.ConvertStringToColor(UX.GetMessageColour("error")));
            }
            else
            {
                MessageBox.Show(errorMessage);
            }
        }

    }
}
