# DTOs (Data Transfer Objects)

本目录包含应用层和UI层之间的数据传输对象（DTOs）。

## 文件组织结构

按照业务实体/数据表分类组织，每个文件包含相关的DTO类：

| 文件名 | 说明 |
|--------|------|
| `ProjectDtos.cs` | 项目相关的DTO（ProjectDto, CreateProjectDto, UpdateProjectDto） |
| `WorkFolderDtos.cs` | 工作文件夹相关的DTO（WorkFolderDto, CreateWorkFolderDto, UpdateWorkFolderDto） |
| `WorkSpaceDtos.cs` | 工作空间相关的DTO（WorkSpaceDto, CreateWorkSpaceDto, UpdateWorkSpaceDto, WorkSpaceProjectSettingsDto, UpdateWorkSpaceProjectSettingsDto） |
| `TagDtos.cs` | 标签相关的DTO（TagDto, CreateTagDto, UpdateTagDto, AddProjectTagDto, AddWorkSpaceTagDto） |
| `ProjectBundleDtos.cs` | 项目包相关的DTO（ProjectBundleDto, ProjectBundleItemDto, CreateProjectBundleDto, UpdateProjectBundleDto, ProjectBundleItemInput） |
| `CleanupDtos.cs` | 清理功能相关的DTO（CleanupScanResultDto, CleanupItemInfoDto, CleanupProfileDto） |

## 命名规范

### DTO 类型命名
- **实体DTO**: `{Entity}Dto` - 用于查询和展示实体数据
  - 例如: `ProjectDto`, `TagDto`, `WorkSpaceDto`

- **创建DTO**: `Create{Entity}Dto` - 用于创建新实体时的输入验证
  - 例如: `CreateProjectDto`, `CreateTagDto`

- **更新DTO**: `Update{Entity}Dto` - 用于更新现有实体时的输入验证
  - 例如: `UpdateProjectDto`, `UpdateTagDto`

- **操作DTO**: 描述特定操作的输入参数
  - 例如: `AddProjectTagDto`, `AddWorkSpaceTagDto`, `UpdateWorkSpaceProjectSettingsDto`

### 命名空间
所有 DTO 都使用统一命名空间: `ProjectHub.Application.DTOs`

## 使用示例

```csharp
using ProjectHub.Application.DTOs;

// 创建项目
var createDto = new CreateProjectDto
{
    Name = "My Project",
    Type = ProjectType.VisualStudio,
    Path = @"C:\Projects\MyProject.sln"
};
var projectDto = await _projectAppService.CreateAsync(createDto);

// 更新标签
var updateDto = new UpdateTagDto
{
    Id = 1,
    Name = "Important",
    Color = "#FF5733",
    Description = "重要项目",
    SortOrder = 1
};
var tagDto = await _tagAppService.UpdateAsync(updateDto);
```

## 设计原则

1. **单一职责**: 每个DTO类只负责特定场景的数据传输
2. **验证**: 创建和更新DTO使用数据注解进行输入验证（`[Required]`, `[MaxLength]` 等）
3. **不可变性**: 只读属性使用 `IReadOnlyList<T>` 确保外部无法修改
4. **性能优化**: 可选属性使用 `?` 标记，避免不必要的数据库查询
5. **集合初始化**: 使用集合表达式（`[]`）简化初始化代码（C# 12+）

## 迁移说明

原 `ProjectHub.Application.Interfaces.DTOs.cs` 文件已被拆分并迁移到本目录。
所有引用已更新为新的命名空间 `ProjectHub.Application.DTOs`。
