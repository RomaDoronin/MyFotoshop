using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyFotoshop
{
    public partial class Form1 : Form
    {
        Bitmap image, image1;
        public Form1()
        {
            InitializeComponent();
        }

        private void фильтрыToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
            }
            pictureBox1.Image = image;
            image1 = image;
            pictureBox1.Refresh();
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*Invert*/Filters filter = new InvertFilter();
            /*Bitmap resultImage = filter.processImage(image, backgroundWorker1);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();*/
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            image = ((Filters)e.Argument).processImage(image, backgroundWorker1);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Image = image;
            pictureBox1.Refresh();
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlueFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void гауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScalFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void секпияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepia();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void точечьныеToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Brightness();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new ResFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new TisFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void собеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter1 = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter1);            
        }

        private void эффектСтеклаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter1 = new WindowsFilter();
            backgroundWorker1.RunWorkerAsync(filter1);
        }

        private void переносВлевоToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Filters filter1 = new GoLeftFilter();
            backgroundWorker1.RunWorkerAsync(filter1);            
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter1 = new TurnFiter();
            backgroundWorker1.RunWorkerAsync(filter1); 
        }

        private void волныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter1 = new Wave1Fiter();
            backgroundWorker1.RunWorkerAsync(filter1); 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            image = image1;
            pictureBox1.Image = image1;
            pictureBox1.Refresh();
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter1 = new GrayWorld(image);
            backgroundWorker1.RunWorkerAsync(filter1); 
        }

        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter1 = new Dilation();
            backgroundWorker1.RunWorkerAsync(filter1); 
        }
    }
}
