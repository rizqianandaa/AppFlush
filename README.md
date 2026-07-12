# AppFlush

**AppFlush** adalah aplikasi uninstaller untuk Windows: satu tempat untuk melihat semua
aplikasi yang terpasang, meng-uninstall-nya lewat uninstaller resminya masing-masing, dan
membersihkan folder sisa yang biasanya ditinggalkan setelah proses uninstall biasa.

Dibuat sebagai alternatif ringan untuk *Apps & Features* bawaan Windows — tanpa jendela
console, tanpa langkah tersembunyi, dan tanpa proses uninstall tiruan: AppFlush selalu
memanggil langsung uninstaller resmi yang sudah didaftarkan tiap aplikasi di Windows.

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

## Lisensi

Belum ditentukan — tambahkan file `LICENSE` sesuai kebutuhan sebelum dirilis publik.
