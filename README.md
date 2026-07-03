# DUKOM PLIN Utility Professional

WPF .NET 8 desktop aplikacija za obradu očitanja.

## Moduli

- Dashboard
- Holosys WalkBy
- NB-IoT
- Zgrade
- Logovi
- Postavke
- O programu

## Glavne funkcije

### Shared Source
U Postavkama se postavlja zajednički `ZaRtf.txt` iz KODING-a (STA + PRI). Koristi se u modulima WalkBy, NB-IoT i Zgrade.

### WalkBy
- XML `Mjerilo` parsing: `Broj` + `Stanje`
- generira `output.txt`, `result.txt`, `missing.txt` i `walkby.log`
- format `result.txt`: `UserCode;Name;OMM;Stanje;;Broj;RFID`

### NB-IoT
- učitava tab-delimited/semicolon export (`.xls`, `.txt`, `.csv`)
- export format: `User Code;Name;;Last Reading whole part;Date;Meter Code;`
- Last Reading koristi samo cijeli dio broja
- datumi se normaliziraju u `dd.MM.yyyy` bez vremena
- hrvatski znakovi se zamjenjuju ASCII znakovima
- Missing UserCode kontrola vrijedi samo kad je `Last Reading > 1`
- Expected Date tolerancija ±2 dana

### Zgrade
- dodavanje više TXT fajlova
- parsira whitespace/fixed-width redove gdje su prva dva polja `User Code` i `Brojilo`
- spaja fajlove u `Zgrade_obrade/zgrade_merged.txt`
- provjerava par `User Code + Brojilo` prema Shared Source
- kontrolira da novo stanje nije manje od Source stanja
- kontrolira duplikate cijelog retka, duplikat para `User Code + Brojilo` i `User Code` s više brojila
- prikazuje greške u tablici u aplikaciji

## Build

Otvoriti `DukomPlinUtility.sln` u Visual Studiju 2022 ili pokrenuti:

```bat
build_exe.bat
```

Output EXE je u `publish`.

## Git

Preporučeni branch workflow:

- `main` = stabilna verzija
- `develop` = razvoj
- `feature/*` = nove funkcije

## Developer note

This version starts code cleanup after the initial Git version. MainWindow still owns the UI, but common logic has been moved into Services/Helpers so future modules can be added with less risk.

## v1.3.4 Operations Center

This version adds a live dashboard overview after module processing, global status/progress bar and colored validation rows.
