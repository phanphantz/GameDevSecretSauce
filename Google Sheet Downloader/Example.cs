using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhEngine.GoogleSheets
{
    public class Example : MonoBehaviour
    {
        [SerializeField] SheetDownloader downloader;
        [SerializeField] string spreadsheetId;
        [SerializeField] string sheetNameAndRange;
        [SerializeField] List<GoogleSheetData> resultList = new List<GoogleSheetData>();
        
        void Start()
        {
            downloader
                .CreateRequest<GoogleSheetData>(spreadsheetId, sheetNameAndRange)
                .OnSuccess(list => resultList = list)
                .Download();
        }
    }

    [Serializable]
    public class GoogleSheetData
    {
        public string date;
        public int code;
        public string letters;
        public float score;
    }
}
