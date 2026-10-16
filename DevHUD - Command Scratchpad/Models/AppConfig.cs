using System;
using System.Collections.Generic;

namespace DevHUD___Command_Scratchpad.Models
{
    public class AppConfig
    {
        public DateTime LastActiveDate { get; set; } = DateTime.Today;
        public string ActiveTabId { get; set; } = string.Empty;
        public int DefaultPomodoroMinutes { get; set; } = 25;
        public double GhostOpacity { get; set; } = 0.75;
        public bool SoundEnabled { get; set; } = true;
        public double WindowLeft { get; set; } = 150;
        public double WindowTop { get; set; } = 100;
        public double WindowWidth { get; set; } = 960;
        public double WindowHeight { get; set; } = 640;
        public string Language { get; set; } = "vi";
        public List<TabModel> Tabs { get; set; } = new();
    }
}
