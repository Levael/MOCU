using System;
using System.IO;
using System.Collections.Generic;

using DaemonsRelated;


namespace MeshisExperiment
{
    public class MeshisSavedData
    {
        public DateTime Date;
        public MeshisParameters Parameters;
        public List<MeshisTrial> Trials;


        public MeshisSavedData()
        {
            Date = DateTime.UtcNow;
        }

        public void Save()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string specificPath = Path.Combine(documentsPath, "MocuME");
            Directory.CreateDirectory(specificPath);    // if needed

            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"ExperimentData_{dateTime}.json";
            string fullPath = Path.Combine(specificPath, fileName);

            var json = JsonHelper.SerializeJson(this);
            File.WriteAllText(fullPath, json);
        }
    }
}