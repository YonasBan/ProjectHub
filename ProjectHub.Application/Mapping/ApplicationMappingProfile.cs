using AutoMapper;
using ProjectHub.Application.DTOs;
using ProjectHub.Domain.Entities;

namespace ProjectHub.Application.Mapping;

/// <summary>
/// AutoMapper 映射配置
/// 负责 Entity 和 DTO 之间的映射
/// </summary>
public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        ConfigureProjectMappings();
        ConfigureWorkSpaceMappings();
        ConfigureWorkFolderMappings();
    }

    /// <summary>
    /// 项目相关映射配置
    /// </summary>
    private void ConfigureProjectMappings()
    {
        // Project -> ProjectDto
        CreateMap<Project, ProjectDto>();

        // CreateProjectDto -> Project (通过工厂方法创建，这里仅配置字段映射用于参考)
        CreateMap<CreateProjectDto, Project>();

        // UpdateProjectDto -> Project (用于更新操作参考)
        CreateMap<UpdateProjectDto, Project>();
    }

    /// <summary>
    /// 工作空间相关映射配置
    /// </summary>
    private void ConfigureWorkSpaceMappings()
    {
        // WorkSpace -> WorkSpaceDto
        CreateMap<WorkSpace, WorkSpaceDto>();

        // WorkSpaceProjectLaunchOrder -> WorkSpaceProjectLaunchOrderDto
        CreateMap<WorkSpaceProjectLaunchOrder, WorkSpaceProjectLaunchOrderDto>();

        // ProjectWorkSpace -> WorkSpaceProjectSettingItemDto
        CreateMap<ProjectWorkSpace, WorkSpaceProjectSettingItemDto>()
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder));

        // CreateWorkSpaceDto -> WorkSpace (通过工厂方法创建)
        CreateMap<CreateWorkSpaceDto, WorkSpace>();

        // UpdateWorkSpaceDto -> WorkSpace
        CreateMap<UpdateWorkSpaceDto, WorkSpace>();
    }

    /// <summary>
    /// 工作文件夹相关映射配置
    /// </summary>
    private void ConfigureWorkFolderMappings()
    {
        // WorkFolder -> WorkFolderDto
        CreateMap<WorkFolder, WorkFolderDto>();

        // CreateWorkFolderDto -> WorkFolder
        CreateMap<CreateWorkFolderDto, WorkFolder>();

        // UpdateWorkFolderDto -> WorkFolder
        CreateMap<UpdateWorkFolderDto, WorkFolder>();
    }
}
