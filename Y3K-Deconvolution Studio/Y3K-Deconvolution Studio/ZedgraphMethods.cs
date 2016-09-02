using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;
using System.Collections.Generic;
using System.Drawing;

namespace Y3K_Deconvolution_Studio
{
    public class ZedgraphMethods
    {
        private List<Color> colors;
        private int colorIndex = 0;
        public List<Color> gradColors = new List<Color>();

        public ZedgraphMethods()
        {
            colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.Navy);
            colors.Add(Color.ForestGreen);
            colors.Add(Color.Goldenrod);
            colors.Add(Color.Purple);
            colors.Add(Color.Orange);
            colors.Add(Color.HotPink);
            colors.Add(Color.DodgerBlue);
        }

        public static void PlotReflection(Chromatogram topChroma, Chromatogram bottomChroma, ZedGraphControl control, bool isColor = true)
        {
            control.GraphPane.CurveList.Clear();
            var ptListTop = new PointPairList();
            var ptListBottom = new PointPairList();
            foreach (var peak in topChroma)
            {
                ptListTop.Add(new PointPair(peak.Time, (peak.Intensity)));
            }
            foreach (var peak in bottomChroma)
            {
                ptListBottom.Add(new PointPair(peak.Time, -(peak.Intensity)));
            }
            var topLine = new LineItem("", ptListTop, Color.DodgerBlue, SymbolType.None);
            var bottomLine = new LineItem("", ptListBottom, Color.Red, SymbolType.None);
            if (!isColor)
            {
                topLine.Line.Color = Color.Gray;
                bottomLine.Line.Color = Color.Gray;
            }
            control.GraphPane.CurveList.Add(topLine);
            control.GraphPane.CurveList.Add(bottomLine);
            control.GraphPane.Title.Text = "Analyte v. Background";
            control.GraphPane.XAxis.Title.Text = "Retention Time (Minutes)";
            control.GraphPane.YAxis.Title.Text = "Intensity";
            control.GraphPane.XAxis.Scale.Min = topChroma.FirstTime;
            control.GraphPane.XAxis.Scale.Max = topChroma.LastTime;
            control.AxisChange();
            control.Update();
            control.Refresh();
        }

        public void PlotXICs(double[,] chroma, ZedGraphControl control)
        {
            var list = new PointPairList();
            for (int i = 0; i < chroma.Length / 2; i++)
            {
                list.Add(chroma[i, 0], chroma[i, 1]);
            }
            var line = new LineItem("", list, colors[colorIndex], SymbolType.None);
            line.Line.IsOptimizedDraw = true;
            line.Line.IsSmooth = true;
            line.Line.Width = 1;
            control.GraphPane.CurveList.Add(line);
            colorIndex++;
            if (colorIndex == colors.Count)
            {
                colorIndex = 0;
            }
        }

        public void PlotXICs(double[,] chroma, ZedGraphControl control, Color color)
        {
            var list = new PointPairList();
            for (int i = 0; i < chroma.Length / 2; i++)
            {
                list.Add(chroma[i, 0], chroma[i, 1]);
            }
            var line = new LineItem("", list, color, SymbolType.None);
            line.Line.IsOptimizedDraw = true;
            line.Line.IsSmooth = true;
            line.Line.Width = 2;
            control.GraphPane.CurveList.Add(line);
            colorIndex++;
            if (colorIndex == colors.Count)
            {
                colorIndex = 0;
            }
        }

        public void PlotChroma(Chromatogram chroma, ZedGraphControl control, Color color)
        {
            control.GraphPane.XAxis.Scale.MaxAuto = true;
            control.GraphPane.XAxis.Scale.MinAuto = true;
            var list = new PointPairList();
            foreach (var peak in chroma)
            {
                list.Add(new PointPair(peak.Time, peak.Intensity));
            }
            var line = new LineItem("", list, color, SymbolType.None);
            line.Line.IsOptimizedDraw = true;
            line.Line.IsSmooth = true;
            control.GraphPane.CurveList.Add(line);
            colorIndex++;
            if (colorIndex == colors.Count)
            {
                colorIndex = 0;
            }
            control.GraphPane.XAxis.Scale.Min = chroma.First().Time;
            control.GraphPane.XAxis.Scale.Max = chroma.Last().Time;
        }

        public void PlotXICs(List<RTPeak> xic, ZedGraphControl control)
        {
            var list = new PointPairList();
            foreach (var peak in xic)
            {
                list.Add(peak.RT, peak.Intensity);
            }
            var line = new LineItem("", list, colors[colorIndex], SymbolType.None);
            //line.Line.IsOptimizedDraw = true;
            line.Line.IsSmooth = true;
            line.Line.Width = 1;
            control.GraphPane.CurveList.Add(line);
            colorIndex++;
            if (colorIndex == colors.Count)
            {
                colorIndex = 0;
            }
        }

