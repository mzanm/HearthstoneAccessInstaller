using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
namespace HearthstoneAccessInstaller;

static class Patcher
{
    const string PATCH_DIR = "patch/";

    public static bool IsHsDirectory(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) return false;
        directory = Path.GetFullPath(directory);
        if (Directory.Exists(directory) && Path.GetFileName(directory) == "Hearthstone" && File.Exists(Path.Combine(directory, "Hearthstone_Data", "Managed", "Assembly-CSharp.dll")))
        {
            return true;
        }
        return false;
    }

    public static string? LocateHearthstone()
    {
        string? path = Environment.GetEnvironmentVariable("HEARTHSTONE_HOME");
        if (path != null && IsHsDirectory(path))
        {
            return Path.GetFullPath(path);
        }
        string programFiles;
        if (Environment.Is64BitOperatingSystem)
        {
            programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        }
        else
        {
            programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        path = Path.Combine(programFiles, "Hearthstone");
        if (IsHsDirectory(path))
        {
            return path;
        }
        return null;
    }

    static public Document[] UnpackAndPatch(Stream downloaded, string directory)
    {
        List<Document> documents = new();
        using ZipArchive archive = new ZipArchive(downloaded, ZipArchiveMode.Read, false);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryPath = entry.FullName;
            if (String.IsNullOrWhiteSpace(entryPath) || entryPath.EndsWith('/')) continue;
            entryPath = entryPath.TrimStart('/');
            if (!entryPath.Contains('/') && (entryPath.EndsWith("txt", StringComparison.OrdinalIgnoreCase) || entryPath.EndsWith("md", StringComparison.OrdinalIgnoreCase)))
            {
                string title = entry.Name;
                using (StreamReader reader = new(entry.Open()))
                {
                    string content = reader.ReadToEnd();
                    if (!String.IsNullOrWhiteSpace(content))
                    {
                        documents.Add(new Document(title, content));
                    }

                }
            }
            if (!entryPath.StartsWith(PATCH_DIR, StringComparison.OrdinalIgnoreCase)) continue;
            entryPath = entry.FullName.Substring(PATCH_DIR.Length);
            entryPath = Path.Join(entryPath.Split('/'));
            entryPath = Path.Join(directory, entryPath);
            string? entryDirectory = Path.GetDirectoryName(entryPath)!;
            Directory.CreateDirectory(entryDirectory);
            using (FileStream fileStream = new(entryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (Stream entryStream = entry.Open())
                {
                    entryStream.CopyTo(fileStream);
                }
            }
        }
        return documents.ToArray();
    }
}
