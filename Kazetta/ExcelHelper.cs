using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Workbook file = excel.Workbooks.Open(filename, ReadOnly:true);
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
                { "gitár", Instrument.Guitar },
                { "Basszusgitár", Instrument.Bass },
                { "basszusgitár", Instrument.Bass },
                { "Kórusének", Instrument.Voice },
                { "Szólóének", Instrument.Voice },
                { "ének", Instrument.Voice },
                { "Zongora, billentyűs hangszerek", Instrument.Keyboards },
                { "zongora", Instrument.Keyboards },
                { "Dallamhangszer (fuvola, hegedű, cselló stb.)", Instrument.Solo },
                { "improvizáció", Instrument.Solo },
                { "ütőhangszerek", Instrument.Percussion },
                { "Ütőhangszerek", Instrument.Percussion }
            };
            try
            {                
                Worksheet sheet = file.Worksheets["Résztvevők"];
                List<Person> ppl = new List<Person>();
                
                foreach(Range row in sheet.UsedRange.Rows)
                {
                    if (row.Row == 1)
                        continue;
                    Range col = row.Columns;
                    if (col[3].Value == null)
                        break;

                    string name = col[3].Value.Trim();
                    if (name == "Csima Zsolt" || name.Contains ("Grafné"))
                        continue;
                    
                    var person = new Person
                    {
                        Name = name,
                        Email = col[2].Value,
                        Sex = SexMapping[col[4].Value],                        
                        SkillLevel = LevelMapping[col[18].Value],
                        IsVocalistToo = col[19].Value == "Igen",
                        BirthYear = col[6].Value.Year,
                        Type = PersonType.Student
                    };
                    var instrument = col[17].Value;
                    if (InstrumentMapping.ContainsKey(instrument))
                        person.Instrument = InstrumentMapping[instrument];
                    else if (instrument.ToString().ToLower().Contains("dallamh"))
                        person.Instrument = Instrument.Solo;
                    else if (instrument.ToString().ToLower().Contains("zongor"))
                        person.Instrument = Instrument.Keyboards;
                    else
                        throw new Exception(instrument);

                    if (person.Instrument == Instrument.Voice)
                        person.IsVocalistToo = false;

                    ppl.Add(person);
                    
                }

                sheet = file.Worksheets["Tanárok"];

                foreach(Range row in sheet.UsedRange.Rows)
                {
                    if (row.Row == 1)
                        continue;
                    Range col = row.Columns;
                    var instrument = col[2].Value;
                    if (instrument == null)
                        break;
                    ppl.Add(new Person
                    {
                        Name = col[1].Value.Trim(),
                        Instrument = InstrumentMapping[col[2].Value],
                        Type = PersonType.Teacher
                    });
                }

                sheet = file.Worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "Énektanár preferenciák");

                if (sheet != null)
                {
                    foreach (Range row in sheet.UsedRange.Rows)
                    {
                        if (row.Row == 1)
                            continue;
                        Range col = row.Columns;
                        string name = col[2].Value;
                        if (name == null)
                            continue;
                        Person p = ppl.Find(q => q.Name == name.Trim());
                        p.PreferredVocalTeachers[0] = ppl.Single(q => q.Name == col[3].Value);
                        p.PreferredVocalTeachers[1] = ppl.Single(q => q.Name == col[4].Value);
                    }
                }

                sheet = file.Worksheets.OfType<Worksheet>().FirstOrDefault(ws => ws.Name == "Tanárválasztás");

                if (sheet != null)
                {
                    foreach (Range row in sheet.UsedRange.Rows)
                    {
                        if (row.Row == 1)
                            continue;
                        Range col = row.Columns;
                        string email = col[2].Value;                        
                        string teacherName = col[6].Value;
                        string vocalTeacherName = col[8].Value;
                        if (email == null)
                            break;
                        bool likesTeacher = col[5].Value == "IGEN";
                        bool likesVocalTeacher = col[7].Value == "IGEN";
                        Person p = ppl.Find(q => q.Email == email.Trim());
                        if (teacherName != null)
                        {
                            Person teacher = ppl.Find(q => q.Name == teacherName.Trim());
                            if (likesTeacher)
                                p.PreferredTeacher = teacher;
                            else
                                p.AvoidTeacher = teacher;                            
                        }
                        if (vocalTeacherName != null)
                        {
                            Person vocalTeacher = ppl.Find(q => q.Name == vocalTeacherName.Trim());
                            if (likesVocalTeacher)
                            {
                                p.PreferredVocalTeacher = vocalTeacher;
                                p.PreferredVocalTeachers[0] = vocalTeacher;
                            }
                            else
                                p.AvoidVocalTeacher = vocalTeacher;
                        }                                                
                    }
                }

                // Culling: if there are more vocalists then vocalist slots, then discard the youngest vocalists
                int numberOfVocalists = ppl.Count(p => p.Type == PersonType.Student && p.IsVocalist);
                int numberOfVocalTeachers = ppl.Count(p => p.Type == PersonType.Teacher && p.IsVocalist);
                int diff = numberOfVocalists - numberOfVocalTeachers * (int)Algorithms.NumberOfSlots;
                if (diff > 0)
                {
                    var range = (from p in ppl
                                where p.Type == PersonType.Student && p.IsVocalist
                                orderby p.BirthYear descending
                                select p).Take (diff).ToList ();
                    Console.WriteLine(range);
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
                Worksheet sheet = file.Worksheets[0];               
                Range c = sheet.Cells;
                int i = 2;
                foreach (Person p in data.Students)
                {
                    c[i, 1].Activate();
                    string[] nev = p.Name.Split(new Char[] { ' ' }, 2);
                    c[i, 1] = nev[0];
                    c[i, 2] = nev[1];
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

