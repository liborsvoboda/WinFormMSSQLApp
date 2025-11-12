using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Nancy.Json;
using Newtonsoft.Json;

namespace TypPostriku
{
    internal class GlobalFunctions
    {

        public static void LoadSettings()
        {
            try
            {
                string json = File.ReadAllText(Path.Combine(App.appRuntimeData.settingFolder, App.appRuntimeData.appSettingFile), FileDetectEncoding(Path.Combine(App.appRuntimeData.settingFolder, App.appRuntimeData.appSettingFile)));
                Dictionary<string, string> SettingData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                App.appRuntimeData.AppClientSettings.Add(SettingData.Keys.FirstOrDefault(), SettingData.Values.FirstOrDefault());
            }
            catch (Exception ex) {  }
        }



        /// <summary>
        /// Function for saving Application Configuration This is client configuration only
        /// </summary>
        /// <returns></returns>
        public static Exception SaveSettings()
        {
            try
            {
                using (StreamWriter sw = File.CreateText(Path.Combine(App.appRuntimeData.settingFolder, App.appRuntimeData.appSettingFile)))
                {
                    sw.Write(ConvertDictionaryListToJson(App.appRuntimeData.AppClientSettings));
                    sw.Flush(); sw.Close();
                }
                return null;
            }
            catch (Exception ex) { return ex; }
        }

        public static string ConvertDictionaryListToJson(Dictionary<string, string> keyList)
        {
            bool tempBool; int tempInt = 0;
            Dictionary<string, object> exportJsonList = new Dictionary<string, object>();
            keyList.ToList().ForEach(key => {
                string valueType = bool.TryParse(key.Value, out tempBool) ? "bool" : int.TryParse(key.Value, out tempInt) ? "int" : "string";
                exportJsonList.Add(key.Key, (valueType == "bool" ? (object)tempBool : valueType == "int" ? (object)tempInt : (object)key.Value));
            });
            return new JavaScriptSerializer().Serialize(exportJsonList);
        }

        /// <summary>
        /// Unicodes to ut f8.
        /// </summary>
        /// <param name="strFrom">The string from.</param>
        /// <returns></returns>
        public static string UnicodeToUTF8(string strFrom)
        {
            byte[] bytes = Encoding.Default.GetBytes(strFrom);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Checks the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static bool CheckFile(string file)
        {
            return File.Exists(file);
        }

        /// <summary>
        /// Prepared Method for Files Copy
        /// </summary>
        /// <param name="sourcePath">     </param>
        /// <param name="destinationPath"></param>
        public static void CopyFiles(string sourcePath, string destinationPath)
        {
            string[] filePaths = Directory.GetFiles(sourcePath);
            foreach (string fullFilePath in filePaths)
            {
                string fileName = Path.GetFileName(fullFilePath);
                if (!File.Exists(Path.Combine(destinationPath, fileName)))
                {
                    File.Copy(Path.Combine(sourcePath, fileName), Path.Combine(destinationPath, fileName));
                }
            }
        }

        /// <summary>
        /// Creates the path recursively.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="clearIfExist"></param>
        /// <returns></returns>
        public static bool CreatePath(string path, bool clearIfExist = false)
        {
            try
            {
                if (clearIfExist && Directory.Exists(path)) { Directory.Delete(path, true); }
                string[] pathParts = path.Split('\\');

                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (i > 0)
                        pathParts[i] = System.IO.Path.Combine(pathParts[i - 1], pathParts[i]);

                    if (!Directory.Exists(pathParts[i]))
                        Directory.CreateDirectory(pathParts[i]);

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static byte[] ReadFile(string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int size = (int)f.Length;
            byte[] data = new byte[size];
            size = f.Read(data, 0, size);
            f.Close();
            return data;
        }

        /// <summary>
        /// Write ByteArray to File
        /// </summary>
        /// <param name="fileName"> Name of the file.</param>
        /// <param name="byteArray">The byte array.</param>
        /// <returns></returns>
        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Prepared Method for Create empty file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool CreateFile(string file)
        {
            if (!File.Exists(file))
                File.Create(file).Close();

            return CheckFile(file);
        }

        /// <summary>
        /// Write String to File Used for JsonSaving
        /// If rewrite file is false, content is append
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        /// <param name="rewrite"></param>
        public static void WriteToFile(string file, string content, bool rewrite = true)
        {
            if (CreateFile(file))
            {
                if (rewrite) { DeleteFile(file); }
                StreamWriter objWriter = new StreamWriter(file, true);
                objWriter.WriteLine(content);
                objWriter.Close();
            }
        }

        /// <summary>
        /// Prepared Method for Get Information of File encoding UTF8,WIN1250,etc
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static Encoding FileDetectEncoding(string FileName)
        {
            string enc = "";
            if (File.Exists(FileName))
            {
                FileStream filein = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                if ((filein.CanSeek))
                {
                    byte[] bom = new byte[5];
                    filein.Read(bom, 0, 4);
                    // EF BB BF = utf-8 FF FE = ucs-2le, ucs-4le, and ucs-16le FE FF = utf-16 and
                    // ucs-2 00 00 FE FF = ucs-4
                    if ((((bom[0] == 0xEF) && (bom[1] == 0xBB) && (bom[2] == 0xBF)) || ((bom[0] == 0xFF) && (bom[1] == 0xFE)) || ((bom[0] == 0xFE) && (bom[1] == 0xFF)) || ((bom[0] == 0x0) && (bom[1] == 0x0) && (bom[2] == 0xFE) && (bom[3] == 0xFF))))
                        enc = "Unicode";
                    else
                        enc = "ASCII";
                    // Position the file cursor back to the start of the file
                    filein.Seek(0, SeekOrigin.Begin);
                }
                filein.Close();
            }
            if (enc == "Unicode")
                return Encoding.UTF8;
            else
                return Encoding.Default;
        }

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        /// <summary>
        /// Checks the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        public static bool CheckDirectory(string directory)
        {
            return Directory.Exists(directory);
        }



        /// <summary>
        /// Get Folder Files from Direct Folder
        /// or FULL Structure by fileMask
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="fileMask"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<string> GetPathFiles(string sourcePath, string fileMask, SearchOption searchOption)
        {
            return Directory.GetFiles(sourcePath, fileMask, searchOption).ToList();
        }


        /// <summary>
        /// Copy Full directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }
            Directory.CreateDirectory(targetPath);

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">  To.</param>
        /// <returns></returns>
        public static bool CopyFile(string from, string to)
        {
            try
            {
                File.Copy(from, to, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public static void CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static bool DeleteFile(string file)
        {
            File.Delete(file);

            if (!CheckFile(file))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Full Clear Folder
        /// </summary>
        /// <param name="FolderName">Name of the folder.</param>
        public static void ClearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }


        /// <summary>
        /// Return Full File path to the operating system default slashes.
        /// !!! USE as + With Path Combine Path.Combine(yourpath) + ConvertSystemFilePathFromUrl(string webpath);
        /// </summary>
        /// <param name="webpath"></param>
        public static string ConvertSystemFilePathFromUrl(string webpath)
        {
            if (string.IsNullOrEmpty(webpath)) return webpath;
            char slash = Path.DirectorySeparatorChar;
            if (!webpath.StartsWith("/")) { webpath = $"/{webpath}"; }
            webpath = webpath.Replace('/', slash).Replace('\\', slash).Replace(slash.ToString() + slash.ToString(), slash.ToString());
            return webpath;
        }
    }
}