        //public void PlotXICs(List<RTPeak> xic, ZedGraphControl control, int size)
        //{
        //    var list = new PointPairList();
        //    foreach (var peak in xic)
        //    {
        //        list.Add(peak.RT, peak.Intensity);
        //    }
        //    var line = new LineItem("", list, colors[colorIndex], SymbolType.None);
        //    //line.Line.IsOptimizedDraw = true;
        //    line.Line.IsSmooth = true;
        //    line.Line.Width = size;
        //    control.GraphPane.CurveList.Add(line);
        //    colorIndex++;
        //    if (colorIndex == colors.Count)
        //    {
        //        colorIndex = 0;
        //    }
        //}

        public void PlotXICs(List<RTPeak> xic, ZedGraphControl control, Color color)
        {
            control.GraphPane.XAxis.Scale.MaxAuto = true;
            control.GraphPane.XAxis.Scale.MinAuto = true;
            var list = new PointPairList();
            foreach (var peak in xic)
            {
                list.Add(peak.RT, peak.Intensity);
            }
            var line = new LineItem("", list, color, SymbolType.None);
            line.Line.IsOptimizedDraw = true;
            line.Line.Width = 1;
            control.GraphPane.CurveList.Add(line);
            colorIndex++;
            if (colorIndex == colors.Count)
            {
                colorIndex = 0;
            }
        }

        public void PlotXICs(List<RTPeak> xic, ZedGraphControl control, int index)
        {
            control.GraphPane.XAxis.Scale.MaxAuto = true;
            control.GraphPane.XAxis.Scale.MinAuto = true;
            var list = new PointPairList();
            foreach (var peak in xic)
            {
                list.Add(peak.RT, peak.Intensity);
            }
            var color = colors[index % colors.Count];
            var line = new LineItem("", list, color, SymbolType.None);
            line.Line.IsOptimizedDraw = true;
            line.Line.Width = 1;
            control.GraphPane.CurveList.Add(line);
            colorIndex++;
            if (colorIndex == colors.Count)
            {
                colorIndex = 0;
            }
        }

        public void PlotXICs(List<Feature> features, ZedGraphControl control, Color color)
        {
            control.GraphPane.XAxis.Scale.MaxAuto = true;
            control.GraphPane.XAxis.Scale.MinAuto = true;
            foreach (var feature in features)
            {
                var list = new PointPairList();
                foreach (var peak in feature.smoothRTPeaks)
                {
                    list.Add(peak.RT, peak.Intensity);
                }
                var line = new LineItem("", list, color, SymbolType.None);
                line.Tag = feature.ID_Number;
                line.Line.IsOptimizedDraw = true;
                line.Line.Width = 1;
                control.GraphPane.CurveList.Add(line);
                colorIndex++;
                if (colorIndex == colors.Count)
                {
                    colorIndex = 0;
                }
            }
        }

        public void PlotXICs(List<RTPeak> xic, ZedGraphControl control, Color color, double threshold = 0)
        {
            control.GraphPane.XAxis.Scale.MaxAuto = true;
            control.GraphPane.XAxis.Scale.MinAuto = true;
            double max = 0;
            var list = new PointPairList();
            foreach (var peak in xic)
            {
                list.Add(peak.RT, peak.Intensity);
                if (peak.Intensity > max)
                {
                    max = peak.Intensity;
                }
            }
            if (max > threshold)
            {
                var line = new LineItem("", list, color, SymbolType.None);
                line.Line.IsOptimizedDraw = true;
                line.Line.Width = 1;
                control.GraphPane.CurveList.Add(line);
                colorIndex++;
                if (colorIndex == colors.Count)
                {
                    colorIndex = 0;
                }
            }
        }

        public void PlotSpectralOverlap(ZedGraphControl control, List<MZPeak> top, List<MZPeak> bottom, Color topColor, Color bottomColor, string title = "")
        {
            foreach (var peak in top)
            {
                var topList = new PointPairList();
                topList.Add(new PointPair(peak.MZ, peak.Intensity));
                topList.Add(new PointPair(peak.MZ, 0));
                var line = new LineItem("", topList, topColor, SymbolType.None);
                line.Line.Width = 2;
                control.GraphPane.CurveList.Add(line);
            }
            foreach (var peak in bottom)
            {
                var bottomList = new PointPairList();
                bottomList.Add(new PointPair(peak.MZ, -peak.Intensity));
                bottomList.Add(new PointPair(peak.MZ, 0));
                var line = new LineItem("", bottomList, bottomColor, SymbolType.None);
                line.Line.Width = 2;
                control.GraphPane.CurveList.Add(line);
            }


            //var topList = new PointPairList();
            //foreach (var peak in top)
            //{
            //    topList.Add(new PointPair(peak.MZ, peak.Intensity));
            //}
            //var bottomList = new PointPairList();
            //foreach (var peak in bottom)
            //{
            //    bottomList.Add(new PointPair(peak.MZ, -peak.Intensity));
            //}
            //var topBar = control.GraphPane.AddBar(title, topList, topColor);
            //topBar.Bar.Fill.Type = FillType.Solid;
            //topBar.Bar.Border.IsVisible = true;
            //topBar.Bar.Border.Width = 1;
            //topBar.Bar.Border.Color = topColor;
            //var bottomBar = control.GraphPane.AddBar("", bottomList, bottomColor);
            //bottomBar.Bar.Fill.Type = FillType.Solid;
            //bottomBar.Bar.Border.IsVisible = true;
            //bottomBar.Bar.Border.Width = 1;
            //bottomBar.Bar.Border.Color = bottomColor;
        }

