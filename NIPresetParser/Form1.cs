using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NIPresetParser
{
    public partial class Form1 : Form
    {
        private int initialOffset = 236;
        private string endOfComment = "\0\0ÿ\uffff";
        private string colHeadings = "Name\tAuthor\tCompany\tComment\tProgram\tPreset Pack\tAttributes";
        private char[] data;
        private int index;

        

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void button1_Click(object sender, EventArgs e) {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                targetFolder.Text = folderBrowserDialog.SelectedPath;
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    List<string> dirs = new List<string>();
                    dirs.Add(folderBrowserDialog.SelectedPath);
                    dirs.AddRange(Directory.GetDirectories(folderBrowserDialog.SelectedPath));
                    List<List<string>> presets = new List<List<string>>();
                    foreach (string dir in dirs) {
                        foreach (string file in Directory.GetFiles(dir, "*.nmsv")) {
                            BinaryReader reader = new BinaryReader(new FileStream(file, FileMode.Open), Encoding.BigEndianUnicode);
                            data = reader.ReadChars((int)reader.BaseStream.Length);
                            presets.Add(parseData());
                            Console.WriteLine("preset added");
                        }
                    }

                    StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                    //writer.Write(colHeadings);
                    foreach(List<string> preset in presets)
                    {
                        foreach(string field in preset)
                        {
                            writer.Write(field);
                            writer.Write('\t');
                        }
                        writer.Write('\n');
                    }
                }
            }
        }

        private List<String> parseData() 
        {
            Func<char, bool> charIsText = c => Char.IsLetterOrDigit(c) || c == ' ';
            Func<char, bool> charIsNotText = c => !(Char.IsLetterOrDigit(c) || c == ' ');

            data = data.Skip(initialOffset).ToArray();
            List<string> strings = new List<string>();

            index = 0;
            if (data[index] == 0)
            {
                index += 1;
            }

            while((strings.Count <= 0 || !strings.Last().Equals("color")) && !(data[index] == '\a' && data[index + 2] == '\a')) {
                int fieldLength = data[index];
                if (strings.Count == 3)
                {
                    IEnumerable<char> takewhile = data.Skip(index + 2).TakeWhile(takeUntilCommentEnd);
                    string s = new string(takewhile.ToArray());
                    if (!s.Equals(endOfComment))
                    {
                        strings.Add(s.Substring(0, s.Length - endOfComment.Length));
                    } else
                    {
                        strings.Add("");
                    }
                    index += strings.Last().Length + 22;
                    strings[strings.Count() - 1] = strings.Last().Replace('\n', ' ');
                    continue;
                }  else
                {
                    strings.Add(new string(data.Skip(index + 2).Take(fieldLength).ToArray()));
                }
                index += fieldLength + 2;
                if (strings.Count == 6)
                {
                    index += 2;
                }
            }
            strings.RemoveAt(strings.Count - 1);

            return strings;
        }
        private bool takeUntilCommentEnd(char c, int i)
        {
            bool keepGoing = !(data[index + i] == 'ÿ' && data[index + i + 1] == '\uffff');
            if (!keepGoing)
            {
                Console.WriteLine("aoighe");
            }
            return keepGoing;
        }
    }

   
}
