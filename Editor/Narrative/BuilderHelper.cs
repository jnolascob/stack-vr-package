using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Singularis.StackVR.Editor;

namespace Singularis.StackVR.Narrative.Editor {
    public static class BuilderHelper {

        public static Tour ImportNodes(string zipPath, out string folderName, out string pathNode, out string pathHostpot) {
            if (!string.IsNullOrEmpty(zipPath)) {
                try {
                    string parentFolder = Path.Combine(Application.dataPath, "ImportedFiles");
                    string uniqueFolderName = "Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string importFolder = Path.Combine(parentFolder, uniqueFolderName);
                    folderName = uniqueFolderName;

                    if (Directory.Exists(importFolder))
                        Directory.Delete(importFolder, true);

                    Directory.CreateDirectory(importFolder);

                    using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                        bool hasJson = archive.Entries.Any(entry => entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                        if (!hasJson) {
                            Debug.LogError("El archivo ZIP no contiene un archivo JSON. Importación cancelada.");
                            pathNode = "";
                            pathHostpot = "";
                            return null;
                        }

                        foreach (ZipArchiveEntry entry in archive.Entries) {
                            string fullPath = Path.Combine(importFolder, entry.FullName);

                            // Si es un directorio, lo creamos
                            if (entry.FullName.EndsWith("/")) {
                                Directory.CreateDirectory(fullPath);
                                continue;
                            }

                            // Asegurar que la carpeta del archivo existe
                            string directoryPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directoryPath)) {
                                Directory.CreateDirectory(directoryPath);
                            }

                            // Si el archivo ya existe, agregar un sufijo numérico
                            string fileName = Path.GetFileName(fullPath);
                            string fileDirectory = Path.GetDirectoryName(fullPath);
                            string destinationPath = fullPath;
                            int count = 1;

                            while (File.Exists(destinationPath)) {
                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                                string extension = Path.GetExtension(fileName);
                                destinationPath = Path.Combine(fileDirectory, $"{fileNameWithoutExt}_{count}{extension}");
                                count++;
                            }

                            // Extraer archivo con el nuevo nombre si es necesario
                            entry.ExtractToFile(destinationPath);
                        }
                    }

                    //ZipFile.ExtractToDirectory(zipPath, importFolder);
                    Debug.Log($"ZIP extraído en: {importFolder}");

                    // Buscar el archivo JSON dentro de la carpeta extraída
                    string[] jsonFiles = Directory.GetFiles(importFolder, "*.json", SearchOption.AllDirectories);

                    if (jsonFiles.Length == 0) {
                        Debug.LogError("No se encontró ningún archivo JSON en el ZIP.");
                        pathNode = "";
                        pathHostpot = "";
                        return null;
                    }
                    AssetDatabase.Refresh();
                    string jsonPath = jsonFiles[0]; // Tomar el primer JSON encontrado
                    string jsonContent = File.ReadAllText(jsonPath);
                    // Read the contents of the file


                    // Process the JSON content (example: log it)
                    Debug.Log($"JSON Content: {jsonContent}");

                    // Optionally, deserialize it into a class
                    var resultJson = JsonConvert.DeserializeObject<Tour>(jsonContent);

                    pathNode = Path.Combine("Assets", "ImportedFiles", uniqueFolderName);
                    AssetDatabase.CreateFolder(pathNode, "Nodes");
                    AssetDatabase.Refresh();
                    pathNode = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, "Nodes");

                    pathHostpot = Path.Combine("Assets", "ImportedFiles", uniqueFolderName);
                    AssetDatabase.CreateFolder(pathHostpot, "Hostpots");
                    AssetDatabase.Refresh();
                    pathHostpot = Path.Combine("Assets", "ImportedFiles", uniqueFolderName, "Hostpots");

                    return resultJson;
                }
                catch (Exception ex) {
                    Debug.Log(ex.Message);
                    pathNode = "";
                    pathHostpot = "";
                    folderName = "";
                    return null;
                }
            }

            pathNode = "";
            pathHostpot = "";
            folderName = "";
            return null;
        }

        public static void SaveJsonFile(ref List<string> pathFiles, int nodeId, List<NodeDataOld> nodesData) {
            Tour testExperience = new Tour();
            testExperience.version = 1;
            testExperience.date = "000";
            testExperience.start = nodeId;
            testExperience.nodes = nodesData;
            testExperience.createAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            testExperience.updateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string resultJson = JsonConvert.SerializeObject(testExperience, Formatting.Indented);
            Debug.Log(resultJson);

            string resourcesPath = "Assets/Singularis/StackVR/Resources/tour_data.json";
            File.WriteAllText(resourcesPath, resultJson);
            Debug.Log($"?? JSON guardado en: {resourcesPath}");

            // ?? Actualizar el sistema de archivos en Unity para que reconozca el nuevo archivo
            AssetDatabase.Refresh();
            pathFiles.Add(resourcesPath);
        }

        public static void ExportFileAsZip(string[] filePaths) {
            // Verifica que el archivo exista
            // ??? Abre la ventana para elegir dónde guardar el archivo ZIP
            string savePath = EditorUtility.SaveFilePanel("Guardar ZIP", "", "Archivos Zip" + ".zip", "zip");

            if (string.IsNullOrEmpty(savePath)) return; // Si se cancela, no hace nada

            // Asegura que la carpeta de destino exista
            string directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            // ?? Comprime el archivo en un ZIP
            using (FileStream zipToCreate = new FileStream(savePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create)) {
                foreach (string filePath in filePaths) {

                    string fileName = Path.GetFileName(filePath);
                    string entryName = fileName;

                    if (filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".jpeg")) {
                        entryName = Path.Combine("Images", fileName);
                    }
                    archive.CreateEntryFromFile(filePath, entryName);
                }
            }

            EditorUtility.DisplayDialog("Succes", "Sucess Exported Proejct", "OK");
            Debug.Log($"?? ZIP guardado en: {savePath}");
        }

    }
}
