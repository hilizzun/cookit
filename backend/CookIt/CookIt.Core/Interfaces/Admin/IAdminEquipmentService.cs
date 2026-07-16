using CookIt.Core.Dtos.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookIt.Core.Interfaces.Admin
{
    public interface IAdminEquipmentService
    {
        Task<List<EquipmentAdminDto>> GetAllEquipmentsAsync(bool includeDeleted = false);
        Task<EquipmentAdminDto> GetEquipmentByIdAsync(int id);
        Task<EquipmentAdminDto> CreateEquipmentAsync(CreateEquipmentDto dto);
        Task<EquipmentAdminDto> UpdateEquipmentAsync(UpdateEquipmentDto dto);
        Task<bool> DeleteEquipmentAsync(int id);
        Task<bool> RestoreEquipmentAsync(int id);
        Task<bool> IsEquipmentUsedAsync(int id);
    }
}
