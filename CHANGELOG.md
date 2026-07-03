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

## v1.0.2 - Module Views Refactor

### Changed
- Split module UI into separate WPF UserControls under `Views/`.
- `MainWindow.xaml` is now mostly shell/navigation layout.
- Existing functionality is kept through view events and service calls.

## v1.1.0 - MVVM Migration

### Refactored
- Added `Infrastructure/ObservableObject` and `Infrastructure/RelayCommand`.
- Added module ViewModels: Dashboard, WalkBy, NB-IoT, Zgrade, Logs, Settings, About.
- Added `MainViewModel` as application shell ViewModel.
- Converted navigation to `NavigateCommand` and `ContentControl` DataTemplates.
- Moved module run actions from `MainWindow.xaml.cs` into ViewModels.
- Reduced `MainWindow.xaml.cs` to shell startup and settings persistence.

### Kept
- Existing WalkBy, NB-IoT and Zgrade business logic.
- Existing output formats and validation rules.
- Existing light UI design.

## v1.1.1 - Module Functionality Stabilization

### Fixed
- Restored module functionality after MVVM migration checks.
- Zgrade parser now reads the actual building reading from the first value after date/time (`parts[3]`) instead of the trailing status/control column.
- Zgrade **Open output** now opens the generated `Zgrade_obrade` subfolder after processing.

### Kept
- Existing WalkBy, NB-IoT and Zgrade output formats.
- Shared Source behavior from Settings.
- Light UI and module navigation.
