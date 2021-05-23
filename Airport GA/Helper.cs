using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        public static void ColorLine(string sigType, RichTextBox rtb)
        {
            string text = rtb.Text;
            foreach(var line in rtb.Lines)
            {
                if (line.Contains("ALARM"))
                {
                    int firstCharIndx = rtb.GetFirstCharIndexOfCurrentLine();
                    int currentLine = rtb.GetLineFromCharIndex(firstCharIndx);
                    rtb.Select(firstCharIndx, 10);
                    rtb.SelectionColor = Color.Red;
                    rtb.DeselectAll();
                    rtb.Select(rtb.Text.Length, 0);
                }
                else if (line.Contains("TROUBLE"))
                {
                    int firstCharIndx = rtb.GetFirstCharIndexOfCurrentLine();
                    int currentLine = rtb.GetLineFromCharIndex(firstCharIndx);
                    rtb.Select(firstCharIndx, 10);
                    rtb.SelectionColor = Color.Orange;
                    rtb.DeselectAll();
                    rtb.Select(rtb.Text.Length, 0);
                }
                else if (line.Contains("SUPERVISORY"))
                {
                    int firstCharIndx = rtb.GetFirstCharIndexOfCurrentLine();
                    int currentLine = rtb.GetLineFromCharIndex(firstCharIndx);
                    rtb.Select(firstCharIndx, 10);
                    rtb.SelectionColor = Color.IndianRed;
                    rtb.DeselectAll();
                    rtb.Select(rtb.Text.Length, 0);
                }
                else
                {
                    int firstCharIndx = rtb.GetFirstCharIndexOfCurrentLine();
                    int currentLine = rtb.GetLineFromCharIndex(firstCharIndx);
                    rtb.Select(firstCharIndx, 10);
                    rtb.SelectionColor = Color.Gold;
                    rtb.DeselectAll();
                    rtb.Select(rtb.Text.Length, 0);
                }
            }
        }
        public static void HighlightLineContaining(RichTextBox rtb, int line, string search, Color color)
        {
            int c0 = rtb.GetFirstCharIndexFromLine(line);
            int c1 = rtb.GetFirstCharIndexFromLine(line + 1);
            if (c1 < 0)
                c1 = rtb.Text.Length;
            rtb.SelectionStart = c0;
            rtb.SelectionLength = c1 - c0;
            if (rtb.SelectedText.Length > 3)
                if (rtb.SelectedText.Contains(search))
                    rtb.SelectionColor = color;
            rtb.SelectionLength = 0;
        }
        //public static string GetMD5HashFromFile(string fileName)
        //{
        //    if (File.Exists(fileName))
        //    {
        //        MD5 md5 = new MD5CryptoServiceProvider();
        //        byte[] retVal;
        //        while (true)
        //        {
        //            try
        //            {
        //            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //                {
        //                retVal = md5.ComputeHash(stream);
        //                stream.Close();
        //                }
        //                break;
        //            }
        //            catch
        //            {

        //            }
        //        }

        //        StringBuilder sb = new StringBuilder();
        //        for (int i = 0; i < retVal.Length; i++)
        //        {
        //            sb.Append(retVal[i].ToString("x2"));
        //        }
        //        return sb.ToString();
        //    }
        //    else
        //        return null;
        //}

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
