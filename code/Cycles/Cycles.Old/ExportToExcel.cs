using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Cycles.Old
{
    public class ExportToExcel
    {
        public static void ExportCyclesToExcel(IEnumerable<MergedCycle> mcs, string fileName)
        {
            var workbook = new HSSFWorkbook();
            ISheet? sheet = null;

            int rowNumber = 0;
            foreach (MergedCycle mc in mcs)
            {
                if (mc.Label != sheet?.SheetName)
                {
                    sheet = workbook.CreateSheet(mc.Label);
                    rowNumber = 0;
                }

                var row = sheet.CreateRow(rowNumber++);

                var columnNumber = 0;
                row.CreateCell(columnNumber++).SetCellValue(mc.OrderLevel);
                row.CreateCell(columnNumber++).SetCellValue(mc.GetCypher());

                var maxRowCount = Math.Max(mc.NodeSets.Max(ns => ns.Count), mc.Relationships.Max(rs => rs.Count));
                var rows = new List<IRow>() { row };
                for (int i = 1; i < maxRowCount; i++)
                {
                    rows.Add(sheet.CreateRow(rowNumber++));
                }

                for (int columnIndex = 0; columnIndex < mc.NodeSets.Count; columnIndex++)
                {
                    var ns = mc.NodeSets[columnIndex];
                    var ne = ns.GetEnumerator();
                    for (int rowIndex = 0; rowIndex < ns.Count; rowIndex++)
                    {
                        ne.MoveNext();
                        rows[rowIndex].CreateCell(columnNumber + columnIndex * 2).SetCellValue(ne.Current.Display);
                    }
                }

                for (int columnIndex = 0; columnIndex < mc.Relationships.Count; columnIndex++)
                {
                    var rs = mc.Relationships[columnIndex];
                    var re = rs.GetEnumerator();
                    for (int rowIndex = 0; rowIndex < rs.Count; rowIndex++)
                    {
                        re.MoveNext();
                        var r = re.Current;
                        rows[rowIndex].CreateCell(columnNumber + columnIndex * 2 + 1).SetCellValue($"1/{r.LeftTotalRelationships} {(r.LeftToRight ? ">" : "<")} 1/{r.RightTotalRelationships}");
                    }
                }
            }

            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(stream);
            }
        }
    }
}
