# Gallery - MAUI Photo Gallery App with Tagging

A feature-rich photo gallery application built with .NET MAUI that allows users to organize, tag, and search their photos using an intuitive interface.

## Features

- 📸 **Photo Management**: Add single or multiple photos to your gallery
- 🏷️ **Image Tagging**: Organize photos with custom tags and predefined categories
- 🔍 **Smart Search**: Search photos by title, description, filename, or tags
- 📱 **Cross-Platform**: Runs on Windows, Android, iOS, and macOS
- 💾 **Local Storage**: SQLite database with Entity Framework Core
- 🎨 **Modern UI**: Clean interface with material design elements
- 🌓 **Dark/Light Theme**: Support for both light and dark themes

## Screenshots

*[Screenshots would be added here when the app is running]*

## Architecture & Design

### Technology Stack

- **.NET 10**: Latest .NET framework
- **MAUI**: Cross-platform UI framework
- **Entity Framework Core 10.0**: Object-relational mapping
- **SQLite**: Local database storage
- **CommunityToolkit.Mvvm**: MVVM framework and helpers
- **CommunityToolkit.Maui**: Additional MAUI community controls
- **Syncfusion.Maui.Toolkit**: Enhanced UI components

### Project Structure

```
Gallery/
├── Data/
│   └── GalleryContext.cs           # EF Core database context
├── Models/
│   ├── Image.cs                    # Image entity model
│   ├── Tag.cs                      # Tag entity model
│   └── ImageTag.cs                 # Many-to-many relationship model
├── Services/
│   ├── DatabaseService.cs          # Database initialization and seeding
│   ├── ImageService.cs             # Image management operations
│   ├── TagService.cs               # Tag management operations
│   └── ImagePickerService.cs       # File picker functionality
├── Pages/
│   ├── MainPage.xaml               # Gallery grid view
│   └── ImageDetailPage.xaml        # Image detail and tagging view
├── PageModels/
│   ├── MainPageModel.cs            # Main gallery view model
│   └── ImageDetailPageModel.cs     # Image detail view model
└── Resources/                      # Fonts, images, and other assets
```

### Data Model

#### Core Entities

**Image Entity**
- `Id`: Primary key
- `FilePath`: Local file system path
- `FileName`: Original filename
- `Title`: User-defined title
- `Description`: User description
- `FileSize`: File size in bytes
- `CreatedAt`: Database insertion timestamp
- `TakenAt`: Photo creation/modification timestamp

**Tag Entity**
- `Id`: Primary key
- `Name`: Tag name (unique, 50 char max)
- `Description`: Tag description
- `CreatedAt`: Creation timestamp

**ImageTag Entity (Junction Table)**
- `Id`: Primary key
- `ImageId`: Foreign key to Image
- `TagId`: Foreign key to Tag
- `CreatedAt`: Association timestamp

#### Relationships

- **Image ↔ Tag**: Many-to-many relationship through ImageTag junction table
- **Cascade Delete**: Deleting an image removes all its tag associations
- **Unique Constraints**: Prevent duplicate tags and duplicate image-tag relationships

### Architecture Patterns

#### MVVM (Model-View-ViewModel)

The application follows the MVVM pattern using CommunityToolkit.Mvvm:

- **Models**: Data entities (Image, Tag, ImageTag)
- **Views**: XAML pages (MainPage, ImageDetailPage)
- **ViewModels**: Page models with observable properties and relay commands

#### Repository Pattern via Services

Services act as repositories providing abstraction over data access:

- **ImageService**: CRUD operations for images, search functionality
- **TagService**: Tag management and querying
- **DatabaseService**: Database initialization and maintenance

#### Dependency Injection

All services and view models are registered in the DI container:

```csharp
// Services
builder.Services.AddDbContext<GalleryContext>();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<ImagePickerService>();

// ViewModels and Pages
builder.Services.AddSingleton<MainPageModel>();
builder.Services.AddTransient<ImageDetailPageModel>();
```

### Key Features Implementation

#### Image Storage

- **Local Storage**: Images are copied to app's local storage directory
- **Unique Naming**: Timestamp + GUID prevents filename conflicts
- **File Management**: Physical file deletion when removing from database

#### Search Functionality

**Multi-criteria Search**:
- Text search across title, description, filename, and tag names
- Tag filtering with removable filter chips
- Combination search (text + tag filters)

**Search Implementation**:
```csharp
// Comprehensive text search
public async Task<IEnumerable<ImageModel>> SearchImagesAsync(string searchTerm)
{
    return await _context.Images
        .Include(i => i.ImageTags)
        .ThenInclude(it => it.Tag)
        .Where(i => i.Title.ToLower().Contains(searchTerm) ||
                   i.Description.ToLower().Contains(searchTerm) ||
                   i.FileName.ToLower().Contains(searchTerm) ||
                   i.ImageTags.Any(it => it.Tag.Name.ToLower().Contains(searchTerm)))
        .ToListAsync();
}
```

#### User Interface Design

**Main Gallery Page**:
- Grid layout with 3 columns for optimal photo viewing
- Pull-to-refresh functionality
- Search bar with real-time filtering
- Tag filter chips for quick filtering
- Toolbar buttons for adding single/multiple images

**Image Detail Page**:
- Large image display with aspect-fit scaling
- Editable title and description fields
- Current tags display with removal functionality
- Tag suggestion system for quick tagging
- Add new tag functionality
- Delete image with confirmation

#### Database Design Considerations

**Performance**:
- Indexed fields: FilePath (unique), Tag.Name (unique)
- Eager loading for related data to minimize queries
- Composite index on ImageTag(ImageId, TagId) for uniqueness

