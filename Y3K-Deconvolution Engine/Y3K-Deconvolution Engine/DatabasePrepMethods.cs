using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Y3K_Deconvolution_Engine
{
    public class DatabasePrepMethods
    {
        public event EventHandler<ProgressStatusEventArgs> Progress;

        public event EventHandler Finished;

        private void OnProgressUpdate(double percent)
        {
            var handler = Progress;
            if (handler != null)
            {
                handler(this, new ProgressStatusEventArgs(percent));
            }
        }

        private void OnFinish()
        {
            var handler = Finished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void PrepDatabase(string rawFilePath, string outputDirectory)
        {
            var rawFile = new ThermoRawFile(rawFilePath);
            rawFile.Open();
            var path = outputDirectory + "\\" + rawFile.Name + "_ExtractedFeatures.gcfeat";
            var conn = new SQLiteConnection(@"Data Source=" + path);
            conn.Open();
            SQLiteIOMethods.CreateTablesInDatabase(conn);
            SQLiteIOMethods.AddRawFileEntry(rawFile, conn);
            SQLiteIOMethods.AddRawFileTICChroma(rawFile, conn);
            SQLiteIOMethods.CreateIndices(conn);
            conn.Close();
            conn.Dispose();
            rawFile.ClearCachedScans();
            rawFile.Dispose();
            OnFinish();
        }
    }
}
