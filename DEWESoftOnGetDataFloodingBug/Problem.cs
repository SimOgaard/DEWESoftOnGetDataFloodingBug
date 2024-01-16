using DEWESoft;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DEWESoftOnGetDataFloodingBug
{
    public class Problem
    {
        private Process? DEWESoftProcess { get; set; }
        private App? app { get; set; }
        private int timerInterval => app?.TimerInterval ?? 33;

        public bool StartDEWESoft()
        {
            Console.WriteLine("Starting DEWESoft... Wait...");
            App? dewesoftApp = null;
            while (!Dewesoft.StartDewesoftApplication(out dewesoftApp))
            {
                Console.WriteLine("Retrying... Wait...");
                DEWESoftCleanup();
            }
            app = dewesoftApp;

            if (app is null)
                return false;

            // this one will break it (takes ~3ms)
            //app.OnGetData += App_OnGetData_Break;

            // this one wont (takes ~0.05ms)
            app.OnGetData += App_OnGetData_Work;

            // get the process
            DEWESoftProcess = Process.GetProcessesByName("DEWEsoft").FirstOrDefault();
            return true;
        }

        private void App_OnGetData_Break()
        {
            MemoryAddressReader();
        }
        private void App_OnGetData_Work()
        {
            Task.Run(MemoryAddressReader);
        }

        private void DEWESoftCleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (app is not null)
            {
                Marshal.ReleaseComObject(app);
                app = null;
            }
            try
            {
                Process[] DEWESoftProcesses = Process.GetProcessesByName("DEWEsoft");
                if (DEWESoftProcesses.Length <= 0)
                    return;

                foreach (Process process in DEWESoftProcesses)
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private bool _readAddresses = false;
        private bool readAddresses
        {
            get => _readAddresses;
            set
            {
                _readAddresses = value;
                if (value == false)
                    return;
                foreach ((Signal signal, IChannel iChannel) in liveChannels)
                {
                    signal.DBPos = ((iChannel.DBPos - 1 + iChannel.DBBufSize) % iChannel.DBBufSize) * iChannel.Bytes;
                }
            }
        }

        private bool criticalDewesoftError { get; set; } = false;
        private readonly Dictionary<Signal, IChannel> liveChannels = new();
        private void MemoryAddressReader()
        {
            if (criticalDewesoftError)
                return;

            if (!readAddresses)
                return;

            foreach ((Signal signal, IChannel iChannel) in liveChannels)
            {
                try
                {
                    if (iChannel.DBDataSize <= 0)
                        continue;

                    int dbPos = ((iChannel.DBPos - 1 + iChannel.DBBufSize) % iChannel.DBBufSize) * iChannel.Bytes;
                    if (dbPos == iChannel.DBPos)
                        continue;

                    IntPtr dbAddress = new(iChannel.GetDBAddress64());
                    int test = iChannel.DBBufSize * iChannel.Bytes;
                    if (dbPos < signal.DBPos)
                    {
                        // it has wrapped so we have to divide memory read into two parts
                        // from signal.DBPos to end
                        ReadMemoryAddress(signal, dbAddress, signal.DBPos, iChannel.DBBufSize * iChannel.Bytes);
                        // from 0 to dbPos
                        ReadMemoryAddress(signal, dbAddress, 0, dbPos);
                    }

                    // it has not wraped so we can read one big sweep of memory
                    ReadMemoryAddress(signal, dbAddress, signal.DBPos, dbPos);
                }
                catch (Exception e)
                {
                    criticalDewesoftError = true;
                    Console.WriteLine(e);
                }
            };
        }

        private void ReadMemoryAddress(Signal signal, IntPtr dbAddress, int dbPosFrom, int dbPosTo)
        {
            int bufferSize = dbPosTo - dbPosFrom;
            int addressOffset = dbPosFrom;

            if (bufferSize <= 0)
                return;

            byte[] buffer = new byte[bufferSize];
            bool success = NativeMethods.ReadProcessMemory(
                DEWESoftProcess.Handle,
                IntPtr.Add(dbAddress, addressOffset),
                buffer,
                bufferSize,
                out IntPtr bytesRead
            );

            if (!success)
                return;

            int sleep = timerInterval / (5 * liveChannels.Count);
            signal.DBPos = dbPosTo;
            Console.WriteLine($"Read {bufferSize} bytes from {signal.Name}, sleeping {sleep}ms to simulate light work");
            Thread.Sleep(sleep);
        }

        private void PromptUser()
        {
            Console.WriteLine("Setup any number of signals in dewesoft manually and press any button.");
            Console.ReadKey();

            foreach (IChannel iChannel in app.Data.UsedChannels)
            {
                IChannelConnection iChannelConnection = iChannel.CreateConnection();
                iChannelConnection.Start();
                liveChannels.Add(new(iChannel.Name), iChannel);
                Console.WriteLine($"Found {iChannel.Name}, adding...");
            }
            Console.WriteLine("Go to measure manually and when it is done click anywhere multiple times to se that the bug is not occuring, now press any button to start reading data and try again.");
            Console.ReadKey();
            readAddresses = true;
            Console.ReadKey();
        }

        ~Problem()
        {
            DEWESoftCleanup();
        }

        public Problem()
        {
            if (!StartDEWESoft())
            {
                Console.WriteLine("Did not manage to start DEWESoft!");
                return;
            }
            Console.WriteLine("DEWESoft started successfully!");
            PromptUser();
        }
    }
}
