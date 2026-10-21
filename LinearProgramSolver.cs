namespace Variant24LinearProgramming;

public sealed record Point2(double X, double Y)
{
    public override string ToString() => $"({Format(X)}; {Format(Y)})";

    public static string Format(double value)
    {
        if (Math.Abs(value) < 1e-9) value = 0;
        if (Math.Abs(value - Math.Round(value)) < 1e-9)
            return Math.Round(value).ToString();
        return value.ToString("0.###", System.Globalization.CultureInfo.GetCultureInfo("uk-UA"));
    }
}

public sealed record Vertex(Point2 Point, double Z, string Name);

public sealed class Solution
{
    public required List<Vertex> Vertices { get; init; }
    public required Vertex Optimum { get; init; }
    public required List<string> Steps { get; init; }
}

public static class LinearProgramSolver
{
    // Variant 24:
    // Z = 2x1 + 5x2 -> min
    // 7x1 + 2x2 >= 14
    // 5x1 + 6x2 <= 60
    // 3x1 + 8x2 >= 24
    // x1 >= 0, x2 >= 0

    private sealed record Line(double A, double B, double C, string Name);

    public static Solution Solve()
    {
        // Convert all inequalities to the common form A*x + B*y <= C
        // so feasibility checks are consistent.
        var constraints = new List<(double A, double B, double C)>
        {
            (-7, -2, -14), // 7x + 2y >= 14
            ( 5,  6,  60), // 5x + 6y <= 60
            (-3, -8, -24), // 3x + 8y >= 24
            (-1,  0,   0), // x >= 0
            ( 0, -1,   0)  // y >= 0
        };

        var boundaryLines = new List<Line>
        {
            new(7, 2, 14, "7x₁ + 2x₂ = 14"),
            new(5, 6, 60, "5x₁ + 6x₂ = 60"),
            new(3, 8, 24, "3x₁ + 8x₂ = 24"),
            new(1, 0, 0, "x₁ = 0"),
            new(0, 1, 0, "x₂ = 0")
        };

        var candidates = new List<Point2>();
        for (int i = 0; i < boundaryLines.Count; i++)
        {
            for (int j = i + 1; j < boundaryLines.Count; j++)
            {
                var p = Intersect(boundaryLines[i], boundaryLines[j]);
                if (p is not null && constraints.All(c => c.A * p.X + c.B * p.Y <= c.C + 1e-7))
                {
                    if (!candidates.Any(q => Math.Abs(q.X - p.X) < 1e-7 && Math.Abs(q.Y - p.Y) < 1e-7))
                        candidates.Add(p);
                }
            }
        }

        // Sort around centroid to form a polygon for graph shading.
        double cx = candidates.Average(p => p.X);
        double cy = candidates.Average(p => p.Y);
        candidates = candidates.OrderBy(p => Math.Atan2(p.Y - cy, p.X - cx)).ToList();

        var vertices = candidates.Select((p, i) =>
            new Vertex(p, 2 * p.X + 5 * p.Y, $"V{i + 1}")).ToList();
        var optimum = vertices.MinBy(v => v.Z)
            ?? throw new InvalidOperationException("Не вдалося знайти допустимі вершини.");

        var steps = new List<string>
        {
            "1. Записуємо цільову функцію: Z = 2x₁ + 5x₂ → min.",
            "2. Будуємо граничні прямі, замінюючи нерівності рівностями:",
            "   7x₁ + 2x₂ = 14; 5x₁ + 6x₂ = 60; 3x₁ + 8x₂ = 24; x₁ = 0; x₂ = 0.",
            "3. Знаходимо точки перетину прямих і залишаємо лише ті, що задовольняють усі обмеження.",
            "4. Перетин 7x₁ + 2x₂ = 14 та 3x₁ + 8x₂ = 24:",
            "   Помножимо перше рівняння на 4: 28x₁ + 8x₂ = 56.",
            "   Віднімемо друге рівняння: 25x₁ = 32, тому x₁ = 32/25 = 1,28.",
            "   Підставляємо: 7·1,28 + 2x₂ = 14 ⇒ x₂ = 2,52 = 63/25.",
            "5. Інші вершини допустимої області утворюються на осях: (0; 7), (0; 10), (8; 0), (12; 0).",
            "6. Обчислюємо Z = 2x₁ + 5x₂ у кожній допустимій вершині. Для задачі лінійного програмування оптимум досягається в одній із вершин.",
            $"7. Найменше значення: Zmin = {Point2.Format(optimum.Z)} у точці {optimum.Point}."
        };

        return new Solution { Vertices = vertices, Optimum = optimum, Steps = steps };
    }

    private static Point2? Intersect(Line p, Line q)
    {
        double d = p.A * q.B - q.A * p.B;
        if (Math.Abs(d) < 1e-10) return null;
        double x = (p.C * q.B - q.C * p.B) / d;
        double y = (p.A * q.C - q.A * p.C) / d;
        return new Point2(x, y);
    }
}
