﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using CustomSpawns.Utils;
using TaleWorlds.Library;

namespace CustomSpawns.CampaignData.Config
{
    class CampaignDataConfigLoader
    {

        private static CampaignDataConfigLoader _instance = null;

        private static Dictionary<Type, object> typeToConfig = new Dictionary<Type, object>();

        public static CampaignDataConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CampaignDataConfigLoader();
                }

                return _instance;
            }
        }

        private CampaignDataConfigLoader()
        {
            string path = "";
            path = Path.Combine(BasePath.Name, "Modules", Main.ModuleName, "ModuleData", "custom_spawns_campaign_data_config.xml");
            ConstructConfigs(path);
        }

        private void ConstructConfigs(string xmlPath)
        {
            var type = typeof(ICampaignDataConfig);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(p => type.IsAssignableFrom(p) && p != type);

            try
            {

                XDocument xDocument = XDocument.Load(xmlPath);

                foreach(var t in types) //doing it this way to detec errors/missing for specific types.
                {

                    bool processed = false;

                    foreach(var ele in xDocument.Root.Elements())
                    {
                        if(ele.Name.LocalName.ToString() == t.Name)
                        {
                            var config = DeserializeNode(ele, t);
                            typeToConfig.Add(t, config);
                            processed = true;
                        }
                    }

                    if (!processed)
                    {
                        ErrorHandler.ShowPureErrorMessage("Could not find Campaign Data config file for type " + t.Name);
                    }
                }
            }
            catch (System.Exception e)
            {
                ErrorHandler.HandleException(e, "CAMPAIGN DATA XML READING");
            }

        }

        private static object DeserializeNode(XElement data, Type t) 
        {
            if (data == null)
                return null;

            var ser = new XmlSerializer(t);
            return ser.Deserialize(data.CreateReader());
        }

        public T GetConfig<T>() where T: class, ICampaignDataConfig, new()
        {
            if (typeToConfig.ContainsKey(typeof(T)))
            {
                return typeToConfig[typeof(T)] as T;
            }

            return null;
        }
    }

}

