﻿using GenshinDB_Core.Types;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

using DBEnv = GenshinDB_Core.DBEnvironment;

namespace GenshinDB_Core
{
    public class GenshinDB
    {
        public enum Locations { Mondstadt, Liyue }
        public enum ElementTypes { Pyro, Hydro, Dendro, Electro, Anemo, Cryo, Geo }


        const string EMBED_NAMESPACE = "GenshinDB_Core.DB.";

        public List<Character> characters;
        public List<TalentItem> talentItems;
        public List<WeaponAscensionItem> weaponAscensionItems;
        public List<Lang> langs;

        public DataTable langDT;

        public GenshinDB(CultureInfo cultureInfo = null)
        {
            DBEnv.dbCultureInfo = cultureInfo ?? CultureInfo.CurrentCulture;

            characters = new List<Character>();
            talentItems = new List<TalentItem>();
            weaponAscensionItems = new List<WeaponAscensionItem>();
            langs = new List<Lang>();

            LoadDB();
        }

        private void LoadDB()
        {
            using var characterDT = new DataTable();
            using var talentItemDT = new DataTable();
            using var weaponAscensionDT = new DataTable();
            using var langDT = new DataTable();

            var assembly = Assembly.GetExecutingAssembly();

            using var characterStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Character.xml");
            using var talentItemStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Item_Talent.xml");
            using var weaponAscensionStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Item_Weapon_Ascension.xml");
            using var langStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Lang.xml");

            characterDT.ReadXml(characterStream);
            talentItemDT.ReadXml(talentItemStream);
            weaponAscensionDT.ReadXml(weaponAscensionStream);
            langDT.ReadXml(langStream);

            foreach (DataRow dr in characterDT.Rows)
            {
                characters.Add(new Character(dr));
            }

            foreach (DataRow dr in talentItemDT.Rows)
            {
                talentItems.Add(new TalentItem(dr));
            }
            foreach (DataRow dr in weaponAscensionDT.Rows)
            {
                weaponAscensionItems.Add(new WeaponAscensionItem(dr));
            }
            foreach (Locations location in Enum.GetValues(typeof(Locations)))
            {
                talentItems.Add(new TalentItem(location, new[] { DayOfWeek.Sunday }));
                weaponAscensionItems.Add(new WeaponAscensionItem(location, new[] { DayOfWeek.Sunday }));
            }

            foreach (DataRow dr in langDT.Rows)
            {
                langs.Add(new Lang(dr));
            }
        }

        public static string[] GetResources()
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceNames();
        }

        public string GetLocationName(Locations location)
        {
            return location switch
            {
                Locations.Mondstadt => "Mondstadt",
                Locations.Liyue => "Liyue",
                _ => string.Empty
            };
        }

        public List<string> GetAllLocations()
        {
            //const string CINDEX = "Name";

            var langIndex = GetLangIndex();

            return new List<string>
            {
                (from lang in langs where lang.Name.Equals("Mondstadt") select lang).First().Dic[langIndex],
                (from lang in langs where lang.Name.Equals("Liyue") select lang).First().Dic[langIndex]
            };
        }

        public string FindLangDic(string name) => langs.Find(x => x.Name.Equals(name)).Dic[GetLangIndex()];

        private string GetLangIndex()
        {
            return DBEnv.dbCultureInfo.Name switch
            {
                "ko-KR" => "ko",
                _ => "Default"
            };
        }
    }
}
