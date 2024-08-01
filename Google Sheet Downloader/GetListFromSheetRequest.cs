using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace PhEngine.GoogleSheets
{
    public class GetListFromSheetRequest<T>
    {
        public string SpreadsheetId { get; private set; }
        public string SheetNameAndRange { get; private set; }
        public Exception Exception { get; private set; }
        public TaskStatus Status => task.Status;
        public bool IsLogging { get; private set; }
        
        SpreadsheetsResource.ValuesResource.GetRequest request;
        Task<ValueRange> task;
        T result;
        
        Action<Exception> onFail;
        Action<List<T>> onSuccess;
        
        public GetListFromSheetRequest(SheetsService service, string spreadsheetId, string sheetNameAndRange, bool isLogging = true)
        {
            SpreadsheetId = spreadsheetId;
            SheetNameAndRange = sheetNameAndRange;
            IsLogging = isLogging;
            request = service.Spreadsheets.Values.Get(spreadsheetId, sheetNameAndRange);
        }

        public GetListFromSheetRequest<T> OnSuccess(Action<List<T>> callback)
        {
            onSuccess += callback;
            return this;
        }
        
        public GetListFromSheetRequest<T> OnFail(Action<Exception> callback)
        {
            onFail += callback;
            return this;
        }

        public void Download()
        {
            task = request.ExecuteAsync();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    ReportFailure(task.Exception);
                }
                else if (t.IsCompleted)
                {
                    var listOfRows = task.Result.Values;
                    var data = listOfRows.Select(innerList => innerList.Select(item => item.ToString()).ToList()).ToList();
                    if (data.Count == 0)
                    {
                        ReportFailure(new Exception("Did not find any rows from sheet"));
                        return;
                    }

                    var jsonString = ToJson(data);
                    if (string.IsNullOrEmpty(jsonString))
                    {
                        ReportFailure(new Exception("Failed to serialize rows to json format"));
                        return;
                    }

                    List<T> resultList;
                    try
                    {
                        resultList = JsonConvert.DeserializeObject<List<T>>(jsonString);
                    }
                    catch (Exception e)
                    {
                        ReportFailure(e);
                        throw;
                    }
                    if (IsLogging)
                        Debug.Log(string.Join(",\n", resultList.Select(r => JsonConvert.SerializeObject(r)).ToArray()));
                    onSuccess?.Invoke(resultList);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void ReportFailure(Exception e)
        {
            Exception = e;
            onFail?.Invoke(Exception);
            Debug.LogError(e);
        }
        
        static string ToJson(List<List<string>> data)
        {
            // Extract field names (first row)
            List<string> fieldNames = data[0];

            // Create a list of dictionaries for each row (excluding the first row)
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

            for (int i = 1; i < data.Count; i++)
            {
                Dictionary<string, string> row = new Dictionary<string, string>();
                for (int j = 0; j < Math.Min(fieldNames.Count, data[i].Count); j++)
                {
                    row[fieldNames[j]] = data[i][j];
                }

                rows.Add(row);
            }

            // Serialize to JSON
            return JsonConvert.SerializeObject(rows, Formatting.Indented);
        }
    }
}
