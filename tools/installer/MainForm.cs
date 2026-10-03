using System.Diagnostics;
using System.Reflection;

namespace AglaeaInstaller;

public sealed class MainForm : Form
{
    private readonly TextBox _pathBox;
    private readonly TextBox _logBox;
    private readonly Button _installButton;
    private readonly Button _browseButton;
    private readonly Button _readmeButton;

    public MainForm()
    {
        Text = "Aglaea 模组安装器";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(620, 480);
        Size = new Size(680, 520);
        Font = new Font("Microsoft YaHei UI", 9f);

        var pathLabel = new Label
        {
            Text = "游戏目录：",
            AutoSize = true,
            Location = new Point(16, 18),
        };

        _pathBox = new TextBox
        {
            Location = new Point(16, 42),
            Width = 520,
            ReadOnly = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        _browseButton = new Button
        {
            Text = "浏览…",
            Location = new Point(546, 41),
            Width = 100,
            Height = 25,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _browseButton.Click += OnBrowse;

        var hint = new Label
        {
            Text = "安装前请先完全关闭《杀戮尖塔 2》。",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(16, 74),
        };

        _installButton = new Button
        {
            Text = "安装",
            Location = new Point(16, 102),
            Width = 120,
            Height = 34,
            Enabled = false,
        };
        _installButton.Click += OnInstall;

        _readmeButton = new Button
        {
            Text = "查看说明",
            Location = new Point(146, 102),
            Width = 120,
            Height = 34,
        };
        _readmeButton.Click += OnShowReadme;

        _logBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Location = new Point(16, 150),
            Width = 630,
            Height = 310,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.White,
        };

        Controls.AddRange([pathLabel, _pathBox, _browseButton, hint, _installButton, _readmeButton, _logBox]);

        Load += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Log("正在自动检测游戏目录…");
        var detected = GameLocator.AutoDetect();
        if (detected != null)
        {
            _pathBox.Text = detected;
            _installButton.Enabled = true;
            Log($"已找到游戏目录：{detected}");
        }
        else
        {
            Log("未能自动找到游戏目录。请点击「浏览…」手动选择《杀戮尖塔 2》的安装文件夹。");
        }
    }

    private void OnBrowse(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择《杀戮尖塔 2》的安装文件夹（包含 SlayTheSpire2.exe）",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
        };

        var current = _pathBox.Text;
        if (!string.IsNullOrWhiteSpace(current) && Directory.Exists(current))
        {
            dialog.SelectedPath = current;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var picked = dialog.SelectedPath;
        if (!GameLocator.IsValidGameDir(picked))
        {
            Log($"所选目录无效：{picked}");
            MessageBox.Show(this,
                $"该文件夹看起来不是《杀戮尖塔 2》的安装目录。\n\n请选择包含 {GameLocator.GameExeName} 的文件夹。",
                "目录无效", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _pathBox.Text = picked;
        _installButton.Enabled = true;
        Log($"已选择游戏目录：{picked}");
    }

    private void OnInstall(object? sender, EventArgs e)
    {
        var gameDir = _pathBox.Text;
        if (string.IsNullOrWhiteSpace(gameDir))
        {
            return;
        }

        _installButton.Enabled = false;
        try
        {
            var result = Installer.Install(gameDir, Log);

            if (result.Success)
            {
                MessageBox.Show(this, result.Message, "安装成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (result.NeedsAdmin)
            {
                var choice = MessageBox.Show(this,
                    result.Message + "\n\n是否以管理员身份重新启动安装器？",
                    "需要管理员权限", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (choice == DialogResult.Yes && Installer.RestartAsAdmin())
                {
                    Close();
                }
                return;
            }

            MessageBox.Show(this, result.Message, "安装失败",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _installButton.Enabled = true;
        }
    }

    private void OnShowReadme(object? sender, EventArgs e)
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("payload.README.txt");

        if (stream == null)
        {
            MessageBox.Show(this, "安装包内没有找到说明文件。", "说明",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        // 写到临时文件后用系统默认程序打开，保证中文与排版正常。
        try
        {
            var temp = Path.Combine(Path.GetTempPath(), "Aglaea_README.txt");
            File.WriteAllText(temp, text);
            Process.Start(new ProcessStartInfo { FileName = temp, UseShellExecute = true });
        }
        catch
        {
            MessageBox.Show(this, text, "说明", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void Log(string message)
    {
        _logBox.AppendText(message + Environment.NewLine);
    }
}