        public void PlotTICOverlap(ZedGraphControl control, FeatureGroup top, FeatureGroup bottom, List<Feature> otherTop, List<EISpectrum> otherBottom,
            Color topColor, Color bottomColor, double apexTime, double threshold = 0)
        {
            //find max intensity analyte and background
            double analyteMax = 0;
            double backgroundMax = 0;
            foreach (var feat in top.allFeatures)
            {
                var ptList = new PointPairList();
                bool overThresh = false;
                foreach (var peak in feat.smoothRTPeaks)
                {
                    ptList.Add(new PointPair(peak.RT, peak.Intensity));
                    if (peak.Intensity > analyteMax)
                    {
                        analyteMax = peak.Intensity;
                    }
                    if (peak.Intensity > threshold)
                    {
                        overThresh = true;
                    }
                }
                if (overThresh)
                {
                    var topLine = new LineItem("", ptList, topColor, SymbolType.None);
                    topLine.Tag = feat.ID_Number;
                    topLine.Line.IsOptimizedDraw = true;
                    control.GraphPane.CurveList.Add(topLine);
                }
            }
            foreach (var feat in bottom.allFeatures)
            {
                var ptList = new PointPairList();
                foreach (var peak in feat.smoothRTPeaks)
                {
                    ptList.Add(new PointPair(peak.RT, -peak.Intensity));
                    bool overThresh = false;
                    if (peak.Intensity > backgroundMax)
                    {
                        backgroundMax = peak.Intensity;
                    }
                    if (peak.Intensity > threshold)
                    {
                        overThresh = true;
                    }
                    if (overThresh)
                    {
                        var bottomLine = new LineItem("", ptList, bottomColor, SymbolType.None);
                        bottomLine.Tag = feat.ID_Number;
                        bottomLine.Line.IsOptimizedDraw = true;
                        control.GraphPane.CurveList.Add(bottomLine);
                    }
                }
            }

            backgroundMax = 0;

            foreach (var feat in otherTop)
            {
                bool overThresh = false;
                var ptList = new PointPairList();
                foreach (var peak in feat.smoothRTPeaks)
                {
                    ptList.Add(new PointPair(peak.RT, peak.Intensity));
                    if (peak.Intensity > threshold)
                    {
                        overThresh = true;
                    }
                }
                if (overThresh)
                {
                    var line = new LineItem("", ptList, Color.LightGray, SymbolType.None);
                    line.Tag = feat.ID_Number;
                    line.Line.IsOptimizedDraw = true;
                    control.GraphPane.CurveList.Add(line);
                }
            }
            foreach (var group in otherBottom)
            {
                foreach (var feat in group.FeatureGroup.allFeatures)
                {
                    bool overThresh = false;
                    var ptList = new PointPairList();
                    foreach (var peak in feat.smoothRTPeaks)
                    {
                        ptList.Add(new PointPair(peak.RT, -peak.Intensity));
                        if (peak.Intensity > threshold)
                        {
                            overThresh = true;
                        }
                    }
                    if (overThresh)
                    {
                        var line = new LineItem("", ptList, Color.LightGray, SymbolType.None);
                        line.Tag = feat.ID_Number;
                        line.Line.IsOptimizedDraw = true;
                        control.GraphPane.CurveList.Add(line);
                    }
                }
            }
            double max = Math.Max(analyteMax, backgroundMax);
            control.GraphPane.YAxis.Scale.Max = max * 1.25;
            control.GraphPane.YAxis.Scale.Min = (-max * 1.25);
            control.GraphPane.XAxis.Scale.Min = apexTime - .1;
            control.GraphPane.XAxis.Scale.Max = apexTime + .1;
        }

