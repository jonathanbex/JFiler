using JFiler.Domain.Models.DB;
using JFiler.Domain.Models.ViewModel;

namespace JFiler.Domain.Mapper
{
  public static class ViewModelMapper
  {
    public static UserViewModel MapEntityToViewModel(User entity)
    {
      return new UserViewModel
      {
        Id = entity.Id,
        Username = entity.Username,
        Email = entity.Email,
        Admin = entity.Admin.GetValueOrDefault(false)
      };
    }
  }
}
