using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BMSFileConverter : MonoBehaviour
{
    [SerializeField]
    private GameObject noticeBoard;
    [SerializeField]
    private GameObject convertingStartButton;
    [SerializeField]
    private GameObject escapeButton;
    [SerializeField]
    private TextMeshProUGUI processivityText;

    private string rootPath;
    private string[] convertingFolderNameArray;

    private int threadCount;
    private int completeThreadCount;
    private int currentCompleteConvertingCount;
    private readonly object threadLock = new object();

    public void NoticeBoardSetActive(int value)
    {
        noticeBoard.SetActive(value == 1 ? true : false);
        convertingStartButton.SetActive(true);
        escapeButton.SetActive(true);
        processivityText.gameObject.SetActive(false);
    }

    public void BMSFileConvert()
    {
        threadCount = System.Environment.ProcessorCount;
        completeThreadCount = 0;
        currentCompleteConvertingCount = 0;
        rootPath = $@"{Directory.GetParent(Application.dataPath)}\BMSFiles";
        convertingFolderNameArray = Directory.GetDirectories($@"{rootPath}\Converting");
        for (int i = 0; i < threadCount; i++)
        {
            ConvertingThread(i);
        }

        _ = ConvertingProcessivityTextUpdate();
        _ = WaitConverting();
    }

    async private void ConvertingThread(int threadIndex)
    {
        await Task.Run(() =>
        {
            int start = (int)(threadIndex / (double)threadCount * convertingFolderNameArray.Length);
            int end = (int)((threadIndex + 1) / (double)threadCount * convertingFolderNameArray.Length);
            for (int i = start; i < end; i++)
            {
                FileInfo[] currentConvertingFolderFiles = new DirectoryInfo(convertingFolderNameArray[i]).GetFiles();
                for (int j = 0; j < currentConvertingFolderFiles.Length; j++)
                {
                    string extend = Path.GetExtension(currentConvertingFolderFiles[j].Name).ToLower();
                    if (extend.CompareTo(".bms") != 0 && extend.CompareTo(".bme") != 0 && extend.CompareTo(".bml") != 0) { continue; }
                    if (!CheckNewSongFile(convertingFolderNameArray[i], currentConvertingFolderFiles[j].Name) &&
                        !Directory.Exists($@"{rootPath}\MusicFolder\{Path.GetFileName(convertingFolderNameArray[i])}")) { continue; }
                    if (!CompatibleBMSFileCheck($@"{convertingFolderNameArray[i]}\{currentConvertingFolderFiles[j].Name}")) { continue; }

                    File.Copy($@"{convertingFolderNameArray[i]}\{currentConvertingFolderFiles[j].Name}",
                        $@"{rootPath}\TextFolder\{Path.GetFileName(convertingFolderNameArray[i])}_AR{
                            AllocatePatternFileIndex(rootPath, convertingFolderNameArray[i], currentConvertingFolderFiles[j].Name).ToString("D2")}.bms", true);
                }

                DirectoryMove($@"{rootPath}\Converting\{Path.GetFileName(convertingFolderNameArray[i])}", $@"{rootPath}\MusicFolder\{Path.GetFileName(convertingFolderNameArray[i])}");
                DirectoryDelete($@"{rootPath}\Converting\{Path.GetFileName(convertingFolderNameArray[i])}");
                lock (threadLock)
                {
                    currentCompleteConvertingCount++;
                }
            }
            lock (threadLock)
            {
                completeThreadCount++;
            }
        });
    }

    private async UniTask WaitConverting()
    {
        await UniTask.WaitUntil(() => completeThreadCount == threadCount);
        NoticeBoardSetActive(0);
    }

    private async UniTask ConvertingProcessivityTextUpdate()
    {
        convertingStartButton.SetActive(false);
        escapeButton.SetActive(false);
        processivityText.gameObject.SetActive(true);
        while (currentCompleteConvertingCount < convertingFolderNameArray.Length)
        {
            processivityText.text = currentCompleteConvertingCount + " / " + convertingFolderNameArray.Length;
            await UniTask.Yield();
        }
    }

    private bool CheckNewSongFile(string folderPath, string textFilePath)
    {
        StreamReader reader = new StreamReader($@"{folderPath}\{textFilePath}", Encoding.GetEncoding(932));
        bool isNewSongFile = false;
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length <= 3) { continue; }

            if (line.Substring(0, 4).CompareTo("#WAV") == 0)
            {
                string keySoundPath = line.Substring(7, line.Length - 11);
                if (File.Exists($@"{folderPath}\{keySoundPath}.ogg") || File.Exists($@"{folderPath}\{keySoundPath}.wav") || File.Exists($@"{folderPath}\{keySoundPath}.mp3"))
                {
                    isNewSongFile = true;
                }
                break;
            }
        }
        reader.Close();
        return isNewSongFile;
    }

    private int AllocatePatternFileIndex(string rootPath, string folderPath, string patternFileName)
    {
        List<string> patternFileList = new List<string>();
        foreach (var fileInfo in new DirectoryInfo($@"{rootPath}\TextFolder").GetFiles())
        {
            if (fileInfo.Name.Contains($"{Path.GetFileName(folderPath)}"))
            {
                patternFileList.Add(fileInfo.Name);
            }
        }

        int index = 0;
        if (File.Exists($@"{rootPath}\MusicFolder\{Path.GetFileName(folderPath)}\{patternFileName}")) 
        {
            StreamReader reader = new StreamReader($@"{rootPath}\MusicFolder\{Path.GetFileName(folderPath)}\{patternFileName}", Encoding.GetEncoding(932));
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.CompareTo("*---------------------- MAIN DATA FIELD") == 0) { break; }
            }
            List<string> musicFolderPatternFile = new List<string>(50);
            for (int i = 0; i < 50; i++)
            {
                if ((line = reader.ReadLine()) == null) { break; }
                musicFolderPatternFile.Add(line);
            }

            for (int i = 0; i < patternFileList.Count; i++)
            {
                reader = new StreamReader($@"{rootPath}\TextFolder\{patternFileList[i]}", Encoding.GetEncoding(932));
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.CompareTo("*---------------------- MAIN DATA FIELD") == 0) { break; }
                }

                bool isSameFile = true;
                for (int j = 0; j < musicFolderPatternFile.Count; j++)
                {
                    if ((line = reader.ReadLine()) == null) { break; }
                    if (musicFolderPatternFile[j].CompareTo(line) != 0)
                    {
                        isSameFile = false;
                        break;
                    }
                }
                if (isSameFile)
                {
                    index = int.Parse(patternFileList[i].Substring(patternFileList[i].Length - 6, 2));
                    break;
                }
            }
            reader.Close();
        }
        else
        {
            for (int i = 0; i < patternFileList.Count; i++)
            {
                index = Mathf.Max(index, int.Parse(patternFileList[i].Substring(patternFileList[i].Length - 6, 2)) + 1);
            }
        }
        return index;
    }

    private bool CompatibleBMSFileCheck(string bmsFilePath)
    {
        StreamReader reader = new StreamReader(bmsFilePath, Encoding.GetEncoding(932));
        bool isCompatibleBMSFile = true;
        bool isMainData = false;
        string line;
        while ((line = reader.ReadLine()) != null && isCompatibleBMSFile)
        {
            if (line.Length <= 6) { continue; }

            if (!isMainData)
            {
                if (line.Length > 9 && line.Substring(0, 10).CompareTo("#PLAYLEVEL") == 0)
                {
                    float level;
                    float.TryParse(line.Substring(11), out level);
                    if (level < 1 || level > 20)
                    {
                        isCompatibleBMSFile = false;
                    }
                }
                else if (line.Substring(0, 5).CompareTo("#STOP") == 0)
                {
                    isCompatibleBMSFile = false;
                }
                else if (line[6] == ':')
                {
                    isCompatibleBMSFile = false;
                }
                else if (line.CompareTo("*---------------------- MAIN DATA FIELD") == 0)
                {
                    isMainData = true;
                }
            }
            else
            {
                if ((line[4] == '1' || line[4] == '5') && (line[5] < '1' || line[5] > '5'))
                {
                    isCompatibleBMSFile = false;
                }
                else if (line.Contains("#IF") || line.Contains("#RANDOM"))
                {
                    isCompatibleBMSFile = false;
                }
            }
        }
        reader.Close();
        return isCompatibleBMSFile;
    }

    private void DirectoryMove(string movingFolderPath, string destinationPath)
    {
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }
        string[] files = Directory.GetFiles(movingFolderPath);
        string[] directories = Directory.GetDirectories(movingFolderPath);
        foreach (string s in files)
        {
            if (File.Exists(Path.Combine(destinationPath, Path.GetFileName(s)))) { continue; }
            File.Move(s, Path.Combine(destinationPath, Path.GetFileName(s)));
        }
        foreach (string d in directories)
        {
            DirectoryMove(Path.Combine(movingFolderPath, Path.GetFileName(d)), Path.Combine(destinationPath, Path.GetFileName(d)));
        }
    }

    public void DirectoryDelete(string targetDirectory)
    {
        File.SetAttributes(targetDirectory, FileAttributes.Normal);

        string[] files = Directory.GetFiles(targetDirectory);
        string[] dirs = Directory.GetDirectories(targetDirectory);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DirectoryDelete(dir);
        }

        Directory.Delete(targetDirectory, false);
    }
}
