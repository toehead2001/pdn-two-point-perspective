using System;
using System.Drawing;
using System.Reflection;
using System.Drawing.Text;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace TwoPointPerspectiveEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Two-point Perspective")]
    public class TwoPointPerspectiveEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Two-point Perspective";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(TwoPointPerspectiveEffectPlugin), "TwoPointPerspective.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;  // Programmer's chosen default
            }
        }

        public TwoPointPerspectiveEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5,
            Amount6,
            Amount7
        }


        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, 100, 0, 1000));
            props.Add(new Int32Property(PropertyNames.Amount2, 200, 0, 1000));
            props.Add(new Int32Property(PropertyNames.Amount3, 150, 0, 1000));
            props.Add(new DoubleVectorProperty(PropertyNames.Amount6, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new BooleanProperty(PropertyNames.Amount5, true));
            props.Add(new DoubleVectorProperty(PropertyNames.Amount7, Pair.Create(0.0, 1.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new BooleanProperty(PropertyNames.Amount4, true));

            return new PropertyCollection(props);
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
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DisplayName, "Vanishing Point position (y-axis only)");
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
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.DisplayName, "Cuboid position");
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


            cuboidSurface = new Surface(srcArgs.Surface.Size);

            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();

            Bitmap metaBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics metaGraphics = Graphics.FromImage(metaBitmap);
            metaGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            metaGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;


            float centerX = selection.Width / 2f;
            float centerY = selection.Height / 2f;

            // Vanishing Points
            float leftVpX = 0;
            float leftVpY = (float)(centerY + centerY * Amount6.Second);
            float rightVpX = selection.Width - 4;
            float rightVpY = leftVpY;

            if (Amount5)
            {
                using (SolidBrush vanishPointBrush = new SolidBrush(Color.Red))
                {
                    metaGraphics.FillEllipse(vanishPointBrush, leftVpX, leftVpY, 3, 3);
                    metaGraphics.FillEllipse(vanishPointBrush, rightVpX, rightVpY, 3, 3);
                }
            }


            // Convert degrees to radian for later use
            double rad90 = Math.PI / 180 * 90;
            double rad180 = Math.PI / 180 * 180;
            double rad360 = Math.PI / 180 * 360;


            #region Base points
            //float baseBottomX = centerX - (Amount3 - Amount2) / 2f;
            float baseBottomX = centerX;
            float baseBottomY = selection.Height - 2;

            baseBottomX = (float)(baseBottomX + baseBottomX * Amount7.First);
            baseBottomY = (float)((baseBottomY / 2f) + (baseBottomY * Amount7.Second / 2f));

            float baseTopX = baseBottomX;
            float baseTopY = baseBottomY - Amount1;
            #endregion


            #region Left bottom point
            float leftBottomDistanceX = baseBottomX - leftVpX;
            if (leftBottomDistanceX < Amount2)
            {
                using (SolidBrush fontBrush = new SolidBrush(Color.Red))
                using (Font font = new Font(new FontFamily("Arial"), 14))
                {
                    metaGraphics.DrawString("Cuboid is Out of Bounds", font, fontBrush, 15, 15);
                }

                metaSurface = Surface.CopyFromBitmap(metaBitmap);
                metaBitmap.Dispose();
                return;
            }

            float leftBottomDistanceY = baseBottomY - leftVpY;

            double leftBottomSine = leftBottomDistanceY / Math.Sqrt(Math.Pow(leftBottomDistanceX, 2) + Math.Pow(leftBottomDistanceY, 2));
            double leftBottomAngle = Math.Asin(leftBottomSine);

            float leftBottomX = baseBottomX - Amount2;
            float leftBottomY = (float)(baseBottomY - (leftBottomSine * Amount2 / Math.Sin(rad90 - leftBottomAngle)));

            double leftBottomLength = Amount2 / Math.Sin(rad90 - leftBottomAngle);
            #endregion


            #region Left top point
            float leftTopDistanceX = baseTopX - leftVpX;
            float leftTopDistanceY = baseTopY - leftVpY;

            double leftTopSine = leftTopDistanceY / Math.Sqrt(Math.Pow(leftTopDistanceX, 2) + Math.Pow(leftTopDistanceY, 2));
            double leftTopAngle = Math.Asin(leftTopSine);

            float leftTopX = baseTopX - Amount2;
            float leftTopY = (float)(baseTopY - (leftTopSine * Amount2 / Math.Sin(rad90 - leftTopAngle)));

            double leftTopLength = Amount2 / Math.Sin(rad90 - leftTopAngle);
            #endregion


            #region Right bottom point
            float rightBottomDistanceX = baseBottomX - rightVpX;
            if (Math.Abs(rightBottomDistanceX) < Amount3)
            {
                using (SolidBrush fontBrush = new SolidBrush(Color.Red))
                using (Font font = new Font(new FontFamily("Arial"), 14))
                {
                    metaGraphics.DrawString("Cuboid is Out of Bounds", font, fontBrush, 15, 15);
                }

                metaSurface = Surface.CopyFromBitmap(metaBitmap);
                metaBitmap.Dispose();
                return;
            }

            float rightBottomDistanceY = baseBottomY - rightVpY;

            double rightBottomSine = rightBottomDistanceY / Math.Sqrt(Math.Pow(rightBottomDistanceX, 2) + Math.Pow(rightBottomDistanceY, 2));
            double rightBottomAngle = Math.Asin(rightBottomSine);

            float rightBottomX = baseBottomX + Amount3;
            float rightBottomY = (float)(baseBottomY - (rightBottomSine * Amount3 / Math.Sin(rad90 - rightBottomAngle)));

            double rightBottomLength = Amount3 / Math.Sin(rad90 - rightBottomAngle);
            #endregion


            #region Right top point
            float rightTopDistanceX = baseTopX - rightVpX;
            float rightTopDistanceY = baseTopY - rightVpY;

            double rightTopSine = rightTopDistanceY / Math.Sqrt(Math.Pow(rightTopDistanceX, 2) + Math.Pow(rightTopDistanceY, 2));
            double rightTopAngle = Math.Asin(rightTopSine);

            float rightTopX = baseTopX + Amount3;
            float rightTopY = (float)(baseTopY - (rightTopSine * Amount3 / Math.Sin(rad90 - rightTopAngle)));

            double rightTopLength = Amount3 / Math.Sin(rad90 - rightTopAngle);
            #endregion



            // Top face stuff
            #region Top face
            float topFaceLeftDistanceX = leftTopX - rightVpX;
            float topFaceLeftDistanceY = leftTopY - rightVpY;

            double topFaceLeftSine = topFaceLeftDistanceY / Math.Sqrt(Math.Pow(topFaceLeftDistanceX, 2) + Math.Pow(topFaceLeftDistanceY, 2));
            double topFaceLeftAngle = Math.Asin(topFaceLeftSine);


            float topFaceRightDistanceX = rightTopX - leftVpX;
            float topFaceRightDistanceY = rightTopY - leftVpY;

            double topFaceRightSine = topFaceRightDistanceY / Math.Sqrt(Math.Pow(topFaceRightDistanceX, 2) + Math.Pow(topFaceRightDistanceY, 2));
            double topFaceRightAngle = Math.Asin(topFaceRightSine);


            // Headache ahead
            double topFaceAngle1 = rad180 - leftTopAngle - rightTopAngle;
            double topFaceAngle2 = topFaceLeftAngle + leftTopAngle;
            double topFaceAngle3 = topFaceRightAngle + rightTopAngle;
            double topFaceAngle4 = rad360 - topFaceAngle3 - topFaceAngle2 - topFaceAngle1;

            double topFaceLength = Math.Sqrt((Math.Pow(leftTopLength, 2) + Math.Pow(rightTopLength, 2)) - (2 * leftTopLength * rightTopLength * Math.Cos(topFaceAngle1)));
            double topFaceHelperAngle1 = Math.Asin(rightTopLength / (topFaceLength / Math.Sin(topFaceAngle1)));
            double topFacehelperAngle2 = topFaceAngle2 - topFaceHelperAngle1;

            double rightTopApexLengh = topFaceLength * Math.Sin(topFacehelperAngle2) / Math.Sin(topFaceAngle4);

            float TopApexY = (float)(rightTopY - (rightTopApexLengh * topFaceRightSine));
            float TopApexX = (float)(rightTopX - (rightTopApexLengh * Math.Sin(rad90 - topFaceRightAngle)));

            bool hiddenTopApex = false;
            if (TopApexY > baseTopY)
                hiddenTopApex = true;
            #endregion


            // Bottom face stuff
            #region Bottom face
            float bottomFaceLeftDistanceX = leftBottomX - rightVpX;
            float bottomFaceLeftDistanceY = leftBottomY - rightVpY;

            double bottomFaceLeftSine = bottomFaceLeftDistanceY / Math.Sqrt(Math.Pow(bottomFaceLeftDistanceX, 2) + Math.Pow(bottomFaceLeftDistanceY, 2));
            double bottomFaceLeftAngle = Math.Asin(bottomFaceLeftSine);


            float bottomFaceRightDistanceX = rightBottomX - leftVpX;
            float bottomFaceRightDistanceY = rightBottomY - leftVpY;

            double bottomFaceRightSine = bottomFaceRightDistanceY / Math.Sqrt(Math.Pow(bottomFaceRightDistanceX, 2) + Math.Pow(bottomFaceRightDistanceY, 2));
            double bottomFaceRightAngle = Math.Asin(bottomFaceRightSine);


            // Headache ahead
            double bottomFaceAngle1 = rad180 - leftBottomAngle - rightBottomAngle;
            double bottomFaceAngle2 = bottomFaceLeftAngle + leftBottomAngle;
            double bottomFaceAngle3 = bottomFaceRightAngle + rightBottomAngle;
            double bottomFaceAngle4 = rad360 - bottomFaceAngle3 - bottomFaceAngle2 - bottomFaceAngle1;

            double bottomFaceLength = Math.Sqrt((Math.Pow(leftBottomLength, 2) + Math.Pow(rightBottomLength, 2)) - (2 * leftBottomLength * rightBottomLength * Math.Cos(bottomFaceAngle1)));
            double bottomFaceHelperAngle1 = Math.Asin(rightBottomLength / (bottomFaceLength / Math.Sin(bottomFaceAngle1)));
            double bottomFaceHelperAngle2 = bottomFaceAngle2 - bottomFaceHelperAngle1;

            double rightBottomApexLengh = bottomFaceLength * Math.Sin(bottomFaceHelperAngle2) / Math.Sin(bottomFaceAngle4);

            float bottomApexY = (float)(rightBottomY - (rightBottomApexLengh * bottomFaceRightSine));
            float bottomApexX = (float)(rightBottomX - (rightBottomApexLengh * Math.Sin(rad90 - bottomFaceRightAngle)));

            bool hiddenBottomApex = false;
            if (bottomApexY < baseBottomY)
                hiddenBottomApex = true;
            #endregion



            // Set coordinates, and draw lines connecting them
            Bitmap cuboidBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics cuboidGraphics = Graphics.FromImage(cuboidBitmap);
            cuboidGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Pen cuboidPen = new Pen(Color.Black, 2);
            cuboidPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            cuboidPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            PointF frontBottom = new PointF(baseBottomX, baseBottomY);
            PointF frontTop = new PointF(baseTopX, baseTopY);

            cuboidGraphics.DrawLine(cuboidPen, frontTop, frontBottom);

            PointF leftBottom = new PointF(leftBottomX, leftBottomY);
            PointF leftTop = new PointF(leftTopX, leftTopY);

            cuboidGraphics.DrawLine(cuboidPen, leftTop, frontTop);


            cuboidGraphics.DrawLine(cuboidPen, leftBottom, frontBottom);

            cuboidGraphics.DrawLine(cuboidPen, leftTop, leftBottom);


            PointF rightBottom = new PointF(rightBottomX, rightBottomY);
            PointF rightTop = new PointF(rightTopX, rightTopY);


            cuboidGraphics.DrawLine(cuboidPen, rightTop, frontTop);

            cuboidGraphics.DrawLine(cuboidPen, rightBottom, frontBottom);

            cuboidGraphics.DrawLine(cuboidPen, rightTop, rightBottom);

            PointF apexTop = new PointF(TopApexX, TopApexY);

            Pen hiddenPen = new Pen(Color.Black, 2);
            hiddenPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            if (!hiddenTopApex)
            {
                cuboidGraphics.DrawLine(cuboidPen, apexTop, rightTop);
                cuboidGraphics.DrawLine(cuboidPen, apexTop, leftTop);
            }
            else if (hiddenTopApex && Amount4)
            {
                cuboidGraphics.DrawLine(hiddenPen, apexTop, rightTop);
                cuboidGraphics.DrawLine(hiddenPen, apexTop, leftTop);
            }

            PointF apexBottom = new PointF(bottomApexX, bottomApexY);

            if (!hiddenBottomApex)
            {
                cuboidGraphics.DrawLine(cuboidPen, apexBottom, rightBottom);
                cuboidGraphics.DrawLine(cuboidPen, apexBottom, leftBottom);
            }
            else if (hiddenBottomApex && Amount4)
            {
                cuboidGraphics.DrawLine(hiddenPen, apexBottom, rightBottom);
                cuboidGraphics.DrawLine(hiddenPen, apexBottom, leftBottom);
            }

            if (Amount4)
                cuboidGraphics.DrawLine(hiddenPen, apexBottom, apexTop);

            cuboidPen.Dispose();
            hiddenPen.Dispose();


            cuboidSurface = Surface.CopyFromBitmap(cuboidBitmap);
            cuboidBitmap.Dispose();

            metaSurface = Surface.CopyFromBitmap(metaBitmap);
            metaBitmap.Dispose();


            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
            }
        }

        #region CodeLab
        int Amount1 = 100; // [0,1000] Height
        int Amount2 = 200; // [0,1000] Left
        int Amount3 = 150; // [0,1000] Right
        bool Amount4 = true; // [0,1] Draw Hidden Edges
        bool Amount5 = true; // [0,1] Draw Vanishing Points
        Pair<double, double> Amount6 = Pair.Create(0.0, 0.0); // Offset
        Pair<double, double> Amount7 = Pair.Create(0.0, 1.0); // Offset
        #endregion

        Surface cuboidSurface, metaSurface;
        readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

            ColorBgra sourcePixel, cuboidPixel, metaPixel;

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    sourcePixel = src[x, y];
                    metaPixel = metaSurface.GetBilinearSample(x - selection.Left, y - selection.Top);
                    cuboidPixel = cuboidSurface.GetBilinearSample(x - selection.Left, y - selection.Top);

                    cuboidPixel = normalOp.Apply(cuboidPixel, metaPixel);

                    dst[x, y] = normalOp.Apply(sourcePixel, cuboidPixel);
                }
            }
        }
    }
}
