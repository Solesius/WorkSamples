using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WimBuilder
{
    class DISMClient : IDisposable
    {
        private readonly SubProcess _dism = new("Dism.exe");
        private readonly SubProcess _powershell = new("C:/windows/system32/WindowsPowerShell/v1.0/powershell.exe");

        public DISMClient() { } 

        public void LoadISO(string isoFile)
        {
            _powershell.SetArgs(
                new List<string>() {
                    "Mount-DiskImage",
                    "-ImagePath",
                    isoFile
            });
            _powershell.RunHeadlessNoOutput();
        }

        public void UnloadISO(string isoFile)
        {
            _powershell.SetArgs(
                new List<string>() {
                "Unmount-DiskImage",
                "-ImagePath",
                isoFile
            });
            _powershell.RunHeadlessNoOutput();
        }

        public List<Wim> GetWimInfo(string isoDrive)
        {
            _dism.SetArgs($"/get-wiminfo /wimfile:{isoDrive}sources\\install.wim");
            string str = _dism.RunHeadless();

            StringBuilder sb = new();
            var ints = Enumerable.Range(str.IndexOf("Index"), str.Length).ToArray();
            foreach (int _ in ints)
            {
                try { sb.Append(str[_]); }catch {} 
            } 

            return DismParser.ParseWimInfo(sb.ToString().Split('\n'));
        }

        public void MountWim(string isoDrive, int idx)
        {
            string args = $"/Mount-Wim /wimfile:{isoDrive}sources\\install.wim /index:{idx} /mountdir:mountdir";
            _dism.SetArgs(args);
            _dism.RunWindowed();
        }
        public void SaveWim()
        {
            _dism.SetArgs($"/Unmount-Wim /mountdir:mountdir /commit");
            _dism.RunWindowed();
        }

        public void DiscardWim()
        {
            _dism.SetArgs($"/Unmount-Wim /mountdir:mountdir /discard");
            _dism.RunWindowed();
        }

        public string GetAppxPackages()
        {
            _powershell.SetArgs(new List<string> { "(Get-AppxProvisionedPackage -Path mountdir).PackageName" });
            return _powershell.RunHeadless();
        }
        public List<string> RemoveAppxPackage(List<object> packages)
        {
            List<string> vec = new();
            foreach (var package in packages)
            {
                _powershell.SetArgs($"Remove-AppxProvisionedPackage -path mountdir -packagename {package}");
                vec.Add(_powershell.RunHeadless());
            }
            return vec;

        }
        public void Dispose()
        {
            _dism.Dispose();
            _powershell.Dispose();
        }
    }
}
