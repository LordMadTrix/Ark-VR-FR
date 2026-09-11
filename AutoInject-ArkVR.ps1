# Auto-Injecteur UEVR pour ARK (ShooterGame) - LordMadTrix
param(
    [string]$DllPath = ""
)

if (-not $DllPath -or -not (Test-Path $DllPath)) {
    exit 1
}

$Definition = @"
using System;
using System.Runtime.InteropServices;
public class Win32Inj {
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);
}
"@

try {
    Add-Type -TypeDefinition $Definition -ErrorAction SilentlyContinue
} catch {}

for ($i = 0; $i -lt 50; $i++) {
    Start-Sleep -Seconds 1
    $proc = Get-Process -Name "ShooterGame" -ErrorAction SilentlyContinue
    if ($proc) {
        # Attente de l'initialisation du moteur DirectX / fenêtre de jeu
        Start-Sleep -Milliseconds 4000
        $hProcess = [Win32Inj]::OpenProcess(0x1F0FFF, $false, $proc.Id)
        if ($hProcess -ne [IntPtr]::Zero) {
            $bytes = [System.Text.Encoding]::Unicode.GetBytes($DllPath + "`0")
            $mem = [Win32Inj]::VirtualAllocEx($hProcess, [IntPtr]::Zero, [uint32]$bytes.Length, 0x3000, 4)
            if ($mem -ne [IntPtr]::Zero) {
                $written = [UIntPtr]::Zero
                [Win32Inj]::WriteProcessMemory($hProcess, $mem, $bytes, [uint32]$bytes.Length, [ref]$written)
                $kernel32 = [Win32Inj]::GetModuleHandle("kernel32.dll")
                $loadLib = [Win32Inj]::GetProcAddress($kernel32, "LoadLibraryW")
                $thread = [Win32Inj]::CreateRemoteThread($hProcess, [IntPtr]::Zero, 0, $loadLib, $mem, 0, [IntPtr]::Zero)
                if ($thread -ne [IntPtr]::Zero) {
                    [Win32Inj]::CloseHandle($thread)
                    [Win32Inj]::CloseHandle($hProcess)
                    exit 0
                }
            }
            [Win32Inj]::CloseHandle($hProcess)
        }
        break
    }
}
exit 0
