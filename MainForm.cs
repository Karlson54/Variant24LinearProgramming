using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot;
using ScottPlot.WinForms;
using DrawingColor = System.Drawing.Color;

namespace Variant24LinearProgramming;

public sealed class MainForm : Form
{
    private readonly DrawingColor bg = ColorTranslator.FromHtml("#111318");
    private readonly DrawingColor card = ColorTranslator.FromHtml("#1b1f26");
    private readonly DrawingColor cardAlt = ColorTranslator.FromHtml("#232832");
    private readonly DrawingColor textColor = ColorTranslator.FromHtml("#e7eaf0");
    private readonly DrawingColor muted = ColorTranslator.FromHtml("#8d96a6");
    private readonly DrawingColor grid = ColorTranslator.FromHtml("#303641");

    private readonly Solution solution = LinearProgramSolver.Solve();
    private readonly FormsPlot graph = new() { Dock = DockStyle.Fill };
    private readonly DataGridView table = new();
    private readonly RichTextBox steps = new();

    public MainForm()
    {
        Text = "Лінійне програмування • Варіант 24";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 720);
        Size = new Size(1420, 900);
        BackColor = bg;
        ForeColor = textColor;
        Font = new Font("Segoe UI", 10);
        BuildLayout();
        DrawGraph();
    }

    private void BuildLayout()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = bg, Padding = new Padding(28, 16, 28, 10) };
        header.Controls.Add(new System.Windows.Forms.Label
        {
            Text = "Контрольна робота · Варіант 24",
            Dock = DockStyle.Left, AutoSize = false, Width = 650,
            Font = new Font("Segoe UI Semibold", 24), ForeColor = textColor, TextAlign = ContentAlignment.MiddleLeft
        });
        header.Controls.Add(new System.Windows.Forms.Label
        {
            Text = "Метод лінійного програмування",
            Dock = DockStyle.Right, Width = 350, ForeColor = muted,
            Font = new Font("Segoe UI", 12), TextAlign = ContentAlignment.MiddleRight
        });
        Controls.Add(header);

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(24, 0, 24, 22),
            ColumnCount = 2, RowCount = 2, BackColor = bg
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        Controls.Add(body);
        header.BringToFront();

        var solutionCard = MakeCard();
        solutionCard.Padding = new Padding(20);
        var leftContent = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = card };
        leftContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        leftContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        leftContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftContent.Controls.Add(new System.Windows.Forms.Label
        {
            Text = "Покрокове розв’язання", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 18), ForeColor = textColor, TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        leftContent.Controls.Add(new System.Windows.Forms.Label
        {
            Text = "Z = 2x₁ + 5x₂ → min\r\n7x₁ + 2x₂ ≥ 14\r\n5x₁ + 6x₂ ≤ 60\r\n3x₁ + 8x₂ ≥ 24\r\nx₁ ≥ 0; x₂ ≥ 0",
            Dock = DockStyle.Fill, BackColor = cardAlt, ForeColor = textColor,
            Font = new Font("Cambria Math", 13), Padding = new Padding(14), AutoSize = false
        }, 0, 1);
        steps.Dock = DockStyle.Fill;
        steps.ReadOnly = true;
        steps.BorderStyle = BorderStyle.None;
        steps.BackColor = card;
        steps.ForeColor = textColor;
        steps.Font = new Font("Segoe UI", 11);
        steps.WordWrap = true;
        steps.Text = string.Join(Environment.NewLine + Environment.NewLine, solution.Steps);
        leftContent.Controls.Add(steps, 0, 2);
        solutionCard.Controls.Add(leftContent);
        body.Controls.Add(solutionCard, 0, 0);

        var graphCard = MakeCard();
        graphCard.Padding = new Padding(14);
        var graphLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = card };
        graphLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        graphLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        graphLayout.Controls.Add(new System.Windows.Forms.Label
        {
            Text = "Графічний розв’язок", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 17), ForeColor = textColor, TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        graphLayout.Controls.Add(graph, 0, 1);
        graphCard.Controls.Add(graphLayout);
        body.Controls.Add(graphCard, 1, 0);

        var tableCard = MakeCard();
        tableCard.Padding = new Padding(16);
        var tableLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = card };
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tableLayout.Controls.Add(new System.Windows.Forms.Label
        {
            Text = "Таблиця вершин допустимої області", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 15), ForeColor = textColor, TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        ConfigureTable();
        tableLayout.Controls.Add(table, 0, 1);
        tableCard.Controls.Add(tableLayout);
        body.Controls.Add(tableCard, 0, 1);

        var answer = MakeCard();
        answer.BackColor = ColorTranslator.FromHtml("#252e3f");
        answer.Padding = new Padding(22);
        answer.Controls.Add(new System.Windows.Forms.Label
        {
            Dock = DockStyle.Fill,
            Text = $"ВІДПОВІДЬ\r\nОптимальний план: x₁ = {Point2.Format(solution.Optimum.Point.X)}, x₂ = {Point2.Format(solution.Optimum.Point.Y)}\r\nZmin = {Point2.Format(solution.Optimum.Z)}",
            ForeColor = textColor, BackColor = answer.BackColor,
            Font = new Font("Segoe UI Semibold", 17), TextAlign = ContentAlignment.MiddleLeft
        });
        body.Controls.Add(answer, 1, 1);
    }

    private Panel MakeCard() => new() { Dock = DockStyle.Fill, BackColor = card, Margin = new Padding(7) };

    private void ConfigureTable()
    {
        table.Dock = DockStyle.Fill;
        table.BackgroundColor = card;
        table.BorderStyle = BorderStyle.None;
        table.GridColor = grid;
        table.EnableHeadersVisualStyles = false;
        table.ColumnHeadersDefaultCellStyle.BackColor = cardAlt;
        table.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
        table.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
        table.DefaultCellStyle.BackColor = card;
        table.DefaultCellStyle.ForeColor = textColor;
        table.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#314f58");
        table.DefaultCellStyle.SelectionForeColor = textColor;
        table.RowHeadersVisible = false;
        table.AllowUserToAddRows = false;
        table.AllowUserToDeleteRows = false;
        table.ReadOnly = true;
        table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        table.Columns.Add("vertex", "Вершина");
        table.Columns.Add("x1", "x₁");
        table.Columns.Add("x2", "x₂");
        table.Columns.Add("z", "Z = 2x₁ + 5x₂");

        foreach (var v in solution.Vertices.OrderBy(v => v.Z))
        {
            int row = table.Rows.Add(v.Name, Point2.Format(v.Point.X), Point2.Format(v.Point.Y), Point2.Format(v.Z));
            if (Math.Abs(v.Z - solution.Optimum.Z) < 1e-8)
            {
                table.Rows[row].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#314f58");
                table.Rows[row].DefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            }
        }
    }

    private void DrawGraph()
    {
        var plt = graph.Plot;
        plt.Clear();
        plt.FigureBackground.Color = ScottPlot.Color.FromHex("#1b1f26");
        plt.DataBackground.Color = ScottPlot.Color.FromHex("#1b1f26");
        plt.Axes.Color(ScottPlot.Color.FromHex("#e7eaf0"));
        plt.Grid.MajorLineColor = ScottPlot.Color.FromHex("#303641");
        plt.Grid.MinorLineColor = ScottPlot.Color.FromHex("#252a33");
        plt.Grid.MajorLineWidth = 1;

        // Обмеження:
        // 7x1 + 2x2 = 14 => при x1=0, x2=7; при x1=2, x2=0
        AddLine(plt, new[] { 0.0, 2.0 }, new[] { 7.0, 0.0 }, "#8aa4d6", "7x₁ + 2x₂ = 14");
        // 5x1 + 6x2 = 60 => при x1=0, x2=10; при x1=12, x2=0
        AddLine(plt, new[] { 0.0, 12.0 }, new[] { 10.0, 0.0 }, "#c9a875", "5x₁ + 6x₂ = 60");
        // 3x1 + 8x2 = 24 => при x1=0, x2=3; при x1=8, x2=0
        AddLine(plt, new[] { 0.0, 8.0 }, new[] { 3.0, 0.0 }, "#81b29a", "3x₁ + 8x₂ = 24");

        // Багатокутник допустимої області
        var polygon = new[]
        {
            new Point2(0, 7),
            new Point2(32.0 / 25.0, 63.0 / 25.0),
            new Point2(8, 0),
            new Point2(12, 0),
            new Point2(0, 10),
            new Point2(0, 7)
        };
        var px = polygon.Select(p => p.X).ToArray();
        var py = polygon.Select(p => p.Y).ToArray();
        var outline = plt.Add.Scatter(px, py);
        outline.Color = ScottPlot.Color.FromHex("#81b29a");
        outline.LineWidth = 2.5f;
        outline.MarkerSize = 0;

        foreach (var v in solution.Vertices)
        {
            var s = plt.Add.Scatter(new[] { v.Point.X }, new[] { v.Point.Y });
            bool isOptimum = Math.Abs(v.Z - solution.Optimum.Z) < 1e-8;
            s.Color = isOptimum
                ? ScottPlot.Color.FromHex("#c9a875")
                : ScottPlot.Color.FromHex("#8aa4d6");
            s.MarkerSize = isOptimum ? 12 : 8;
            s.LineWidth = 0;

            var label = plt.Add.Text($"{v.Name}{v.Point}", v.Point.X + 0.15, v.Point.Y + 0.25);
            label.LabelFontColor = ScottPlot.Color.FromHex("#e7eaf0");
            label.LabelFontSize = 11f;
        }

        // Лінія цільової функції у точці оптимуму Zmin
        var objective = plt.Add.Scatter(new[] { 0.0, 4.0 }, new[] { solution.Optimum.Z / 5.0, (solution.Optimum.Z - 8.0) / 5.0 });
        objective.Color = ScottPlot.Color.FromHex("#c9a875");
        objective.LineWidth = 1.5f;
        objective.LinePattern = ScottPlot.LinePattern.Dashed;

        plt.Title("Допустима область та вершини");
        plt.XLabel("x₁");
        plt.YLabel("x₂");

        plt.Axes.SetLimits(-0.7, 13, -0.7, 11);
        graph.Refresh();
    }

    private static void AddLine(Plot plt, double[] xs, double[] ys, string color, string label)
    {
        var s = plt.Add.Scatter(xs, ys);
        s.Color = ScottPlot.Color.FromHex(color);
        s.LineWidth = 2f;
        s.MarkerSize = 0;
        var text = plt.Add.Text(label, xs[0] + 0.3, ys[0] - 0.35);
        text.LabelFontColor = ScottPlot.Color.FromHex(color);
        text.LabelFontSize = 10f;
    }
}