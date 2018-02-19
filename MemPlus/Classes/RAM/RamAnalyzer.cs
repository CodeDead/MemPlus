using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using MemPlus.Classes.RAM.ViewModels;

namespace MemPlus.Classes.RAM
{
    internal sealed class RamAnalyzer
    {
        private readonly List<ProcessData> _processDataList;

        internal delegate void ProcessAddedEvent(ProcessData processData);
        internal delegate void ProcessRemovedEvent(ProcessData processData);

        private readonly ProcessAddedEvent _processAddedEvent;
        private readonly ProcessRemovedEvent _processRemovedEvent;

        internal RamAnalyzer(int delay, ProcessAddedEvent processAddedEvent, ProcessRemovedEvent processRemovedEvent)
        {
            _processAddedEvent = processAddedEvent;
            _processRemovedEvent = processRemovedEvent;

            _processDataList = new List<ProcessData>();
            Timer updateTimer = new Timer
            {
                Interval = delay,
                Enabled = true
            };

            updateTimer.Elapsed += UpdateTimerOnElapsed;
            UpdateTimerOnElapsed(null, null);
        }

        private async void UpdateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            List<string> currentPaths = new List<string>();
            await Task.Run(async () =>
            {
                foreach (Process p in Process.GetProcesses())
                {
                    try
                    {
                        currentPaths.Add(p.MainModule.FileName);

                        ProcessData processData = EqualsPath(p.MainModule.FileName);
                        bool addProcessData = false;
                        if (processData == null)
                        {
                            processData = new ProcessData();
                            addProcessData = true;
                        }

                        processData.ProcessName = p.ProcessName;
                        processData.ProcessLocation = p.MainModule.FileName;
                        processData.Pid = p.Id;
                        processData.WorkingSet = (p.WorkingSet64 / 1024 / 1024).ToString("F2") + " MB";

                        if (addProcessData)
                        {
                            _processDataList.Add(processData);
                            _processAddedEvent.Invoke(processData);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                await CleanProcessList(currentPaths);
            });
        }

        private async Task CleanProcessList(IReadOnlyCollection<string> currentPaths)
        {
            await Task.Run(() =>
            {
                for (int i = _processDataList.Count - 1; i >= 0; i--)
                {
                    bool remove = true;

                    foreach (string s in currentPaths)
                    {
                        if (_processDataList[i].ProcessLocation == s)
                        {
                            remove = false;
                            break;
                        }
                    }

                    if (remove)
                    {
                        _processRemovedEvent.Invoke(_processDataList[i]);
                        _processDataList.RemoveAt(i);
                    }
                }
            });
        }

        private ProcessData EqualsPath(string path)
        {
            foreach (ProcessData pd in _processDataList)
            {
                if (pd.ProcessLocation == path)
                {
                    return pd;
                }
            }
            return null;
        }

        internal List<ProcessData> GetProcessData()
        {
            return _processDataList;
        }
    }
}
