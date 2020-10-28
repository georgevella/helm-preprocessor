using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HelmPreprocessor.Services
{
    public class SopsSecretsHandler : ISecretsHandler
    {
        public FileInfo Decode(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                throw new FileNotFoundException("Cannot decode non-existent file", fileInfo.Name);
            
            // create a copy of the file (this makes it so the repo is not tainted)
            var temporaryFile = Path.Combine(fileInfo.DirectoryName, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}-dec.yaml");
            fileInfo.CopyTo(temporaryFile, true);
            
            // decode the file
            var targetFileInfo = new FileInfo(temporaryFile);
            var psi = new ProcessStartInfo(
                "sops", 
                $"-d -i {targetFileInfo.FullName}"
            );

            var process = Process.Start(psi);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"SopsSecretsHandler failed to decrypt the secret files (exitCode: {process.ExitCode}).");
            }
            
            return targetFileInfo;
        }
        
        public Task<FileInfo> DecodeAsync(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                throw new FileNotFoundException("Cannot decode non-existent file", fileInfo.Name);
            
            // create a copy of the file (this makes it so the repo is not tainted)
            var temporaryFile = Path.Combine(fileInfo.DirectoryName, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}-dec.yaml");
            fileInfo.CopyTo(temporaryFile, true);
            
            // decode the file
            var targetFileInfo = new FileInfo(temporaryFile);
            var psi = new ProcessStartInfo(
                "sops", 
                $"-d -i {targetFileInfo.FullName}"
                );
            
            var tcs = new TaskCompletionSource<FileInfo>();
            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(targetFileInfo);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}