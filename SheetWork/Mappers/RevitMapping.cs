using Autodesk.Revit.DB;
using AxiomSabaleuski.Models;
using Profile = AutoMapper.Profile;

namespace AxiomSabaleuski.Mappers;

public class RevitMapping : Profile
{
    public RevitMapping()
    {
        DisableConstructorMapping();
        CreateMap<Sheet, ViewSheet>().ReverseMap()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(
                    src => src.Title))
            .ForMember(dest => dest.Number,
                opt => opt.MapFrom(
                    src => src.SheetNumber));
    }
}