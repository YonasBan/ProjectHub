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
    /// 分组 DbSet
    /// </summary>
    public DbSet<Group> Groups => Set<Group>();

    /// <summary>
    /// 标签 DbSet
    /// </summary>
    public DbSet<Tag> Tags => Set<Tag>();

    /// <summary>
    /// 项目 - 标签关联 DbSet
    /// </summary>
    public DbSet<ProjectTag> ProjectTags => Set<ProjectTag>();

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
            
            entity.HasIndex(e => e.Path)
                .IsUnique();
            
            entity.Property(e => e.Type)
                .IsRequired();
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.CustomIconPath)
                .HasMaxLength(500);
            
            entity.Property(e => e.ColorTag)
                .HasMaxLength(50);
            
            // 索引优化
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.IsArchived);
            entity.HasIndex(e => e.LastOpenedAt);
            entity.HasIndex(e => e.IsFavorite);
            
            // 忽略不存储的字段
            entity.Ignore(e => e.DiskSpaceBytes);
            entity.Ignore(e => e.CleanableSpaceBytes);
        });

        // ========== Group 配置 ==========
        modelBuilder.Entity<Group>(entity =>
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
        });

        // ========== Tag 配置 ==========
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.HasIndex(e => e.Name)
                .IsUnique();
            
            entity.Property(e => e.Color)
                .HasMaxLength(50);
        });

        // ========== ProjectTag 配置 (多对多关联表) ==========
        modelBuilder.Entity<ProjectTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne<Project>()
                .WithMany() // 注意：Project 实体中暂不添加导航属性集合，保持简洁
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne<Tag>()
                .WithMany()
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.ProjectId, e.TagId })
                .IsUnique();
        });

        // TODO: 未来添加 ProjectBundle 配置
        // TODO: 未来添加 Customer/Contact 配置
        // TODO: 未来添加 Attachment 配置
    }
}
