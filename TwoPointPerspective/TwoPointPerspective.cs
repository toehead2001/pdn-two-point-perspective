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
        public string Author => ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
        public string Copyright => ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
        public string DisplayName => ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("http://forums.getpaint.net/index.php?showtopic=84519");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Two-point Perspective")]
    public class TwoPointPerspectiveEffectPlugin : PropertyBasedEffect
    {
        int Amount1 = 100; // [0,1000] Height
        int Amount2 = 200; // [0,1000] Left
        int Amount3 = 150; // [0,1000] Right
        bool Amount4 = true; // [0,1] Draw Hidden Edges
        bool Amount5 = false; // [0,1] Draw Vanishing Points
        Pair<double, double> Amount6 = Pair.Create(0.0, 0.0); // Offset
        Pair<double, double> Amount7 = Pair.Create(0.0, 1.0); // Offset
        int Amount8 = 2; // [0,10] Edge Outline Width
        ColorBgra Amount9 = ColorBgra.FromBgr(0, 0, 0); // Edge Outline Color
        byte Amount10 = 0; // Fill Style|None|Solid|Shaded
        ColorBgra Amount11 = ColorBgra.FromBgr(0, 0, 0); // Fill Color
        bool Amount12 = false; // [0,1] Draw Vanishing Points Guides

        const double rad180 = Math.PI / 180 * 180;
        const double rad360 = Math.PI / 180 * 360;

        Surface CuboidSurface;


        private const string StaticName = "Two-point Perspective";
        private static readonly Image StaticIcon = new Bitmap(typeof(TwoPointPerspectiveEffectPlugin), "TwoPointPerspective.png");

        public TwoPointPerspectiveEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuNames.Render, EffectFlags.Configurable)
        {
        }

        private enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5,
            Amount6,
            Amount7,
            Amount8,
            Amount9,
            Amount10,
            Amount11,
            Amount12
        }

        private enum Amount10Options
        {
            Amount10Option1,
            Amount10Option2,
            Amount10Option3
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, 100, 0, 1000));
            props.Add(new Int32Property(PropertyNames.Amount2, 200, 0, 1000));
            props.Add(new Int32Property(PropertyNames.Amount3, 150, 0, 1000));
            props.Add(new DoubleVectorProperty(PropertyNames.Amount6, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new BooleanProperty(PropertyNames.Amount5, false));
            props.Add(new BooleanProperty(PropertyNames.Amount12, false));
            props.Add(new DoubleVectorProperty(PropertyNames.Amount7, Pair.Create(0.0, 1.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new Int32Property(PropertyNames.Amount8, 2, 0, 10));
            props.Add(new BooleanProperty(PropertyNames.Amount4, true));
            props.Add(new Int32Property(PropertyNames.Amount9, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));
            props.Add(StaticListChoiceProperty.CreateForEnum<Amount10Options>(PropertyNames.Amount10, 0, false));
            props.Add(new Int32Property(PropertyNames.Amount11, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.SecondaryColor.B, EnvironmentParameters.SecondaryColor.G, EnvironmentParameters.SecondaryColor.R, 255)), 0, 0xffffff));

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>();
            propRules.Add(new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.Amount11, PropertyNames.Amount10, Amount10Options.Amount10Option1, false));
            propRules.Add(new ReadOnlyBoundToValueRule<int, Int32Property>(PropertyNames.Amount4, PropertyNames.Amount8, 0, false));
            propRules.Add(new ReadOnlyBoundToValueRule<int, Int32Property>(PropertyNames.Amount9, PropertyNames.Amount8, 0, false));

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Height");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Length");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.Description, "Draw Hidden Edges");
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.Description, "Draw Vanishing Points");
            configUI.SetPropertyControlValue(PropertyNames.Amount12, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Amount12, ControlInfoPropertyNames.Description, "Draw Vanishing Points Guides");
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DisplayName, "Vanishing Point Position (y-axis only)");
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DecimalPlaces, 3);
            Rectangle selection6 = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource imageResource6 = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection6));
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.StaticImageUnderlay, imageResource6);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.DisplayName, "Cuboid Position");
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.DecimalPlaces, 3);
            Rectangle selection7 = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource imageResource7 = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection7));
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.StaticImageUnderlay, imageResource7);
            configUI.SetPropertyControlValue(PropertyNames.Amount8, ControlInfoPropertyNames.DisplayName, "Edge Outline Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount9, ControlInfoPropertyNames.DisplayName, "Edge Outline Color");
            configUI.SetPropertyControlType(PropertyNames.Amount9, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount10, ControlInfoPropertyNames.DisplayName, "Fill Style");
            PropertyControlInfo Amount10Control = configUI.FindControlForPropertyName(PropertyNames.Amount10);
            Amount10Control.SetValueDisplayName(Amount10Options.Amount10Option1, "None");
            Amount10Control.SetValueDisplayName(Amount10Options.Amount10Option2, "Solid");
            Amount10Control.SetValueDisplayName(Amount10Options.Amount10Option3, "Shaded");
            configUI.SetPropertyControlValue(PropertyNames.Amount11, ControlInfoPropertyNames.DisplayName, "Fill Color");
            configUI.SetPropertyControlType(PropertyNames.Amount11, PropertyControlType.ColorWheel);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            Amount2 = newToken.GetProperty<Int32Property>(PropertyNames.Amount2).Value;
            Amount3 = newToken.GetProperty<Int32Property>(PropertyNames.Amount3).Value;
            Amount4 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount4).Value;
            Amount5 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount5).Value;
            Amount6 = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Amount6).Value;
            Amount7 = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Amount7).Value;
            Amount8 = newToken.GetProperty<Int32Property>(PropertyNames.Amount8).Value;
            Amount9 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount9).Value);
            Amount10 = (byte)((int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Amount10).Value);
            Amount11 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount11).Value);
            Amount12 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount12).Value;


            if (CuboidSurface == null)
                CuboidSurface = new Surface(srcArgs.Size);
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
                    Y = (float)(selCenter.Y + selCenter.Y * Amount6.Second)
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
                    X = (float)(selCenter.X + selCenter.X * Amount7.First),
                    Y = (float)(((selection.Bottom - 2) / 2f) + ((selection.Bottom - 2) * Amount7.Second / 2f))
                };

                PointF frontTop = new PointF
                {
                    X = frontBottom.X,
                    Y = frontBottom.Y - Amount1
                };
                #endregion

                if (Amount2 > frontBottom.X - leftVp.X || Amount3 > rightVp.X - frontBottom.X)
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
                double frontLeftBottomLength = Amount2 / Math.Cos(frontLeftBottomAngle);

                PointF leftBottom = new PointF
                {
                    X = frontBottom.X - Amount2,
                    Y = (float)(frontBottom.Y - Math.Tan(frontLeftBottomAngle) * Amount2)
                };
                #endregion

                #region Left Top point
                double frontLeftTopAngle = Math.Atan((frontTop.Y - leftVp.Y) / (frontTop.X - leftVp.X));
                double frontLeftTopLength = Amount2 / Math.Cos(frontLeftTopAngle);

                PointF leftTop = new PointF
                {
                    X = frontTop.X - Amount2,
                    Y = (float)(frontTop.Y - Math.Tan(frontLeftTopAngle) * Amount2)
                };
                #endregion

                #region Right Bottom point
                double frontRightBottomAngle = Math.Atan((frontBottom.Y - rightVp.Y) / (rightVp.X - frontBottom.X));
                double frontRightBottomLength = Amount3 / Math.Cos(frontRightBottomAngle);

                PointF rightBottom = new PointF
                {
                    X = frontBottom.X + Amount3,
                    Y = (float)(frontBottom.Y - Math.Tan(frontRightBottomAngle) * Amount3)
                };
                #endregion

                #region Right Top point
                double frontRightTopAngle = Math.Atan((frontTop.Y - rightVp.Y) / (rightVp.X - frontTop.X));
                double frontRightTopLength = Amount3 / Math.Cos(frontRightTopAngle);

                PointF rightTop = new PointF
                {
                    X = frontTop.X + Amount3,
                    Y = (float)(frontTop.Y - Math.Tan(frontRightTopAngle) * Amount3)
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
                switch (Amount10)
                {
                    case 1: // Solid
                        using (SolidBrush fillBrush = new SolidBrush(Amount11))
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
                    case 2: // Shaded
                        HsvColor fillColorBase = HsvColor.FromColor(Amount11);
                        fillColorBase.Saturation = 100;

                        HsvColor fillColorDark = fillColorBase;
                        fillColorDark.Value = 80;

                        HsvColor fillColorLight = fillColorBase;
                        fillColorLight.Saturation = 66;

                        HsvColor fillColorLighter = fillColorBase;
                        fillColorLighter.Saturation = 33;

                        using (SolidBrush fillBrush = new SolidBrush(Amount11))
                        using (Pen seamPen = new Pen(Amount11, 1))
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
                if (Amount8 != 0)
                {
                    using (Pen visiblePen = new Pen(Amount9, Amount8))
                    using (Pen hiddenPen = new Pen(Amount9, Amount8))
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
                        else if (Amount4)
                        {
                            g.DrawLine(hiddenPen, backTop, rightTop);
                            g.DrawLine(hiddenPen, backTop, leftTop);
                        }

                        if (bottomFaceVisible)
                        {
                            g.DrawLine(visiblePen, backBottom, rightBottom);
                            g.DrawLine(visiblePen, backBottom, leftBottom);
                        }
                        else if (Amount4)
                        {
                            g.DrawLine(hiddenPen, backBottom, rightBottom);
                            g.DrawLine(hiddenPen, backBottom, leftBottom);
                        }

                        if (Amount4)
                            g.DrawLine(hiddenPen, backBottom, backTop);
                    }
                }
                #endregion

                #region Draw Vanishing Points
                if (Amount5)
                {
                    g.FillRectangle(Brushes.Red, leftVp.X, leftVp.Y - 2, 4, 4);
                    g.FillRectangle(Brushes.Blue, rightVp.X - 4, rightVp.Y - 2, 4, 4);
                }

                if (Amount12)
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
