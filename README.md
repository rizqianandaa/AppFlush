# AppFlush

**AppFlush** adalah aplikasi uninstaller untuk Windows: satu tempat untuk melihat semua
aplikasi yang terpasang, meng-uninstall-nya, dan membersihkan folder sisa yang biasanya
ditinggalkan setelah proses uninstall biasa. Dibuat karena Windows tidak punya cara
bawaan yang cepat untuk itu semua.

Tidak ada jendela console yang muncul. AppFlush selalu tampil sebagai jendela aplikasi
biasa, seperti Revo Uninstaller atau CCleaner.

## Fitur

- **Daftar semua aplikasi terpasang**, lengkap dengan versi dan penerbit, plus kotak
  pencarian untuk memfilter dengan cepat.
- **Uninstall satu klik** — AppFlush memanggil langsung uninstaller resmi yang sudah
  didaftarkan tiap aplikasi di Windows (bukan proses tiruan), jadi hasilnya selalu
  konsisten dengan uninstall manual lewat *Control Panel* / *Apps & Features*.
- **Cari & hapus folder sisa** — setelah uninstall, AppFlush bisa memindai lokasi umum
  (`Program Files`, `Program Files (x86)`, `ProgramData`, `AppData\Roaming`,
  `AppData\Local`) untuk folder yang kemungkinan sisa dari aplikasi tersebut, lalu
  menghapusnya setelah kamu konfirmasi.
- **Menu klik-kanan "Uninstall dengan AppFlush"** pada shortcut `.lnk` — bisa langsung
  uninstall aplikasi dari shortcut-nya di Desktop/Start Menu tanpa membuka jendela
  utama AppFlush dulu.
- **Ringan & portable-friendly** — dibangun sebagai *self-contained single file*, jadi
  tidak perlu install .NET Runtime terpisah di komputer yang menjalankannya.

## Cara pakai

1. Download installer terbaru (`AppFlush-Setup-x.x.x.exe`) lalu jalankan.
2. Buka **AppFlush** dari Start Menu atau Desktop.
3. Ketik nama aplikasi di kotak pencarian untuk menemukannya dengan cepat.
4. Pilih aplikasinya di daftar, lalu klik tombol merah **Uninstall**.
5. Uninstaller resmi dari aplikasi itu akan berjalan — ikuti langkah-langkahnya seperti
   biasa (beberapa aplikasi punya dialog konfirmasi sendiri, beberapa langsung berjalan
   diam-diam; itu tergantung masing-masing aplikasi, bukan AppFlush).
6. Setelah selesai, klik **Cari folder sisa** untuk memindai dan membersihkan folder
   yang mungkin masih tertinggal.

### Uninstall lewat menu klik-kanan

Klik kanan file shortcut (`.lnk`) aplikasi (misalnya di Desktop atau Start Menu), lalu
pilih **"Uninstall dengan AppFlush"**.

> **Catatan untuk Windows 11:** menu klik-kanan baru di Windows 11 hanya menampilkan
> sebagian item di daftar teratas; item pihak ketiga seperti punya AppFlush (dan
> hampir semua aplikasi lain yang menambah menu klik-kanan lewat cara klasik) selalu
> ditaruh di submenu **"Show more options"**. Ini keterbatasan Windows 11 itu sendiri,
> bukan sesuatu yang bisa diperbaiki lewat registry biasa — satu-satunya cara resmi
> supaya tampil di daftar teratas adalah membungkus aplikasi sebagai paket MSIX
> dengan ekstensi menu dari Windows App SDK, sebuah model distribusi yang jauh lebih
> rumit daripada installer `.exe` biasa.

### Soal dialog konfirmasi

