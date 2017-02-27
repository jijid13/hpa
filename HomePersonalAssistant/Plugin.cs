using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

#if MICRO
using System.Speech.Recognition;
#endif

#if KINECT
using Microsoft.Speech.Recognition;
#endif

namespace HomePersonalAssistant
{
    internal class Plugin
    {
        public string _idPlugin;

        public Plugin(String IdPlugin)
        {
            this._idPlugin = IdPlugin;
        }

        public static Dictionary<string, Plugin> loadPlugins(SpeechRecognitionEngine P_speechEngine)
        {
            var _PluginsList = new Dictionary<string, Plugin>();
            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\plugins")) {
                string _pluginDirectory = Directory.GetCurrentDirectory() + @"\plugins";

                //Logger.Info("plugins");
                try
                {
                    var DirName = string.Empty;
                    Plugin _plugin;
                    foreach (string d in Directory.GetDirectories(_pluginDirectory))
                    {
                        try
                        {
                            DirName = d.Remove(0, _pluginDirectory.Length + 1);
                            if (File.Exists(d + @"\index.js"))
                            {
                                //Logger.Info("loading plugin : " + DirName);
                                P_speechEngine.LoadGrammar(new Grammar(_pluginDirectory + @"\" + DirName + @"\grammar.xml", DirName));
                                //Logger.Info("grammar loaded " + DirName);
                                Console.WriteLine("grammar loaded " + DirName);

                                _plugin = new Plugin(DirName);
                                _PluginsList.Add(DirName, _plugin);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            //Logger.Error(ex.Message);
                        }
                    }

                    if (File.Exists(Directory.GetCurrentDirectory() + @"\HPA.xml"))
                    {
                        XmlDocument GrammarXmlDoc = getGrammarXMLFile(Directory.GetCurrentDirectory() + @"\HPA.xml");
                        var stream = new MemoryStream();
                        GrammarXmlDoc.Save(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        P_speechEngine.LoadGrammar(new Grammar(stream, "HPA"));
                        Console.WriteLine("grammar loaded HPA");
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                    //Logger.Error(excpt.Message);
                }
            }
            return _PluginsList;
        }

        private static XmlDocument getGrammarXMLFile(String P_FileName)
        {
            var lFile = P_FileName;
            XmlDocument xd1 = null;

            if (!File.Exists(lFile))
            {
                return null;
            }

            try
            {
                xd1 = new XmlDocument();
                xd1.Load(lFile);
                xd1["grammar"]["rule"]["one-of"]["item"].FirstChild.Value = SpeechRecognition.jsonProperties["ServiceName"];
                xd1["grammar"]["rule"]["one-of"]["item"]["tag"].FirstChild.Value = "out = \"" + SpeechRecognition.jsonProperties["ServiceName"] + "\";";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Logger.Error(ex.Message);
                return null;
            }

            return xd1;
        }
    }
}
