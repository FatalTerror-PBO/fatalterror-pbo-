# Fatal Terror — Game Horor First-Person (Unity)

Game horor first-person yang dibuat dengan **Unity 6** dan **C#**. Pemain menjelajahi sebuah rumah sakit gelap, mengumpulkan kunci, menghindari hantu, lalu kabur lewat pintu keluar.

Project ini dibuat untuk tugas mata kuliah **Pemrograman Berorientasi Objek (PBO)** dan menerapkan konsep OOP seperti inheritance, interface, abstract class, collection, dictionary, dan generic.

---

## Fitur

- Gerak first-person (jalan, lari dengan stamina, kamera mouse)
- Senter dengan baterai yang bisa habis dan berkedip
- Sistem nyawa dan medkit
- Inventory (kunci, baterai, medkit, catatan)
- Pintu yang bisa dibuka, dan pintu keluar yang terkunci
- Dua jenis musuh:
  - **Ghost**: berpatroli, lalu mengejar jika melihat pemain
  - **Stalker**: hanya bergerak saat tidak sedang dilihat pemain
- Jump-scare acak (memakai object pool)
- Suara efek dan ambience
- HUD (bar nyawa, baterai, stamina), menu Pause, layar Menang dan Game Over

## Kontrol

| Tombol | Fungsi |
|---|---|
| `W` `A` `S` `D` | Berjalan |
| `Mouse` | Melihat sekitar |
| `Shift` | Berlari |
| `F` | Senter on/off |
| `E` | Interaksi (ambil item, buka pintu) |
| `Esc` | Jeda |

## Tujuan Game

1. Ambil **kunci** di ruangan paling barat daya.
2. Hindari atau kelabui hantu. Gunakan senter hemat-hemat.
3. Buka **pintu hijau** di ujung timur koridor untuk menang.

---

## Cara Menjalankan

### Persyaratan
- **Unity 6** (dikembangkan dengan 6000.6.x), template **3D (URP)**
- Package **AI Navigation** (Window → Package Manager → Unity Registry)
- Package **TextMeshPro** (biasanya sudah terpasang)

### Langkah
1. Clone repo ini:
   ```bash
   git clone https://github.com/FatalTerror-PBO/fatalterror-pbo-.git
   ```
2. Buka folder hasil clone lewat **Unity Hub → Add → Add project from disk**.
3. Tunggu Unity selesai mengimpor dan compile.
4. Pastikan pengaturan input: **Edit → Project Settings → Player → Other Settings → Active Input Handling = Both**. Script memakai `Input` klasik. Restart Unity jika berubah.
5. Bangun scene dari menu: **Horror → 1. Build Complete Demo Scene**
   - Jika muncul dialog TextMeshPro, klik **Impor**, lalu jalankan menu itu lagi.
6. Jika NavMesh gagal dibake otomatis: pilih **GameObject → AI → NavMesh Surface**, lalu klik **Bake**.
7. Tekan tombol **Play**.

> Scene yang dibuat otomatis memakai bentuk dasar Unity (kotak dan kapsul) sebagai model, dan suara sintetis di `Assets/HorrorDemo/Audio`.

### Memakai map Hospital Horror Pack (opsional)
Paket aset **PBR - Hospital Horror Pack. Free** (DNK_DEV) tidak disertakan di repo karena ukurannya besar. Unduh dari Unity Asset Store dan impor ke project jika ingin memakainya. Setelah membuka scene map-nya, jalankan menu **Horror → 2. Add Player + Managers + UI to Current Scene**.

---

## Struktur Folder

```
Assets/
├── Editor/            HorrorSceneBuilder (pembuat scene otomatis)
├── HorrorDemo/        Suara (.wav), material, scene hasil build
└── Scripts/
    ├── Core           Enums, GameEvents, GameManager
    ├── Interfaces     IInteractable, IDamageable
    ├── Utils          EventChannel<T>, ObjectPool<T>, Registry<TKey,TValue>
    ├── Player         PlayerController, PlayerHealth, Flashlight, PlayerInteraction
    ├── Items          Inventory, ItemInfo, ItemCatalog
    ├── Interactables  InteractableBase, PickupBase, KeyPickup, BatteryPickup,
    │                  MedkitPickup, NotePickup, Door
    ├── Enemies        EnemyBase, GhostEnemy, StalkerEnemy
    ├── Effects        ScareManager, ScareEffect, LightFlicker
    ├── Audio          AudioManager
    └── UI             HUDController
```

---

## Penerapan Konsep OOP

| Konsep | File | Keterangan |
|---|---|---|
| **Property get/set** | `Player/PlayerHealth.cs`, `Player/Flashlight.cs`, `Player/PlayerController.cs` | Property `CurrentHealth`, `Battery`, `Stamina` dengan setter private dan validasi (clamp) |
| **Inheritance** | `Enemies/GhostEnemy.cs`, `StalkerEnemy.cs` : `EnemyBase`; `KeyPickup` : `PickupBase` : `InteractableBase` | Pewarisan bertingkat dengan `override` |
| **Interface** | `Interfaces/IInteractable.cs`, `IDamageable.cs` | Diimplementasi oleh `InteractableBase` dan `PlayerHealth` |
| **Abstract class** | `Enemies/EnemyBase.cs`, `Interactables/InteractableBase.cs`, `PickupBase.cs` | Method abstract `UpdateBehaviour`, `Interact`, `OnPickedUp` |
| **Collection** | `Items/Inventory.cs` (`List<string>`), `Enemies/GhostEnemy.cs` (`List<Transform>`), `Utils/ObjectPool.cs` (`Queue<T>`) | List dan Queue |
| **Dictionary** | `Items/Inventory.cs` (`Dictionary<ItemType,int>`), `Audio/AudioManager.cs` (`Dictionary<string,AudioClip>`) | Penyimpanan berpasangan key-value |
| **Generic** | `Utils/EventChannel.cs`, `ObjectPool.cs`, `Registry.cs` | Class generik dengan constraint `where` |

---

## Teknologi

- Unity 6 (URP)
- C#
- TextMeshPro, Unity AI Navigation (NavMesh)


## Lisensi

Dibuat untuk keperluan tugas kuliah. Aset pihak ketiga (jika dipakai) mengikuti lisensi masing-masing penyedia.