        public void PlotSpectrum(ZedGraphControl control, List<MZPeak> peaks, Color color, string Title = "",
            bool positive = true, bool resetScale = true)
        {
            peaks = peaks.OrderBy(x => x.MZ).ToList();
            PointPairList Spectrum = new PointPairList();
            if (positive)
            {
                //foreach (MZPeak peak in peaks)
                //{
                //    Spectrum.Add(new PointPair(Math.Round(peak.MZ), peak.Intensity));
                //}
                //BarItem userBar = control.GraphPane.AddBar(Title, Spectrum, color);
                //userBar.Bar.Fill.Type = FillType.Solid;
                //userBar.Bar.Border.IsVisible = false;
                foreach (var peak in peaks)
                {
                    var ptList = new PointPairList();
                    ptList.Add(new PointPair(peak.MZ, 0));
                    ptList.Add(new PointPair(peak.MZ, peak.Intensity));
                    var line = new LineItem("", ptList, color, SymbolType.None);
                    line.Line.Width = 2;
                    control.GraphPane.CurveList.Add(line);
                }
            }
            else
            {
                //foreach (MZPeak peak in peaks)
                //{
                //    Spectrum.Add(new PointPair(Math.Round(peak.MZ), -peak.Intensity));
                //}
                //BarItem userBar = control.GraphPane.AddBar(Title, Spectrum, color);
                //userBar.Bar.Fill.Type = FillType.Solid;
                //userBar.Bar.Border.IsVisible = false;
                foreach (var peak in peaks)
                {
                    var ptList = new PointPairList();
                    ptList.Add(new PointPair(peak.MZ, 0));
                    ptList.Add(new PointPair(peak.MZ, -peak.Intensity));
                    var line = new LineItem("", ptList, color, SymbolType.None);
                    line.Line.Width = 2;
                    control.GraphPane.CurveList.Add(line);
                }
            }
            if (resetScale)
            {
                control.GraphPane.XAxis.Scale.Min = (int)peaks.First().MZ - 10;
                control.GraphPane.XAxis.Scale.Max = (int)peaks.Last().MZ + 10;
            }
        }

        public void PlotTriange(double x, double y, ZedGraphControl control)
        {
            var trianglePt = new PointPair(x, (y));
            var list = new PointPairList();
            list.Add(trianglePt);
            var lineItem = new LineItem("Selected Point", list, Color.Red, SymbolType.TriangleDown);
            lineItem.Symbol.Fill = new Fill(Color.Red);
            lineItem.Line.IsVisible = false;
            control.GraphPane.CurveList.Add(lineItem);
        }

        public void ChangeTitle(string title, ZedGraphControl control)
        {
            control.GraphPane.Title.Text = title;
        }

        public void ChangeXAxisTitle(string title, ZedGraphControl control)
        {
            control.GraphPane.XAxis.Title.Text = title;
        }

        public void ChangeYAxisTitle(string title, ZedGraphControl control)
        {
            control.GraphPane.YAxis.Title.Text = title;
        }

        public static void ClearZedgraph(ZedGraphControl control)
        {
            control.GraphPane.CurveList.Clear();
            control.GraphPane.GraphObjList.Clear();
        }

        public static void UpdateZedgraph(ZedGraphControl control)
        {
            control.AxisChange();
            control.Update();
            control.Refresh();
        }

        public static void ClearZedGraph(ZedGraphControl control)
        {
            control.GraphPane.CurveList.Clear();
            control.GraphPane.GraphObjList.Clear();
        }

        public static void UpdateZedGraph(ZedGraphControl control)
        {
            control.AxisChange();
            control.Update();
            control.Refresh();
        }

        public void SetYToZero(ZedGraphControl control)
        {
            control.GraphPane.YAxis.Scale.Min = 0;
        }

        private void AddLabel(string name, double x, double y, bool rotate = false)
        {
            var text = new TextObj(name, x, y);
            text.FontSpec.Border.IsVisible = false;
            text.FontSpec.Size = 8;
            if (rotate)
            {
                text.FontSpec.Angle = 90;
            }
            //zedGraphControl1.GraphPane.GraphObjList.Add(text);
        }

        public static List<Color> GetGradientColors(Color start, Color end, int steps)
        {
            return GetGradientColors(start, end, steps, 0, steps - 1);
        }

        public static List<Color> GetGradientColors(Color start, Color end, int steps, int firstStep, int lastStep)
        {
            var colorList = new List<Color>();
            if (steps <= 0 || firstStep < 0 || lastStep > steps - 1)
                return colorList;

            double aStep = (end.A - start.A) / steps;
            double rStep = (end.R - start.R) / steps;
            double gStep = (end.G - start.G) / steps;
            double bStep = (end.B - start.B) / steps;

            for (int i = firstStep; i < lastStep; i++)
            {
                var a = start.A + (int)(aStep * i);
                var r = start.R + (int)(rStep * i);
                var g = start.G + (int)(gStep * i);
                var b = start.B + (int)(bStep * i);
                colorList.Add(Color.FromArgb(a, r, g, b));
            }

            return colorList;
        }
    }
}
