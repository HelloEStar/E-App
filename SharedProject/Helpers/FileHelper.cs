using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace SharedProject
{
    class FileHelper
    {
        public static string GetContent(string path)
        {
            string str;
            try
            {
                Uri uri = new Uri(path, UriKind.Relative);
                Stream src = Application.GetResourceStream(uri).Stream;
                str = new StreamReader(src, Encoding.UTF8).ReadToEnd();
            }
            catch (Exception)
            {
                throw;
            }
            return str;
        }
    }
}
