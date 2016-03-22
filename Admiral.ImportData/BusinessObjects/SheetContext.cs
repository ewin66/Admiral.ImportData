using System.Collections.Generic;
using DevExpress.Spreadsheet;

namespace Admiral.ImportData
{
    public class SheetContext
    {
        public SheetContext(Worksheet workbook)
        {
            this.workbook = workbook;
            var cells = workbook.Rows[1];
            for (int i = 1; i <= workbook.Columns.LastUsedIndex; i++)
            {
                if (!cells[i].Value.IsEmpty)
                    CaptionIndex.Add(cells[i].DisplayText, i);
            }
        }



        Worksheet workbook;
        Dictionary<string, int> CaptionIndex = new Dictionary<string, int>();

        public Cell GetErrorCell(string caption, int rowIndex)
        {
            return this.workbook.Cells[rowIndex, CaptionIndex[caption]];
        }
    }
}