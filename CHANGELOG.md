# CHANGELOG

## 1.0.0 - Initial repository version

- Light UI with left module navigation and DUKOM PLIN logo.
- Shared Source in Settings.
- Holosys WalkBy module.
- NB-IoT module with date normalization, Croatian character normalization, Missing UserCode > 1 rule and date mismatch control.
- Zgrade module with multi-file merge, mismatch validation, lower reading validation and duplicate checks.
- Logs viewer.
- Window size/position persistence with safe NaN/Infinity protection.

## v1.0.1-refactor

### Changed
- Refactored `MainWindow.xaml.cs` into smaller responsibilities.
- Added `FilePickerService` for file/folder dialogs.
- Added `WindowSettingsService` for safe window state persistence.
- Added `LogViewerService` for safe log loading.
- Added `DragDropHelper` for WPF drag/drop logic.
- Added `AppModule` enum for module navigation.

### Fixed
- Prevent duplicate file entries in Zgrade file list.
- Keep settings save errors from closing the app.
