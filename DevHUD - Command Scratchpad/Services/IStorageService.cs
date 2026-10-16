using System.Collections.Generic;
using DevHUD___Command_Scratchpad.Models;

namespace DevHUD___Command_Scratchpad.Services
{
    public interface IStorageService
    {
        AppConfig LoadConfig();
        void SaveConfig(AppConfig config);
        void CreateDailySnapshot(AppConfig config);
        bool ProcessDailyHabitRespawn(AppConfig config);
        List<TabModel> GetSeedData();
    }
}
