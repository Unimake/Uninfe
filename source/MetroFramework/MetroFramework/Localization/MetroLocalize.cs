/**
 * MetroFramework - Modern UI for WinForms
 * 
 * The MIT License (MIT)
 * Copyright (c) 2011 Sven Walter, http://github.com/viperneo
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in the 
 * Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MetroFramework.Localization
{
    internal class MetroLocalize
    {
        private DataSet languageDataset;

        public string DefaultLanguage()
        {
            return "en";
        }

        public string CurrentLanguage()
        {
            string language = Application.CurrentCulture.TwoLetterISOLanguageName;
            if (language.Length == 0)
            {
                language = DefaultLanguage();
            }

            return language.ToLower();
        }

        public MetroLocalize(string ctrlName)
        {
            importManifestResource(ctrlName);
        }

        public MetroLocalize(Control ctrl)
        {
            importManifestResource(ctrl.Name);            
        }

        private void importManifestResource(string ctrlName)
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();

            string localizationFilename = callingAssembly.GetName().Name + ".Localization." + CurrentLanguage()  + "." + ctrlName + ".xml";
            Stream xmlStream = callingAssembly.GetManifestResourceStream(localizationFilename);

            if (xmlStream == null)
            {
                localizationFilename = callingAssembly.GetName().Name + ".Localization." + DefaultLanguage() + "." + ctrlName + ".xml";
                xmlStream = callingAssembly.GetManifestResourceStream(localizationFilename);
            }


            if (languageDataset == null)
                languageDataset = new DataSet();

            if (xmlStream != null)
            {
                DataSet importDataset = new DataSet();
                importDataset.ReadXml(xmlStream);

                languageDataset.Merge(importDataset);
                xmlStream.Close();
            }
        }


        public string translate(string key)
        {
            if ((string.IsNullOrEmpty(key))) {
                return "";
            }

            if (languageDataset == null) {
                return "~" + key;
            }

            if (languageDataset.Tables["Localization"] == null) {
                return "~" + key;
            }

            DataRow[] languageRows = languageDataset.Tables["Localization"].Select("Key='" + key + "'");
            if (languageRows.Length <= 0)
            {
                return "~" + key;
            }

            return languageRows[0]["Value"].ToString();
        }


    }
}
