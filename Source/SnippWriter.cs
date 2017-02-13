using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BorderlessAlphaWin
{
    internal class SnippWriter
    {
        private static List<DraggableViewModel> _list;
        private static List<DraggableViewModel> _newInRepo;

        public static string HistoryFile {
            get
            {
                return "./save.json";
            }
        }

        public static string RepoFile
        {
            get
            {
                return "./repo.json";
            }
        }

        internal static void SaveAllSnippets(List<DraggableViewModel> windows)
        {
            _list = new List<DraggableViewModel>();
            _newInRepo = new List<DraggableViewModel>();
            foreach(var win in windows)
            {
                SaveToLastSession(win);
                SaveToSnippRepo(win);
            }

            if(_newInRepo.Count > 0)
            {
                var serialized = JsonConvert.SerializeObject(_newInRepo);
                File.WriteAllText(SnippWriter.RepoFile, serialized);
            }

            var serialiezd = JsonConvert.SerializeObject(_list);
            Console.WriteLine(serialiezd);
            File.WriteAllText(HistoryFile, serialiezd.ToString()); 
        }

        private static void SaveToSnippRepo(DraggableViewModel win)
        {
            var obj = JsonConvert.SerializeObject(win);
            if (File.Exists(SnippWriter.RepoFile))
            {
                var text = File.ReadAllText(SnippWriter.RepoFile); 
                var jobj = JArray.Parse(text);
                var stored = jobj.Any(x => Int32.Parse(x["UniqueIdentifier"].ToString())  == win.UniqueIdentifier);
                if(stored)
                {
                    //re write store oject and delete old one, to be able to change tags and stuff
                    for (var i = 0; i < jobj.Count; i++)
                    {
                        if (jobj[i]["UniqueIdentifier"] != null && Int32.Parse(jobj[i]["UniqueIdentifier"].ToString()) == win.UniqueIdentifier)
                        {
                            //var storedObject = jobj.Where(x => Int32.Parse(x["UniqueIdentifier"].ToString())  == win.UniqueIdentifier); 
                            var parsed = JToken.Parse(obj);
                            jobj[i] = parsed; 
                        }
                    }
                    //storedObject = JToken.Parse(obj);
                }
                else
                { 
                    var token = JToken.Parse(obj);
                    jobj.Add(token);
                }
                File.WriteAllText(SnippWriter.RepoFile, jobj.ToString());
            }
            else
            {
                _newInRepo.Add(win); 
            }
        }

        internal static void SaveToLastSession(DraggableViewModel vm)
        {
            //Bitmap bmp = new Bitmap(vm.ImageSource); 
            //var stream = new MemoryStream();
            //bmp.Save(stream, ImageFormat.Jpeg);
            //var base64Image = Convert.ToBase64String(stream.ToArray());
            AppendToSaved(vm);
        }

        private static void AppendToSaved(DraggableViewModel vm)
        {
            _list.Add(vm);
        }
    }
}