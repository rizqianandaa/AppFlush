using AppFlush.Core.Models;
using AppFlush.Core.RegistryAccess;
using AppFlush.Core.Services;

namespace AppFlush.App;

/// <summary>Jendela utama AppFlush: daftar aplikasi terpasang + tombol Uninstall/Cari Sisa Folder.</summary>
public sealed class MainForm : Form
{
    private readonly IRegistryBackend _backend;
    private readonly IFileSystem _fileSystem;
    private readonly ListView _listView;
    private readonly TextBox _searchBox;
    private readonly Button _uninstallButton;
    private readonly Button _leftoversButton;
    private readonly Button _refreshButton;
    private readonly Label _statusLabel;
    private IReadOnlyList<InstalledApp> _allApps = Array.Empty<InstalledApp>();

    public MainForm(IRegistryBackend backend, IFileSystem fileSystem)
    {
        _backend = backend;
        _fileSystem = fileSystem;

        Text = "AppFlush";
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(760, 500);
        MinimumSize = new Size(620, 380);

        // Pakai ikon yang sudah dibundel ke dalam .exe (lewat ApplicationIcon di csproj)
        // supaya jendela + taskbar juga menampilkan logo AppFlush, bukan ikon default .NET.
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Kalau gagal (misalnya dijalankan dari lokasi yang tidak biasa), biarkan ikon default.
        }

        _searchBox = new TextBox
        {
            Location = new Point(12, 14),
            Size = new Size(630, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            PlaceholderText = "Cari aplikasi...",
        };
        _searchBox.TextChanged += (_, _) => ApplyFilter();

        _refreshButton = new Button
        {
            Text = "Muat ulang",
            Location = new Point(654, 12),
            Size = new Size(94, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _refreshButton.Click += (_, _) => LoadApps();

        _listView = new ListView
        {
            Location = new Point(12, 52),
            Size = new Size(736, 388),
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            GridLines = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };
        _listView.Columns.Add("Nama aplikasi", 340);
        _listView.Columns.Add("Versi", 120);
        _listView.Columns.Add("Penerbit", 260);
        _listView.SelectedIndexChanged += (_, _) => UpdateButtonState();

        _uninstallButton = new Button
        {
            Text = "Uninstall",
            Location = new Point(12, 456),
            Size = new Size(120, 32),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Enabled = false,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(211, 47, 47),
            ForeColor = Color.White,
        };
        _uninstallButton.FlatAppearance.BorderSize = 0;
        _uninstallButton.Click += (_, _) => UninstallSelected();

        _leftoversButton = new Button
        {
            Text = "Cari folder sisa",
            Location = new Point(144, 456),
            Size = new Size(140, 32),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Enabled = false,
        };
        _leftoversButton.Click += (_, _) => FindLeftoversForSelected();

        _statusLabel = new Label
        {
            Location = new Point(296, 462),
            Size = new Size(452, 20),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = SystemColors.GrayText,
            TextAlign = ContentAlignment.MiddleRight,
        };

        Controls.AddRange(new Control[]
        {
            _searchBox, _refreshButton, _listView,
            _uninstallButton, _leftoversButton, _statusLabel,
        });

        // Menu klik-kanan "Uninstall dengan AppFlush" sudah otomatis didaftarkan saat
        // instalasi (lihat installer.iss -> register-menu) dan dihapus otomatis saat
        // uninstall (-> unregister-menu). Tidak ada lagi opsi manual untuk menyalakan/
        // mematikannya di jendela ini -- selalu aktif selama AppFlush terpasang.
        Load += (_, _) => LoadApps();
    }

    private void LoadApps()
    {
        _allApps = RegistryScanner.ScanInstalledApps(_backend);
        ApplyFilter();
        _statusLabel.Text = $"{_allApps.Count} aplikasi terpasang ditemukan.";
    }

    private void ApplyFilter()
    {
        var query = _searchBox.Text.Trim();
        var filtered = query.Length == 0 ? _allApps : AppMatcher.FindByQuery(_allApps, query);

        _listView.BeginUpdate();
        _listView.Items.Clear();
        foreach (var app in filtered)
        {
            var item = new ListViewItem(app.Name) { Tag = app };
            item.SubItems.Add(app.Version ?? "-");
            item.SubItems.Add(app.Publisher ?? "-");
            _listView.Items.Add(item);
        }
        _listView.EndUpdate();
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        var hasSelection = _listView.SelectedItems.Count > 0;
        _uninstallButton.Enabled = hasSelection;
        _leftoversButton.Enabled = hasSelection;
    }

    private InstalledApp? SelectedApp() =>
        _listView.SelectedItems.Count > 0 ? (InstalledApp)_listView.SelectedItems[0].Tag! : null;

    private void UninstallSelected()
    {
        var app = SelectedApp();
        if (app is null) return;

        // PENTING: sengaja TIDAK ada lagi dialog konfirmasi buatan AppFlush di sini.
        // Klik tombol "Uninstall" ini sendiri sudah tindakan yang disengaja, dan
        // uninstaller resmi aplikasi yang dipilih umumnya menampilkan konfirmasinya
        // sendiri setelah proses ini berjalan. Untuk aplikasi yang uninstaller-nya
        // benar-benar diam (tanpa dialog apa pun), proses akan langsung berjalan
        // TANPA konfirmasi apa pun begitu tombol ini diklik.
        try
        {
            var exitCode = UninstallRunner.Run(app, new WindowsProcessRunner());
            _statusLabel.Text = $"'{app.Name}' selesai diproses (kode keluar {exitCode}).";
        }
        catch (UninstallException ex)
        {
            MessageBox.Show(this, ex.Message, "AppFlush", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            // PENTING: sebelumnya hanya UninstallException yang ditangkap di sini.
            // Kegagalan lain (mis. Process.Start gagal karena path tidak valid) bisa
            // lolos tanpa pesan apa pun, terlihat seperti aplikasi "diam saja". Sekarang
            // semua jenis error uninstall SELALU ditampilkan dengan jelas.
            MessageBox.Show(this,
                $"Gagal menjalankan uninstall '{app.Name}':\n\n{ex.Message}\n\n({ex.GetType().Name})",
                "AppFlush", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            LoadApps();
        }
    }

    private void FindLeftoversForSelected()
    {
        var app = SelectedApp();
        if (app is null) return;

        var candidates = LeftoverScanner.FindLeftoverDirs(app, _fileSystem);
        if (candidates.Count == 0)
        {
            MessageBox.Show(this, $"Tidak ditemukan folder sisa untuk '{app.Name}' di lokasi umum.", "AppFlush",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var listText = string.Join("\n", candidates.Select(c => $"- {c.Path}  ({c.Reason})"));
        var confirm = MessageBox.Show(this,
            $"Ditemukan {candidates.Count} folder yang dicurigai sisa dari '{app.Name}':\n\n{listText}\n\nHapus SEMUA folder di atas?",
            "AppFlush", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        var deleted = LeftoverScanner.DeleteCandidates(_fileSystem, candidates.Select(c => c.Path));
        MessageBox.Show(this, $"Terhapus {deleted.Count} folder.", "AppFlush", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
