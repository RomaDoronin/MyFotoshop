using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace MyFotoshop
{
    abstract class Filters
    {
       public Random rand = new Random(/*DateTime.Now.Millisecond*/);

        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j<sourceImage.Height; j++)
                {
                    worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
                return resultImage;
        }
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.B, 255 - sourceColor.B);
            return resultColor;
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class BlueFilter : MatrixFilter
    {
        public BlueFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class GrayScalFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int tmp = (int) (0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(tmp,
                                               tmp,
                                               tmp);
            return resultColor;
        }
    }

    class Sepia : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 50;
            int tmp = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(Clamp(tmp + 2*k, 0, 255),
                                               Clamp((int)(tmp + 0.5 * k), 0, 255),
                                               Clamp(tmp - 1 * k, 0, 255));
            return resultColor;
        }
    }


    class Brightness : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 50;
            Color resultColor = Color.FromArgb(Clamp(sourceColor.R + k, 0, 255),
                                               Clamp(sourceColor.G + k, 0, 255),
                                               Clamp(sourceColor.B + k, 0, 255));
            return resultColor;
        }
    }

    class SobelFilter : MatrixFilter
    {
        float[,] kernel1 = new float[3, 3] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        float[,] kernel2 = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, -1 } };
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            kernel = kernel1;
            Color Color1 = base.calculateNewPixelColor(sourceImage, x, y);
            kernel = kernel2;
            Color Color2 = base.calculateNewPixelColor(sourceImage, x, y);
            return Color.FromArgb(
               Clamp((int)Math.Sqrt(Color1.R * Color1.R + Color1.R * Color1.R), 0, 255),
               Clamp((int)Math.Sqrt(Color1.G * Color1.G + Color1.G * Color1.G), 0, 255),
               Clamp((int)Math.Sqrt(Color1.B * Color1.B + Color1.B * Color1.B), 0, 255)
               );
        }
    }

    class ResFilter : MatrixFilter
    {
        public void createResKernel(int radius)
        {
            int size = /*2 **/ radius /*+ 1*/;
            kernel = new float[size, size];        
            kernel[0, 0] = -1; kernel[0, 1] = -1; kernel[0, 2] = -1;
            kernel[1, 0] = -1; kernel[1, 1] = 9; kernel[1, 2] = -1;
            kernel[2, 0] = -1; kernel[2, 1] = -1; kernel[2, 2] = -1;
        }
        public ResFilter()
        {
            createResKernel(3);
        }
    }

    class TisFilter : MatrixFilter
    {
        float[,] kernel1 = new float[3, 3] { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            kernel = kernel1;
            Color color = base.calculateNewPixelColor(sourceImage, x, y);
            int k = 128;
            return Color.FromArgb(
                Clamp((int)(color.R + k), 0, 255),
                Clamp((int)(color.G + k), 0, 255),
                Clamp((int)(color.B + k), 0, 255)
                );
        }
    }

    class GoLeftFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color resultColor;

            if (x < (sourceImage.Width - 50))
            {
                resultColor = sourceImage.GetPixel(x + 50, y);

            }
            else
            {
                resultColor = Color.FromArgb(0, 0, 0);
            }

            return resultColor;
        }
    }

    class WindowsFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color resultColor;
        
            double tmp1, tmp2;

            tmp1 = rand.NextDouble();
            tmp2 = rand.NextDouble();

            if ((x < (sourceImage.Width - 5)) && (x > 5) && (y < (sourceImage.Height - 5)) && (y > 5))
            {
                resultColor = sourceImage.GetPixel((int)(x + (tmp1 - 0.5) * 10), (int)(y + (tmp2 - 0.5) * 10));
            }
            else 
            {
                resultColor = sourceImage.GetPixel(x, y);
            }                     

            return resultColor;
        }
    }

    class TurnFiter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color resultColor;

            int x0 = (sourceImage.Width-1) / 2;
            int y0 = (sourceImage.Height-1) / 2;

            // Mu - угол поворота
            double Mu = Math.PI/4;

            int tmp1 = (int)((x - x0) * Math.Cos(Mu) - (y - y0) * Math.Sin(Mu) + x0);
            int tmp2 = (int)((x - x0) * Math.Sin(Mu) + (y - y0) * Math.Cos(Mu) + y0);

            if ((tmp1 > 0) && (tmp2 > 0) && (tmp1 <= sourceImage.Width-1) && (tmp2 <= sourceImage.Height-1))
                resultColor = sourceImage.GetPixel(tmp1, tmp2);                
            else
                resultColor = Color.FromArgb(0, 0, 0);
                

            return resultColor;
        }
    }

    class Wave1Fiter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color resultColor;

            double Mu = 2 * Math.PI * y / 60;

            int tmp1 = (int)(x + 20 * Math.Sin(Mu));
            int tmp2 = y;

            if ((tmp1 > 0) && (tmp2 > 0) && (tmp1 <= sourceImage.Width - 1) && (tmp2 <= sourceImage.Height - 1))
               resultColor = sourceImage.GetPixel(tmp1, tmp2);                
            else
                resultColor = Color.FromArgb(0, 0, 0);
                

            return resultColor;
        }
    }

    //---------------
    //Класс Серый Мир
    class GrayWorld : Filters
    {
        private int Rsr = 0, Gsr = 0, Bsr = 0; //Переменные для хранения средних зачений
        private int size = 0;
        private float Avg = 0;

        public GrayWorld(Bitmap sourceImage)
        {
            Color sourceColor;
            size = (sourceImage.Height - 1) * (sourceImage.Width - 1);

            for (int i = 0; i < sourceImage.Width; i++)
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    sourceColor = sourceImage.GetPixel(i, j);
                    Rsr += sourceColor.R;
                    Gsr += sourceColor.G;
                    Bsr += sourceColor.B;
                }
            Avg = (float)((Rsr + Gsr + Bsr) / (3 * size));
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            int k = Clamp((int)(sourceColor.R * Avg * size / Rsr), 0, 255);
            int l = Clamp((int)(sourceColor.G * Avg * size / Gsr), 0, 255);
            int m = Clamp((int)(sourceColor.B * Avg * size / Bsr), 0, 255);

            Color resultColor = Color.FromArgb(k, l, m);

            return resultColor; 
        }

    }

    class Dilation : Filters
    {
        public bool[,] kernel;
        public Dilation()
        {
            int sizeM = 3;
            bool[,] kernel = new bool[sizeM, sizeM];

            this.kernel = kernel;

            kernel[0, 0] = false; kernel[0, 1] = true; kernel[0, 2] = false;
            kernel[1, 0] = true; kernel[1, 1] = true; kernel[1, 2] = true;
            kernel[2, 0] = false; kernel[2, 1] = true; kernel[2, 2] = false;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color resultColor;
            Color maxColor = Color.Black;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY); 
                    if ((kernel[k+1,l+1]) && (neighborColor.R > maxColor.R))
                    {
                        maxColor = neighborColor;
                    }
                }
            resultColor = maxColor;
            return resultColor;
        }
    }

    class Erosion : Filters
    {
        public bool[,] kernel;
        public Erosion()
        {
            int sizeM = 3;
            bool[,] kernel = new bool[sizeM, sizeM];

            this.kernel = kernel;

            kernel[0, 0] = false; kernel[0, 1] = true; kernel[0, 2] = false;
            kernel[1, 0] = true; kernel[1, 1] = true; kernel[1, 2] = true;
            kernel[2, 0] = false; kernel[2, 1] = true; kernel[2, 2] = false;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            Color resultColor;
            Color minColor = Color.White;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    if ((kernel[k + 1, l + 1]) && (neighborColor.R < minColor.R))
                    {
                        minColor = neighborColor;
                    }
                }
            resultColor = minColor;
            return resultColor;
        }
    }
}
