#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public static class FFMPegInstaller
{

#if UNITY_STANDALONE_OSX    
    private const string DownloadUrl = "https://assets.singularisvr.com/develop/stackvr/ffmpeg/windows/ffmpeg.zip";
#elif UNITY_STANDALONE_WIN
    private const string DownloadUrl = "https://assets.singularisvr.com/develop/stackvr/ffmpeg/macos/ffmpeg";
#endif

#if UNITY_STANDALONE_OSX
    private const string ExtractPath = "Assets/Plugins/Macos";
#endif

#if UNITY_STANDALONE_WIN
    private const string ExtractPath = "Assets/Plugins/Windows";
#endif
    private const string TempZipPath = "Temp/libreria_temp.zip";


    static FFMPegInstaller()
    {
        Events.registeredPackages += OnPackagesRegistered;
    }

    private static void OnPackagesRegistered(PackageRegistrationEventArgs args)
    {
        foreach (var addedPackage in args.added)
        {
            if (addedPackage.name == "com.singularisvr.stackvr") // Tu nombre exacto de package
            {
                Debug.Log("Tu package fue instalado!");
                DownloadAndExtractLibrary();
            }
        }
    }

    [MenuItem("Singularis/Develop/DownloadFFMpeg")]
    public static void ManualDownloadAndExtract()
    {
        DownloadAndExtractLibrary();
    }

    private static void RunFirstTimeSetup()
    {
        // Descarga tu librería, muestra popup, etc.
        Debug.Log("Ejecutando setup inicial...");
    }

    public static void DownloadAndExtractLibrary()
    {
#if UNITY_STANDALONE_OSX
        Debug.Log("Descargando libreria externa para macOS...");

        // Ensure the directory exists
        if (!Directory.Exists(ExtractPath))
            Directory.CreateDirectory(ExtractPath);

        // Download the file as "ffmpeg" into the ExtractPath directory
        string targetPath = Path.Combine(ExtractPath, "FFMpeg");
        using (var client = new WebClient())
        {
            client.DownloadFile(DownloadUrl, targetPath);
        }

        AssetDatabase.Refresh();
        Debug.Log("Libreria descargada para macOS!");
#else
        Debug.Log("Descargando libreria externa...");

        using (var client = new WebClient())
        {
            client.DownloadFile(DownloadUrl, TempZipPath);
        }

        if (Directory.Exists(ExtractPath))
            Directory.Delete(ExtractPath, true);

        ZipFile.ExtractToDirectory(TempZipPath, ExtractPath);

        File.Delete(TempZipPath);
        AssetDatabase.Refresh();

        Debug.Log("Libreria descargada y extraida!");
#endif
    }
}
#endif
