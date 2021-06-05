using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Airport_GA
{
    class Helper
    {

        public static void TxtColor(RichTextBox rtb, Color color)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = color;
        }
        public static void TxtColor2(RichTextBox rtb, Color color)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = color;
        }

        public static void ColorLine(RichTextBox rtb)
        {
            rtb.DeselectAll();
            foreach (var line in rtb.Lines)
            {
                int firstCharIndx = rtb.GetFirstCharIndexOfCurrentLine();
                int length = line.Length;
                rtb.SelectionStart = firstCharIndx;
                rtb.SelectionLength = length;
                if (line.StartsWith("ALARM"))
                    rtb.SelectionColor = Color.Red;
                else if (line.StartsWith("TROUBLE"))
                    rtb.SelectionColor = Color.Orange;
                else if (line.StartsWith("SUPERV") | line.StartsWith("ACTIVE"))
                    rtb.SelectionColor = Color.IndianRed;
                else
                    rtb.SelectionColor = Color.Gold;
                rtb.Select(firstCharIndx + length + 1, 0);
            }
            rtb.Select(rtb.Text.Length, 0);
        }
        public static void BackupRestoreZones(List<List<RectangleF>> led, List<List<RectangleF>> led_temp,
            List<List<RectangleF>> uniform, List<List<RectangleF>> uniform_temp,
            List<List<PointF[]>> unUniform, List<List<PointF[]>> unUniform_temp, int current_floor, int drawingCount)
        {
            current_floor = (current_floor+1) % drawingCount;
            for (int i = 0; i < drawingCount; i++) // change to number of drawings
            {
                if (i != current_floor)
                {//backup
                    foreach (RectangleF rec in led[i])
                        led_temp[i].Add(rec);
                    foreach (RectangleF rec in uniform[i])
                        uniform_temp[i].Add(rec);
                    foreach (PointF[] rec in unUniform[i])
                        unUniform_temp[i].Add(rec);
                    led[i].Clear();
                    uniform[i].Clear();
                    unUniform[i].Clear();
                }
                else
                {//restore
                    foreach (RectangleF rec in led_temp[current_floor])
                        led[current_floor].Add(rec);
                    foreach (RectangleF rec in uniform_temp[current_floor])
                        uniform[current_floor].Add(rec);
                    foreach (PointF[] rec in unUniform_temp[current_floor])
                        unUniform[current_floor].Add(rec);
                    led_temp[current_floor].Clear();
                    uniform_temp[current_floor].Clear();
                    unUniform_temp[current_floor].Clear();
                }
            }
        }
    }
}
