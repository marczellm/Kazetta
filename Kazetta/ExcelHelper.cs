using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kazetta
{
	public class ExcelHelper
	{
		private static Instrument InstrumentMapping(string providedAnswer)
		{
			providedAnswer = providedAnswer.ToLower();
			var mapping = new List<KeyValuePair<string, Instrument>>
			{// Order is important
                new KeyValuePair<string, Instrument> ("basszusgitár", Instrument.Bass),
				new KeyValuePair<string, Instrument> ("gitár", Instrument.Guitar),
				new KeyValuePair<string, Instrument> ("ének", Instrument.Voice),
				new KeyValuePair<string, Instrument> ("ukulele", Instrument.Voice), // we have no ukulele teacher
                new KeyValuePair<string, Instrument> ("zongora", Instrument.Keyboards),
				new KeyValuePair<string, Instrument> ("dallamhangszer", Instrument.Solo),
				new KeyValuePair<string, Instrument> ("improvizáció", Instrument.Solo),
				new KeyValuePair<string, Instrument> ("szolfézs", Instrument.Solo),
				new KeyValuePair<string, Instrument> ("ütőhangszer", Instrument.Percussion)
			};
			foreach (var pair in mapping)
				if (providedAnswer.Contains(pair.Key))
					return pair.Value;
			throw new Exception(providedAnswer);
		}

		private static readonly uint COL_NAME = 3;
		private static readonly uint COL_SEX = 4;
		private static readonly uint COL_BIRTHDATE = 6;
		private static readonly uint COL_INSTRUMENT = 18;
		private static readonly uint COL_LEVEL = 19;
		private static readonly uint COL_VOCALISTTOO = 20;

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
			var SexMapping = new Dictionary<string, Sex>
			{
				{ "Nő", Sex.Female },
				{ "Férfi", Sex.Male }
			};
			var LevelMapping = new Dictionary<string, Level>
			{
				{ "Alapszint", Level.Beginner },
				{ "Középhaladó", Level.Intermediate },
				{ "Haladó", Level.Advanced}
			};
			try
			{
				Worksheet sheet = file.Worksheets["Résztvevők"];
				List<Person> ppl = new List<Person>();

				foreach (Range row in sheet.UsedRange.Rows)
				{
					if (row.Row == 1)
						continue;
					Range col = row.Columns;

					var instrument = col[COL_INSTRUMENT].Value;
					if (instrument == null)
						break;
					else if (instrument.Contains("keverés"))
						continue;

					var person = new Student
					{
						Name = col[COL_NAME].Value.Trim(),
						Sex = SexMapping[col[COL_SEX].Value],
						SkillLevel = LevelMapping[col[COL_LEVEL].Value],
						IsVocalistToo = col[COL_VOCALISTTOO].Value == "Igen",
						BirthYear = col[COL_BIRTHDATE].Value.Year
					};

					person.Instrument = InstrumentMapping(instrument);
					if (person.Instrument == Instrument.Voice)
					{
						person.IsVocalistToo = false;
					}
					ppl.Add(person);
				}

				sheet = file.Worksheets["Tanárok"];

				foreach (Range row in sheet.UsedRange.Rows)
				{
					Range col = row.Columns;
					var instrument = col[2].Value;
					if (instrument == null)
						break;
					var secondaryInstrument = col[3].Value;
					var instruments = secondaryInstrument == null ? new Instrument[] { InstrumentMapping(instrument) } : new Instrument[] { InstrumentMapping(instrument), InstrumentMapping(secondaryInstrument) };
					ppl.Add(new Teacher
					{
						Name = col[1].Value.Trim(),
						Instruments = instruments
					});
				}

				sheet = file.Worksheets["Énektanár preferenciák"];

				foreach (Range row in sheet.UsedRange.Rows)
				{
					Range col = row.Columns;
					string name = col[1].Value;
					if (name == null)
						continue;
					Student p = ppl.OfType<Student>().First(q => q.Name == name.Trim());
					p.PreferredVocalTeachers[0] = ppl.OfType<Teacher>().Single(q => q.Name == col[2].Value);
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
					string[] nev = p.Name.Split(new char[] { ' ' }, 2);
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

