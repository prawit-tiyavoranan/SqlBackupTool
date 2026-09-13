# SQL Server Backup Tool

โปรแกรมสำรองข้อมูล SQL Server อัตโนมัติ เขียนด้วย **Visual Basic .NET (WinForms)**
รองรับ Full Backup ตามเวลาที่กำหนด, Transaction Log Backup เป็นช่วง, ลบไฟล์เก่าอัตโนมัติ
และสำรอง Log ก่อนเครื่องปิดทุกครั้ง

---

## สารบัญ

- [คุณสมบัติ](#คุณสมบัติ)
- [ความต้องการของระบบ](#ความต้องการของระบบ)
- [การติดตั้ง](#การติดตั้ง)
- [การตั้งค่าก่อนใช้งาน](#การตั้งค่าก่อนใช้งาน)
- [วิธีใช้งาน](#วิธีใช้งาน)
- [โครงสร้างโปรเจกต์](#โครงสร้างโปรเจกต์)
- [รูปแบบไฟล์ Backup](#รูปแบบไฟล์-backup)
- [การตั้งให้รันอัตโนมัติ](#การตั้งให้รันอัตโนมัติ)
- [การกู้คืนข้อมูล (Restore)](#การกู้คืนข้อมูล-restore)
- [แก้ปัญหาที่พบบ่อย](#แก้ปัญหาที่พบบ่อย)

---

## คุณสมบัติ

| ฟีเจอร์ | รายละเอียด |
|---|---|
| Full Backup รายวัน | กำหนดเวลาเริ่มได้เอง ทำงานวันละ 1 ครั้ง |
| Log Backup ตามช่วงเวลา | ตั้งได้ตั้งแต่ 1–1440 นาที |
| สร้างไฟล์ใหม่ทุกวัน | ชื่อไฟล์ผูกกับวันที่ ไฟล์ไม่บวมสะสม |
| ลบไฟล์เก่าอัตโนมัติ | กำหนดจำนวนวันที่ต้องการเก็บย้อนหลัง |
| ทำ Mirror file .bak --> .zip และ .log ไปสำรองใน drive Mirror พร้อมกำหนด retention ลบไฟล์อัตโนมัติ |
| Backup Log ตอน Shutdown | ดักสัญญาณปิดเครื่อง 3 ชั้น กันพลาด |
| Backup Compression | ลดขนาดไฟล์ (Standard/Enterprise เท่านั้น) |
| ทำงานเบื้องหลัง | ย่อลง System Tray ได้ |
| Log การทำงาน | บันทึกลงไฟล์รายวันในโฟลเดอร์ `Logs` |

---

## ความต้องการของระบบ

- **OS:** Windows 10 / 11 / Server 2016 ขึ้นไป
- **Runtime:** .NET 8.0 Desktop Runtime (x64)
- **Database:** SQL Server 2016 ขึ้นไป (รองรับ Express แต่ไม่รองรับ Compression)
- **IDE (สำหรับพัฒนา):** Visual Studio 2022 v17.8 ขึ้นไป

ตรวจสอบ Runtime ที่ติดตั้งแล้ว:

```powershell
dotnet --list-runtimes
```

ต้องเห็นบรรทัดที่ขึ้นต้นด้วย `Microsoft.WindowsDesktop.App 8.0.x`

---

## การติดตั้ง

### สำหรับผู้ใช้งาน

1. ติดตั้ง [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. แตกไฟล์โปรแกรมไปยังโฟลเดอร์ที่ต้องการ เช่น `C:\Tools\SqlBackupTool`
3. รัน `SqlBackupTool.exe` แบบ **Run as Administrator**

### สำหรับนักพัฒนา

```bash
git clone <repository-url>
cd SqlBackupTool
dotnet restore
dotnet build
```

**NuGet Package ที่ใช้:**

```powershell
Install-Package Microsoft.Data.SqlClient
```

---

## การตั้งค่าก่อนใช้งาน

### 1. เปลี่ยน Recovery Model เป็น FULL

**จำเป็นมาก** — ถ้าเป็น `SIMPLE` จะ Backup Log ไม่ได้เลย

```sql
ALTER DATABASE [ชื่อฐานข้อมูล] SET RECOVERY FULL;
```

ตรวจสอบสถานะปัจจุบัน:

```sql
SELECT name, recovery_model_desc FROM sys.databases;
```

> หลังเปลี่ยนเป็น FULL ต้องทำ Full Backup อย่างน้อย 1 ครั้งก่อน จึงจะ Backup Log ได้

### 2. กำหนดสิทธิ์ผู้ใช้งาน

```sql
USE [master];
CREATE LOGIN [DOMAIN\BackupUser] FROM WINDOWS;
ALTER SERVER ROLE [dbcreator] ADD MEMBER [DOMAIN\BackupUser];

USE [ชื่อฐานข้อมูล];
CREATE USER [DOMAIN\BackupUser] FOR LOGIN [DOMAIN\BackupUser];
ALTER ROLE [db_backupoperator] ADD MEMBER [DOMAIN\BackupUser];
```

### 3. สิทธิ์โฟลเดอร์ปลายทาง

⚠️ **จุดที่พลาดกันบ่อยที่สุด** — คำสั่ง `BACKUP DATABASE` เขียนไฟล์โดย **SQL Server Service Account**
ไม่ใช่ user ที่รันโปรแกรม ดังนั้นต้องให้สิทธิ์ Write กับบัญชีนั้นด้วย

ตรวจสอบว่า SQL Server รันด้วยบัญชีอะไร:

```sql
SELECT servicename, service_account
FROM sys.dm_server_services;
```

จากนั้นให้สิทธิ์ Modify กับบัญชีนั้นบนโฟลเดอร์ที่ใช้เก็บ Backup

---

## วิธีใช้งาน

### หน้าจอหลัก

| ส่วน | รายละเอียด |
|---|---|
| **การเชื่อมต่อ SQL Server** | ระบุ Server, ฐานข้อมูล และวิธี Authentication |
| **ตารางเวลา** | ตั้งเวลา Full Backup และความถี่ Log Backup |
| **ที่เก็บไฟล์ / อายุไฟล์** | เลือกโฟลเดอร์ปลายทาง และจำนวนวันที่เก็บย้อนหลัง |
| **แถบสถานะ** | แสดงเวลา Full Backup ครั้งถัดไป และเวลา Log ล่าสุด |
| **หน้าต่าง Log** | แสดงผลการทำงานแบบเรียลไทม์ |

### ขั้นตอนเริ่มต้นใช้งาน

1. กรอกชื่อ **Server** เช่น `localhost` หรือ `192.168.1.10\SQLEXPRESS`
2. กด **ทดสอบการเชื่อมต่อ** เพื่อยืนยันว่าต่อได้
3. กด **โหลดรายชื่อ DB** แล้วเลือกฐานข้อมูลที่ต้องการ
4. ตั้งเวลา Full Backup (แนะนำช่วงกลางคืน เช่น 02:00)
5. ตั้งความถี่ Log Backup (แนะนำ 30–60 นาที)
6. เลือกโฟลเดอร์เก็บไฟล์และจำนวนวันที่เก็บ
7. กด **บันทึกค่า** → กด **เริ่มทำงาน**

### ปุ่มควบคุม

- **บันทึกค่า** — เก็บการตั้งค่าลง `BackupSettings.xml`
- **เริ่มทำงาน / หยุด** — เปิด-ปิดตัวจับเวลาอัตโนมัติ
- **Full Backup ทันที** — สั่ง Full Backup แบบ manual
- **Log Backup ทันที** — สั่ง Log Backup แบบ manual

> ปิดหน้าต่างด้วยปุ่ม X ขณะทำงาน โปรแกรมจะย่อลง System Tray ไม่ได้ปิดจริง
> ดับเบิลคลิกไอคอนใน Tray เพื่อเรียกกลับขึ้นมา

---

## โครงสร้างโปรเจกต์

```
SqlBackupTool/
├── Form1.vb                  # UI logic + event handlers + scheduler
├── Form1.Designer.vb         # หน้าจอ (แก้ผ่าน Designer View ได้)
├── BackupEngine.vb           # หัวใจการ Backup / Cleanup
├── AppSettings.vb            # คลาสเก็บค่า config (XML)
├── Logger.vb                 # เขียน log ลงไฟล์
├── BackupSettings.xml        # ไฟล์ config (สร้างอัตโนมัติ)
└── Logs/
    └── app_yyyyMMdd.log      # log รายวัน
```

### หน้าที่ของแต่ละคลาส

| ไฟล์ | หน้าที่ |
|---|---|
| `AppSettings` | โหลด/บันทึกค่าตั้งค่า และสร้าง Connection String |
| `BackupEngine` | `RunFullBackup()`, `RunLogBackup()`, `CleanupOldBackups()` |
| `Logger` | เขียน log แบบ thread-safe พร้อม event แจ้ง UI |
| `Form1` | ควบคุม UI, ตัวจับเวลา และดักสัญญาณ Shutdown |

---

## รูปแบบไฟล์ Backup

```
{ชื่อDB}_FULL_{yyyyMMdd}.bak     ← 1 ไฟล์ต่อวัน (WITH INIT ทับของเดิม)
{ชื่อDB}_LOG_{yyyyMMdd}.trn      ← 1 ไฟล์ต่อวัน (WITH NOINIT ต่อท้าย)
```

**ตัวอย่างเมื่อตั้งเก็บย้อนหลัง 3 วัน:**

```
MyDB_FULL_20260906.bak
MyDB_LOG_20260906.trn
MyDB_FULL_20260907.bak
MyDB_LOG_20260907.trn
MyDB_FULL_20260908.bak
MyDB_LOG_20260908.trn
```

### เหตุผลที่ออกแบบแบบนี้

- **Full ใช้ `WITH INIT`** — เขียนทับไฟล์ของวันเดียวกัน ป้องกันไฟล์บวมเมื่อสั่ง manual ซ้ำ
- **Log ใช้ `WITH NOINIT`** — ต่อท้ายไฟล์เดิมของวันนั้น เพื่อให้ restore ต่อเนื่องได้
- **ตัดไฟล์ใหม่ทุกวัน** — จำกัดขนาดไฟล์ และลบทีละวันได้สะดวก

---

## การตั้งให้รันอัตโนมัติ

แนะนำใช้ **Task Scheduler** แทนการทำเป็น Windows Service เลือก Atlogon และ delay ประมาณ 10นาที เพื่อไม่ให้ load ตอน startup
เพราะ Service ปกติจะไม่ได้รับสัญญาณ Shutdown แบบเดียวกับโปรแกรม UI (ต้องเขียน `SERVICE_CONTROL_PRESHUTDOWN` เพิ่ม)

1. เปิด **Task Scheduler** → **Create Task**
2. แท็บ **General:** ติ๊ก `Run with highest privileges`
3. แท็บ **Triggers:** เลือก `At log on`
4. แท็บ **Actions:** ชี้ไปที่ `SqlBackupTool.exe`
5. แท็บ **Conditions:** ยกเลิกติ๊ก `Stop if the computer switches to battery power`
6. แท็บ **Settings:** ยกเลิกติ๊ก `Stop the task if it runs longer than...`

---

## การกู้คืนข้อมูล (Restore)

### กู้คืนจาก Full Backup อย่างเดียว

```sql
RESTORE DATABASE [MyDB]
FROM DISK = 'C:\SQLBackup\MyDB_FULL_20260908.bak'
WITH REPLACE, RECOVERY;
```

### กู้คืนแบบ Point-in-Time (Full + Log)

```sql
-- 1. Restore Full ค้างไว้ในสถานะ NORECOVERY
RESTORE DATABASE [MyDB]
FROM DISK = 'C:\SQLBackup\MyDB_FULL_20260908.bak'
WITH REPLACE, NORECOVERY;

-- 2. ดูว่าในไฟล์ .trn มีกี่ชุด (จดเลข Position ไว้)
RESTORE HEADERONLY
FROM DISK = 'C:\SQLBackup\MyDB_LOG_20260908.trn';

-- 3. Restore log ทีละชุดตามลำดับ
RESTORE LOG [MyDB]
FROM DISK = 'C:\SQLBackup\MyDB_LOG_20260908.trn'
WITH FILE = 1, NORECOVERY;

-- 4. ชุดสุดท้าย ระบุเวลาที่ต้องการย้อนกลับไป
RESTORE LOG [MyDB]
FROM DISK = 'C:\SQLBackup\MyDB_LOG_20260908.trn'
WITH FILE = 2, STOPAT = '2026-09-08 14:30:00', RECOVERY;
```

> ⚠️ ควรซ้อมการ Restore บนเครื่องทดสอบอย่างน้อยเดือนละครั้ง
> Backup ที่ไม่เคยทดสอบ Restore = ไม่มี Backup

---

## แก้ปัญหาที่พบบ่อย

### ปัญหาการเชื่อมต่อ

| อาการ | สาเหตุ | วิธีแก้ |
|---|---|---|
| `A network-related or instance-specific error` | ชื่อ Server ผิด / Service ไม่ทำงาน | เช็คใน SQL Server Configuration Manager |
| `certificate chain was issued by an authority that is not trusted` | `Microsoft.Data.SqlClient` บังคับเข้ารหัส | เพิ่ม `TrustServerCertificate=True` ใน connection string |
| `Login failed for user` | สิทธิ์ไม่พอ | เพิ่ม role `db_backupoperator` |

### ปัญหาการ Backup

| อาการ | สาเหตุ | วิธีแก้ |
|---|---|---|
| `Operating system error 5 (Access is denied)` | SQL Service Account ไม่มีสิทธิ์เขียนโฟลเดอร์ | ให้สิทธิ์ Modify กับบัญชีที่ SQL รันอยู่ |
| `BACKUP LOG cannot be performed because there is no current database backup` | ยังไม่เคยทำ Full Backup | สั่ง Full Backup 1 ครั้งก่อน |
| `[LOG] ข้าม: ฐานข้อมูลไม่ได้อยู่ใน FULL Recovery Model` | Recovery Model เป็น SIMPLE | `ALTER DATABASE ... SET RECOVERY FULL` |
| `BACKUP DATABASE ... COMPRESSION is not supported` | ใช้ SQL Server Express | เอาติ๊ก **ใช้ Backup Compression** ออก |

### ปัญหาตอนพัฒนา (Visual Studio)

| อาการ | วิธีแก้ |
|---|---|
| `Type 'SqlCommand' is not defined` | ติดตั้ง `Microsoft.Data.SqlClient` + เปลี่ยน `Imports` |
| `'HighDpiMode' is not a member of 'MyApplication'` | ลบ `<HighDpiMode>` ออกจาก `Application.myapp` (เกิดเมื่อลด TFM ลงเป็น .NET Framework) |
| `Failed to launch the design tools server process` | ปิด VS → ลบ `bin`, `obj` และโฟลเดอร์ `%LocalAppData%\Microsoft\VisualStudio\17.0_xxxxx\ComponentModelCache` |
| `IDE1006 Naming rule violation` | เป็นแค่ Message ไม่ใช่ Error — ชื่อ event handler แบบ `btnXxx_Click` เป็นมาตรฐาน WinForms |

---

## ข้อควรระวังด้านความปลอดภัย

- ไฟล์ `BackupSettings.xml` เก็บรหัสผ่านแบบ **plain text** — แนะนำให้ใช้ Windows Authentication แทน
- หากจำเป็นต้องใช้ SQL Authentication ควรจำกัดสิทธิ์เข้าถึงไฟล์ config ด้วย NTFS permission
- ควรสำเนาไฟล์ Backup ไปเก็บนอกเครื่องด้วย (ตามหลัก **3-2-1 rule**: 3 ชุด, 2 สื่อ, 1 ชุดนอกสถานที่)

---

## แผนพัฒนาต่อ

- [ ] แจ้งเตือนทางอีเมลเมื่อ Backup ล้มเหลว
- [ ] รองรับ Backup หลายฐานข้อมูลพร้อมกัน
- [ ] Differential Backup
- [ ] ตรวจสอบไฟล์หลัง Backup ด้วย `RESTORE VERIFYONLY`
- [ ] เข้ารหัสรหัสผ่านใน config ด้วย DPAPI

---

## License

MIT License

---

## ผู้พัฒนา

พัฒนาด้วย VB.NET + WinForms บน .NET 8