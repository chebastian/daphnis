using Newtonsoft.Json;
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

        internal static void SaveAllSnippets(List<DraggableViewModel> windows)
        {
            _list = new List<DraggableViewModel>();
            foreach(var win in windows)
            {
                SaveSnipp(win); 
            }

            var serialiezd = JsonConvert.SerializeObject(_list);
            Console.WriteLine(serialiezd);
            File.WriteAllText("./save.json", serialiezd.ToString()); 
        }

        internal static void SaveSnipp(DraggableViewModel vm)
        {
            if(vm.IsUnsaved)
            {
                Bitmap bmp = new Bitmap(vm.ImageSource);
                var stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Jpeg);
                var base64Image = Convert.ToBase64String(stream.ToArray());
                vm.Image64 = base64Image;
                AppendToSaved(vm);
            }
        } 

        private static void AppendToSaved(DraggableViewModel vm)
        {
            _list.Add(vm);
        }
    }
}