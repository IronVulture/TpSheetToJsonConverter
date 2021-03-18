using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TpSheetToJson2
{
    public partial class Form1 : Form
    {
        private TpSheetToJson tpSheetToJson;
        public Form1()
        {
            InitializeComponent();
            tpSheetToJson = new TpSheetToJson();
            tpSheetToJson.form1 = this;
        }

        public void txtChanger(string addTxt)
        {
            textBox1.Text += "\r\n" + addTxt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select .tpsheet Files";
            dialog.InitialDirectory = ".\\";
            dialog.Filter = "tpsheet files (*.*)|*.tpsheet";
            dialog.Multiselect = true;

            List<string> tpsheetTxts = new List<string>();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                status.Text = "Running";
                List<string> tpsheetPaths = dialog.FileNames.ToList();

                List<FileInfo> fileInfos = new List<FileInfo>();
                foreach (string fileName in dialog.FileNames)
                {
                    if (File.Exists(fileName))
                    {
                        fileInfos.Add(new FileInfo(fileName));
                    }
                }

                foreach (FileInfo fileInfo in fileInfos)
                {
                    ConvertTpSheetToJsonFile(fileInfo);
                    if (additionalFileNameSubfix1.Text != "")
                        ConvertTpSheetToJsonFile(fileInfo, additionalFileNameSubfix1.Text);
                    if (additionalFileNameSubfix2.Text != "")
                        ConvertTpSheetToJsonFile(fileInfo, additionalFileNameSubfix2.Text);
                }
                status.Text = "Complete";
                //FileReader2.fileReader2(dialog.FileNames, textBlock, useEncoding, saveFolderPath);
            }
        }

        private void ConvertTpSheetToJsonFile(FileInfo fileInfo, string subfix = "")
        {
            var streamReader = new StreamReader(fileInfo.FullName, Encoding.UTF8);
            var tpsheetTxt = streamReader.ReadToEnd();
            JObject json = tpSheetToJson.ConvertTpSheetToJson(tpsheetTxt, subfix);

            var fileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            var filePath = fileInfo.DirectoryName + "\\" + fileName + subfix + ".json";
            txtChanger(filePath);

            var stream = JsonConvert.SerializeObject(json, Formatting.Indented);

            var streamWriter = new StreamWriter(filePath, false);
            streamWriter.Write(stream);
            streamWriter.Close();
        }
    }
}
