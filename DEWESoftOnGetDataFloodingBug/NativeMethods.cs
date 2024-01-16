using System.Runtime.InteropServices;

namespace DEWESoftOnGetDataFloodingBug
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
    }
}
