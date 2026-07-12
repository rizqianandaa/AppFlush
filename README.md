# AppFlush

**AppFlush** adalah aplikasi uninstaller untuk Windows: satu tempat untuk melihat semua
aplikasi yang terpasang, meng-uninstall-nya lewat uninstaller resminya masing-masing, dan
membersihkan folder sisa yang biasanya ditinggalkan setelah proses uninstall biasa.

Dibuat sebagai alternatif ringan untuk *Apps & Features* bawaan Windows — tanpa jendela
console, tanpa langkah tersembunyi, dan tanpa proses uninstall tiruan: AppFlush selalu
memanggil langsung uninstaller resmi yang sudah didaftarkan tiap aplikasi di Windows.

## Fitur

- **Daftar semua aplikasi terpasang**, lengkap dengan versi dan penerbit, dengan kotak
  pencarian untuk memfilter dengan cepat.
- **Uninstall satu klik** yang memanggil langsung uninstaller resmi aplikasi tersebut,
  sehingga hasilnya selalu konsisten dengan uninstall manual lewat Control Panel.
- **Pemindaian folder sisa** di lokasi instalasi umum Windows (`Program Files`,
  `Program Files (x86)`, `ProgramData`, `AppData\Roaming`, `AppData\Local`), dengan opsi
  hapus setelah konfirmasi.
- **Menu klik-kanan "Uninstall dengan AppFlush"** pada shortcut `.lnk`, untuk uninstall
  langsung dari Desktop/Start Menu tanpa membuka jendela utama.
- **Self-contained single file** — tidak bergantung pada .NET Runtime terpisah di
  komputer yang menjalankannya.

## Bagaimana AppFlush bekerja

AppFlush membaca daftar aplikasi terpasang langsung dari kunci `Uninstall` di Windows
Registry — sumber data yang sama dipakai *Apps & Features* — lalu menjalankan
`UninstallString` resmi milik aplikasi tersebut apa adanya. AppFlush tidak menulis ulang
atau menebak-nebak proses uninstall aplikasi manapun, sehingga dialog/UI konfirmasi yang
muncul setelahnya (kalau ada) adalah milik aplikasi itu sendiri, bukan buatan AppFlush.

Fitur pemindaian folder sisa bekerja dengan mencocokkan nama folder di lokasi instalasi
umum terhadap nama/penerbit aplikasi (pencocokan heuristik, mirip mode standar pada
Revo Uninstaller) — bukan pemindaian mendalam berbasis jejak registry saat instalasi,
jadi tidak menjamin seluruh sisa file/registry ikut terhapus di setiap kasus.

Menu klik-kanan didaftarkan sebagai verb shell klasik lewat Windows Registry (bukan
ekstensi context-menu MSIX). Konsekuensinya, di Windows 11 item ini akan berada di
submenu **"Show more options"** alih-alih tampil di daftar teratas — ini perilaku
Windows 11 untuk semua verb shell klasik pihak ketiga, bukan hanya AppFlush.

## Keamanan

- Tidak minta hak Administrator saat dibuka (`asInvoker`); hanya proses uninstall
  aplikasi tertentu yang mungkin memicu dialog UAC miliknya sendiri, sama seperti
  uninstall manual lewat Control Panel.
- Pendaftaran menu klik-kanan ditulis ke `HKEY_CURRENT_USER` (bukan
  `HKEY_LOCAL_MACHINE`), sehingga tidak memerlukan hak admin untuk mendaftar/melepasnya.
- Build lewat GitHub Actions menghasilkan checksum SHA256 untuk verifikasi integritas
  installer.

## Rilis versi baru (untuk maintainer)

Repo ini sengaja tidak menyimpan `bin/`, `obj/`, `publish/`, maupun hasil compile Inno
Setup (`installer/Output/`) — semua di-ignore lewat `.gitignore` supaya ukuran repo
tetap kecil. Installer untuk pengguna akhir selalu didistribusikan lewat tab
**Releases** GitHub.

Membuat rilis baru: push git tag berawalan `v` (misalnya `v1.0.0`). GitHub Actions
otomatis build, test, publish, compile installer, lalu membuat GitHub Release dengan
`AppFlush-Setup-x.x.x.exe` + checksum SHA256 terlampir sebagai asset.

```powershell
git tag v1.0.0
git push origin v1.0.0
```

## Build dari source (untuk kontributor)

Butuh [.NET 8 SDK](https://dotnet.microsoft.com/download) dan opsional Visual Studio
2022 (workload ".NET desktop development").

```powershell
dotnet restore AppFlush.sln
dotnet build AppFlush.sln -c Release
dotnet test tests/AppFlush.Core.Tests/AppFlush.Core.Tests.csproj -c Release
dotnet publish src/AppFlush.App/AppFlush.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

Installer `.exe` dibuat dari `installer/installer.iss` lewat [Inno Setup], atau otomatis
lewat GitHub Actions (`.github/workflows/build-windows.yml`).

Struktur proyek:

```
AppFlush.sln
src/
  AppFlush.Core/     Logika bisnis (scan registry, cocokkan shortcut, uninstall,
                     cari folder sisa, menu klik-kanan) -- tanpa kode UI.
  AppFlush.App/      Aplikasi WinForms (GUI) + entry point.
tests/
  AppFlush.Core.Tests/  Unit test (xUnit) untuk AppFlush.Core.
installer/
  installer.iss      Skrip Inno Setup untuk installer .exe.
.github/workflows/
  build-windows.yml   CI: build, test, publish, buat installer & release otomatis.
```

## Lisensi

Belum ditentukan — tambahkan file `LICENSE` sesuai kebutuhan sebelum dirilis publik.
