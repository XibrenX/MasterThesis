using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Cycles
{
    public static class ExportToExcel
    {
        public static void ExportCyclesToExcel(Dictionary<PathPattern, List<RecursiveCycle>> rcs, string fileName)
        {
            var workbook = new XSSFWorkbook();

            foreach (var (pattern, cycles) in rcs)
            {
                int rowNumber = 0;
                var sheet = workbook.CreateSheet(pattern.ToShortString());
                var r = sheet.CreateRow(0);
                var (NodeLabels, RelationshipLabels, _) = pattern.Extract();
                for (var i = 0; i < NodeLabels.Count; i++)
                {
                    r.CreateCell(i * 2 + 1).SetCellValue(NodeLabels[i].ToString());
                }
                for (var i = 0; i < RelationshipLabels.Count; i++)
                {
                    r.CreateCell(i * 2 + 2).SetCellValue(RelationshipLabels[i].ToString());
                }
                rowNumber = 1;
                sheet.CreateFreezePane(0, 1, 0, 1);

                foreach (RecursiveCycle rc in cycles)
                {
                    var row = sheet.CreateRow(rowNumber);

                    var columnNumber = 0;
                    var endRowNumber = WriteRecursiveCycle(sheet, rowNumber, rc, row, ref columnNumber);

                    rowNumber = endRowNumber + 1;
                }
            }

            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(stream);
            }
        }

        private static int WriteRecursiveCycle(ISheet? sheet, int rowNumber, RecursiveCycle rc, IRow row, ref int columnNumber)
        {
            row.CreateCell(columnNumber++).SetCellValue(rc.GetCypher());

            var aRowNumber = rowNumber;
            var bRowNumber = rowNumber;
            var centerColumn = columnNumber + rc.PathA.Pattern.Length * 2;
            WriteRecursivePath(rc.PathA, sheet, ref aRowNumber, centerColumn, true);
            if (rc is not MirroredRecursiveCycle)
            {
                WriteRecursivePath(rc.PathB, sheet, ref bRowNumber, centerColumn, false);
            }
            var endRowNumber = Math.Max(aRowNumber, bRowNumber);
            return endRowNumber;
        }

        private static void WriteRecursivePath(RecursivePath path, ISheet sheet, ref int rowNumber, int columnNumber, bool inverse)
        {
            sheet.GetOrCreateRow(rowNumber).CreateCell(columnNumber).SetCellValue(path.StartNode.GetDisplay());
            columnNumber = inverse ? columnNumber - 1 : columnNumber + 1;
            var nextNodeColumnNumber = inverse ? columnNumber - 1 : columnNumber + 1;
            if (path.Segments.Count > 0)
                rowNumber -= 1;
            foreach (var segment in path.Segments)
            {
                rowNumber += 1;
                var direction = inverse ? segment.Relationship.LeftNodeDirection.Inverse() : segment.Relationship.LeftNodeDirection;
                sheet.GetOrCreateRow(rowNumber).CreateCell(columnNumber).SetCellValue(direction.GetChar());
                if (segment.To is not null)
                {
                    WriteRecursivePath(segment.To, sheet, ref rowNumber, nextNodeColumnNumber, inverse);
                }
                else
                {
                    sheet.GetOrCreateRow(rowNumber).CreateCell(nextNodeColumnNumber).SetCellValue(segment.NextNode.GetDisplay());
                }
            }
        }

        private static IRow GetOrCreateRow(this ISheet sheet, int rowNumber)
        {
            return sheet.GetRow(rowNumber) ?? sheet.CreateRow(rowNumber);
        }
    }
}
