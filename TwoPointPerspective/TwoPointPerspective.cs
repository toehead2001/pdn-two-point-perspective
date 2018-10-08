using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace TwoPointPerspectiveEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=84519");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Two-point Perspective")]
    public class TwoPointPerspectiveEffectPlugin : PropertyBasedEffect
    {
        private const double rad180 = Math.PI / 180 * 180;
        private const double rad360 = Math.PI / 180 * 360;

        private Surface CuboidSurface;

        private const string StaticName = "Two-point Perspective";
        private static readonly Image StaticIcon = new Bitmap(typeof(TwoPointPerspectiveEffectPlugin), "TwoPointPerspective.png");

        public TwoPointPerspectiveEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuNames.Render, EffectFlags.Configurable)
        {
        }

        private enum PropertyNames
        {
            Height,
            Length,
            Width,
            HiddenEdges,
            VanishingPts,
            VanishingPtsPos,
            CuboidPos,
            EdgeOutlineWidth,
            EdgeOutlineColor,
            FillStyle,
            FillColor,
            VanishingPtsGuides
        }

        private enum FillStyle
        {
            None,
            Solid,
            Shaded
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Height, 100, 0, 1000),
                new Int32Property(PropertyNames.Length, 200, 0, 1000),
                new Int32Property(PropertyNames.Width, 150, 0, 1000),
                new DoubleVectorProperty(PropertyNames.VanishingPtsPos, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)),
                new BooleanProperty(PropertyNames.VanishingPts, false),
                new BooleanProperty(PropertyNames.VanishingPtsGuides, false),
                new DoubleVectorProperty(PropertyNames.CuboidPos, Pair.Create(0.0, 1.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)),
                new Int32Property(PropertyNames.EdgeOutlineWidth, 2, 0, 10),
                new BooleanProperty(PropertyNames.HiddenEdges, true),
                new Int32Property(PropertyNames.EdgeOutlineColor, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff),
                StaticListChoiceProperty.CreateForEnum<FillStyle>(PropertyNames.FillStyle, 0, false),
                new Int32Property(PropertyNames.FillColor, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.SecondaryColor.B, EnvironmentParameters.SecondaryColor.G, EnvironmentParameters.SecondaryColor.R, 255)), 0, 0xffffff)
            };

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.FillColor, PropertyNames.FillStyle, FillStyle.None, false),
                new ReadOnlyBoundToValueRule<int, Int32Property>(PropertyNames.HiddenEdges, PropertyNames.EdgeOutlineWidth, 0, false),
                new ReadOnlyBoundToValueRule<int, Int32Property>(PropertyNames.EdgeOutlineColor, PropertyNames.EdgeOutlineWidth, 0, false)
            };

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Height, ControlInfoPropertyNames.DisplayName, "Height");
            configUI.SetPropertyControlValue(PropertyNames.Length, ControlInfoPropertyNames.DisplayName, "Length");
            configUI.SetPropertyControlValue(PropertyNames.Width, ControlInfoPropertyNames.DisplayName, "Width");
            configUI.SetPropertyControlValue(PropertyNames.HiddenEdges, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.HiddenEdges, ControlInfoPropertyNames.Description, "Draw Hidden Edges");
            configUI.SetPropertyControlValue(PropertyNames.VanishingPts, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPts, ControlInfoPropertyNames.Description, "Draw Vanishing Points");
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsGuides, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsGuides, ControlInfoPropertyNames.Description, "Draw Vanishing Points Guides");
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.DisplayName, "Vanishing Point Position (y-axis only)");
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.DecimalPlaces, 3);
            Rectangle selection6 = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource imageResource6 = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection6));
            configUI.SetPropertyControlValue(PropertyNames.VanishingPtsPos, ControlInfoPropertyNames.StaticImageUnderlay, imageResource6);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.DisplayName, "Cuboid Position");
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.DecimalPlaces, 3);
            Rectangle selection7 = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource imageResource7 = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection7));
            configUI.SetPropertyControlValue(PropertyNames.CuboidPos, ControlInfoPropertyNames.StaticImageUnderlay, imageResource7);
            configUI.SetPropertyControlValue(PropertyNames.EdgeOutlineWidth, ControlInfoPropertyNames.DisplayName, "Edge Outline Width");
            configUI.SetPropertyControlValue(PropertyNames.EdgeOutlineColor, ControlInfoPropertyNames.DisplayName, "Edge Outline Color");
            configUI.SetPropertyControlType(PropertyNames.EdgeOutlineColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.FillStyle, ControlInfoPropertyNames.DisplayName, "Fill Style");
            PropertyControlInfo Amount10Control = configUI.FindControlForPropertyName(PropertyNames.FillStyle);
            Amount10Control.SetValueDisplayName(FillStyle.None, "None");
            Amount10Control.SetValueDisplayName(FillStyle.Solid, "Solid");
            Amount10Control.SetValueDisplayName(FillStyle.Shaded, "Shaded");
            configUI.SetPropertyControlValue(PropertyNames.FillColor, ControlInfoPropertyNames.DisplayName, "Fill Color");
            configUI.SetPropertyControlType(PropertyNames.FillColor, PropertyControlType.ColorWheel);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            int height = newToken.GetProperty<Int32Property>(PropertyNames.Height).Value;
            int length = newToken.GetProperty<Int32Property>(PropertyNames.Length).Value;
            int width = newToken.GetProperty<Int32Property>(PropertyNames.Width).Value;
            bool hiddenEdges = newToken.GetProperty<BooleanProperty>(PropertyNames.HiddenEdges).Value;
            bool vanishingPts = newToken.GetProperty<BooleanProperty>(PropertyNames.VanishingPts).Value;
            Pair<double, double> vanishingPtsPos = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.VanishingPtsPos).Value;
            Pair<double, double> cuboidPos = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.CuboidPos).Value;
            int edgeOutlineWidth = newToken.GetProperty<Int32Property>(PropertyNames.EdgeOutlineWidth).Value;
            ColorBgra edgeOutlineColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.EdgeOutlineColor).Value);
            FillStyle fillStyle = (FillStyle)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.FillStyle).Value;
            ColorBgra fillColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.FillColor).Value);
            bool vanishingPtsGuides = newToken.GetProperty<BooleanProperty>(PropertyNames.VanishingPtsGuides).Value;

            if (CuboidSurface == null)
            {
                CuboidSurface = new Surface(srcArgs.Size);
            }

            CuboidSurface.CopySurface(srcArgs.Surface);

            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            PointF selCenter = new PointF
            {
                X = ((selection.Right - selection.Left) / 2f) + selection.Left,
                Y = ((selection.Bottom - selection.Top) / 2f) + selection.Top
            };

            using (Graphics g = new RenderArgs(CuboidSurface).Graphics)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                #region Vanishing Points
                PointF leftVp = new PointF
                {
                    X = selection.Left,
                    Y = (float)(selCenter.Y + selCenter.Y * vanishingPtsPos.Second)
                };

                PointF rightVp = new PointF
                {
                    X = selection.Right,
                    Y = leftVp.Y
                };
                #endregion

                #region Front Bottom & Front Top points
                PointF frontBottom = new PointF
                {
                    X = (float)(selCenter.X + selCenter.X * cuboidPos.First),
                    Y = (float)(((selection.Bottom - 2) / 2f) + ((selection.Bottom - 2) * cuboidPos.Second / 2f))
                };

                PointF frontTop = new PointF
                {
                    X = frontBottom.X,
                    Y = frontBottom.Y - height
                };
                #endregion

                if (length > frontBottom.X - leftVp.X || width > rightVp.X - frontBottom.X)
                {
                    using (Font font = new Font(new FontFamily("Arial"), 14))
                    {
                        g.DrawString("Cuboid is Out of Bounds", font, Brushes.Red, selection.Left + 15, selection.Top + 15);
                    }
                    base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
                    return;
                }

                #region Left Bottom point
                double frontLeftBottomAngle = Math.Atan((frontBottom.Y - leftVp.Y) / (frontBottom.X - leftVp.X));
                double frontLeftBottomLength = length / Math.Cos(frontLeftBottomAngle);

                PointF leftBottom = new PointF
                {
                    X = frontBottom.X - length,
                    Y = (float)(frontBottom.Y - Math.Tan(frontLeftBottomAngle) * length)
                };
                #endregion

                #region Left Top point
                double frontLeftTopAngle = Math.Atan((frontTop.Y - leftVp.Y) / (frontTop.X - leftVp.X));
                double frontLeftTopLength = length / Math.Cos(frontLeftTopAngle);

                PointF leftTop = new PointF
                {
                    X = frontTop.X - length,
                    Y = (float)(frontTop.Y - Math.Tan(frontLeftTopAngle) * length)
                };
                #endregion

                #region Right Bottom point
                double frontRightBottomAngle = Math.Atan((frontBottom.Y - rightVp.Y) / (rightVp.X - frontBottom.X));
                double frontRightBottomLength = width / Math.Cos(frontRightBottomAngle);

                PointF rightBottom = new PointF
                {
                    X = frontBottom.X + width,
                    Y = (float)(frontBottom.Y - Math.Tan(frontRightBottomAngle) * width)
                };
                #endregion

                #region Right Top point
                double frontRightTopAngle = Math.Atan((frontTop.Y - rightVp.Y) / (rightVp.X - frontTop.X));
                double frontRightTopLength = width / Math.Cos(frontRightTopAngle);

                PointF rightTop = new PointF
                {
                    X = frontTop.X + width,
                    Y = (float)(frontTop.Y - Math.Tan(frontRightTopAngle) * width)
                };
                #endregion

                #region Back Top point
                double backLeftTopAngle = Math.Atan((leftTop.Y - rightVp.Y) / (rightVp.X - leftTop.X));
                double backRightTopAngle = Math.Atan((rightTop.Y - leftVp.Y) / (rightTop.X - leftVp.X));

                // Headache ahead
                double topFaceAngle1 = rad180 - frontLeftTopAngle - frontRightTopAngle;
                double topFaceAngle2 = backLeftTopAngle + frontLeftTopAngle;
                double topFaceAngle3 = backRightTopAngle + frontRightTopAngle;
                double topFaceAngle4 = rad360 - topFaceAngle3 - topFaceAngle2 - topFaceAngle1;

                double topFaceLength = Math.Sqrt((Math.Pow(frontLeftTopLength, 2) + Math.Pow(frontRightTopLength, 2)) - (2 * frontLeftTopLength * frontRightTopLength * Math.Cos(topFaceAngle1))); // SAS
                double topFaceHelperAngle1 = Math.Asin(frontRightTopLength / (topFaceLength / Math.Sin(topFaceAngle1))); // ASS
                double topFacehelperAngle2 = topFaceAngle2 - topFaceHelperAngle1;

                double backRightTopLength = topFaceLength * Math.Sin(topFacehelperAngle2) / Math.Sin(topFaceAngle4); // AAS

                PointF backTop = new PointF
                {
                    X = (float)(rightTop.X - (backRightTopLength * Math.Cos(backRightTopAngle))),
                    Y = (float)(rightTop.Y - (backRightTopLength * Math.Sin(backRightTopAngle)))
                };

                bool topFaceVisible = (backTop.Y < frontTop.Y);
                #endregion

                #region Back Bottom point
                double backLeftBottomAngle = Math.Atan((leftBottom.Y - rightVp.Y) / (rightVp.X - leftBottom.X));
                double backRightBottomAngle = Math.Atan((rightBottom.Y - leftVp.Y) / (rightBottom.X - leftVp.X));

                // Headache ahead
                double bottomFaceAngle1 = rad180 - frontLeftBottomAngle - frontRightBottomAngle;
                double bottomFaceAngle2 = backLeftBottomAngle + frontLeftBottomAngle;
                double bottomFaceAngle3 = backRightBottomAngle + frontRightBottomAngle;
                double bottomFaceAngle4 = rad360 - bottomFaceAngle3 - bottomFaceAngle2 - bottomFaceAngle1;

                double bottomFaceLength = Math.Sqrt((Math.Pow(frontLeftBottomLength, 2) + Math.Pow(frontRightBottomLength, 2)) - (2 * frontLeftBottomLength * frontRightBottomLength * Math.Cos(bottomFaceAngle1))); // SAS
                double bottomFaceHelperAngle1 = Math.Asin(frontRightBottomLength / (bottomFaceLength / Math.Sin(bottomFaceAngle1))); // AAS
                double bottomFaceHelperAngle2 = bottomFaceAngle2 - bottomFaceHelperAngle1;

                double backRightBottomLength = bottomFaceLength * Math.Sin(bottomFaceHelperAngle2) / Math.Sin(bottomFaceAngle4); // AAS

                PointF backBottom = new PointF
                {
                    X = (float)(rightBottom.X - (backRightBottomLength * Math.Cos(backRightBottomAngle))),
                    Y = (float)(rightBottom.Y - (backRightBottomLength * Math.Sin(backRightBottomAngle)))
                };

                bool bottomFaceVisible = (backBottom.Y > frontBottom.Y);
                #endregion

                System.Diagnostics.Debug.Assert(!(topFaceVisible && bottomFaceVisible), "Both Top and Bottom faces shouldn't be visible at the same time!!");

                #region Fill sides
                switch (fillStyle)
                {
                    case FillStyle.Solid:
                        using (SolidBrush fillBrush = new SolidBrush(fillColor))
                        {
                            if (topFaceVisible)
                            {
                                PointF[] fillPoints = { leftTop, backTop, rightTop, rightBottom, frontBottom, leftBottom };
                                g.FillPolygon(fillBrush, fillPoints);
                            }
                            else if (bottomFaceVisible)
                            {
                                PointF[] fillPoints = { leftTop, frontTop, rightTop, rightBottom, backBottom, leftBottom };
                                g.FillPolygon(fillBrush, fillPoints);
                            }
                            else
                            {
                                PointF[] fillPoints = { leftTop, frontTop, rightTop, rightBottom, frontBottom, leftBottom };
                                g.FillPolygon(fillBrush, fillPoints);
                            }
                        }
                        break;
                    case FillStyle.Shaded:
                        HsvColor fillColorBase = HsvColor.FromColor(fillColor);
                        fillColorBase.Saturation = 100;

                        HsvColor fillColorDark = fillColorBase;
                        fillColorDark.Value = 80;

                        HsvColor fillColorLight = fillColorBase;
                        fillColorLight.Saturation = 66;

                        HsvColor fillColorLighter = fillColorBase;
                        fillColorLighter.Saturation = 33;

                        using (SolidBrush fillBrush = new SolidBrush(fillColor))
                        using (Pen seamPen = new Pen(fillColor, 1))
                        {
                            PointF[] leftFillPoints = { frontBottom, leftBottom, leftTop, frontTop };
                            fillBrush.Color = fillColorLight.ToColor();
                            g.FillPolygon(fillBrush, leftFillPoints);
                            seamPen.Color = fillBrush.Color;
                            g.DrawPolygon(seamPen, leftFillPoints);

                            PointF[] rightFillPoints = { frontBottom, rightBottom, rightTop, frontTop };
                            fillBrush.Color = fillColorBase.ToColor();
                            g.FillPolygon(fillBrush, rightFillPoints);
                            seamPen.Color = fillBrush.Color;
                            g.DrawPolygon(seamPen, rightFillPoints);

                            if (topFaceVisible)
                            {
                                PointF[] topFillPoints = { frontTop, leftTop, backTop, rightTop };
                                fillBrush.Color = fillColorLighter.ToColor();
                                g.FillPolygon(fillBrush, topFillPoints);
                            }
                            else if (bottomFaceVisible)
                            {
                                PointF[] bottomFillPoints = { backBottom, leftBottom, frontBottom, rightBottom };
                                fillBrush.Color = fillColorDark.ToColor();
                                g.FillPolygon(fillBrush, bottomFillPoints);
                            }
                        }
                        break;
                }
                #endregion

                #region Draw Edges
                if (edgeOutlineWidth != 0)
                {
                    using (Pen visiblePen = new Pen(edgeOutlineColor, edgeOutlineWidth))
                    using (Pen hiddenPen = new Pen(edgeOutlineColor, edgeOutlineWidth))
                    {
                        visiblePen.StartCap = LineCap.Round;
                        visiblePen.EndCap = LineCap.Round;

                        hiddenPen.DashStyle = DashStyle.Dot;
                        visiblePen.StartCap = LineCap.Round;
                        visiblePen.EndCap = LineCap.Round;

                        g.DrawLine(visiblePen, frontTop, frontBottom);

                        g.DrawLine(visiblePen, leftTop, frontTop);
                        g.DrawLine(visiblePen, leftBottom, frontBottom);
                        g.DrawLine(visiblePen, leftTop, leftBottom);

                        g.DrawLine(visiblePen, rightTop, frontTop);
                        g.DrawLine(visiblePen, rightBottom, frontBottom);
                        g.DrawLine(visiblePen, rightTop, rightBottom);

                        if (topFaceVisible)
                        {
                            g.DrawLine(visiblePen, backTop, rightTop);
                            g.DrawLine(visiblePen, backTop, leftTop);
                        }
                        else if (hiddenEdges)
                        {
                            g.DrawLine(hiddenPen, backTop, rightTop);
                            g.DrawLine(hiddenPen, backTop, leftTop);
                        }

                        if (bottomFaceVisible)
                        {
                            g.DrawLine(visiblePen, backBottom, rightBottom);
                            g.DrawLine(visiblePen, backBottom, leftBottom);
                        }
                        else if (hiddenEdges)
                        {
                            g.DrawLine(hiddenPen, backBottom, rightBottom);
                            g.DrawLine(hiddenPen, backBottom, leftBottom);
                        }

                        if (hiddenEdges)
                            g.DrawLine(hiddenPen, backBottom, backTop);
                    }
                }
                #endregion

                #region Draw Vanishing Points
                if (vanishingPts)
                {
                    g.FillRectangle(Brushes.Red, leftVp.X, leftVp.Y - 2, 4, 4);
                    g.FillRectangle(Brushes.Blue, rightVp.X - 4, rightVp.Y - 2, 4, 4);
                }

                if (vanishingPtsGuides)
                {
                    using (Pen guidePen = new Pen(Color.Red, 1))
                    {
                        guidePen.DashStyle = DashStyle.Dot;

                        g.DrawLine(guidePen, leftVp, frontBottom);
                        g.DrawLine(guidePen, leftVp, frontTop);
                        g.DrawLine(guidePen, leftVp, rightBottom);
                        g.DrawLine(guidePen, leftVp, rightTop);

                        guidePen.Color = Color.Blue;
                        g.DrawLine(guidePen, rightVp, frontBottom);
                        g.DrawLine(guidePen, rightVp, frontTop);
                        g.DrawLine(guidePen, rightVp, leftBottom);
                        g.DrawLine(guidePen, rightVp, leftTop);
                    }
                }
                #endregion
            }

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                DstArgs.Surface.CopySurface(CuboidSurface, renderRects[i].Location, renderRects[i]);
            }
        }
    }
}
