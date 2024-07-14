using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CirclePuzzle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int circles = 4;

        private float[] angles = {0, 0, 40, 0};

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            Bitmap cen = new Bitmap(Properties.Resources.paint).Clone(new RectangleF(300, 300, 1200, 1200),
                PixelFormat.DontCare);

            float dis = cen.Width/(circles*2f);

            for (int i = circles - 1; i >= 0; i--)
            {
                Image crop = ClipToCircle(cen, new PointF(cen.Width / 2f, cen.Height / 2f), (i+1)*dis, Color.FromArgb(0, 0, 0, 0));

                Bitmap ro = RotateImage(crop, angles[i]);

                e.Graphics.DrawImage(ro, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
                
            }

            float dis2 = pictureBox1.Width / (circles * 2f);

            for (int i = circles - 1; i >= 0; i--)
            {
                float radius = ((i + 1) * dis2);

                e.Graphics.DrawEllipse(Pens.White, new RectangleF(pictureBox1.Width / 2f - radius, pictureBox1.Height / 2f - radius, 2 * radius, 2 * radius));
            }

            
        }


        private void DrawCicle(Graphics g, Bitmap paint, int i)
        {


            GraphicsPath curra = new GraphicsPath();

            float middleX = pictureBox1.Width / 2f;
            float middleY = pictureBox1.Height / 2f;


            curra.AddEllipse(new RectangleF(middleX - i / 2f, middleY - i / 2f, i, i));


            Region reg = new Region(curra);
            pictureBox1.Region = reg;

            g.DrawImage(paint, new RectangleF(middleX - i / 2f, middleY - i / 2f, i, i));

        }

        // makes nice round ellipse/circle images from rectangle images
        public Image ClipToCircle(Image srcImage, PointF center, float radius, Color backGround)
        {
            Image dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);

            using (Graphics g = Graphics.FromImage(dstImage))
            {
                RectangleF r = new RectangleF(center.X - radius, center.Y - radius,
                                                         radius * 2, radius * 2);

                // enables smoothing of the edge of the circle (less pixelated)
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // fills background color
                using (Brush br = new SolidBrush(backGround))
                {
                    g.FillRectangle(br, 0, 0, dstImage.Width, dstImage.Height);
                }

                // adds the new ellipse & draws the image again 
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(r);
                g.SetClip(path);
                g.DrawImage(srcImage, 0, 0);

                return dstImage;
            }
        }


        public static Bitmap RotateImage(Image img, float rotationAngle)
        {
            //create an empty Bitmap image
            Bitmap bmp = new Bitmap(img.Width, img.Height);

            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Properties.Resources.paint,
                new RectangleF(5, 5, pictureBox1.Width - 5, pictureBox1.Height - 5));
        }

        private bool click = false;

        private PointF before;

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            click = true;

            before = new PointF(e.X, e.Y);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            click = true;

            before = new PointF(e.X, e.Y);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(!click)
                return;

            PointF newPointF = new PointF(e.X, e.Y);

            PointF pbefore = new PointF(before.X - pictureBox1.Width/2f, pictureBox1.Height/2f - before.Y);
            PointF pnewPointF = new PointF(newPointF.X - pictureBox1.Width/2f, pictureBox1.Height/2f - newPointF.Y);

            float angle1f = Angle(pbefore);
            float angle2f = Angle(pnewPointF);

            float angle1 = (float)(angle1f*180/Math.PI);
            float angle2 = (float)(angle2f * 180 / Math.PI);

            int sum = 0;

            if (angle1 > angle2)
                sum = 10;
            else if (angle2 > angle1)
                sum = -10;

            int pos1 = GetCircle(pbefore);
            int pos2 = GetCircle(pnewPointF);

            if (pos1 == pos2)
            {
                angles[pos2] += sum;

                if (OK())
                    MessageBox.Show("Has ganado", "Ganaste", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                pictureBox1.Invalidate();

                
            }

            before = newPointF;
        }

        private bool OK()
        {
            for (int i = 0; i < circles; i++)
            {
                if (Math.Abs(angles[i]) > 0.001)
                    return false;
            }

            return true;
        }

        private int GetCircle(PointF pbefore)
        {
            float dis2 = pictureBox1.Width / (circles * 2f);
            
            for (int i = 0; i < circles; i++)
            {
                float radius = ((i + 1) * dis2);

                if (Inter(pbefore.X, pbefore.Y, radius))
                    return i;
            }

            return -1;
        }

        private static bool Inter(float x, float y, float radius)
        {
            return x*x + y*y <= radius*radius;
        }

        private static float Angle(PointF p1,PointF p2)
        {

            float magnitud1 = Magnitude(p1);
            float magnitud2 = Magnitude(p2);

            float toCos = (p1.X*p2.X + p1.Y*p2.Y)/(magnitud1*magnitud2);

            return (float) Math.Acos(toCos);

        }

        private static float Angle(PointF p1)
        {

            float theta = Math.Abs((float)Math.Atan(p1.Y/p1.X));

            switch (Quad(p1))
            {
                case 1:
                    return theta;
                case 2:
                    return (float)Math.PI - theta;
                case 3:
                    return (float)Math.PI + theta;
                case 4:
                    return  2f*(float)Math.PI - theta;
            }

            return 0;

        }

        private static int Quad(PointF p1)
        {
            if (p1.X >= 0 && p1.Y >= 0)
                return 1;

            if (p1.X < 0 && p1.Y >= 0)
                return 2;

            if (p1.X < 0 && p1.Y <= 0)
                return 3;

            if (p1.X > 0 && p1.Y <= 0)
                return 4;

            return -1;
        }

        private static float Magnitude(PointF p)
        {
            return (float)Math.Sqrt(p.X*p.X + p.Y*p.Y);
        }



        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            click = false;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            click = false;
        }
    }
}
