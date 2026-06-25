using System;
using System.Collections.Generic;
using System.Text;

namespace AiHighlightsWinFormsUi.Rendering
{
    public static class TableRenderer
    {
        /// Formats any array of flat records as an aligned monospace table.
        /// Reads column names and values by reflection, so it works for MatchEvent[],
        /// PlayerAppearance[], and any future record without bespoke code.
        public static string Render<T>(IReadOnlyList<T> rows)
        {
            if (rows.Count == 0) return "(no results)";

            var props = typeof(T).GetProperties();          // column order = declaration order
            var headers = props.Select(p => p.Name).ToArray();

            // Build the cell grid as strings.
            var grid = rows.Select(r => props.Select(p => Format(p.GetValue(r))).ToArray()).ToList();

            // Column width = widest of header and any cell in that column.
            var widths = headers.Select((h, c) =>
                Math.Max(h.Length, grid.Max(row => row[c].Length))).ToArray();

            var sb = new StringBuilder();
            AppendRow(sb, headers, widths);
            AppendRow(sb, widths.Select(w => new string('-', w)).ToArray(), widths);
            foreach (var row in grid) AppendRow(sb, row, widths);
            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, IReadOnlyList<string> cells, int[] widths)
        {
            for (int c = 0; c < cells.Count; c++)
                sb.Append(cells[c].PadRight(widths[c])).Append("  ");
            sb.AppendLine();
        }

        private static string Format(object? value) => value switch
        {
            null => "",
            int i => i.ToString(),
            _ => value.ToString() ?? ""
        };
    }
}
