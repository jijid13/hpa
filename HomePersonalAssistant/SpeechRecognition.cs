using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.SpeechBasics;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using Newtonsoft.Json.Linq;

#if MICRO
using System.Speech.Recognition;
using System.Speech.AudioFormat;
#endif

#if KINECT
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.Net;
#endif

namespace HomePersonalAssistant
{
    class SpeechRecognition
    {
        private SpeechSynthesizer speaker = new SpeechSynthesizer();
        private DateTime endtime;
        public bool occupied = false;
        private Dictionary<string, Plugin> _PluginsList;
        private string ServiceName;
        private double ConfidenceThreshold = 0.7;
        private Boolean startListen = false;
        [DllImport("Kernel32.dll")]
        public static extern bool Beep(Int32 dwFreq, Int32 dwDuration);
        private static string nodejsUrl = "";
        public static Dictionary<string, string> jsonProperties;

#if MICRO
        protected SpeechRecognitionEngine speechEngine = null;
#endif

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        public KinectSensor sensor;

        /// <summary>
        /// Stream for 32b-16b conversion.
        /// </summary>
        private KinectAudioStream convertStream = null;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
#if KINECT
        private SpeechRecognitionEngine speechEngine;
#endif

        public void Load()
        {
            string json = System.IO.File.ReadAllText("properties.json");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            jsonProperties = serializer.Deserialize<Dictionary<string, string>>(json);
            nodejsUrl = jsonProperties["url"] + ":" + jsonProperties["port"] + "/";

            this.ServiceName = jsonProperties["ServiceName"];
            this.endtime = DateTime.Now;
            speaker.SelectVoice(jsonProperties["SelectedVoice"]);
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "fr-FR".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        public void Start()
        {
#if KINECT

            this.sensor = KinectSensor.GetDefault();

            if (null != this.sensor)
            {
                try
                {
                    this.sensor.Open();
                }
                catch (IOException ex)
                {
                    this.sensor = null;
                    //Logger.Error(ex.Message);
                }
            }

            if (null == this.sensor)
            {
                return;
            }
            var ri = GetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                var audioBeamList = this.sensor.AudioSource.AudioBeams;
                var audioStream = audioBeamList[0].OpenInputStream();
                convertStream = new KinectAudioStream(audioStream);

                // Create a grammar definition ...

                this._PluginsList = Plugin.loadPlugins(this.speechEngine);

                speechEngine.SpeechRecognized += SpeechRecognized;

                convertStream.SpeechActive = true;
                speechEngine.SetInputToAudioStream(convertStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                Console.WriteLine("ok");
#endif
#if MICRO
            this.speechEngine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("fr-FR"));
            this._PluginsList = Plugin.loadPlugins(this.speechEngine);
            speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            speechEngine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", (int)(this.ConfidenceThreshold * 100));

            speechEngine.MaxAlternates = 10;
            speechEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(0);
            speechEngine.BabbleTimeout = TimeSpan.FromSeconds(0);
            speechEngine.EndSilenceTimeout = TimeSpan.FromSeconds(0.150);
            speechEngine.EndSilenceTimeoutAmbiguous = TimeSpan.FromSeconds(0.500);
            speechEngine.SetInputToDefaultAudioDevice();
#endif


                speechEngine.RecognizeAsync(RecognizeMode.Multiple);

                speaker.Speak(ConfigurationManager.AppSettings["OperationalSystem"]);
#if KINECT
            }
            else
            {
                speaker.Speak(ConfigurationManager.AppSettings["ErroredSystem"]);
            }
#endif
        }

        private async void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= ConfidenceThreshold && (e.Result.Audio.StartTime > this.endtime))
            {
                //start listening
                if (e.Result.Text.Equals(ServiceName) || startListen)
                {
                    if (!startListen)
                    {
                        Console.WriteLine("start listening");
                        startListen = true;
                        Beep(750, 300);
                        this.endtime = DateTime.Now;
                        System.Threading.Thread.Sleep(1500);
                        return;
                    }
                    else
                    {
                        if ((DateTime.Now - this.endtime).TotalSeconds > 6)
                        {
                            Console.WriteLine("listening timeout");
                            startListen = false;
                            Beep(750, 300);
                            return;
                        }
                    }
                }

                foreach (Plugin _plugin in _PluginsList.Values)
                {
                    try
                    {
                        if (_plugin._idPlugin == e.Result.Grammar.RuleName || _plugin._idPlugin == e.Result.Grammar.Name)
                        {
                            //Logger.Info("Recognized text : " + e.Result.Text + ", Grammar rule name : " + e.Result.Grammar.RuleName);
                            string json = "{";

                            foreach (KeyValuePair<string, SemanticValue> sv in e.Result.Semantics)
                            {
                                if (sv.Value.Value.ToString() != "")
                                {
                                    json += "\"" + sv.Key.ToString() + "\":\"" + sv.Value.Value.ToString() + "\",";
                                }
                            }
                            json = json.Substring(0, json.Length-1) + "}";
                            Console.WriteLine(json);
#if !TEST
                            Console.WriteLine("Recognized text : " + e.Result.Text + ", Grammar rule name : " + e.Result.Grammar.RuleName);
                            string lRet =  callJS(_plugin._idPlugin, json, speaker);
                            if (lRet != null && lRet != string.Empty)
                            {
                                speaker.Speak(lRet);
                            }
#endif
#if TEST
                            Console.WriteLine("Recognized text : " + e.Result.Text + ", Grammar rule name : " + e.Result.Grammar.RuleName);
                            Console.WriteLine("URL to be called : " + nodejsUrl + _plugin._idPlugin.ToLower());
                            speaker.Speak("c'est fait");
#endif
                            startListen = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //Logger.Error(ex.Message);
                    }
                }

                this.endtime = DateTime.Now;
            }
        }

        public string callJS(string plugin, string jsonParam, SpeechSynthesizer speaker)
        {
            using (var webClient = new System.Net.WebClient())
            {

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(nodejsUrl + plugin.ToLower());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonParam);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string ret;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var o = JObject.Parse(result);
                    if (o["status"].ToString().Equals("ok"))
                    {
                        ret = o["answer"].ToString();
                    }
                    else
                    {
                        ret = "erreur ! " + o["answer"].ToString();
                    }
                }

                return ret;
            }
        }

        public void Close()
        {
            if (null != this.convertStream)
            {
                this.convertStream.SpeechActive = false;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= this.SpeechRecognized;
                this.speechEngine.RecognizeAsyncStop();
            }

            if (null != this.sensor)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }


    }
}
