using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using UnityEngine;

namespace PhEngine.GoogleSheets
{
    [CreateAssetMenu(menuName = "PhEngine/SheetDownloader", fileName = "SheetDownloader", order = 0)]
    public class SheetDownloader : ScriptableObject
    {
        [SerializeField] TextAsset keyJson;
        [SerializeField] bool isShowingLog;
        SheetsService service;

        public GetListFromSheetRequest<T> CreateRequest<T>(string spreadsheetId, string sheetNameAndRange)
        {
            return new GetListFromSheetRequest<T>(GetSheetsService(),spreadsheetId, sheetNameAndRange, isShowingLog);
        }
        
        public SheetsService GetSheetsService()
        {
            if (service != null)
                return service;
            
            InitializeSheetService();
            return service;
        }
        
        void InitializeSheetService()
        {
            if (keyJson == null)
                throw new NullReferenceException(nameof(keyJson));
            
            if (service != null)
                return;

            string jsonKey = keyJson.ToString();
            var credential = GoogleCredential
                .FromJson(jsonKey)
                .CreateScoped(SheetsService.Scope.Spreadsheets)
                .UnderlyingCredential;

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
        }
    }
}
