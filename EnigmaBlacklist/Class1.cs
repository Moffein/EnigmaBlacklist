using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using RoR2;
using RoR2.Artifacts;
using UnityEngine;

namespace EnigmaBlacklist
{
    [BepInPlugin("com.Moffein.EnigmaBlacklist", "Enigma Blacklist", "1.0.1")]
    public class EnigmaBlacklist : BaseUnityPlugin
    {
        public static bool blacklistLunars = true;
        public static bool useWhitelist = false;
        public static HashSet<EquipmentIndex> blacklist = new HashSet<EquipmentIndex>();
        public static HashSet<EquipmentIndex> whitelist = new HashSet<EquipmentIndex>();

        public static string blacklistString = "";
        public static string whitelistString = "";

        private void ReadConfig()
        {
            blacklistLunars = Config.Bind("Settings", "Blacklist Lunars", true, "Prevent Lunars from being rolled.").Value;
            blacklistString = Config.Bind("Settings", "Blacklist String", "Recycle, Tonic, BossHunter, BossHunterConsumed", "Equipments to blacklist. Separated by comma, case-sensitive. Full list can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names").Value;
            useWhitelist = Config.Bind("Settings", "Use Whitelist", false, "Use a whitelist instead of a blacklist. Disables Blacklist String setting.").Value;
            whitelistString = Config.Bind("Settings", "Whitelist String", "", "Equipments to whitelist. Separated by comma, case-sensitive. Full list can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names").Value;
        }

        public void Awake()
        {
            ReadConfig();

            On.RoR2.EquipmentCatalog.Init += (orig) =>
            {
                orig();

                blacklistString = new string(blacklistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitEquipBlacklist = blacklistString.Split(',');
                foreach (string str in splitEquipBlacklist)
                {
                    EquipmentIndex ei = EquipmentCatalog.FindEquipmentIndex(str);
                    if (ei != EquipmentIndex.None)
                    {
                        blacklist.Add(ei);
                    }
                }

                whitelistString = new string(whitelistString.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
                string[] splitEquipwhitelist = whitelistString.Split(',');
                foreach (string str in splitEquipwhitelist)
                {
                    EquipmentIndex ei = EquipmentCatalog.FindEquipmentIndex(str);
                    if (ei != EquipmentIndex.None)
                    {
                        whitelist.Add(ei);
                    }
                }
            };

            On.RoR2.Artifacts.EnigmaArtifactManager.OnRunStartGlobal += (orig, run) =>
            {
                orig(run);

                List<EquipmentIndex> toRemove = new List<EquipmentIndex>();
                foreach (EquipmentIndex ei in EnigmaArtifactManager.validEquipment)
                {
                    if (!Run.instance.availableEquipment.Contains(ei) || ((useWhitelist && !whitelist.Contains(ei)) || (!useWhitelist && blacklist.Contains(ei))))
                    {
                        toRemove.Add(ei);
                    }
                    else if (blacklistLunars)
                    {
                        EquipmentDef ed = EquipmentCatalog.GetEquipmentDef(ei);
                        if (ed.isLunar)
                        {
                            toRemove.Add(ei);
                        }
                    }
                }
                foreach (EquipmentIndex ei in toRemove)
                {
                    EnigmaArtifactManager.validEquipment.Remove(ei);
                }
            };
        }
    }
}
