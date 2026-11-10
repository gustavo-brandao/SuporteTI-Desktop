using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SuporteTI.Grafico;

// O handle não é herdável. Fechá-lo encerra apenas os processos desta consulta,
// inclusive filhos cujo PowerShell pai já encerrou.
internal sealed class ArvoreConsulta : IDisposable
{
    private readonly SafeFileHandle job;

    public ArvoreConsulta()
    {
        job = CreateJobObject(IntPtr.Zero, null);
        if (job.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
        var limites = new LimitesEstendidos { Basicos = new LimitesBasicos { Flags = 0x2000 } };
        if (!SetInformationJobObject(job, 9, ref limites, (uint)Marshal.SizeOf<LimitesEstendidos>()))
        {
            var erro = Marshal.GetLastWin32Error();
            job.Dispose();
            throw new Win32Exception(erro);
        }
    }

    public void Associar(Process processo)
    {
        if (AssignProcessToJobObject(job, processo.SafeHandle)) return;
        var erro = Marshal.GetLastWin32Error();
        if (!processo.HasExited)
            throw new Win32Exception(erro, "O Windows não permitiu controlar os processos da consulta.");
    }

    public void Dispose() => job.Dispose();

    [StructLayout(LayoutKind.Sequential)]
    private struct LimitesBasicos
    {
        public long TempoProcesso, TempoJob;
        public uint Flags;
        public UIntPtr MinimoMemoria, MaximoMemoria;
        public uint ProcessosAtivos;
        public UIntPtr Afinidade;
        public uint Prioridade, Agendamento;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ContadoresIo
    {
        public ulong Leituras, Escritas, Outros, BytesLidos, BytesEscritos, OutrosBytes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LimitesEstendidos
    {
        public LimitesBasicos Basicos;
        public ContadoresIo Io;
        public UIntPtr MemoriaProcesso, MemoriaJob, PicoProcesso, PicoJob;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateJobObject(IntPtr seguranca, string? nome);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetInformationJobObject(SafeFileHandle job, int classe, ref LimitesEstendidos dados, uint tamanho);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AssignProcessToJobObject(SafeFileHandle job, SafeProcessHandle processo);
}
