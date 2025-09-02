using Verse;

namespace PersistentDate
{
    public class PersistentDate_Mod : Mod
    {
        public PersistentDate_ModSettings Settings => GetSettings<PersistentDate_ModSettings>();
        
        public PersistentDate_Mod(ModContentPack content) : base(content)
        {
        }

        public override string SettingsCategory() => "Persistent Date";
    }

    public class PersistentDate_ModSettings : ModSettings
    {
        
    }
}