# SDUI Designer

Modern MVVM pattern ile tasarlanmış Visual Studio benzeri bir UI Designer.

## Mimari

### MVVM Pattern Uygulaması

Proje **Model-View-ViewModel** pattern'ini kullanarak modern ve bakımı kolay bir mimari sunmaktadır:

#### ViewModels
- **ViewModelBase**: INotifyPropertyChanged implementasyonu ile tüm ViewModeller için temel sınıf
- **RelayCommand**: Generic ve non-generic command implementasyonları
- **DesignSurfaceViewModel**: Tasarım yüzeyi için business logic
- **DesignerViewModel**: Ana pencere için koordinasyon logic

#### Views
- **DesignerMainWindow**: MVVM pattern ile bağlanmış ana pencere
- **DesignSurface**: Drag-drop tasarım yüzeyi
- **Toolbox**: Kontrol paleti
- **PropertiesPanel**: Özellik editörü

### Özellikler

✅ **MVVM Pattern**
- ViewModeller UI'dan tamamen ayrılmış
- INotifyPropertyChanged ile otomatik UI güncellemeleri
- RelayCommand ile komut-based işlemler
- ObservableCollection ile koleksiyon takibi

✅ **Undo/Redo Sistemi**
- Command Pattern implementasyonu
- 100 seviye history desteği
- Add, Delete, Move, Resize, Property Change commandları
- Ctrl+Z / Ctrl+Y kısayolları
- StatusBar'da command bilgisi

✅ **Modern Mimari**
- Separation of Concerns
- Single Responsibility Principle
- Dependency Injection hazır yapı
- Event-based iletişim

✅ **Kod Üretimi**
- Gerçek zamanlı C# kod üretimi
- SDUI kontrollerini kullanarak clean code
- Controls.Add() ile modern API kullanımı

✅ **Grid & Snap Sistemi**
- Ayarlanabilir grid boyutu (5px - 50px)
- Grid lines gösterme/gizleme
- Snap to grid özelliği
- Grid snapping açma/kapama

✅ **Keyboard Shortcuts**
- **Ctrl+Z**: Undo
- **Ctrl+Y**: Redo
- **Delete**: Seçili kontrolü sil
- **Escape**: Seçimi temizle

✅ **Kapsamlı Kontrol Desteği**
- Basic: Button, Label, TextBox, Panel, CheckBox, Radio, ComboBox
- Containers: GroupBox, SplitContainer, TabControl, FlowLayoutPanel
- Lists: ListView, TreeView, ListBox
- Input: NumUpDown, TrackBar, ToggleButton
- Visual: ProgressBar, ShapeProgressBar, ScrollBar
- Special: PictureBox, ChatBubble, PropertyGrid
- Menus: MenuStrip, StatusStrip, ContextMenuStrip

### Kullanım

1. Sol panelden bir kontrol seçin (Toolbox)
2. Tasarım yüzeyine tıklayarak ekleyin
3. Sürükleyerek taşıyın, resize handles ile boyutlandırın
4. Sağ panelde otomatik üretilen kodu görün
5. Properties panel'de seçili kontrol özelliklerini düzenleyin
6. Ctrl+Z ile değişikliklerinizi geri alın

### Teknik Detaylar

**Layout Yönetimi**: SuspendLayout/ResumeLayout ile optimize edilmiş
**Dock Sırası**: Fill dock'lu kontroller en son eklenerek doğru yerleşim sağlanır
**Event Handling**: ViewModel'ler arası gevşek bağlı event sistemi
**Command Pattern**: Undo/Redo için tam featured command sistemi

### Genişletme Noktaları

Projeye kolayca yeni özellikler ekleyebilirsiniz:

1. **Yeni Kontrol Ekleme**: `DesignSurface.CreateControl()` metodunu güncelleyin
2. **Yeni Komut Ekleme**: `DesignCommand` base class'ından türetin
3. **Repository Pattern**: Design'ları kaydetmek için `IDesignRepository` implementasyonu eklenebilir
4. **Undo/Redo Genişletme**: Yeni command tipleri `Commands` klasörüne eklenebilir

## Modern Mimari Prensipleri

- ✅ Separation of Concerns
- ✅ Single Responsibility
- ✅ Open/Closed Principle
- ✅ Dependency Inversion (DI-ready)
- ✅ MVVM Pattern
- ✅ Command Pattern
- ✅ Event-Driven Architecture

## Gelecek Özellikler

- Copy/Paste desteği
- Multi-selection desteği
- Zoom in/out
- Alignment tools
- Distribution tools
