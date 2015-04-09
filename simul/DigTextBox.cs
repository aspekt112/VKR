using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace simul
{
    public class DigTextBox : TextBox
    {
        public DigTextBox()
            : base()
        {
 
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.PageUp:
                case Keys.PageDown:
                    e.SuppressKeyPress = false;
                    return;
              
                    
            }

            char currentKey = (char)e.KeyCode;
            bool modifier = e.Control || e.Alt || e.Shift;
            bool nonNumber = char.IsLetter(currentKey) || char.IsSymbol(currentKey) || char.IsWhiteSpace(currentKey) || char.IsPunctuation(currentKey);

            if (!modifier && nonNumber)
                e.SuppressKeyPress = true;

            
            if (e.Control && e.KeyCode == Keys.V)
            {
             
                string pasteText = Clipboard.GetText();
                string strippedText = "";
                for (int i = 0; i < pasteText.Length; i++)
                {
                    if (char.IsDigit(pasteText[i]))
                        strippedText += pasteText[i].ToString();
                }

                if (strippedText != pasteText)
                {
                  
                    e.SuppressKeyPress = true;

                   
                    TextBox me = (TextBox)this;
                    int start = me.SelectionStart;
                    string newTxt = me.Text;
                    newTxt = newTxt.Remove(me.SelectionStart, me.SelectionLength); 
                    newTxt = newTxt.Insert(me.SelectionStart, strippedText); 
                    me.Text = newTxt;
                    me.SelectionStart = start + strippedText.Length;

                }
                else
                    e.SuppressKeyPress = false;
            }

        }

    }
}
