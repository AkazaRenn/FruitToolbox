using System;

namespace FruitLanguageSwitcher
{
    internal class Constants
    {
        public const string AppID = "AkazaRenn.82975CBC0BB1_fhf2jh1qk9hx4!App";

        public delegate void EventHandler(object sender, LanguageEvent e);

        public class LanguageEvent: EventArgs
        {
            public LanguageEvent(int lcid)
            {
                LCID = lcid;
            }

            public int LCID { get; }
        }
    }
}
