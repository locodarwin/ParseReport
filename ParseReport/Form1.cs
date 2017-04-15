using System;
using System.IO;
using System.Data;
using System.Windows.Forms;

namespace ParseReport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // In order to follow your basic AutoIt coding methodology and have it work in C#
        // We need to create a globals class. This isn't how you'd normally do it in C# though.
        // Also, make sure in AutoIt not to assign globals inside a function - that can cause issues
        public static class Globals
        {
            // These will mimic yours
            public static int iEntryCount = 0, iExitCount = 0;
            public static string sCurrentLineName = "", sCurrentEmployeeName = "";

            // Create Datatable to hold the final set of data - this is not needed but
            // It's here to kind of mimic what you're doing (but a .NET datatable instead of an array)
            public static DataTable dTable = new DataTable();
        }

        // Button 1 was clicked (event handler) - this is our browse to filename button
        private void button1_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a file.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PACS Report Files|*.txt";     // filer by txt
            openFileDialog1.Title = "Select a raw PACS report file";

            // Open dialog and allow user to choose file
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // When chosen, put filename in the textbox  
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        // Button 2 was clicked (event handler) - let's process the file! 
        private void button2_Click(object sender, EventArgs e)
        {
            // Check to see if file exists if it doesn't, msgbox
            string FileIn = textBox1.Text;
            if (!File.Exists(FileIn))
            {
                MessageBox.Show("Couldn't find the file! Try again.");
                return;
            }

            button1.Enabled = false;   // turn off the button (so we don't click it again while the script runs)

            // Prepare the global Datatable
            Globals.dTable.Clear();
            Globals.dTable.Columns.Clear();
            Globals.dTable.Columns.Add("EmployeeName", typeof(string));
            Globals.dTable.Columns.Add("EntryCount", typeof(string));
            Globals.dTable.Columns.Add("ExitCount", typeof(string));

            // Create the file reader (aka StreamReader) object
            System.IO.StreamReader file = new System.IO.StreamReader(FileIn);

            // Iterate through the file
            string line;
            while ((line = file.ReadLine()) != null)
            {

                // Create an array and split the incoming line into pieces
                string[] lineparts = line.Split(',');
                Globals.sCurrentLineName = lineparts[9] + lineparts[10];

                // Test incoming name against temp stored iteration name
                if (Globals.sCurrentLineName == Globals.sCurrentEmployeeName)
                {
                    switch (lineparts[13])
                    {
                        case "\"Entry\"":
                            Globals.iEntryCount +=1;
                            break;
                        case "\"Exit\"":
                            Globals.iExitCount +=1;
                            break;
                    }
                }
                else
                {
                    SaveDataToDatatable();
                    Globals.sCurrentEmployeeName = Globals.sCurrentLineName;
                    switch (lineparts[13])
                    {
                        case "\"Entry\"":
                            Globals.iEntryCount +=1;
                            break;
                        case "\"Exit\"":
                            Globals.iExitCount +=1;
                            break;
                    }
                }           
            }
            SaveDataToDatatable();
            file.Close();

            // Spit out a file with datatable contents
            Globals.dTable.Rows.RemoveAt(0);  // the way the algorithm is currently laid out the first row is always junk
            StreamWriter swOut = new StreamWriter("output.txt");
            foreach (DataRow row in Globals.dTable.Rows)
            {
                swOut.Write(row[0] + ",");
                swOut.Write(row[1] + ",");
                swOut.Write(row[2]);
                swOut.WriteLine();
            }
            swOut.Close();
            button1.Enabled = true;
        }

        // This is not necessary in the long run - I'll show you why later
        private void SaveDataToDatatable()
        {
            Globals.dTable.Rows.Add(Globals.sCurrentEmployeeName, Globals.iEntryCount, Globals.iExitCount);
            Globals.iEntryCount = 0;
            Globals.iExitCount = 0;
        }
    }
}
