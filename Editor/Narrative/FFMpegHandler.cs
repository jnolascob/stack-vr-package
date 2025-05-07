using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Singularis.StackVR.Narrative.Editor {
    public static class FFMpegHandler {
        public static string ffmpegPath = "";


        public static bool InitFMpeg() {
            // Set ffmpegPath based on platform
#if UNITY_EDITOR_OSX
            string fullPath = Path.GetFullPath("Assets/Plugins/Macos/FFMpeg/ffmpeg");
#elif UNITY_EDITOR_WIN 
            string fullPath = Path.GetFullPath("Assets/Plugins/Windows/FFMpeg/ffmpeg.exe");
#else
            UnityEngine.Debug.LogError("Unsupported platform for FFmpeg.");
            return false;
#endif

            ffmpegPath = fullPath;

            // Comprobar si FFmpeg existe en la ruta especificada
            if (!File.Exists(ffmpegPath)) {
                UnityEngine.Debug.LogError("?? FFmpeg no encontrado en: " + ffmpegPath);
                return false;
            }

            // Crear la información para el proceso
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = ffmpegPath,
                Arguments = "-version", // Argumento para obtener la versión
                RedirectStandardOutput = true, // Redirigir la salida estándar
                RedirectStandardError = true,  // Redirigir los errores
                UseShellExecute = false,      // No usar la shell
                CreateNoWindow = true         // No mostrar ventana de consola
            };

            try {
                // Iniciar el proceso
                Process process = new Process { StartInfo = startInfo };
                process.Start();

                // Leer la salida de la consola
                string output = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // Mostrar la salida (versión de FFmpeg)
                if (!string.IsNullOrEmpty(output)) {
                    UnityEngine.Debug.Log("? FFmpeg versión:\n" + output);
                    return true;
                }

                // Mostrar cualquier error en caso de que ocurra
                if (!string.IsNullOrEmpty(errorOutput)) {
                    UnityEngine.Debug.LogError("?? Error al obtener la versión de FFmpeg:\n" + errorOutput);
                    return false;
                }
            }
            catch (System.Exception ex) {
                UnityEngine.Debug.LogError("? Error al ejecutar FFmpeg: " + ex.Message);
                return false;
            }

            return false;      
        }

        public static async Task<string> ExtractFirstFrame(string videoPath, string outputImagePath, int seconds) {
            string filePath = Path.Combine("Assets", "Singularis", "StackVR", "ImageVideos", outputImagePath);
            filePath = Path.GetFullPath(filePath);

            if (File.Exists(filePath)) {
                File.Delete(filePath);
                AssetDatabase.Refresh();
            }
            string directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }





            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -ss {seconds} -vframes 1 -q:v 2 \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new Process();
            process.StartInfo = startInfo;
            //process.OutputDataReceived += (sender, args) => { if (!string.IsNullOrEmpty(args.Data)) UnityEngine.Debug.Log($"FFmpeg: {args.Data}"); };
            //process.ErrorDataReceived += (sender, args) => { if (!string.IsNullOrEmpty(args.Data)) UnityEngine.Debug.LogError($"FFmpeg Error: {args.Data}"); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit());

            if (File.Exists(filePath)) {
                UnityEngine.Debug.Log("? Frame extraído con éxito.");
                return filePath;
            }
            else {
                UnityEngine.Debug.LogError("? Error: El archivo de imagen no fue creado.");
                return null;
            }
        }

        public static Texture2D LoadTextureFromFile(string path) {
            if (!File.Exists(path)) {
                UnityEngine.Debug.LogError("La imagen no existe en: " + path);
                return null;
            }
            byte[] imageBytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadRawTextureData(imageBytes);
            texture.Apply();
            return texture;

        }

    }
}
