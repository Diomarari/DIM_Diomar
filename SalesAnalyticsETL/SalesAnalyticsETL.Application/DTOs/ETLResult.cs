using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Application.DTOs
{
    public class ETLResult
    {
        public bool Success { get; set; }
        public int ExtractedRecords { get; set; }
        public int TransformedRecords { get; set; }
        public int LoadedRecords { get; set; }
        public int InvalidRecords { get; set; }
        public int DuplicateRecords { get; set; }
        public long TotalTimeMs { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
