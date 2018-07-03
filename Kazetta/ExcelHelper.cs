using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Kazetta
{
    public class ExcelHelper
    {
        /// <summary>
        /// Opens Excel in the background and reads available data about participants.
        /// 
        /// Does not use OpenXML SDK because this way we can support the old binary formats too.
        /// Also the amount of code needed is vastly smaller this way.
        /// </summary>
        /// <returns>a list of people</returns>
        public static List<Person> LoadXLS(string filename)
        {
            var excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook file = excel.Workbooks.Open(filename);
            var SexMapping = new Dictionary<String, Sex>
            {
                { "Nő", Sex.Female },
                { "Férfi", Sex.Male }
            };
            var LevelMapping = new Dictionary<String, Level>
            {
                { "Alapszint", Level.Beginner },
                { "Középhaladó", Level.Intermediate },
                { "Haladó", Level.Advanced}
            };
            var InstrumentMapping = new Dictionary<String, Instrument>
            {
                { "Akusztikus gitár", Instrument.Guitar },
                { "Basszusgitár", Instrument.Bass },
                { "Kórusének", Instrument.Voice },
                { "Szólóének", Instrument.Voice },
                { "Zongora, billentyűs hangszerek", Instrument.Keyboards },
                { "Dallamhangszer (fuvola, hegedű, cselló stb.)", Instrument.Solo },
                { "Ütőhangszerek", Instrument.Percussion }
            };
            try
            {                
                Worksheet sheet = file.Worksheets[1];                
                List<Person> ppl = new List<Person>();

                
                foreach(Range row in sheet.UsedRange.Rows)
                {
                    if (row.Row == 1)
                        continue;
                    Range col = row.Columns;
                    var instrument = col[17].Value;
                    if (instrument == null)
                        break;
                    var person = new Person
                    {
                        Name = col[3].Value,
                        Sex = SexMapping[col[4].Value],                        
                        Level = LevelMapping[col[18].Value],
                        VocalistToo = col[19].Value != "Nem"
                    };
                    if (InstrumentMapping.ContainsKey(instrument))
                        person.Instrument = InstrumentMapping[instrument];
                    else if (instrument.ToString().Contains("dallamh"))
                        person.Instrument = Instrument.Solo;
                    ppl.Add(person);
                }

                return ppl;
            }
            finally
            {
                file.Close(false);
                excel.Quit();
            }
        }

        public static void SaveXLS(string filename, ViewModel.MainWindow data)
        {
            Uri uri = new Uri("/Resources/hetvegekezelo.xlsm", UriKind.Relative);

            using (var stream = System.Windows.Application.GetResourceStream(uri).Stream)
            using (var f = File.Create(filename))
            {
                stream.CopyTo(f);
            }
            var excel = new Microsoft.Office.Interop.Excel.Application { Visible = true };
            Workbook file = excel.Workbooks.Open(filename);
            try
            {
                Worksheet sheet = file.Worksheets["Vezérlő adatok"];
                sheet.Cells[2, 2] = ViewModel.MainWindow.WeekendNumber;

                sheet = file.Worksheets["Alvócsoport címek"];
                var m = data.Rooms.Count();
                for (int j = 1; j <= m; j++)
                {
                    sheet.Cells[j + 1, 1] = ((char)(j + 64)).ToString();
                    sheet.Cells[j + 1, 2] = data.AlvocsoportNevek[j - 1];
                }

                sheet = file.Worksheets["Alapadatok"];
                sheet.Activate();
                sheet.Unprotect();
                Range c = sheet.Cells;
                int i = 2;
                foreach (Person p in data.People)
                {
                    c[i, 1].Activate();
                    string[] nev = p.Name.Split(new Char[] { ' ' }, 2);
                    c[i, 1] = nev[0];
                    c[i, 2] = nev[1];                    
                    if (ViewModel.MainWindow.WeekendNumber == 20)
                    {
                        if (data.Szentendre.Contains(p))
                            c[i, 9] = "Szentendre";
                        if (data.MutuallyExclusiveGroups[0].Contains(p))
                            c[i, 9] = "Zugliget";
                    }
                    i++;
                }
            }
            finally
            {
                file.Save();
            }
        }
    }
}

