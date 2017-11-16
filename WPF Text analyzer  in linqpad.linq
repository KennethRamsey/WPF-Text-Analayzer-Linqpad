<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationCore.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationUI.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\ReachFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\System.Printing.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\UIAutomationProvider.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\UIAutomationTypes.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <NuGetReference>Sprache</NuGetReference>
  <Namespace>Sprache</Namespace>
  <Namespace>System.Windows</Namespace>
  <Namespace>System.Windows.Controls</Namespace>
  <Namespace>System.Windows.Media</Namespace>
</Query>

void Main()
{
	// Add arbitrary rules to help you see the patterns in the text,
	// and to help you find outliers that might break your parser.

	var LineTypes = new[] {

		new LineType {
			Description = "file header",
			Pred = s => Regex.IsMatch(s, @"^1"),
			Color = RngBrush(),
			Show = true,
		},

		new LineType {
			Description = "batch header",
			Pred = s => Regex.IsMatch(s, @"^5"),
			Color = RngBrush(),
			Show = true,
		},

		new LineType {
			Description = "batch line",
			Pred = s => Regex.IsMatch(s, @"^6"),
			Color = RngBrush(),
			Show = true,
		},

		new LineType {
			Description = "Unknown",
			Pred = s => true,
			Color = RngBrush(),
			Show = true,
		},
	};


	// Other features that were really nice was having the Color dependant on the content of the string.
	// so if the content changed, then color would change, this is helpful for exact strings.

	// OR have color change of line format change. 
	// (reduce "File043 - the header line page 2" -> "wn o w w w w n" where w = word, n = number, o = other)



	// get Data.
	var path = @"C:\Users\Kenneth\Desktop\fake ach file.txt";

	var i = 1;
	int getI() => i++;

	var lines =
		from line in File.ReadLines(path)
		let type = LineTypes.First(t => t.Pred(line))
		where type.Show
		select new Line { Text = line, Number = getI(), LineType = type };


	// Display in WPF Window.
	var w = new Window();
	w.FontFamily = new FontFamily("Consolas"); // very important to have a monospaced font.

	w.Loaded += (o, e) =>
	{
		var listView = new StackPanel();

		var lineBoxes = lines.Select(getBox).ToArray();

		// load all.
		foreach (var lb in lineBoxes)
			listView.Children.Add(lb);

		w.Content = listView;
	};

	w.Show();
}



string NiceText(Line l) => $"{l.Number,0:D4} | {l.LineType.Description,-12} | {l.Text}";
TextBlock getBox(Line l) => new TextBlock() { Text = NiceText(l), Background = l.LineType.Color };



static Random Rng = new Random();
static byte RngByte() => (byte)Rng.Next(0, 256);


static HashSet<SolidColorBrush> Colors = new HashSet<SolidColorBrush>(); // if there are any colors you don't want rng brush to pick, add them now.

static SolidColorBrush RngBrush()
{
	// make brush, but must ensure that color isn't too close to any other color.
	SolidColorBrush newColor = GetUnusedColor();

	// now add to set.
	Colors.Add(newColor);

	return newColor;
}


static SolidColorBrush GetUnusedColor()
{
	SolidColorBrush newColor = null;

	// a good alpha value is factor, but it looks like if i don't pick an alpha greater than 125ish here,
	// then all colors will be light, and I'll be able to just leave the text dark and have it still be visible.
	// that solves alot of the headache of picking and rng color.
	do newColor = new SolidColorBrush(Color.FromArgb(125, RngByte(), RngByte(), RngByte()));
	while (AlreadyChosen());

	return newColor;

	//helper
	bool AlreadyChosen() => Colors.Any(ColorTooClose);

	bool ColorTooClose(SolidColorBrush a) =>
		Math.Abs(a.Color.R - newColor.Color.R) +
		Math.Abs(a.Color.G - newColor.Color.G) +
		Math.Abs(a.Color.B - newColor.Color.B) < 15;
}



class LineType
{
	public Func<string, bool> Pred { get; set; }
	public string Description { get; set; }
	public SolidColorBrush Color { get; set; }
	public bool Show { get; set; }
}

class Line
{
	public string Text { get; set; }
	public int Number { get; set; }
	public LineType LineType { get; set; }
}