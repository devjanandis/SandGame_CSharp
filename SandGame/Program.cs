using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SandSimulation
{
    static class Program
    {
        static int[,] grid;
        static int[,] velocityGrid;
        static int w = 5;
        static int hueValue = 200;
        static double gravity = 0.1;
        static int cols, rows;
        static Random rnd = new Random();
        static Timer timer = new Timer();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form form = new Form();
            form.Text = "Simulação de Areia/Sand Simulation";
            form.Size = new System.Drawing.Size(600, 500);
            form.Paint += new PaintEventHandler(Form_Paint);
            form.MouseMove += new MouseEventHandler(Form_MouseMove);
            form.MouseDown += new MouseEventHandler(Form_MouseDown);
            InitializeGrid(form.ClientSize.Width, form.ClientSize.Height);

            // Configurar o temporizador e associar o formulário a sua propriedade Tag
            timer.Interval = 1000 / 60; // Defina a taxa de atualização para aproximadamente 60 FPS
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Tag = form; // Associar o formulário ao temporizador
            timer.Start();

            Application.Run(form);
        }

        static void Timer_Tick(object sender, EventArgs e)
        {
            // Atualizar a física da areia
            UpdateSandPhysics();

            // Redesenhar a tela
            ((Form)((Timer)sender).Tag).Invalidate();
        }

        static void InitializeGrid(int width, int height)
        {
            cols = width / w;
            rows = height / w;
            grid = new int[cols, rows];
            velocityGrid = new int[cols, rows];
        }

        static void Form_MouseDown(object sender, MouseEventArgs e)
        {
            int mouseCol = e.X / w;
            int mouseRow = e.Y / w;
            int matrix = 5;
            int extent = matrix / 2;
            for (int i = -extent; i <= extent; i++)
            {
                for (int j = -extent; j <= extent; j++)
                {
                    if (rnd.NextDouble() < 0.75)
                    {
                        int col = mouseCol + i;
                        int row = mouseRow + j;
                        if (WithinCols(col) && WithinRows(row))
                        {
                            grid[col, row] = hueValue;
                            velocityGrid[col, row] = 1;
                        }
                    }
                }
            }
            hueValue += 1;
            if (hueValue > 360)
                hueValue = 1;
        }

        static void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int mouseCol = e.X / w;
                int mouseRow = e.Y / w;
                int matrix = 5;
                int extent = matrix / 2;
                for (int i = -extent; i <= extent; i++)
                {
                    for (int j = -extent; j <= extent; j++)
                    {
                        if (rnd.NextDouble() < 0.75)
                        {
                            int col = mouseCol + i;
                            int row = mouseRow + j;
                            if (WithinCols(col) && WithinRows(row))
                            {
                                grid[col, row] = hueValue;
                                velocityGrid[col, row] = 1;
                            }
                        }
                    }
                }
                hueValue += 1;
                if (hueValue > 360)
                    hueValue = 1;
            }
        }

        static bool WithinCols(int i)
        {
            return i >= 0 && i <= cols - 1;
        }

        static bool WithinRows(int j)
        {
            return j >= 0 && j <= rows - 1;
        }

        static void Form_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (grid[i, j] > 0)
                    {
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 255)))
                        {
                            g.FillRectangle(brush, new Rectangle(i * w, j * w, w, w));
                        }
                    }
                }
            }
        }

        static void UpdateSandPhysics()
        {
            int[,] nextGrid = new int[cols, rows];
            int[,] nextVelocityGrid = new int[cols, rows];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    int state = grid[i, j];
                    int velocity = velocityGrid[i, j];
                    bool moved = false;

                    if (state > 0)
                    {
                        int newPos = (int)(j + velocity + gravity);
                        newPos = Math.Max(0, Math.Min(newPos, rows - 1));
                        for (int y = newPos; y > j; y--)
                        {
                            int below = grid[i, y];
                            int direction = rnd.NextDouble() < 0.5 ? 1 : -1;
                            int belowA = WithinCols(i + direction) ? grid[i + direction, y] : -1;
                            int belowB = WithinCols(i - direction) ? grid[i - direction, y] : -1;

                            if (below == 0)
                            {
                                nextGrid[i, y] = state;
                                nextVelocityGrid[i, y] = velocity + (int)gravity;
                                moved = true;
                                break;
                            }
                            else if (belowA == 0)
                            {
                                nextGrid[i + direction, y] = state;
                                nextVelocityGrid[i + direction, y] = velocity + (int)gravity;
                                moved = true;
                                break;
                            }
                            else if (belowB == 0)
                            {
                                nextGrid[i - direction, y] = state;
                                nextVelocityGrid[i - direction, y] = velocity + (int)gravity;
                                moved = true;
                                break;
                            }
                        }
                    }

                    if (state > 0 && !moved)
                    {
                        nextGrid[i, j] = grid[i, j];
                        nextVelocityGrid[i, j] = velocityGrid[i, j] + (int)gravity;
                    }
                }
            }

            Array.Copy(nextGrid, grid, cols * rows);
            Array.Copy(nextVelocityGrid, velocityGrid, cols * rows);
        }
    }
}
