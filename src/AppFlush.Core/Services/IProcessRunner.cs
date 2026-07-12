namespace AppFlush.Core.Services;

/// <summary>Abstraksi menjalankan sebuah command line, supaya bisa dipalsukan di tes.</summary>
public interface IProcessRunner
{
    int Run(string commandLine);
}
