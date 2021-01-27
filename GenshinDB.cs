﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;

using DBEnv = GenshinDB_Core.DBEnvironment;

namespace GenshinDB_Core
{
    public class GenshinDB
    {
        public enum Locations { Mondstadt, Liyue }

        const string EMBED_NAMESPACE = "GenshinDB_Core.DB.";

        public List<Character> characters;
        public List<TalentItem> talentItems;
        public List<Lang> langs;

        public DataTable langDT;

        public GenshinDB()
        {
            characters = new List<Character>();
            talentItems = new List<TalentItem>();
            langs = new List<Lang>();

            LoadDB();
        }

        private void LoadDB()
        {
            var characterDT = new DataTable();
            var talentItemDT = new DataTable();
            var langDT = new DataTable();

            var assembly = Assembly.GetExecutingAssembly();
            
            using var characterStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Character.xml");
            using var talentItemStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Item_Talent.xml");
            using var langStream = assembly.GetManifestResourceStream($"{EMBED_NAMESPACE}Lang.xml");

            characterDT.ReadXml(characterStream);
            talentItemDT.ReadXml(talentItemStream);
            langDT.ReadXml(langStream);

            foreach (DataRow dr in characterDT.Rows)
            {
                characters.Add(new Character(dr));
            }
            foreach (DataRow dr in talentItemDT.Rows)
            {
                talentItems.Add(new TalentItem(dr));
            }
            foreach (DataRow dr in langDT.Rows)
            {
                langs.Add(new Lang(dr));
            }
        }

        public string[] GetResources()
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

        private string GetLangIndex()
        {
            return CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
            {
                "ko" => "ko",
                _ => "Default"
            };
        }
    }
}