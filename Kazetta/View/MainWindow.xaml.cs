﻿using Kazetta.View;
using Kazetta.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Kazetta
{
	public partial class MainWindow : System.Windows.Window
	{
		public MainWindow()
		{
			InitializeComponent();
			kcs = new DnDItemsControl[] { kcs1, kcs2, kcs3, kcs4, kcs5, kcs6, kcs7, kcs8, kcs9, kcs10, kcs11, kcs12, kcs13, kcs14, kcs15, kcs16, kcs17, kcs18, kcs19, kcs20, kcs21 };
			string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kazetta");
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			filepath = Path.Combine(folder, "state.xml");
		}
		private readonly DnDItemsControl[] kcs;
		private readonly string filepath;
		private CancellationTokenSource cts;
		private readonly object _lock = new object();

		private ViewModel.MainWindow viewModel => (ViewModel.MainWindow)DataContext;

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			ExtendWindowFrame();
			LoadXML();
		}

		private void ExtendWindowFrame()
		{
			try
			{
				IntPtr windowPtr = new WindowInteropHelper(this).Handle;
				HwndSource.FromHwnd(windowPtr).CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
				float rdpiy = System.Drawing.Graphics.FromHwnd(windowPtr).DpiY / 96;
				DwmAPI.Margins margins = new DwmAPI.Margins { top = Convert.ToInt32(150 * rdpiy), left = 1, right = 1, bottom = 1 };
				if (DwmAPI.DwmExtendFrameIntoClientArea(windowPtr, ref margins) < 0)
					Background = SystemColors.WindowBrush;
			}
			catch (DllNotFoundException)
			{
				Background = SystemColors.WindowBrush;
			}
		}

		/// <summary>
		/// Load the app state from AppData\...\Kazetta\state.xml
		/// </summary>
		private void LoadXML()
		{
			var xs = new XmlSerializer(typeof(AppData));
			if (File.Exists(filepath))
			{
				using (var file = new StreamReader(filepath))
				{
					try { viewModel.AppData = (AppData)xs.Deserialize(file); }
					catch { } // If for example the XML is written by a previous version of this app, we shouldn't attempt to load it
				}
			}
			int i = 1;
			foreach (TabItem tab in TabControl.Items)
			{
				if (tab.Visibility == Visibility.Visible)
					tab.Header = String.Format("{0}. {1}", i++, tab.Header);
			}
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var xs = new XmlSerializer(typeof(AppData));
			using (var file = new StreamWriter(filepath))
			{
				xs.Serialize(file, viewModel.AppData);
			}
		}

		private async void LoadXLS(object sender, RoutedEventArgs e)
		{
			var btn = (Button)sender;
			btn.Click -= LoadXLS;
			if (Type.GetTypeFromProgID("Excel.Application") == null)
			{
				MessageBox.Show("Excel nincs telepítve!");
				return;
			}
			XLSLoadingAnimation.Visibility = Visibility.Visible;
			var dialog = new OpenFileDialog
			{
				Filter = "Excel|*.xls;*.xlsx;*.xlsm",
				DereferenceLinks = true,
				AddExtension = false,
				CheckFileExists = true,
				CheckPathExists = true
			};
			if (dialog.ShowDialog(this) == true)
			{
				viewModel.Groups.Clear();
				viewModel.Students.Clear();
				viewModel.Teachers.QuietClear();
				var people = await Task.Run(() =>
				{
					try
					{
						return ExcelHelper.LoadXLS(dialog.FileName);
					}
					catch (Exception ex)
					{
						MessageBox.Show("Hiba az Excel fájl olvasásakor" + Environment.NewLine + ex.Message ?? "" + Environment.NewLine + ex.InnerException?.Message ?? "");
						return new List<Person>();
					}
				});

				
				viewModel.Teachers.AddRange(people.OfType<Teacher>());
				viewModel.Students.AddRange(people.OfType<Student>().OrderBy(p => p.Name));
				viewModel.InitGroups();
			}
			XLSLoadingAnimation.Visibility = Visibility.Hidden;
			btn.Click += LoadXLS;
		}

		private void SaveXLS(object sender, RoutedEventArgs e)
		{
			if (Type.GetTypeFromProgID("Excel.Application") == null)
			{
				MessageBox.Show("Excel nincs telepítve!");
				return;
			}
			XLSSavingAnimation.Visibility = Visibility.Visible;
			var dialog = new SaveFileDialog
			{
				DefaultExt = ".xlsm",
				Filter = "Excel|*.xls;*.xlsx;*.xlsm",
				DereferenceLinks = true,
				AddExtension = true,
				CheckPathExists = true
			};
			if (dialog.ShowDialog(this) == true)
				try
				{
					ExcelHelper.SaveXLS(dialog.FileName, viewModel);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Hiba az Excel fájl írásakor" + Environment.NewLine + ex.Message ?? "" + Environment.NewLine + ex.InnerException?.Message ?? "");
				}
			XLSSavingAnimation.Visibility = Visibility.Hidden;
		}

		private void AddPerson(object sender, RoutedEventArgs e)
		{
			viewModel.Students.CollectionChanged += People_CollectionChanged;
			viewModel.Students.Add(new Student());
		}

		private void People_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			viewModel.Students.CollectionChanged -= People_CollectionChanged;
			Person p = (Person)e.NewItems[0];
			var cp = (FrameworkElement)PeopleView.ItemContainerGenerator.ContainerFromItem(p);
			cp.ApplyTemplate();
			var label = (ContentControl)PeopleView.ItemTemplate.FindName("PersonButton", cp);
            TextBox textBox = new TextBox
            {
                MinWidth = 10,
                Tag = p
            };
            textBox.KeyDown += TextBox_KeyDown;
			label.Content = textBox;
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() => Keyboard.Focus(textBox)));
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				TextBox textBox = (TextBox)sender;
				if (textBox.Text.Contains(' '))
				{
					((Person)textBox.Tag).Name = textBox.Text;
					((ContentControl)textBox.Parent).Content = textBox.Text;
				}
				else
					MessageBox.Show("Kell legyen vezetékneve és keresztneve is!");
			}
		}

		private void AddGroup(object sender, RoutedEventArgs e)
		{
			var group = viewModel.NewGroup;
			if (group.Persons.Contains(null))
				return;

			viewModel.AddGroup(group);
			viewModel.NewGroup = new Group { Autogenerated = false, Persons = new Student[2] };
		}

		private void RemoveGroup(object sender, RoutedEventArgs e)
		{
			Group group = (Group)((FrameworkElement)sender).DataContext;
			viewModel.Groups.Remove(group);
			foreach (Student student in group.Persons)
			{
				student.Group = null;
			}
		}

		private void Group_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
				RemoveGroup(sender, null);
		}

		private void Reset(object sender, RoutedEventArgs e)
		{
			viewModel.Students.Clear();
			viewModel.Groups.Clear();
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				viewModel.StatusText = "";
				var newTab = e.AddedItems[0];
				if (newTab != ScheduleTab)
				{
					foreach (var schedule in viewModel.Schedule)
						BindingOperations.DisableCollectionSynchronization(schedule);
					return;
				}
				string message = null;
				if (viewModel.Students.Count == 0)
				{
					message = "Nincsenek résztvevők!";
					newTab = ParticipantsTab;
				}
				if (message != null)
				{
					viewModel.StatusText = message;
					viewModel.MagicPossible = false;
					// SaveButton.IsEnabled = false;
					// we don't activate newTab here anymore, because it caused weird behaviour
				}
				else if (newTab == ScheduleTab)
				{
					viewModel.InitSchedule();

					for (int i = 0; i < kcs.Length; i++)
					{
						if (i < viewModel.Teachers.Count)
						{
							BindingOperations.GetBindingExpression(kcs[i], ItemsControl.ItemsSourceProperty).UpdateTarget();
							BindingOperations.GetBindingExpression(kcs[i], HeaderedItemsControl.HeaderProperty).UpdateTarget();
						}
						else
							kcs[i].Visibility = Visibility.Collapsed;
					}
					viewModel.Algorithm = new Algorithms(viewModel, _lock);
					viewModel.MagicPossible = true;

					foreach (var schedule in viewModel.Schedule)
						BindingOperations.EnableCollectionSynchronization(schedule, _lock);
				}
			}
		}

		private async void Magic(object sender, RoutedEventArgs e)
		{
			viewModel.StatusText = "";
			MagicAnimation.Visibility = Visibility.Visible;
			var alg = viewModel.Algorithm;
			var btn = (Button)sender;
			btn.Click -= Magic;
			var oldContent = btn.Content;
			btn.Content = "Cancel";
			RoutedEventHandler handler;

			using (cts = new CancellationTokenSource())
			{
				handler = (ender, se) => cts.Cancel();
				btn.Click += handler;
				CancellationToken ct = cts.Token;
				try
				{
					if (!await Task.Run(() => alg.NaiveFirstFit(ct), ct))
						viewModel.StatusText = "Nem sikerült az automatikus beosztás!";
				}
				catch (AggregateException) { }
			}
			MagicAnimation.Visibility = Visibility.Collapsed;
			btn.Click -= handler;
			btn.Click += Magic;
			btn.Content = oldContent;
		}

		private void ClearSchedule(object sender, RoutedEventArgs e)
		{
			viewModel.ClearSchedule();
		}

		private void NewGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var p = (Student)e.AddedItems[0];
				PossiblePairs.ItemsSource = viewModel.GroupEligible.Cast<Student>().Where(q => p != q && q.Group == null && p.Instrument == q.Instrument);
                PossiblePairs2.ItemsSource = viewModel.GroupEligible.Cast<Student>().Where(q => p != q && q.Group == null && p.Instrument == q.Instrument);
            }
			else
			{
				PossiblePairs.ItemsSource = null;
                PossiblePairs2.ItemsSource = null;
            }
		}

		private void AutoCreateGroups(object sender, RoutedEventArgs e)
		{
			var groupEligible = viewModel.GroupEligible.Cast<Student>().ToList();
			while (groupEligible.Count > 1)
			{
				Student p1 = groupEligible.First();
				Student p2 = groupEligible.FirstOrDefault(r => p1 != r && r.Group == null && p1.Instrument == r.Instrument);
				if (p2 != null)
				{
					Group g = new Group { Autogenerated = false, Persons = new Student[] { p1, p2 } };

                    if (groupEligible.Count > 2 * (7 - viewModel.Groups.Count(gg => gg.Persons.Length > 1)))
					{
                        Console.WriteLine("Remaining soloists: " + groupEligible.Count);
                        Console.WriteLine("Groups assigned so far: " + viewModel.Groups.Count);
                        Student p3 = groupEligible.FirstOrDefault(r => p1 != r && p2 != r && r.Group == null && p1.Instrument == r.Instrument);
						if (p3 != null)
						{
							g.Persons = g.Persons.Append(p3).ToArray();
						}
                    }

                    foreach (Student student in g.Persons)
                    {
                        groupEligible.Remove(student);
                    }
                    viewModel.AddGroup(g);
				}
			}
		}
	}
}