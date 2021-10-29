using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WimBuilder
{
    class SubProcess : IDisposable
    {
        private readonly Process _process = new();

        public string FileName() => _process.StartInfo.FileName;

        public SubProcess () {}
        public SubProcess(string filePath)
        {
            _process.StartInfo.FileName = filePath;
        }
        public SubProcess(string filePath, List<string> args)
        {
            _process.StartInfo.FileName = filePath;
            SetArgs(args);
        }

        private void WaitProcessEnd()
        {
            _process.WaitForExit();
            if (!_process.HasExited)
                _process.Kill();
        }
        public void SetArgs(string args)
        {
            _process.StartInfo.ArgumentList.Clear();
            _process.StartInfo.Arguments = args;
        }
   
        public void SetArgs(List<string> args)
        {
            _process.StartInfo.Arguments = string.Empty;
            _process.StartInfo.ArgumentList.Clear();
            args.ForEach(x => _process.StartInfo.ArgumentList.Add(x));
        }
        
        public string RunHeadless()
        {
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.Start();

            var t = _process.StandardOutput.ReadToEnd();
            WaitProcessEnd();

            return t;
        }

        public void RunHeadlessNoOutput()
        {
            if (_process.StartInfo.RedirectStandardOutput)
                _process.StartInfo.RedirectStandardOutput = false;
           
            _process.StartInfo.CreateNoWindow = true;
            _process.Start();
            WaitProcessEnd();
        }

        public void RunWindowed()
        {
            if (_process.StartInfo.RedirectStandardOutput)
                _process.StartInfo.RedirectStandardOutput = false;

            _process.StartInfo.CreateNoWindow = false;
            _process.Start();
            WaitProcessEnd();
        }
     
        public void Dispose()
        {
            if (!_process.HasExited)
            {
                _process.Kill();
                _process.Dispose();
            }
            else
            {
                _process.Dispose();
            }

        }
    }
}