**Data Integrity**:
- Foreign key constraints with cascade delete
- Required field validations
- Maximum length constraints

### Error Handling

- **Service Level**: Try-catch blocks with debug logging
- **UI Level**: User-friendly error messages via DisplayAlert
- **Database**: Transaction rollback for consistency

### Threading and Async

- **UI Thread**: All UI updates on main thread
- **Background**: Database operations async with proper cancellation
- **Image Loading**: Async file operations for responsiveness

## Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio 2022 17.12+ or Visual Studio Code
- Platform-specific SDKs (Android SDK, iOS SDK, etc.)

### Installation

1. Clone the repository:
```bash
git clone [repository-url]
cd Gallery
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run --project Gallery.csproj --framework net10.0-windows10.0.19041.0
```

### First Run

The application will automatically:
1. Create the SQLite database in the app's data directory
2. Run initial migrations
3. Seed default tag categories (Family, Travel, Nature, etc.)

## Usage

### Adding Photos

1. **Single Photo**: Tap the "+" button in the toolbar
2. **Multiple Photos**: Tap the "Add Multiple" button
3. Select photos from your device's photo library
4. Photos are automatically copied to local storage and added to the database

### Tagging Photos

1. Tap any photo in the gallery to open the detail view
2. Use suggested tags for quick tagging
3. Add custom tags using the text input
4. Remove tags by tapping the "×" on tag chips

### Searching Photos

1. **Text Search**: Type in the search bar to search across all text fields and tags
2. **Tag Filtering**: Tap suggested tags to filter by specific tags
3. **Combined Search**: Use both text search and tag filters simultaneously
4. **Clear Filters**: Tap the "×" on filter chips to remove them

### Editing Photo Information

1. Open photo detail view
2. Tap "Edit" in the toolbar
3. Modify title and description
4. Tap "Save" to confirm changes

### Deleting Photos

1. Open photo detail view
2. Tap "Delete" in the toolbar
3. Confirm deletion in the dialog
4. Photo and all associated tags are removed

## Database Schema

```sql
-- Images table
CREATE TABLE Images (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FilePath TEXT NOT NULL UNIQUE,
    FileName TEXT NOT NULL,
    Title TEXT,
    Description TEXT,
    FileSize INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    TakenAt TEXT NOT NULL
);

-- Tags table
CREATE TABLE Tags (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT,
    CreatedAt TEXT NOT NULL
);

-- ImageTags junction table
CREATE TABLE ImageTags (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ImageId INTEGER NOT NULL,
    TagId INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ImageId) REFERENCES Images(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE,
    UNIQUE(ImageId, TagId)
);
```

## API Reference

### ImageService

```csharp
Task<IEnumerable<ImageModel>> GetAllImagesAsync()
Task<ImageModel?> GetImageByIdAsync(int id)
Task<IEnumerable<ImageModel>> SearchImagesAsync(string searchTerm)
Task<IEnumerable<ImageModel>> SearchImagesByTagsAsync(IEnumerable<string> tagNames)
Task<ImageModel> AddImageAsync(string filePath, string fileName, string? title = null, string? description = null)
Task<bool> AddTagToImageAsync(int imageId, int tagId)
Task<bool> RemoveTagFromImageAsync(int imageId, int tagId)
Task<bool> UpdateImageAsync(int imageId, string? title = null, string? description = null)
Task<bool> DeleteImageAsync(int imageId)
```

### TagService

```csharp
Task<IEnumerable<TagModel>> GetAllTagsAsync()
Task<TagModel?> GetTagByIdAsync(int id)
Task<TagModel?> GetTagByNameAsync(string name)
Task<IEnumerable<TagModel>> SearchTagsAsync(string searchTerm)
Task<TagModel> CreateTagAsync(string name, string? description = null)
Task<bool> UpdateTagAsync(int tagId, string? name = null, string? description = null)
Task<bool> DeleteTagAsync(int tagId)
Task<IEnumerable<TagModel>> GetTagsForImageAsync(int imageId)
Task<int> GetImageCountForTagAsync(int tagId)
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Future Enhancements

### Planned Features

- [ ] **Cloud Sync**: Backup photos and tags to cloud storage
- [ ] **Image Editing**: Basic editing capabilities (crop, rotate, filters)
- [ ] **Export/Import**: Tag data export/import functionality
- [ ] **Facial Recognition**: Automatic people tagging
- [ ] **Location Tags**: GPS-based location tagging
- [ ] **Collections**: Album/collection management
- [ ] **Sharing**: Share photos with tags to social media
- [ ] **Advanced Search**: Date ranges, file size filters, advanced queries
- [ ] **Bulk Operations**: Mass tagging and editing
- [ ] **Image Analysis**: Automatic tag suggestions using AI

### Technical Improvements

- [ ] **Performance**: Virtual scrolling for large galleries
- [ ] **Caching**: Image thumbnail caching
- [ ] **Offline**: Better offline capabilities
- [ ] **Sync**: Multi-device synchronization
- [ ] **Security**: Photo encryption options
- [ ] **Backup**: Automated backup strategies

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **Microsoft**: For the excellent MAUI framework
- **Community Toolkit**: For MVVM helpers and additional controls
- **Syncfusion**: For enhanced UI components
- **Entity Framework**: For robust data access capabilities

## Support

For questions, issues, or feature requests:

1. **Issues**: Use the GitHub Issues tab
2. **Discussions**: Use GitHub Discussions for general questions
3. **Documentation**: Check this README and code comments

---

**Built with ❤️ using .NET MAUI**
