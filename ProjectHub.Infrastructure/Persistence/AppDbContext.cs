using Microsoft.EntityFrameworkCore;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Infrastructure.Persistence;

/// <summary>
/// SQLite 数据库上下文
/// 使用 Entity Framework Core 进行数据持久化
/// 
/// DDD 设计要点:
/// - 只负责技术实现，不包含业务逻辑
/// - 通过实现仓储接口与领域层交互
/// - 可替换性：未来可轻松切换到其他数据库 (如 PostgreSQL, MySQL)
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 项目 DbSet
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// 工作文件夹 DbSet
    /// </summary>
    public DbSet<WorkFolder> WorkFolders => Set<WorkFolder>();

    /// <summary>
    /// 工作空间 DbSet
    /// </summary>
    public DbSet<WorkSpace> WorkSpaces => Set<WorkSpace>();

    /// <summary>
    /// 标签 DbSet
    /// </summary>
    public DbSet<Tag> Tags => Set<Tag>();

    /// <summary>
    /// 项目 - 标签关联 DbSet
    /// </summary>
    public DbSet<ProjectTag> ProjectTags => Set<ProjectTag>();

    /// <summary>
    /// 工作空间 - 标签关联 DbSet
    /// </summary>
    public DbSet<WorkSpaceTag> WorkSpaceTags => Set<WorkSpaceTag>();

    /// <summary>
    /// 项目 - 工作文件夹关联 DbSet
    /// </summary>
    public DbSet<ProjectWorkFolder> ProjectWorkFolders => Set<ProjectWorkFolder>();

    /// <summary>
    /// 项目 - 工作空间关联 DbSet
    /// </summary>
    public DbSet<ProjectWorkSpace> ProjectWorkSpaces => Set<ProjectWorkSpace>();

    /// <summary>
    /// 工作空间 - 工作文件夹关联 DbSet
    /// </summary>
    public DbSet<WorkSpaceWorkFolder> WorkSpaceWorkFolders => Set<WorkSpaceWorkFolder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========== Project 配置 ==========
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Path)
                .IsRequired()
                .HasMaxLength(1000);
            
            // 索引优化：Path 用于查询但不唯一（同一项目路径可属于不同工作空间）
            entity.HasIndex(e => e.Path);
            
            entity.Property(e => e.Type)
                .IsRequired();
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.CustomIconPath)
                .HasMaxLength(500);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            // 索引优化
            entity.HasIndex(e => e.LastOpenedAt);
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.FavoritedAt);
            
            // 忽略不存储的字段
            entity.Ignore(e => e.DiskSpaceBytes);
            entity.Ignore(e => e.CleanableSpaceBytes);
        });

        // ========== WorkFolder 配置 ==========
        modelBuilder.Entity<WorkFolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            // 复合唯一索引：同级目录（相同 ParentId）下名称不能重复
            entity.HasIndex(e => new { e.Name, e.ParentId })
                .IsUnique();
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.IconPath)
                .HasMaxLength(500);
            
            // 自引用外键：ParentId
            entity.HasOne<WorkFolder>()
                .WithMany()
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // 防止级联删除
            
            entity.HasIndex(e => e.ParentId);
        });

        // ========== WorkSpace 配置 ==========
        modelBuilder.Entity<WorkSpace>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IconPath)
                .HasMaxLength(500);

            entity.Property(e => e.LaunchOrderJson)
                .HasMaxLength(4000);

            // 索引优化
            entity.HasIndex(e => e.IsFavorite);
            entity.HasIndex(e => e.FavoritedAt);
            entity.HasIndex(e => e.LastOpenedAt);
        });

        // ========== ProjectWorkFolder 配置 (多对多关联表) ==========
        modelBuilder.Entity<ProjectWorkFolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne<WorkFolder>()
                .WithMany()
                .HasForeignKey(e => e.WorkFolderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.ProjectId, e.WorkFolderId })
                .IsUnique();
        });

        // ========== ProjectWorkSpace 配置 (多对多关联表) ==========
        modelBuilder.Entity<ProjectWorkSpace>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne<WorkSpace>()
                .WithMany()
                .HasForeignKey(e => e.WorkSpaceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.ProjectId, e.WorkSpaceId })
                .IsUnique();
        });

        // ========== WorkSpaceWorkFolder 配置 (多对多关联表) ==========
        modelBuilder.Entity<WorkSpaceWorkFolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne<WorkSpace>()
                .WithMany()
                .HasForeignKey(e => e.WorkSpaceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne<WorkFolder>()
                .WithMany()
                .HasForeignKey(e => e.WorkFolderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.WorkSpaceId, e.WorkFolderId })
                .IsUnique();
        });

        // ========== Tag 配置 ==========
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Color)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0);

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.HasIndex(e => e.SortOrder);
        });

        // ========== ProjectTag 配置 (多对多关联表) ==========
        modelBuilder.Entity<ProjectTag>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(p => p.Project)
                .WithMany(p => p.ProjectTags)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Tag)
                .WithMany(t => t.ProjectTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ProjectId, e.TagId })
                .IsUnique();

            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.TagId);
        });

        // ========== WorkSpaceTag 配置 (多对多关联表) ==========
        modelBuilder.Entity<WorkSpaceTag>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(w => w.WorkSpace)
                .WithMany(w => w.WorkSpaceTags)
                .HasForeignKey(e => e.WorkSpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Tag)
                .WithMany(t => t.WorkSpaceTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.WorkSpaceId, e.TagId })
                .IsUnique();

            entity.HasIndex(e => e.WorkSpaceId);
            entity.HasIndex(e => e.TagId);
        });

        // TODO: 未来添加 Customer/Contact 配置
        // TODO: 未来添加 Attachment 配置
    }
}