AppFlush **tidak** menampilkan dialog konfirmasinya sendiri sebelum uninstall — begitu
tombol **Uninstall** atau menu klik-kanan diklik, uninstaller resmi aplikasi tersebut
langsung dijalankan. Aplikasi yang punya dialog konfirmasi sendiri (mayoritas aplikasi)
akan menampilkannya seperti biasa. Untuk aplikasi yang uninstaller-nya benar-benar diam
(tanpa dialog sama sekali), proses uninstall akan langsung berjalan tanpa konfirmasi
apa pun — jadi pastikan kamu memang ingin menghapus aplikasi tersebut sebelum menekan
tombol Uninstall.

## Bagaimana AppFlush bekerja

AppFlush membaca daftar aplikasi terpasang dari Windows Registry (kunci `Uninstall`
yang juga dipakai *Apps & Features*), lalu menjalankan `UninstallString` resmi milik
aplikasi tersebut apa adanya — AppFlush tidak menulis ulang atau menebak-nebak proses
uninstall aplikasi manapun. Fitur "cari folder sisa" murni pencocokan nama folder di
beberapa lokasi umum instalasi Windows; ini pemindaian ringan (mirip mode standar di
Revo Uninstaller), bukan pemindaian mendalam berbasis jejak registry saat instalasi,
jadi tidak menjamin 100% seluruh sisa file/registry ikut terhapus.

## Instalasi

Unduh `AppFlush-Setup-x.x.x.exe` dari halaman [Releases], jalankan, ikuti wizard-nya.
Tidak perlu install .NET Runtime terpisah.

## Build dari source (untuk kontributor)

Butuh [.NET 8 SDK](https://dotnet.microsoft.com/download) dan opsional Visual Studio
2022 (workload ".NET desktop development").

```powershell
dotnet restore AppFlush.sln
dotnet build AppFlush.sln -c Release
dotnet test tests/AppFlush.Core.Tests/AppFlush.Core.Tests.csproj -c Release

# Publish self-contained single-file:
dotnet publish src/AppFlush.App/AppFlush.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

Installer `.exe` dibuat dari `installer/installer.iss` lewat [Inno Setup], atau otomatis
lewat GitHub Actions (`.github/workflows/build-windows.yml`, jalankan manual lewat tab
**Actions** → workflow **"Build Windows exe & installer"** → unduh artifact
**`AppFlush-Setup`**).

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
  build-windows.yml   CI: build, test, publish, buat installer otomatis.
```

## Rilis versi baru (untuk maintainer)

Repo ini sengaja TIDAK menyimpan `bin/`, `obj/`, `publish/`, maupun hasil compile Inno
Setup (`installer/Output/`) -- semua itu di-ignore lewat `.gitignore` supaya ukuran repo
tetap kecil (source code saja, ratusan KB). Installer `.exe` untuk pengguna akhir selalu
didistribusikan lewat tab **Releases** GitHub, persis seperti kebanyakan proyek open
source lain.

Untuk membuat rilis baru: push git tag berawalan `v` (misalnya `v1.0.0`). GitHub Actions
otomatis build, test, publish, compile installer, lalu membuat GitHub Release baru dengan
`AppFlush-Setup-x.x.x.exe` + checksum SHA256 terlampir sebagai asset -- tidak perlu upload
manual.

```powershell
git tag v1.0.0
git push origin v1.0.0
```

## Keamanan

- AppFlush tidak minta hak Administrator saat dibuka (`asInvoker`); hanya proses
  uninstall aplikasi tertentu yang mungkin memicu dialog UAC miliknya sendiri, sama
  seperti uninstall manual lewat Control Panel.
- Pendaftaran menu klik-kanan ditulis ke `HKEY_CURRENT_USER` (bukan
  `HKEY_LOCAL_MACHINE`), jadi tidak perlu hak admin untuk mendaftar/melepasnya.
- Build lewat GitHub Actions menghasilkan checksum SHA256 untuk verifikasi integritas
  file installer.

## Lisensi

Belum ditentukan — tambahkan file `LICENSE` sesuai kebutuhan sebelum dirilis publik.
