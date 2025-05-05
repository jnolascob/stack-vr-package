#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public static class PackageInstallWatcher
{
    static PackageInstallWatcher()
    {
        Events.registeredPackages += OnPackagesRegistered;
    }

    private static void OnPackagesRegistered(PackageRegistrationEventArgs args)
    {
        foreach (var addedPackage in args.added)
        {
            if (addedPackage.name == "com.singularisvr.stackvr") // Tu nombre exacto de package
            {
                Debug.Log("¡Tu package fue instalado!");
                RunFirstTimeSetup();
            }
        }
    }

    private static void RunFirstTimeSetup()
    {
        // Descarga tu librería, muestra popup, etc.
        Debug.Log("Ejecutando setup inicial...");
    }
}
#endif
