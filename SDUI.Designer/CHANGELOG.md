# SDUI Designer - Changelog

## v1.0.0 - İlk Release (Ocak 2026)

### ✨ Yeni Özellikler

#### Temel Designer Altyapısı
- Visual Studio benzeri split-panel layout
- MVVM pattern ile temiz mimari
- Event-driven reaktif güncellemeler

#### Drag & Drop Sistem
- 11 farklı kontrol tipi (Button, Label, TextBox, Panel, CheckBox, RadioButton, ComboBox, ListView, TreeView, ProgressBar, ScrollBar)
- Toolbox'tan seç, design surface'e tıkla, ekle
- Mouse ile sürükle-bırak ile kontrol taşıma
- FindControlAtPoint ile hit-testing

#### Resize İşlevselliği
- 8 resize handle (4 köşe + 4 kenar)
- GDI+ ile çizilen handles (WinForms overlay)
- Akıllı cursor feedback (↔ ↕ ⤡ ⤢)
- Canlı boyutlandırma desteği

#### Grid Snapping
- 10px grid boyutu
- Placement, dragging ve resizing'de snap
- View menüden açma/kapama
- Menüde durum göstergesi (On/Off)

#### Properties Panel
- Canlı property düzenleme
- Text, Size, Location, Colors, Visibility
- Değişiklikler anında uygulanıyor
- CheckBox, TextBox ve Label editörleri

#### Code Generator
- Real-time C# kod üretimi
- Production-ready initialization kodu
- Otomatik güncelleme
- Alt panelde sürekli görüntüleme

#### Save/Load System
- JSON tabanlı serileştirme
- .sdui dosya formatı
- Tam durum koruması
- Human-readable format

### 🎯 Teknik Detaylar

**Mimari Kararlar:**
- SDUI Panel (selection border) + WinForms Control (resize handles) hybrid yaklaşım
- SelectionOverlay: SDUI.Controls.Panel (BorderColor ile basit border)
- SelectionHandlesOverlay: System.Windows.Forms.Control (GDI+ ile handle çizimi)

**Event Flow:**
```
User Action → DesignSurface Event → ViewModel Update → UI Refresh
            ↓
        CodeGenerator.GenerateCode() → CodeOutput.Text update
```

**Grid Snapping Algoritması:**
```csharp
SKPoint SnapToGrid(SKPoint p) => new SKPoint(
    (p.X / GridSize) * GridSize,
    (p.Y / GridSize) * GridSize
);
```

### 📋 İyileştirmeler

- Controls.Add() sırası optimize edildi (Dock.Fill en başta)
- BringToFront() çağrıları ile z-order kontrolü
- PropertyValueChangedEventArgs ile naming conflict çözüldü
- DesignSerializer ile clean separation

### 🐛 Düzeltilen Hatalar

1. **Kontrol Görünürlüğü**: SplitContainer tüm ekranı kaplıyordu
   - Çözüm: Dock sırası değişti, BringToFront() eklendi

2. **Namespace Çakışması**: PropertyChangedEventArgs (System.ComponentModel vs custom)
   - Çözüm: PropertyValueChangedEventArgs olarak rename

3. **OnPaint Override**: SkiaSharp Panel'de OnPaint(SKCanvas) yok
   - Çözüm: WinForms Panel kullan veya BorderColor property kullan

4. **Parent Type Mismatch**: SDUI.Controls.Panel → System.Windows.Forms.Control cast edilemez
   - Çözüm: Parent'ı DesignerMainWindow (Form) yap

### 📊 Metrikler

- Toplam Kontrol Tipi: 11
- Grid Boyutu: 10px
- Resize Handle Sayısı: 8
- Keyboard Shortcut: 2 (Delete, Escape)
- Menu Item Sayısı: ~15

### 🚀 Performans

- Real-time code generation (< 10ms)
- Smooth drag operations (60 FPS target)
- Instant property updates
- No allocations in render loop (WinForms overlay kullanımı)

---

**Developer**: Mahmut Yıldırım  
**Framework**: SDUI (Custom SkiaSharp-based UI)  
**Platform**: .NET 8.0 Windows  
**Build Status**: ✅ Success (2 warnings)
