using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WimBuilder
{

	public partial class Form1 : Form
	{
		private DISMClient _dismClient = new DISMClient();

		public Form1()
		{
			InitializeComponent();
		}

		private string MountedVolumeName()
		{

			try
			{
				return DriveInfo.GetDrives().Where(d => d.DriveFormat.ToString() == "UDF").First().Name;
			}
			catch
			{
				return "";
			}
		}
		private bool IsoLoaded() => string.IsNullOrEmpty(MountedVolumeName());

		private void FilePickerButton_Click(object sender, EventArgs e)
		{

			isoPicker.ShowDialog();

			if (!isoPicker.FileNames.Any())
			{
				return;
			}
			else
			{
				isoDescription.Text = isoPicker.FileName;

				outputBox.Text = "Loading iso...";
				Task.Factory.StartNew(() => _dismClient.LoadISO(isoPicker.FileName)).Wait();

				outputBox.AppendText($"\r\nISO file is now loaded on drive: {MountedVolumeName()}\r\n");
			}
		}

		private void GetWimInfo_Click(object sender, EventArgs e)
		{
			var wims = _dismClient.GetWimInfo(MountedVolumeName()).Where(x => x.Name.Contains("Pro Education"));

			WimList.Items.Clear();
			foreach (var wim in wims)
				WimList.Items.Add($"{wim.Name}|{wim.Index}");
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			AllocConsole();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			_dismClient.UnloadISO(isoPicker.FileName);
		}

		private void MountWim_Click(object sender, EventArgs e)
		{
			var targetDir = "isodir";
			outputBox.AppendText("\r\nBuilding scratch space for extracting contents to disk");
			if (!System.IO.Directory.Exists(targetDir))
			{
				outputBox.AppendText("\r\nDirectory does not exist, creating scratch space in iso_target directory\n");
				System.IO.Directory.CreateDirectory(targetDir);
				Task.Factory.StartNew(() => FileHandler.DirectoryCopy(MountedVolumeName(), "isodir", true)).Wait();
			}

			outputBox.Text += ("\r\nDirectory exists...\n");

			if (WimList.SelectedItem == null || WimList.SelectedItems.Count > 1)
			{
				return;
			}
			outputBox.AppendText("\r\nMounting wim..\r\n");

			//disable readOnly on wim file
			File.SetAttributes("isodir\\sources\\install.wim", 0);

			_dismClient.MountWim(targetDir, Convert.ToInt32(((string)WimList.SelectedItem).Split('|')[1]));
		}

		private void GetAppxPackages_click(object sender, EventArgs e)
		{
			listBox1.Items.Clear();
			foreach (string appx in _dismClient.GetAppxPackages().Split('\n'))
				listBox1.Items.Add(appx);
		}

		private void RemoveAppx_click(object sender, EventArgs e)
		{
			//outputBox.AppendText("\r\n" + listBox1.SelectedItems.Count);
			var vec = new object[listBox1.SelectedItems.Count];


			listBox1.SelectedItems.CopyTo(vec, 0);
			//outputBox.AppendText("\r\n" + vec.Length);
			vec.ToList().ForEach(app => outputBox.AppendText("\r\n" + app));

			outputBox.AppendText("\r\nRemoving selected appx Packages from Wim..");
			List<string> retVal = new();
			Task.Factory.StartNew(() => _dismClient.RemoveAppxPackage(vec.ToList())).ContinueWith(_retval => { retVal = _retval.Result; });

		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		private void Save_Click(object sender, EventArgs e)
		{
			_dismClient.SaveWim();
		}

		private void Discard_Click(object sender, EventArgs e)
		{
			_dismClient.DiscardWim();
		}
	}
}